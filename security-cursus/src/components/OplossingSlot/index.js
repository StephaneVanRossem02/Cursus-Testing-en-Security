import React, { useCallback, useEffect, useState } from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import useIsBrowser from '@docusaurus/useIsBrowser';

import { leesConfig } from '../OefeningAssistent/config';
import DownloadSlot from '../DownloadSlot';
import Inhoud from './Inhoud';
import styles from './styles.module.css';

const OPSLAG_PREFIX = 'oplossing-slot:code:';

function leesBewaardeCode(hoofdstuk) {
  try {
    if (typeof window === 'undefined' || !window.localStorage) return '';
    return window.localStorage.getItem(OPSLAG_PREFIX + hoofdstuk) ?? '';
  } catch {
    return '';
  }
}

function bewaarCode(hoofdstuk, code) {
  try {
    window.localStorage.setItem(OPSLAG_PREFIX + hoofdstuk, code);
  } catch {
    /* opslag geblokkeerd: de code geldt dan enkel voor deze sessie */
  }
}

function wisCode(hoofdstuk) {
  try {
    window.localStorage.removeItem(OPSLAG_PREFIX + hoofdstuk);
  } catch {
    /* niets te doen */
  }
}

export default function OplossingSlot({ hoofdstuk }) {
  const isBrowser = useIsBrowser();
  const { siteConfig } = useDocusaurusContext();
  const config = leesConfig(siteConfig);

  const [codeInvoer, setCodeInvoer] = useState('');
  const [inhoud, setInhoud] = useState(null); // { titel, markdown }
  const [gebruikteCode, setGebruikteCode] = useState('');
  const [bezig, setBezig] = useState(false);
  const [fout, setFout] = useState('');

  const haalOp = useCallback(
    async (code) => {
      const opgekuist = code.trim();
      if (!opgekuist) return;

      if (!config.workerUrl) {
        setFout(
          'De oplossingen worden centraal vrijgegeven, maar die dienst is nog niet ' +
            'geactiveerd voor deze site. Vraag je docent om ze aan te zetten.',
        );
        return;
      }

      setBezig(true);
      setFout('');
      try {
        const antwoord = await fetch(config.workerUrl, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ actie: 'oplossing', hoofdstuk, code: opgekuist }),
        });
        if (antwoord.status === 403) {
          wisCode(hoofdstuk);
          setInhoud(null);
          setFout('Die lescode klopt niet, of deze les is nog niet vrijgegeven.');
          return;
        }
        if (!antwoord.ok) {
          setFout('Er ging iets mis bij het ophalen. Probeer het straks opnieuw.');
          return;
        }
        const data = await antwoord.json();
        setInhoud({ titel: data.titel, markdown: data.markdown });
        setGebruikteCode(opgekuist);
        bewaarCode(hoofdstuk, opgekuist);
      } catch {
        setFout('Geen verbinding. Zit je online? Probeer het straks opnieuw.');
      } finally {
        setBezig(false);
      }
    },
    [config.workerUrl, hoofdstuk],
  );

  // Bij het laden: was de code al eens ingegeven, dan meteen opnieuw ontgrendelen.
  useEffect(() => {
    if (!isBrowser) return;
    const bewaard = leesBewaardeCode(hoofdstuk);
    if (bewaard) haalOp(bewaard);
    // haalOp is stabiel per hoofdstuk/workerUrl.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isBrowser, hoofdstuk]);

  const verstuur = useCallback(
    (event) => {
      event.preventDefault();
      haalOp(codeInvoer);
    },
    [haalOp, codeInvoer],
  );

  const vergrendel = useCallback(() => {
    wisCode(hoofdstuk);
    setInhoud(null);
    setCodeInvoer('');
    setFout('');
  }, [hoofdstuk]);

  if (!isBrowser) {
    return (
      <div className={styles.kader} aria-busy="true">
        <p className={styles.uitleg}>De oplossingen worden geladen…</p>
      </div>
    );
  }

  if (inhoud) {
    return (
      <>
        <div className={styles.ontgrendeldBalk}>
          <span className={styles.slotOpen}>🔓 Oplossingen vrijgegeven</span>
          <button type="button" className={styles.tekstKnop} onClick={vergrendel}>
            Verbergen
          </button>
        </div>
        <Inhoud markdown={inhoud.markdown} />
        <DownloadSlot
          hoofdstuk={hoofdstuk}
          soort="oplossing"
          voorafCode={gebruikteCode}
          label="Download het volledige uitgewerkte project (ZIP)"
        />
      </>
    );
  }

  return (
    <div className={styles.kader}>
      <p className={styles.uitleg}>
        🔒 De uitgewerkte oplossingen van deze les zijn afgeschermd. Ze verschijnen pas nadat
        je de <strong>lescode</strong> invoert die je van je docent krijgt. Probeer de
        oefeningen dus eerst zelf: daar zit de leerwinst.
      </p>
      <form onSubmit={verstuur} className={styles.codeRij}>
        <input
          type="text"
          className={styles.codeInvoer}
          value={codeInvoer}
          onChange={(event) => setCodeInvoer(event.target.value)}
          placeholder="Lescode"
          autoComplete="off"
          spellCheck="false"
          aria-label="Lescode"
          disabled={bezig}
        />
        <button type="submit" className={styles.knop} disabled={bezig || !codeInvoer.trim()}>
          {bezig ? 'Bezig…' : 'Ontgrendelen'}
        </button>
      </form>
      {fout && (
        <p className={styles.fout} role="alert">
          {fout}
        </p>
      )}
    </div>
  );
}
