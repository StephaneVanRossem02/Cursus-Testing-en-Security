import React, { useCallback, useEffect, useRef, useState } from 'react';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import useIsBrowser from '@docusaurus/useIsBrowser';

import Antwoord from './Antwoord';
import { leesConfig } from './config';
import { AssistentFout, vraagAanGemini } from './gemini';
import { bouwSystemPrompt, zoekOefening } from './prompt';
import {
  KEY_GEWIJZIGD,
  bewaarKey,
  leesKey,
  leesVerbruikVandaag,
  verhoogVerbruik,
  wisKey,
} from './storage';
import styles from './styles.module.css';

const AI_STUDIO_URL = 'https://aistudio.google.com/apikey';

/** Zet een fout om in een begrijpelijke boodschap. */
function foutBoodschap(fout) {
  if (!(fout instanceof AssistentFout)) {
    return 'Er ging iets mis. Probeer het straks opnieuw.';
  }
  switch (fout.soort) {
    case 'geen-key':
      return 'Er is nog geen API-key ingesteld.';
    case 'ongeldige-key':
      return 'Deze API-key wordt niet aanvaard. Controleer of je hem volledig gekopieerd hebt en maak eventueel een nieuwe aan in Google AI Studio.';
    case 'geweigerde-key':
      return 'Google weigert deze key. Vaak komt dat omdat de Gemini API nog niet aan staat voor je project, of omdat je key beperkt is tot bepaalde websites.';
    case 'rate-limit':
      return fout.wachtSeconden
        ? `Je stelde te snel te veel vragen. Wacht ongeveer ${fout.wachtSeconden} seconden en probeer opnieuw.`
        : 'Je stelde te snel te veel vragen na elkaar. Wacht een halve minuut en probeer opnieuw. De gratis versie laat maar een beperkt aantal vragen per minuut toe.';
    case 'onbekend-model':
      return 'Het ingestelde model bestaat niet meer. Geef dit door aan je docent.';
    case 'netwerk':
      return 'Geen verbinding met Gemini. Zit je online? Een adblocker of schoolnetwerk kan de aanvraag ook tegenhouden.';
    case 'server':
      return 'De servers van Google antwoorden even niet. Probeer het over een minuut opnieuw.';
    case 'geblokkeerd':
      return 'Je vraag werd tegengehouden door een filter van Google. Herformuleer ze.';
    case 'te-lang':
    case 'leeg':
      return 'Het model gaf geen bruikbaar antwoord. Stel je vraag wat korter en specifieker.';
    default:
      return fout.message || 'Er ging iets mis. Probeer het straks opnieuw.';
  }
}

export default function OefeningAssistent({ oefening, hoofdstuk }) {
  const isBrowser = useIsBrowser();
  const { siteConfig } = useDocusaurusContext();
  const config = leesConfig(siteConfig);

  const oefeningData = zoekOefening(oefening);

  const [key, setKey] = useState('');
  const [keyInvoer, setKeyInvoer] = useState('');
  const [toontKeyForm, setToontKeyForm] = useState(false);
  const [berichten, setBerichten] = useState([]);
  const [vraag, setVraag] = useState('');
  const [code, setCode] = useState('');
  const [toontCode, setToontCode] = useState(false);
  const [bezig, setBezig] = useState(false);
  const [fout, setFout] = useState(null);
  const [verbruik, setVerbruik] = useState(0);

  const gesprekEinde = useRef(null);
  const vraagVeld = useRef(null);

  // localStorage bestaat niet tijdens het bouwen van de site, dus pas na mount lezen.
  useEffect(() => {
    const synchroniseer = () => {
      setKey(leesKey());
      setVerbruik(leesVerbruikVandaag());
    };
    synchroniseer();

    // KEY_GEWIJZIGD: andere assistent op dezelfde pagina. storage: ander tabblad.
    window.addEventListener(KEY_GEWIJZIGD, synchroniseer);
    window.addEventListener('storage', synchroniseer);
    return () => {
      window.removeEventListener(KEY_GEWIJZIGD, synchroniseer);
      window.removeEventListener('storage', synchroniseer);
    };
  }, []);

  useEffect(() => {
    if (berichten.length > 0) {
      gesprekEinde.current?.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    }
  }, [berichten, bezig]);

  const quotaOp = verbruik >= config.vragenPerDag;
  const codeTeLang = code.length > config.maxCodeTekens;
  const vraagTeLang = vraag.length > config.maxVraagTekens;
  const kanVersturen =
    !bezig && !quotaOp && !codeTeLang && !vraagTeLang && vraag.trim().length > 0 && !!key;

  const bewaarNieuweKey = useCallback(
    (event) => {
      event.preventDefault();
      const opgekuist = keyInvoer.trim();
      if (!opgekuist) return;
      bewaarKey(opgekuist);
      setKey(opgekuist);
      setKeyInvoer('');
      setToontKeyForm(false);
      setFout(null);
    },
    [keyInvoer],
  );

  const verwijderKey = useCallback(() => {
    wisKey();
    setKey('');
    setBerichten([]);
    setToontKeyForm(true);
  }, []);

  const verstuur = useCallback(
    async (event) => {
      event?.preventDefault();
      if (!kanVersturen) return;

      const vraagTekst = code.trim()
        ? `${vraag.trim()}\n\nDit is mijn code op dit moment:\n\`\`\`csharp\n${code.trim()}\n\`\`\``
        : vraag.trim();

      const nieuweBerichten = [...berichten, { rol: 'student', tekst: vraagTekst }];
      setBerichten(nieuweBerichten);
      setVraag('');
      setFout(null);
      setBezig(true);

      // Enkel de laatste beurten meesturen; oudere context is zelden nog nuttig.
      const geschiedenis = nieuweBerichten.slice(-config.maxBeurtenGeheugen);

      try {
        const antwoord = await vraagAanGemini({
          apiKey: key,
          // Terugvalprompt, zonder oplossing. Gaat de aanvraag via de Worker, dan bouwt
          // die zijn eigen prompt met de oplossing erbij en wordt deze genegeerd.
          systemPrompt: bouwSystemPrompt({ oefeningId: oefening, hoofdstukId: hoofdstuk }),
          geschiedenis,
          config,
          oefening,
          hoofdstuk,
        });
        setBerichten([...nieuweBerichten, { rol: 'assistent', tekst: antwoord }]);
        // Pas tellen na een geslaagd antwoord: een mislukte poging mag geen quota kosten.
        setVerbruik(verhoogVerbruik());
      } catch (nieuweFout) {
        setFout(nieuweFout);
        // De vraag terugzetten zodat de student ze niet opnieuw moet typen.
        setBerichten(berichten);
        setVraag(vraag);
      } finally {
        setBezig(false);
      }
    },
    [kanVersturen, berichten, vraag, code, key, config, oefening, hoofdstuk],
  );

  const opToetsaanslag = useCallback(
    (event) => {
      if (event.key === 'Enter' && (event.ctrlKey || event.metaKey)) verstuur(event);
    },
    [verstuur],
  );

  const titel = oefeningData?.titel ?? oefening;

  // Tijdens de server-render weten we nog niet of er een key is. Een vaste,
  // neutrale schil voorkomt dat de inhoud verspringt bij het hydrateren.
  if (!isBrowser) {
    return (
      <details className={styles.kader} aria-busy="true">
        <summary className={styles.kop}>
          <span className={styles.badge}>Vastgelopen?</span>
          <h3 className={styles.titel}>Hulp bij {titel}</h3>
          <span className={styles.openHint}>klik voor hulp</span>
        </summary>
        <p className={styles.uitleg}>De assistent wordt geladen…</p>
      </details>
    );
  }

  return (
    <details className={styles.kader}>
      <summary className={styles.kop}>
        <span className={styles.badge}>Vastgelopen?</span>
        <h3 className={styles.titel}>Hulp bij {titel}</h3>
        <span className={styles.openHint}>klik voor hulp</span>
      </summary>

      {key && (
        <p className={styles.quota}>
          {Math.max(0, config.vragenPerDag - verbruik)} van {config.vragenPerDag} vragen over
          vandaag
        </p>
      )}

      {!oefeningData && (
        <p className={styles.waarschuwing}>
          Deze oefening staat nog niet in <code>oefeningen.json</code>. De assistent helpt enkel
          algemeen.
        </p>
      )}

      {!key || toontKeyForm ? (
        <form onSubmit={bewaarNieuweKey} className={styles.keyForm}>
          <p className={styles.uitleg}>
            Deze assistent werkt met je eigen gratis Gemini-key. Je geeft die een keer in, daarna
            blijft hij in deze browser bewaard. Bij elke vraag gaat hij via de server van de
            opleiding naar Google. Wij bewaren hem niet.
          </p>
          <ol className={styles.stappen}>
            <li>
              Ga naar{' '}
              <a href={AI_STUDIO_URL} target="_blank" rel="noreferrer noopener">
                Google AI Studio
              </a>{' '}
              en meld je aan met een Google-account.
            </li>
            <li>Klik op &laquo;Create API key&raquo; en kopieer de key.</li>
            <li>Plak hem hieronder.</li>
          </ol>
          <div className={styles.keyRij}>
            <input
              type="password"
              className={styles.keyInvoer}
              value={keyInvoer}
              onChange={(event) => setKeyInvoer(event.target.value)}
              placeholder="Plak hier je Gemini API-key"
              autoComplete="off"
              spellCheck="false"
              aria-label="Gemini API-key"
            />
            <button type="submit" className={styles.knop} disabled={!keyInvoer.trim()}>
              Bewaren
            </button>
          </div>
          {key && (
            <button
              type="button"
              className={styles.tekstKnop}
              onClick={() => setToontKeyForm(false)}
            >
              Annuleren
            </button>
          )}
        </form>
      ) : (
        <>
          {berichten.length === 0 && (
            <>
              <p className={styles.uitleg}>
                Snap je de opgave niet goed? Vraag om ze anders uit te leggen. Loopt je test of
                code vast? Beschrijf wat er misgaat, en stuur je code mee als je wil. Je krijgt
                nooit de oplossing, wel iets waarmee je zelf verder geraakt.
              </p>
              <div className={styles.voorzetten}>
                {[
                  'Leg deze opgave eens op een andere manier uit.',
                  'Wat moet ik hier precies testen?',
                  'Ik weet niet hoe ik aan deze test begin.',
                  'Mijn test wordt rood en ik snap niet waarom.',
                ].map((tekst) => (
                  <button
                    key={tekst}
                    type="button"
                    className={styles.voorzet}
                    onClick={() => {
                      setVraag(tekst);
                      vraagVeld.current?.focus();
                    }}
                  >
                    {tekst}
                  </button>
                ))}
              </div>
            </>
          )}

          {berichten.length > 0 && (
            <div className={styles.gesprek} role="log" aria-live="polite">
              {berichten.map((bericht, i) => (
                <div
                  key={i}
                  className={bericht.rol === 'student' ? styles.vanStudent : styles.vanAssistent}
                >
                  <span className={styles.rolLabel}>
                    {bericht.rol === 'student' ? 'Jij' : 'Assistent'}
                  </span>
                  <Antwoord tekst={bericht.tekst} />
                </div>
              ))}
              {bezig && (
                <div className={styles.vanAssistent}>
                  <span className={styles.rolLabel}>Assistent</span>
                  <p className={styles.alinea}>Aan het nadenken…</p>
                </div>
              )}
              <div ref={gesprekEinde} />
            </div>
          )}

          {fout && (
            <div className={styles.fout} role="alert">
              {foutBoodschap(fout)}
              {(fout.soort === 'ongeldige-key' || fout.soort === 'geweigerde-key') && (
                <>
                  {' '}
                  <button type="button" className={styles.tekstKnop} onClick={verwijderKey}>
                    Andere key ingeven
                  </button>
                </>
              )}
            </div>
          )}

          {quotaOp && (
            <div className={styles.quotaOp} role="status">
              Je hebt je {config.vragenPerDag} vragen van vandaag opgebruikt. Morgen mag je weer.
              Probeer het intussen eens met de cursus erbij, of vraag het aan een medestudent.
            </div>
          )}

          <form onSubmit={verstuur} className={styles.vraagForm}>
            <textarea
              ref={vraagVeld}
              className={styles.vraagInvoer}
              value={vraag}
              onChange={(event) => setVraag(event.target.value)}
              onKeyDown={opToetsaanslag}
              placeholder="Waar loop je vast? Bijvoorbeeld: mijn mock geeft altijd null terug."
              rows={2}
              disabled={bezig || quotaOp}
              aria-label="Je vraag"
            />

            <button
              type="button"
              className={styles.tekstKnop}
              onClick={() => setToontCode((vorige) => !vorige)}
              aria-expanded={toontCode}
            >
              {toontCode ? 'Code verbergen' : 'Mijn code toevoegen (optioneel)'}
            </button>

            {toontCode && (
              <>
                <textarea
                  className={styles.codeInvoer}
                  value={code}
                  onChange={(event) => setCode(event.target.value)}
                  placeholder="Plak hier je C#-code of je test"
                  rows={8}
                  disabled={bezig || quotaOp}
                  aria-label="Je code"
                />
                <span className={codeTeLang ? styles.tellerFout : styles.teller}>
                  {code.length} / {config.maxCodeTekens} tekens
                  {codeTeLang && ' — dat is te veel. Plak enkel het stuk waar het misloopt.'}
                </span>
              </>
            )}

            {vraagTeLang && (
              <span className={styles.tellerFout}>
                Je vraag is te lang ({vraag.length} van {config.maxVraagTekens} tekens). Vat ze
                korter samen.
              </span>
            )}

            <div className={styles.knopRij}>
              <button type="submit" className={styles.knop} disabled={!kanVersturen}>
                {bezig ? 'Bezig…' : 'Vraag stellen'}
              </button>
              <span className={styles.hint}>Ctrl+Enter om te versturen</span>
              <button type="button" className={styles.tekstKnop} onClick={verwijderKey}>
                Key wissen
              </button>
            </div>
          </form>
        </>
      )}
    </details>
  );
}
