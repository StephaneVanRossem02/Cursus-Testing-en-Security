/**
 * Cloudflare Worker voor de oefening-assistent (cursus Testing & Security).
 *
 * Waarom deze Worker bestaat: de referentie-oplossingen mogen niet in de JS-bundel
 * terechtkomen, want die is leesbaar voor elke student (F5 / view-source). Ze leven
 * daarom in oplossingen.json naast dit bestand, en dat bestand wordt nergens door de
 * frontend geimporteerd. De Worker plakt de juiste oplossing bij de system prompt en
 * stuurt het geheel door naar Google.
 *
 * De Worker heeft GEEN eigen Gemini-key. Hij gebruikt die van de student, die per
 * aanvraag wordt meegestuurd en nergens bewaard of gelogd wordt. Wie dit endpoint vindt,
 * kan er dus geen gedeeld quotum mee leegtrekken.
 *
 * Het antwoord van Google wordt onbewerkt teruggegeven, zodat de foutafhandeling in de
 * browser identiek blijft aan het terugvalpad dat rechtstreeks met Google praat.
 */

import { bouwSystemPrompt, zoekOefening } from '../../src/components/OefeningAssistent/prompt.js';
import oplossingenData from '../oplossingen.json';
import paginaData from '../oplossingen-paginas.json';
import downloadManifest from '../downloads-manifest.json';

const TOEGELATEN_ORIGINS = [
  'https://stephanevanrossem02.github.io',
  'http://localhost:3000',
  'http://localhost:3001',
  'http://localhost:3002',
];

/** Bovengrenzen, zodat een kapotte of kwaadwillige client de Worker niet kan belasten. */
const MAX_BEURTEN = 20;
const MAX_TEKENS_PER_BEURT = 8000;

function corsHeaders(origin) {
  const toegestaan = TOEGELATEN_ORIGINS.includes((origin || '').toLowerCase());
  return {
    'Access-Control-Allow-Origin': toegestaan ? origin : TOEGELATEN_ORIGINS[0],
    'Access-Control-Allow-Methods': 'POST, OPTIONS',
    'Access-Control-Allow-Headers': 'content-type, x-student-key',
    'Access-Control-Max-Age': '86400',
    Vary: 'Origin',
  };
}

function json(data, status, origin) {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'Content-Type': 'application/json', ...corsHeaders(origin) },
  });
}

/** Zelfde vorm als een foutantwoord van Google, zodat de browser maar een pad nodig heeft. */
function fout(boodschap, status, origin) {
  return json({ error: { message: boodschap, code: status } }, status, origin);
}

/** Vergelijkt twee strings in constante tijd, zodat de juiste code niet via timing lekt. */
function gelijkVeilig(a, b) {
  if (a.length !== b.length) return false;
  let verschil = 0;
  for (let i = 0; i < a.length; i += 1) {
    verschil |= a.charCodeAt(i) ^ b.charCodeAt(i);
  }
  return verschil === 0;
}

/**
 * Route "oplossing": geeft de volledige oplossingspagina van een les terug, maar enkel
 * als de juiste lescode is meegestuurd. De codes staan als secret in de Worker
 * (LESCODE_L01 .. LESCODE_L12) en zitten dus niet in de bundel. Is er voor een les geen
 * secret gezet, dan is die les nog niet vrijgegeven.
 */
function geefOplossing(body, env, origin) {
  const hoofdstuk = typeof body.hoofdstuk === 'string' ? body.hoofdstuk : '';
  const pagina = paginaData.paginas?.[hoofdstuk];
  if (!pagina) {
    return fout('Onbekende les.', 404, origin);
  }

  const verwacht = env ? env[`LESCODE_${hoofdstuk}`] : undefined;
  if (!verwacht) {
    return fout('De oplossingen van deze les zijn nog niet vrijgegeven.', 403, origin);
  }

  const gegeven = (typeof body.code === 'string' ? body.code : '').trim();
  // Kleine soepelheid: hoofdletterongevoelig, zodat "Kat" en "kat" allebei werken.
  if (!gegeven || !gelijkVeilig(gegeven.toLowerCase(), verwacht.trim().toLowerCase())) {
    return fout('Onjuiste lescode.', 403, origin);
  }

  return json({ titel: pagina.titel, markdown: pagina.markdown }, 200, origin);
}

/**
 * Route "download": levert een ZIP (startpakket of oplossing-project) uit KV, maar enkel na
 * de juiste code. Startpakketten hangen achter STARTCODE_Lxx, oplossing-projecten achter
 * LESCODE_Lxx (dezelfde code als de oplossingspagina). De ZIP's staan in KV, niet in de
 * publieke site.
 */
async function geefDownload(body, env, origin) {
  const hoofdstuk = typeof body.hoofdstuk === 'string' ? body.hoofdstuk : '';
  const soort = body.soort === 'start' || body.soort === 'oplossing' ? body.soort : '';
  const bestanden = downloadManifest[hoofdstuk];
  if (!soort || !bestanden || !bestanden[soort]) {
    return fout('Onbekende les of soort.', 404, origin);
  }

  const secretNaam = soort === 'start' ? `STARTCODE_${hoofdstuk}` : `LESCODE_${hoofdstuk}`;
  const verwacht = env ? env[secretNaam] : undefined;
  if (!verwacht) {
    const wat = soort === 'start' ? 'Het startpakket' : 'Het oplossing-project';
    return fout(`${wat} van deze les is nog niet vrijgegeven.`, 403, origin);
  }

  const gegeven = (typeof body.code === 'string' ? body.code : '').trim();
  if (!gegeven || !gelijkVeilig(gegeven.toLowerCase(), verwacht.trim().toLowerCase())) {
    return fout('Onjuiste code.', 403, origin);
  }

  const sleutel = `${soort}-${hoofdstuk}`;
  const bytes = env.DOWNLOADS ? await env.DOWNLOADS.get(sleutel, 'arrayBuffer') : null;
  if (!bytes) {
    return fout('Bestand niet gevonden.', 404, origin);
  }

  const bestandsnaam = bestanden[soort];
  return new Response(bytes, {
    status: 200,
    headers: {
      ...corsHeaders(origin),
      'Content-Type': 'application/zip',
      'Content-Disposition': `attachment; filename="${bestandsnaam}"`,
      'X-Download-Filename': bestandsnaam,
      'Access-Control-Expose-Headers': 'X-Download-Filename',
    },
  });
}

export default {
  async fetch(request, env) {
    const origin = request.headers.get('Origin') || '';

    if (request.method === 'OPTIONS') {
      return new Response(null, { status: 204, headers: corsHeaders(origin) });
    }
    if (request.method !== 'POST') {
      return fout('Enkel POST wordt ondersteund.', 405, origin);
    }
    if (origin && !TOEGELATEN_ORIGINS.includes(origin.toLowerCase())) {
      return fout('Deze site mag deze assistent niet gebruiken.', 403, origin);
    }

    let body;
    try {
      body = await request.json();
    } catch {
      return fout('De aanvraag was geen geldige JSON.', 400, origin);
    }

    // Twee routes op hetzelfde endpoint: het vrijgeven van een oplossingspagina met een
    // lescode, en de gewone assistent-aanvraag. De oplossing-route heeft geen Gemini-key
    // nodig.
    if (body?.actie === 'oplossing') {
      return geefOplossing(body, env, origin);
    }
    if (body?.actie === 'download') {
      return geefDownload(body, env, origin);
    }

    // De key van de student. Bewust een eigen header: hij gaat niet in de URL en wordt
    // hieronder nergens gelogd of bewaard.
    const apiKey = request.headers.get('x-student-key');
    if (!apiKey) {
      return fout('Er is geen API-key meegestuurd.', 401, origin);
    }

    const { oefening, hoofdstuk, geschiedenis, model, generationConfig } = body ?? {};

    if (!oefening || typeof oefening !== 'string' || !zoekOefening(oefening)) {
      return fout('Onbekende oefening.', 400, origin);
    }
    if (!Array.isArray(geschiedenis) || geschiedenis.length === 0) {
      return fout('Er is geen vraag meegestuurd.', 400, origin);
    }
    if (geschiedenis.length > MAX_BEURTEN) {
      return fout('Het gesprek is te lang.', 413, origin);
    }
    if (geschiedenis.some((b) => (b?.parts?.[0]?.text ?? '').length > MAX_TEKENS_PER_BEURT)) {
      return fout('Een van je berichten is te lang.', 413, origin);
    }

    const oplossing = oplossingenData.oplossingen?.[oefening] ?? null;
    // Een lege {} in oplossingen.json telt niet als oplossing: dan gedraagt de Worker
    // zich als het terugvalpad in plaats van een leeg blok in de prompt te zetten.
    const heeftInhoud = oplossing && Object.keys(oplossing).length > 0;

    const systemPrompt = bouwSystemPrompt({
      oefeningId: oefening,
      hoofdstukId: hoofdstuk,
      oplossing: heeftInhoud ? oplossing : null,
    });

    const modelNaam = typeof model === 'string' && model ? model : 'gemini-3.5-flash-lite';
    const url = `https://generativelanguage.googleapis.com/v1beta/models/${encodeURIComponent(
      modelNaam,
    )}:generateContent`;

    let antwoord;
    try {
      antwoord = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'x-goog-api-key': apiKey },
        body: JSON.stringify({
          systemInstruction: { parts: [{ text: systemPrompt }] },
          contents: geschiedenis,
          generationConfig: generationConfig ?? {},
        }),
      });
    } catch {
      return fout('De assistent kon Google niet bereiken.', 502, origin);
    }

    // Onbewerkt doorgeven, inclusief de statuscode. De browser vertaalt 400, 429 en de
    // rest al naar begrijpelijke meldingen; dat hoeft hier niet nog eens.
    const tekst = await antwoord.text();
    return new Response(tekst, {
      status: antwoord.status,
      headers: { 'Content-Type': 'application/json', ...corsHeaders(origin) },
    });
  },
};
