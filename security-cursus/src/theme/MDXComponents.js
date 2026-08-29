import MDXComponents from '@theme-original/MDXComponents';
import OefeningAssistent from '@site/src/components/OefeningAssistent';
import OplossingSlot from '@site/src/components/OplossingSlot';
import DownloadSlot from '@site/src/components/DownloadSlot';

/**
 * Globaal beschikbaar maken in MDX, zodat een oefening- of oplossingpagina enkel
 *
 *   <OefeningAssistent oefening="H1-Rubbish" hoofdstuk="H1" />
 *   <OplossingSlot hoofdstuk="H1" />
 *
 * nodig heeft, zonder import bovenaan.
 */
export default {
  ...MDXComponents,
  OefeningAssistent,
  OplossingSlot,
  DownloadSlot,
};
