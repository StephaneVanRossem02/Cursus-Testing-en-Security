---
title: "Les 3: Test Driven Development — Theorie"
sidebar_label: "Theorie"
---

# Les 3: Test Driven Development (TDD) — Theorie

## Wat is TDD?

**Test Driven Development** is een ontwikkelmethodiek waarbij je de test schrijft **vóór** de productiecode. TDD gaat niet over testen — het gaat over **ontwerp**. Door eerst de test te schrijven, denk je na over de interface van je klasse voordat je ook maar één lijn implementatie schrijft.

---

## De drie wetten van Uncle Bob

1. Je mag geen productiecode schrijven behalve om een falende test te doen slagen.
2. Je mag niet meer tests schrijven dan nodig om te falen — ook compilatiefouten tellen.
3. Je mag niet meer productiecode schrijven dan nodig om de falende test te doen slagen.

---

## Red-Green-Refactor

```
Red      → Schrijf een test die faalt (code bestaat nog niet)
Green    → Schrijf net genoeg code om de test te doen slagen
Refactor → Verbeter de code zonder het gedrag te wijzigen
```

Regels:
- Alle tests moeten na refactoring nog steeds slagen.
- Je mag pas een nieuwe test schrijven als alle bestaande tests groen zijn.
- In de groene fase schrijf je de **minimale** implementatie — zelfs `return true` als dat de test doet slagen. De refactorfase verbetert daarna.

---

## De testlijst

Noteer **vooraf** de testgevallen die je verwacht nodig te hebben. Dit is geen contract — het is een startpunt dat helpt nadenken over de vereisten. Voeg gevallen toe naarmate je implementeert.

Voorbeeld voor `CartService`:
```
[] Nieuw mandje heeft totaal 0
[] Eén artikel toevoegen verhoogt het totaal
[] Meerdere artikelen tellen correct op
[] Artikel verwijderen verlaagt het totaal
[] Negatief aantal → ArgumentException
[] Mandje leegmaken reset totaal naar 0
```

---

## Voordelen van TDD

- **Minder bugs** — je schrijft alleen code voor gespecificeerde vereisten
- **Betere architectuur** — testbare code is automatisch beter gestructureerd (losse koppeling, duidelijke interfaces)
- **Veilig refactoren** — de testbank vangt fouten op
- **Levende documentatie** — tests beschrijven wat de code doet

---

## Wanneer doe je wat?

| Fase     | Doel                                        | Magje...                              |
|----------|---------------------------------------------|---------------------------------------|
| Red      | Test schrijven die faalt                    | Enkel de minimale testcode schrijven  |
| Green    | Test doen slagen                            | De snelste oplossing kiezen           |
| Refactor | Code verbeteren                             | Alles verbeteren — tests moeten groen blijven |

---

## TDD vs. tests-achteraf schrijven

| Aspect             | TDD                              | Tests achteraf                     |
|--------------------|----------------------------------|------------------------------------|
| Wanneer testen?    | Vóór implementatie               | Na implementatie                   |
| Ontwerp            | Tests sturen het ontwerp         | Code stuurt de tests               |
| Dekking            | Hoog — je schrijft alleen wat getest is | Lager — makkelijk iets vergeten |
| Refactoring        | Veilig door testbank             | Risicovol zonder testdekking       |