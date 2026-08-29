import React, { useCallback, useEffect, useState } from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import useIsBrowser from '@docusaurus/useIsBrowser';

import { leesConfig } from '../OefeningAssistent/config';
import styles from './styles.module.css';

const OPSLAG_PREFIX = 'download-slot:code:';

function sleutel(soort, hoofdstuk) {
  return `${OPSLAG_PREFIX}${soort}:${hoofdstuk}`;
}

function leesBewaardeCode(soort, hoofdstuk) {
  try {
    if (typeof window === 'undefined' || !window.localStorage) return '';
    return window.localStorage.getItem(sleutel(soort, hoofdstuk)) ?? '';
  } catch {
    return '';
  }
}

function bewaarCode(soort, hoofdstuk, code) {
  try {
    window.localStorage.setItem(sleutel(soort, hoofdstuk), code);
  } catch {
    /* opslag geblokkeerd: dan geldt de code enkel voor deze sessie */
  }
}

/**
 * Downloadknop achter een code. `soort` is 'start' (startpakket, achter STARTCODE) of
 * 'oplossing' (oplossing-project, achter LESCODE). De ZIP komt uit de Worker/KV, niet uit
 * de publieke site.
 */
export default function DownloadSlot({ hoofdstuk, soort = 'start', label, voorafCode }) {
  const isBrowser = useIsBrowser();
  const { siteConfig } = useDocusaurusContext();
  const config = leesConfig(siteConfig);

  const [codeInvoer, setCodeInvoer] = useState(voorafCode || '');
  const [bezig, setBezig] = useState(false);
  const [fout, setFout] = useState('');
  const [klaar, setKlaar] = useState(false);

  // Onthouden code (of een meegegeven voorafCode) voorinvullen, maar niet automatisch
  // downloaden.
  useEffect(() => {
    if (!isBrowser) return;
    const bewaard = leesBewaardeCode(soort, hoofdstuk);
    if (bewaard) setCodeInvoer(bewaard);
    else if (voorafCode) setCodeInvoer(voorafCode);
  }, [isBrowser, soort, hoofdstuk, voorafCode]);

  const woord = soort === 'start' ? 'startpakket' : 'oplossing-project';

  const download = useCallback(
    async (event) => {
      event.preventDefault();
      const code = codeInvoer.trim();
      if (!code) return;

      if (!config.workerUrl) {
        setFout('Deze download is nog niet geactiveerd voor deze site. Vraag je docent.');
        return;
      }

      setBezig(true);
      setFout('');
      setKlaar(false);
      try {
        const antwoord = await fetch(config.workerUrl, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ actie: 'download', hoofdstuk, soort, code }),
        });
        if (antwoord.status === 403) {
          setFout(`Die code klopt niet, of dit ${woord} is nog niet vrijgegeven.`);
          return;
        }
        if (!antwoord.ok) {
          setFout('Er ging iets mis bij het downloaden. Probeer het straks opnieuw.');
          return;
        }
        const bestandsnaam =
          antwoord.headers.get('X-Download-Filename') || `shopwave-${hoofdstuk}.zip`;
        const blob = await antwoord.blob();
        const url = URL.createObjectURL(blob);
        const anker = document.createElement('a');
        anker.href = url;
        anker.download = bestandsnaam;
        document.body.appendChild(anker);
        anker.click();
        anker.remove();
        URL.revokeObjectURL(url);
        bewaarCode(soort, hoofdstuk, code);
        setKlaar(true);
      } catch {
        setFout('Geen verbinding. Zit je online? Probeer het straks opnieuw.');
      } finally {
        setBezig(false);
      }
    },
    [codeInvoer, config.workerUrl, hoofdstuk, soort, woord],
  );

  if (!isBrowser) {
    return <p className={styles.uitleg}>De download wordt geladen…</p>;
  }

  return (
    <div className={styles.kader}>
      <p className={styles.uitleg}>
        🔒 {label || `Download het ${woord} van deze les`}. Voer de{' '}
        <strong>{soort === 'start' ? 'startcode' : 'lescode'}</strong> in die je van je docent
        kreeg.
      </p>
      <form onSubmit={download} className={styles.rij}>
        <input
          type="text"
          className={styles.invoer}
          value={codeInvoer}
          onChange={(event) => setCodeInvoer(event.target.value)}
          placeholder={soort === 'start' ? 'Startcode' : 'Lescode'}
          autoComplete="off"
          spellCheck="false"
          aria-label={soort === 'start' ? 'Startcode' : 'Lescode'}
          disabled={bezig}
        />
        <button type="submit" className={styles.knop} disabled={bezig || !codeInvoer.trim()}>
          {bezig ? 'Bezig…' : 'Download ZIP'}
        </button>
      </form>
      {klaar && !fout && (
        <p className={styles.klaar} role="status">
          ✓ Download gestart. Zie je map met downloads.
        </p>
      )}
      {fout && (
        <p className={styles.fout} role="alert">
          {fout}
        </p>
      )}
    </div>
  );
}
