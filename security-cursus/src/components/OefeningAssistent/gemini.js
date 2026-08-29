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

  let response;
  let viaWorker = false;

  if (config.workerUrl) {
    try {
      response = await fetch(config.workerUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'x-student-key': apiKey },
        body: JSON.stringify({
          oefening,
          hoofdstuk,
          geschiedenis: contents,
          model: config.model,
          generationConfig,
        }),
        signal,
      });
      viaWorker = true;
    } catch (fout) {
      if (fout?.name === 'AbortError') throw fout;
      // Worker onbereikbaar. Niet doorbreken: hieronder valt hij terug op Google.
      // De assistent werkt dan zonder referentie-oplossing, en dat is beter dan niets.
      response = null;
    }
  }

  if (!response) {
    const body = {
      systemInstruction: { parts: [{ text: systemPrompt }] },
      contents,
      generationConfig,
    };
    const url = `${config.apiBasis}/models/${encodeURIComponent(config.model)}:generateContent`;

    try {
      response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'x-goog-api-key': apiKey },
        body: JSON.stringify(body),
        signal,
      });
    } catch (fout) {
      if (fout?.name === 'AbortError') throw fout;
      // fetch gooit hier bij offline zijn, een geblokkeerde request of een CORS-probleem.
      throw new AssistentFout('netwerk', 'Geen verbinding met de Gemini-API.');
    }
  }

  // 5xx van de Worker zelf: ook dan nog een keer rechtstreeks proberen.
  if (viaWorker && response.status >= 500) {
    const body = {
      systemInstruction: { parts: [{ text: systemPrompt }] },
      contents,
      generationConfig,
    };
    const url = `${config.apiBasis}/models/${encodeURIComponent(config.model)}:generateContent`;
    try {
      response = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'x-goog-api-key': apiKey },
        body: JSON.stringify(body),
        signal,
      });
    } catch {
      /* de oorspronkelijke reactie van de Worker wordt hieronder afgehandeld */
    }
  }

  let data = null;
  try {
    data = await response.json();
  } catch {
    if (!response.ok) vertaalHttpFout(response.status, null);
    throw new AssistentFout('onbekend', 'Het antwoord van de server was onleesbaar.');
  }

  if (!response.ok) vertaalHttpFout(response.status, data);

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
