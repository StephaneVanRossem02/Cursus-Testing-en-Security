import React from 'react';
import styles from './styles.module.css';

/**
 * Minimale opmaak voor het antwoord van het model.
 *
 * Bewust geen markdown-bibliotheek: de assistent geeft korte hints met hoogstens
 * een klein codefragment. Codeblokken en vetgedrukte tekst volstaan.
 */

const CODE_BLOK = /```[a-zA-Z#+]*\n?([\s\S]*?)```/g;

/** Zet **vet** om naar <strong>. De rest blijft gewone tekst. */
function metVet(tekst, sleutel) {
  const stukken = tekst.split(/(\*\*[^*]+\*\*)/g);
  return (
    <React.Fragment key={sleutel}>
      {stukken.map((stuk, i) =>
        stuk.startsWith('**') && stuk.endsWith('**') && stuk.length > 4 ? (
          <strong key={i}>{stuk.slice(2, -2)}</strong>
        ) : (
          <React.Fragment key={i}>{stuk}</React.Fragment>
        ),
      )}
    </React.Fragment>
  );
}

export default function Antwoord({ tekst }) {
  const delen = [];
  let laatsteIndex = 0;

  for (const match of tekst.matchAll(CODE_BLOK)) {
    if (match.index > laatsteIndex) {
      delen.push({ type: 'tekst', inhoud: tekst.slice(laatsteIndex, match.index) });
    }
    delen.push({ type: 'code', inhoud: match[1] });
    laatsteIndex = match.index + match[0].length;
  }
  if (laatsteIndex < tekst.length) {
    delen.push({ type: 'tekst', inhoud: tekst.slice(laatsteIndex) });
  }

  return (
    <>
      {delen.map((deel, i) =>
        deel.type === 'code' ? (
          <pre key={i} className={styles.codeblok}>
            <code>{deel.inhoud.replace(/\n$/, '')}</code>
          </pre>
        ) : (
          <p key={i} className={styles.alinea}>
            {deel.inhoud
              .trim()
              .split('\n')
              .map((regel, r, alle) => (
                <React.Fragment key={r}>
                  {metVet(regel, r)}
                  {r < alle.length - 1 && <br />}
                </React.Fragment>
              ))}
          </p>
        ),
      )}
    </>
  );
}
