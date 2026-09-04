/**
 * Rechtstreekse aanroep van de Gemini-API vanuit de browser.
 *
 * Dit kan zonder proxy: generativelanguage.googleapis.com beantwoordt de
 * CORS-preflight met een Access-Control-Allow-Origin voor de oproepende origin
 * en staat de header x-goog-api-key expliciet toe.
 *
 * De key van de student gaat in een header, niet in de URL. Query strings komen
 * in server-logs en in de browsergeschiedenis terecht, headers niet.
 */

/** Fout met een soort, zodat de UI er een begrijpelijke boodschap van kan maken. */
export class AssistentFout extends Error {
  constructor(soort, boodschap, extra = {}) {
    super(boodschap);
    this.name = 'AssistentFout';
    this.soort = soort;
    Object.assign(this, extra);
  }
}

/** Haalt het aantal seconden uit de RetryInfo die Google bij een 429 meestuurt. */
function leesWachttijd(foutBody) {
  const details = foutBody?.error?.details ?? [];
  for (const detail of details) {
    if (typeof detail?.retryDelay === 'string') {
      const seconden = parseInt(detail.retryDelay, 10);
      if (Number.isFinite(seconden)) return seconden;
    }
  }
  return null;
}

function vertaalHttpFout(status, body) {
  const melding = body?.error?.message ?? '';

  if (status === 400 && /api[_ ]?key/i.test(melding)) {
    throw new AssistentFout('ongeldige-key', 'De API-key wordt niet aanvaard door Google.');
  }
  if (status === 401 || status === 403) {
    // Vaak: key beperkt tot bepaalde websites, of Generative Language API niet aan staan.
    throw new AssistentFout('geweigerde-key', melding || 'Google weigert deze API-key.');
  }
  if (status === 429) {
    throw new AssistentFout('rate-limit', melding || 'Te veel aanvragen na elkaar.', {
      wachtSeconden: leesWachttijd(body),
    });
  }
  if (status === 404) {
    throw new AssistentFout('onbekend-model', melding || 'Dit model bestaat niet (meer).');
  }
  if (status >= 500) {
    throw new AssistentFout('server', 'De server van Google antwoordt even niet.');
  }
  throw new AssistentFout('onbekend', melding || `Onverwachte fout (HTTP ${status}).`);
}

/** Haalt de tekst uit het antwoord en negeert eventuele denk-fragmenten. */
function leesAntwoordTekst(kandidaat) {
  const parts = kandidaat?.content?.parts ?? [];
  return parts
    .filter((part) => part?.thought !== true && typeof part?.text === 'string')
    .map((part) => part.text)
    .join('')
    .trim();
}

/** Kleine pauze tussen herpogingen. */
function wacht(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Maakt van een geslaagd (HTTP 200) antwoord de uiteindelijke tekst, of gooit een
 * begrijpelijke fout als er geen bruikbare tekst in zit (blokkade, leeg, afgekapt).
 */
function verwerkAntwoord(data) {
  if (!data) {
    throw new AssistentFout('onbekend', 'Het antwoord van de server was onleesbaar.');
  }

  const blokkade = data?.promptFeedback?.blockReason;
  if (blokkade) {
    throw new AssistentFout('geblokkeerd', `Google blokkeerde deze vraag (${blokkade}).`);
  }

  const kandidaat = data?.candidates?.[0];
  const tekst = leesAntwoordTekst(kandidaat);

  if (!tekst) {
    if (kandidaat?.finishReason === 'MAX_TOKENS') {
      throw new AssistentFout(
        'te-lang',
        'Het model had zijn tokenbudget op voor het aan een antwoord toekwam.',
      );
    }
    if (kandidaat?.finishReason === 'SAFETY') {
      throw new AssistentFout('geblokkeerd', 'Het antwoord werd tegengehouden door een filter.');
    }
    throw new AssistentFout('leeg', 'Het model gaf een leeg antwoord terug.');
  }

  // Wel tekst, maar afgekapt: tonen met een waarschuwing is beter dan weggooien.
  if (kandidaat?.finishReason === 'MAX_TOKENS') {
    return `${tekst}\n\n_(Dit antwoord werd afgekapt. Stel je vraag wat specifieker.)_`;
  }

  return tekst;
}

/**
 * Doet exact EEN aanvraag voor een bepaald model, en geeft de rauwe Response terug
 * (ook bij een 5xx: de beller beslist of er herprobeerd wordt).
 *
 * Staat er een Worker ingesteld, dan gaat de aanvraag daarlangs — die plakt de
 * referentie-oplossing bij de prompt. Is de Worker onbereikbaar (netwerkfout), dan valt
 * deze functie in dezelfde poging terug op een rechtstreekse aanroep van Google, zodat een
 * platliggende Worker de assistent niet stukmaakt (wel zonder oplossing om mee te vergelijken).
 */
async function doeAanvraag({
  apiKey,
  systemPrompt,
  contents,
  generationConfig,
  config,
  signal,
  oefening,
  hoofdstuk,
  modelNaam,
}) {
  if (config.workerUrl) {
    try {
      return await fetch(config.workerUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'x-student-key': apiKey },
        body: JSON.stringify({
          oefening,
          hoofdstuk,
          geschiedenis: contents,
          model: modelNaam,
          generationConfig,
        }),
        signal,
      });
    } catch (fout) {
      if (fout?.name === 'AbortError') throw fout;
      // Worker onbereikbaar: verder naar het rechtstreekse pad hieronder.
    }
  }

  const url = `${config.apiBasis}/models/${encodeURIComponent(modelNaam)}:generateContent`;
  return fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'x-goog-api-key': apiKey },
    body: JSON.stringify({
      systemInstruction: { parts: [{ text: systemPrompt }] },
      contents,
      generationConfig,
    }),
    signal,
  });
}

/**
 * Stelt een vraag aan Gemini.
 *
 * @param {object} opties
 * @param {string} opties.apiKey      key van de student
 * @param {string} opties.systemPrompt
 * @param {Array}  opties.geschiedenis  [{ rol: 'student'|'assistent', tekst }]
 * @param {object} opties.config      resultaat van leesConfig()
 * @param {AbortSignal} [opties.signal]
 * @returns {Promise<string>} het antwoord van de assistent
 */
export async function vraagAanGemini({
  apiKey,
  systemPrompt,
  geschiedenis,
  config,
  signal,
  oefening,
  hoofdstuk,
}) {
  if (!apiKey) {
    throw new AssistentFout('geen-key', 'Er is nog geen API-key ingesteld.');
  }

  const generationConfig = {
    maxOutputTokens: config.maxOutputTokens,
    temperature: config.temperature,
  };
  // Enkel meesturen als het expliciet ingesteld is: oudere modellen kennen dit veld niet.
  if (config.thinkingLevel) {
    generationConfig.thinkingConfig = { thinkingLevel: config.thinkingLevel };
  }

  const contents = geschiedenis.map((beurt) => ({
    role: beurt.rol === 'student' ? 'user' : 'model',
    parts: [{ text: beurt.tekst }],
  }));

  // Eerst het ingestelde model, dan het terugvalmodel. Op een 5xx (meestal "overloaded"
  // op het drukke gratis flash-lite) herproberen we een paar keer; blijft dat model
  // weigeren, dan schakelen we over naar het stabielere terugvalmodel.
  const modellen = [config.model];
  if (config.fallbackModel && config.fallbackModel !== config.model) {
    modellen.push(config.fallbackModel);
  }
  const maxPogingen = Math.max(1, config.maxPogingen ?? 1);
  const basisWacht = config.herpogingWachtMs ?? 500;

  let laatsteTijdelijkeFout = null;

  for (const modelNaam of modellen) {
    for (let poging = 1; poging <= maxPogingen; poging += 1) {
      const isLaatstePoging = poging === maxPogingen;

      let response;
      try {
        response = await doeAanvraag({
          apiKey,
          systemPrompt,
          contents,
          generationConfig,
          config,
          signal,
          oefening,
          hoofdstuk,
          modelNaam,
        });
      } catch (fout) {
        if (fout?.name === 'AbortError') throw fout;
        // Netwerk-/CORS-fout: tijdelijk, dus herproberen heeft zin.
        laatsteTijdelijkeFout = new AssistentFout('netwerk', 'Geen verbinding met de Gemini-API.');
        if (!isLaatstePoging) {
          await wacht(basisWacht * poging);
          continue;
        }
        break; // pogingen op → volgend model proberen
      }

      let data = null;
      try {
        data = await response.json();
      } catch {
        data = null;
      }

      if (response.ok) {
        return verwerkAntwoord(data);
      }

      // Een 5xx is tijdelijk (server/overbelasting): herproberen, of naar het volgende model.
      if (response.status >= 500) {
        laatsteTijdelijkeFout = new AssistentFout(
          'server',
          'De server van Google antwoordt even niet.',
        );
        if (!isLaatstePoging) {
          await wacht(basisWacht * poging);
          continue;
        }
        break; // pogingen op → volgend model proberen
      }

      // 400/401/403/404/429: opnieuw proberen of van model wisselen lost dit niet op.
      // Meteen een begrijpelijke fout tonen.
      vertaalHttpFout(response.status, data);
    }
  }

  // Alle modellen en pogingen uitgeput op een tijdelijke fout.
  throw (
    laatsteTijdelijkeFout ?? new AssistentFout('server', 'De server van Google antwoordt even niet.')
  );
}
