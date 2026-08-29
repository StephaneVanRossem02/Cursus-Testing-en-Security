import React from 'react';
import CodeBlock from '@theme/CodeBlock';
import styles from './styles.module.css';

/**
 * Lichte markdown-renderer voor de oplossingspaginas die de Worker teruggeeft.
 *
 * Bewust geen zware markdown-bibliotheek: de oplossingen gebruiken maar een beperkte
 * set (koppen, lijsten, een enkele tabel, links, inline code/vet en code-fences).
 * Codeblokken gaan door de echte Docusaurus-CodeBlock, zodat ze er net zo uitzien als
 * de rest van de cursus.
 */

const FENCE = /```([a-zA-Z#+]*)\n?([\s\S]*?)```/g;

/** Inline-opmaak: `code`, **vet** en [tekst](url). Codespans worden eerst afgeschermd. */
function inline(tekst, sleutel) {
  const stukken = tekst.split(/(`[^`]+`)/g);
  return (
    <React.Fragment key={sleutel}>
      {stukken.map((stuk, i) => {
        if (stuk.startsWith('`') && stuk.endsWith('`') && stuk.length > 1) {
          return <code key={i}>{stuk.slice(1, -1)}</code>;
        }
        return <React.Fragment key={i}>{vetEnLinks(stuk, i)}</React.Fragment>;
      })}
    </React.Fragment>
  );
}

/** Binnen gewone tekst: **vet** en [tekst](url). */
function vetEnLinks(tekst, sleutel) {
  const stukken = tekst.split(/(\*\*[^*]+\*\*|\[[^\]]+\]\([^)]+\))/g);
  return (
    <React.Fragment key={sleutel}>
      {stukken.map((stuk, i) => {
        if (stuk.startsWith('**') && stuk.endsWith('**') && stuk.length > 4) {
          return <strong key={i}>{stuk.slice(2, -2)}</strong>;
        }
        const link = stuk.match(/^\[([^\]]+)\]\(([^)]+)\)$/);
        if (link) {
          const extern = /^https?:\/\//.test(link[2]);
          return (
            <a
              key={i}
              href={link[2]}
              {...(extern ? { target: '_blank', rel: 'noreferrer noopener' } : {})}
            >
              {link[1]}
            </a>
          );
        }
        return <React.Fragment key={i}>{stuk}</React.Fragment>;
      })}
    </React.Fragment>
  );
}

/** Splitst een tekstsegment (zonder code-fences) in blokken. */
function blokkenUitTekst(tekst) {
  const regels = tekst.split('\n');
  const blokken = [];
  let i = 0;

  const isLijstItem = (r) => /^\s*([-*])\s+/.test(r) || /^\s*\d+\.\s+/.test(r);
  const isTabelRij = (r) => /^\s*\|.*\|\s*$/.test(r);

  while (i < regels.length) {
    const regel = regels[i];

    if (regel.trim() === '') {
      i += 1;
      continue;
    }

    const kop = regel.match(/^(#{1,6})\s+(.*)$/);
    if (kop) {
      blokken.push({ type: 'kop', niveau: kop[1].length, tekst: kop[2] });
      i += 1;
      continue;
    }

    if (/^\s*(---|\*\*\*|___)\s*$/.test(regel)) {
      blokken.push({ type: 'hr' });
      i += 1;
      continue;
    }

    if (isTabelRij(regel)) {
      const rijen = [];
      while (i < regels.length && isTabelRij(regels[i])) {
        rijen.push(regels[i]);
        i += 1;
      }
      blokken.push({ type: 'tabel', rijen });
      continue;
    }

    if (isLijstItem(regel)) {
      const items = [];
      const geordend = /^\s*\d+\.\s+/.test(regel);
      while (i < regels.length && isLijstItem(regels[i])) {
        items.push(regels[i].replace(/^\s*([-*]|\d+\.)\s+/, ''));
        i += 1;
      }
      blokken.push({ type: 'lijst', geordend, items });
      continue;
    }

    // Paragraaf: verzamel opeenvolgende gewone regels tot een lege regel of speciaal blok.
    const alinea = [];
    while (
      i < regels.length &&
      regels[i].trim() !== '' &&
      !/^(#{1,6})\s+/.test(regels[i]) &&
      !/^\s*(---|\*\*\*|___)\s*$/.test(regels[i]) &&
      !isLijstItem(regels[i]) &&
      !isTabelRij(regels[i])
    ) {
      alinea.push(regels[i]);
      i += 1;
    }
    blokken.push({ type: 'alinea', regels: alinea });
  }

  return blokken;
}

function Tabel({ rijen, sleutel }) {
  // Scheidingsrij (|---|---|) overslaan; eerste rij = koppen.
  const cellen = (r) =>
    r
      .trim()
      .replace(/^\|/, '')
      .replace(/\|$/, '')
      .split('|')
      .map((c) => c.trim());
  const dataRijen = rijen.filter((r) => !/^\s*\|?[\s:|-]+\|?\s*$/.test(r));
  if (dataRijen.length === 0) return null;
  const [kop, ...body] = dataRijen;
  return (
    <table key={sleutel} className={styles.tabel}>
      <thead>
        <tr>
          {cellen(kop).map((c, i) => (
            <th key={i}>{inline(c, i)}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {body.map((r, ri) => (
          <tr key={ri}>
            {cellen(r).map((c, ci) => (
              <td key={ci}>{inline(c, ci)}</td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}

export default function Inhoud({ markdown }) {
  // De frontmatter-titel van de pagina toont al een H1; een dubbele H1 uit de markdown
  // weglaten.
  const tekst = markdown.replace(/^\s*#\s+.*\n+/, '');

  const delen = [];
  let laatste = 0;
  let sleutel = 0;

  for (const match of tekst.matchAll(FENCE)) {
    if (match.index > laatste) {
      delen.push({ type: 'tekst', inhoud: tekst.slice(laatste, match.index) });
    }
    delen.push({ type: 'code', taal: match[1] || 'text', inhoud: match[2] });
    laatste = match.index + match[0].length;
  }
  if (laatste < tekst.length) {
    delen.push({ type: 'tekst', inhoud: tekst.slice(laatste) });
  }

  const knoppen = [];
  for (const deel of delen) {
    if (deel.type === 'code') {
      knoppen.push(
        <CodeBlock key={sleutel++} language={deel.taal}>
          {deel.inhoud.replace(/\n$/, '')}
        </CodeBlock>,
      );
      continue;
    }
    for (const blok of blokkenUitTekst(deel.inhoud)) {
      const k = sleutel++;
      if (blok.type === 'kop') {
        const Tag = `h${Math.min(blok.niveau, 6)}`;
        knoppen.push(<Tag key={k}>{inline(blok.tekst, k)}</Tag>);
      } else if (blok.type === 'hr') {
        knoppen.push(<hr key={k} />);
      } else if (blok.type === 'lijst') {
        const Tag = blok.geordend ? 'ol' : 'ul';
        knoppen.push(
          <Tag key={k}>
            {blok.items.map((it, i) => (
              <li key={i}>{inline(it, i)}</li>
            ))}
          </Tag>,
        );
      } else if (blok.type === 'tabel') {
        knoppen.push(<Tabel key={k} rijen={blok.rijen} sleutel={k} />);
      } else if (blok.type === 'alinea') {
        knoppen.push(
          <p key={k}>
            {blok.regels.map((r, ri) => (
              <React.Fragment key={ri}>
                {inline(r, ri)}
                {ri < blok.regels.length - 1 && <br />}
              </React.Fragment>
            ))}
          </p>,
        );
      }
    }
  }

  return <div className={styles.inhoud}>{knoppen}</div>;
}
