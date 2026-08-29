/**
 * localStorage-helpers: API-key en dagquota.
 *
 * Alles hier wordt enkel in de browser aangeroepen. Tijdens het bouwen van de
 * site (server-side) bestaat localStorage niet, vandaar overal een guard.
 */

const KEY_OPSLAG = 'oefening-assistent:gemini-key';
const QUOTA_OPSLAG = 'oefening-assistent:quota';

function beschikbaar() {
  try {
    return typeof window !== 'undefined' && !!window.localStorage;
  } catch {
    // localStorage gooit in privacy-modus of bij geblokkeerde cookies.
    return false;
  }
}

export function leesKey() {
  if (!beschikbaar()) return '';
  try {
    return window.localStorage.getItem(KEY_OPSLAG) ?? '';
  } catch {
    return '';
  }
}

/**
 * Op een oefeningpagina staan meerdere assistenten onder elkaar, een per oefening.
 * Zonder dit event blijft de tweede assistent naar een key-formulier kijken nadat de
 * student de key bij de eerste heeft ingegeven, tot hij de pagina herlaadt.
 */
export const KEY_GEWIJZIGD = 'oefening-assistent:key-gewijzigd';

function meldKeyWijziging() {
  try {
    window.dispatchEvent(new CustomEvent(KEY_GEWIJZIGD));
  } catch {
    /* oude browser zonder CustomEvent-constructor: dan valt enkel het syncen weg */
  }
}

export function bewaarKey(key) {
  if (!beschikbaar()) return;
  try {
    window.localStorage.setItem(KEY_OPSLAG, key.trim());
  } catch {
    /* opslag vol of geblokkeerd: de key geldt dan enkel voor deze sessie */
  }
  meldKeyWijziging();
}

export function wisKey() {
  if (!beschikbaar()) return;
  try {
    window.localStorage.removeItem(KEY_OPSLAG);
  } catch {
    /* niets te doen */
  }
  meldKeyWijziging();
}

/** Datumsleutel in lokale tijd. Niet UTC: de dag moet omslaan om middernacht hier. */
function vandaag() {
  const nu = new Date();
  const maand = String(nu.getMonth() + 1).padStart(2, '0');
  const dag = String(nu.getDate()).padStart(2, '0');
  return `${nu.getFullYear()}-${maand}-${dag}`;
}

/**
 * Aantal vragen dat vandaag al gesteld is.
 * De teller reset vanzelf omdat hij samen met de datum bewaard wordt.
 */
export function leesVerbruikVandaag() {
  if (!beschikbaar()) return 0;
  try {
    const ruw = window.localStorage.getItem(QUOTA_OPSLAG);
    if (!ruw) return 0;
    const data = JSON.parse(ruw);
    return data.datum === vandaag() ? Number(data.aantal) || 0 : 0;
  } catch {
    return 0;
  }
}

export function verhoogVerbruik() {
  if (!beschikbaar()) return 0;
  const nieuw = leesVerbruikVandaag() + 1;
  try {
    window.localStorage.setItem(QUOTA_OPSLAG, JSON.stringify({ datum: vandaag(), aantal: nieuw }));
  } catch {
    /* niet kunnen tellen mag het stellen van de vraag niet blokkeren */
  }
  return nieuw;
}
