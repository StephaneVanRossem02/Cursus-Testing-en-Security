/**
 * Instellingen voor de oefening-assistent.
 *
 * Alles hier kan per site overschreven worden in docusaurus.config.js:
 *
 *   customFields: {
 *     oefeningAssistent: { model: 'gemini-3.5-flash', vragenPerDag: 20 },
 *   },
 */

export const DEFAULT_CONFIG = {
  /**
   * Modelnaam. Stabiele Flash-modellen op het moment van schrijven:
   * gemini-3.6-flash, gemini-3.5-flash, gemini-2.5-flash,
   * gemini-3.5-flash-lite, gemini-3.1-flash-lite, gemini-2.5-flash-lite.
   * Flash-lite is het goedkoopst en heeft de ruimste gratis limieten.
   */
  model: 'gemini-3.5-flash-lite',

  apiBasis: 'https://generativelanguage.googleapis.com/v1beta',

  /**
   * Adres van de Cloudflare Worker. Die voegt de referentie-oplossing toe aan de prompt,
   * iets wat in de browser niet kan zonder ze aan studenten prijs te geven.
   *
   * Staat dit op null, of is de Worker onbereikbaar, dan praat de browser rechtstreeks
   * met Google. De assistent werkt dan nog steeds, maar zonder oplossing om mee te
   * vergelijken. Dat is bewust: een Worker die plat ligt mag de assistent op alle
   * oefeningpagina's niet stukmaken.
   */
  workerUrl: null,

  /**
   * Bewust hoger dan de ~400 die je zou verwachten voor korte hints.
   *
   * Gemini 3.x-modellen denken na voor ze antwoorden, en die denk-tokens tellen
   * mee als output-tokens. Bij een harde limiet van 400 kan het model zijn budget
   * opgebruiken tijdens het denken en een LEEG antwoord teruggeven.
   * De hints blijven kort omdat de system prompt dat oplegt, niet omdat de
   * token-limiet ze afkapt. Die limiet is enkel een noodrem.
   */
  maxOutputTokens: 1500,

  /**
   * Denkniveau ('minimal' | 'low' | 'medium' | 'high'), of null om het veld
   * helemaal niet mee te sturen.
   *
   * Standaard null: niet elk model kent dit veld, en een onbekend veld levert
   * een 400 op. Als jouw model het wel ondersteunt, zet dit op 'minimal' voor
   * snellere en goedkopere antwoorden.
   */
  thinkingLevel: null,

  temperature: 0.4,

  /** Maximum aantal tekens code dat een student in een keer mag plakken. */
  maxCodeTekens: 4000,

  /** Maximum lengte van de vraag zelf. */
  maxVraagTekens: 1000,

  /** Dagquota per student, geteld in localStorage. */
  vragenPerDag: 30,

  /**
   * Aantal eerdere beurten dat meegestuurd wordt als context.
   * Hoger = beter geheugen, maar meer tokens per vraag.
   */
  maxBeurtenGeheugen: 6,
};

/**
 * Alleen op localhost mag de Worker-URL uit localStorage komen. Zo kan je een Worker
 * uitproberen zonder config.js aan te passen, terwijl op de echte site altijd geldt wat
 * er in de configuratie staat.
 *
 * In de console van je browser:
 *   localStorage.setItem('oefening-assistent:worker-url', 'http://127.0.0.1:8787')
 *   localStorage.removeItem('oefening-assistent:worker-url')
 */
function testWorkerUrl() {
  try {
    if (typeof window === 'undefined') return null;
    const host = window.location.hostname;
    if (host !== 'localhost' && host !== '127.0.0.1') return null;
    return window.localStorage.getItem('oefening-assistent:worker-url') || null;
  } catch {
    return null;
  }
}

/** Voegt de overrides uit docusaurus.config.js samen met de standaardwaarden. */
export function leesConfig(siteConfig) {
  const config = { ...DEFAULT_CONFIG, ...(siteConfig?.customFields?.oefeningAssistent ?? {}) };
  return { ...config, workerUrl: testWorkerUrl() ?? config.workerUrl };
}
