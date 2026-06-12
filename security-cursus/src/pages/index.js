import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Heading from '@theme/Heading';
import styles from './index.module.css';

const testing = [
  { num: 1,  title: 'Unit Testing & Mocking',       href: '/docs/testing/unit-testing-mocking',        tags: ['xUnit', 'Moq', 'ZOMBIES'] },
  { num: 3,  title: 'Test Driven Development',       href: '/docs/testing/tdd',                         tags: ['Red–Green–Refactor'] },
  { num: 5,  title: 'Integration Testing',           href: '/docs/testing/integration-testing',         tags: ['WebApplicationFactory'] },
  { num: 8,  title: 'Acceptatietesten (BDD)',        href: '/docs/testing/acceptatietesten',             tags: ['Gherkin', 'Reqnroll'] },
  { num: 10, title: 'Integration Testing (Mockoon)', href: '/docs/testing/integration-testing-mockoon', tags: ['HTTP-mocks', 'Mockoon'] },
];

const security = [
  { num: 2,  title: 'CIA, Hashing & Encryptie',    href: '/docs/security/cia-hashing-encryptie',   tags: ['BCrypt', 'AES', 'salt'] },
  { num: 4,  title: '2FA & Handtekeningen',         href: '/docs/security/handtekeningen-x509', tags: ['X.509', 'TOTP'] },
  { num: 6,  title: 'HTTPS & TLS',                  href: '/docs/security/https-tls',               tags: ['TLS-handshake', 'certificaten'] },
  { num: 7,  title: 'JWT & OAuth2',                 href: '/docs/security/jwt-oauth2',              tags: ['Bearer', 'claims'] },
  { num: 9,  title: 'Secure Coding (OWASP)',        href: '/docs/security/secure-coding-owasp',     tags: ['SQL injection', 'XSS'] },
  { num: 11, title: 'Ethisch Hacken',               href: '/docs/security/ethisch-hacken',          tags: ['pentest', 'OWASP ZAP'] },
  { num: 12, title: 'Herhaling & Globalisatie',     href: '/docs/security/herhaling-globalisatie',  tags: ['overzicht'] },
];

function LessonCard({ num, title, href, tags }) {
  return (
    <Link className={styles.lessonCard} to={href}>
      <span className={styles.lessonNum}>Les {num}</span>
      <span className={styles.lessonTitle}>{title}</span>
      <span className={styles.lessonTags}>
        {tags.map(t => <span key={t} className={styles.tag}>{t}</span>)}
      </span>
    </Link>
  );
}

function Section({ title, accent, lessons }) {
  return (
    <div className={styles.section} style={{ '--accent': accent }}>
      <h2 className={styles.sectionTitle}>{title}</h2>
      <div className={styles.lessonList}>
        {lessons.map(l => <LessonCard key={l.num} {...l} />)}
      </div>
    </div>
  );
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();
  return (
    <Layout title="Cursus" description="AP Hogeschool — Graduaat Programmeren — Testing & Security">
      <header className={styles.hero}>
        <div className="container">
          <p className={styles.heroBadge}>AP Hogeschool — Graduaat Programmeren</p>
          <Heading as="h1" className={styles.heroTitle}>{siteConfig.title}</Heading>
          <p className={styles.heroSub}>
            12 lessen · ShopWave als doorlopend thema · C# / ASP.NET Core
          </p>
          <div className={styles.heroButtons}>
            <Link className={clsx('button button--lg', styles.btnPrimary)} to="/docs/testing/unit-testing-mocking">
              Start met Les 1
            </Link>
            <Link className={clsx('button button--lg', styles.btnSecondary)} to="/docs/security/cia-hashing-encryptie">
              Ga naar Security
            </Link>
          </div>
        </div>
      </header>

      <main className={clsx('container', styles.main)}>
        <div className={styles.stats}>
          <div className={styles.stat}><strong>12</strong><span>lessen</span></div>
          <div className={styles.statDivider} />
          <div className={styles.stat}><strong>5</strong><span>testing</span></div>
          <div className={styles.statDivider} />
          <div className={styles.stat}><strong>7</strong><span>security</span></div>
          <div className={styles.statDivider} />
          <div className={styles.stat}><strong>1</strong><span>ShopWave</span></div>
        </div>

        <div className={styles.grid}>
          <Section title="Testing" accent="var(--sw-testing-color)" lessons={testing} />
          <Section title="Security" accent="var(--sw-security-color)" lessons={security} />
        </div>
      </main>
    </Layout>
  );
}
