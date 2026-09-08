// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import {themes as prismThemes} from 'prism-react-renderer';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'Testing & Security',
  tagline: 'AP Hogeschool — Graduaat Programmeren',
  favicon: 'img/favicon.svg',

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  // Set the production url of your site here
  url: 'https://testing-en-security.apload.be',
  baseUrl: '/',

  organizationName: 'StephaneVanRossem02',
  projectName: 'Cursus-Testing-en-Security',

  customFields: {
    // Overschrijft de standaardwaarden van de oefening-assistent (zie
    // src/components/OefeningAssistent/config.js).
    oefeningAssistent: {
      // Adres van de Cloudflare Worker die de referentie-oplossing aan de prompt
      // toevoegt. Zolang de Worker nog niet gedeployed is, blijft dit null en praat
      // de browser rechtstreeks met Google (zonder oplossing om mee te vergelijken).
      // Zet na deployen de URL hier, of geef ze mee via de env-var
      // OEFENING_ASSISTENT_WORKER_URL. Op localhost heeft de localStorage-override
      // 'oefening-assistent:worker-url' nog voorrang.
      workerUrl:
        process.env.OEFENING_ASSISTENT_WORKER_URL ||
        'https://oefening-assistent-testing.stephanevanrossem2.workers.dev',
    },
  },

  onBrokenLinks: 'throw',

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: './sidebars.js',
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      // Replace with your project's social card
      image: 'img/social-card.png',
      colorMode: {
        respectPrefersColorScheme: true,
      },
      docs: {
        sidebar: {
          hideable: true,
          autoCollapseCategories: true,
        },
      },
      navbar: {
        title: 'Testing & Security',
        logo: {
          alt: 'AP Hogeschool Logo',
          src: 'img/logo.svg',
        },
        items: [
          {
            type: 'docSidebar',
            sidebarId: 'cursusSidebar',
            position: 'left',
            label: 'Cursus',
          },
        ],
      },
      footer: {
        style: 'dark',
        copyright: `Copyright © ${new Date().getFullYear()} AP Hogeschool — Graduaat Programmeren. Built with Docusaurus.`,
      },
      prism: {
        theme: prismThemes.github,
        darkTheme: prismThemes.dracula,
        additionalLanguages: ['csharp'],
      },
    }),
};

export default config;
