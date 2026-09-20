# Banana Humper — Hinweise für Claude

Unity-6-Projekt (C#, Built-in Render Pipeline), Umsetzung von `docs/GDD.md`.
Vor jeder größeren Änderung: relevantes GDD-Kapitel lesen, nicht raten.

## Scope-Disziplin (wichtigste Regel)

Das GDD markiert jedes Feature mit einer Versions-Stufe:
- **[Demo]** / **[V1]** — vor Release umzusetzen
- **[V2]** / **[V3+]** — spätere Ideen, keine Zusage
- **[E]** — Entscheidung von Olli (fix), **[A]** — Annahme/Vorschlag (noch zu bestätigen)

Regel aus dem GDD: **vor Release wird nur gebaut, was [Demo] oder [V1] ist.**
Zusätzlich befindet sich das Projekt aktuell in Roadmap-**Stufe A** (siehe
README, Abschnitt „Nächste Schritte laut Roadmap"): nur der Kern-Loop
(Auflegen/Balancieren/Belastung, GDD Kap. 3, plus Minimal-Ressourcen aus
4.1–4.3). Bewusst nicht enthalten: Tagesquote, Verwarnungen, Körper/Shop,
Prestige, Zufallsereignisse, Cutter-Sprüche, Sound.

**Wenn eine Anfrage ein Feature aus einer späteren Stufe oder aus [V2]/[V3+]
betrifft: das ansprechen statt es einfach zu bauen.** Scope-Creep zu
verhindern ist der ganze Zweck des GDD (siehe GDD-Kopf).

## Architektur-Konventionen

- **Keine handgeschriebene `.unity`-Szenendatei.** Alle Objekte (Kamera,
  Boden, Kulisse, Cutter, Trailer, Spieler, HUD) werden von
  `Assets/Scripts/Bootstrap/GameBootstrap.cs` zur Laufzeit erzeugt. Grund:
  YAML-Szenendateien lassen sich ohne laufenden Editor nicht zuverlässig
  verifizieren — das hält das Risiko einer kaputten Szene bei null. Neue
  Szenenobjekte gehören in `GameBootstrap`, nicht in eine `.unity`-Datei.
- **Tuning-Werte gehören in `BalanceConfig` (ScriptableObject)**, nicht als
  Konstanten in Gameplay-Scripts. So kann im Inspector getunt werden, ohne
  Code anzufassen und ohne Recompile.
- **Namespaces:** `BananaHumper.<Bereich>` (`Config`, `Gameplay`, `UI`,
  `Bootstrap`, `Util`). Neue Scripts entsprechend einordnen.
- **Kommentare referenzieren GDD-Kapitel** (z. B. `// GDD 3.5`) und erklären
  *warum* ein Wert/Ansatz so gewählt wurde, wenn es nicht offensichtlich ist
  (siehe `BalanceConfig.controlStrength` oder der `stepPhase`-Kommentar in
  `BalanceController.BeginTrip` als Beispiele) — nicht *was* der Code tut.
- **Art:** zuerst prüfen, ob ein passendes CC0-Asset unter
  `Assets/Resources/Art/` existiert bzw. sich in `docs/THIRD_PARTY_ASSETS.md`
  ergänzen lässt (Kenney.nl). Erst wenn kein passendes freies Asset
  existiert, prozedural über `SpriteFactory` (kantengeglättete Vektorformen)
  bauen — nicht harte Rechtecke.

## Testen — was ich kann und was nicht

Ich kann Code headless kompilieren (`Unity.exe -batchmode -nographics -quit`),
das prüft aber nur, ob es kompiliert — **kein Play-Mode, kein Gefühl für
Balance/Timing/Spielspaß**. Nach jeder Gameplay-Änderung explizit sagen: „das
kompiliert, bitte im Editor testen" statt zu behaupten, es funktioniere.
Playtesting-Feedback (Zahlen, Beobachtungen, keine Ursachen-Vermutungen)
kommt vom Nutzer und wird in `docs/PLAYTEST_LOG.md` festgehalten.

## Dokumentation aktuell halten

- Neue Gameplay-Datei → Zeile in der Code-Struktur-Tabelle in `README.md`
  ergänzen (Datei, GDD-Kapitel, Zweck).
- Bewusste Abweichung vom GDD oder nicht-triviale Architektur-Entscheidung →
  Eintrag in `docs/DECISIONS.md`, nicht nur ein Code-Kommentar.
- Neue importierte Assets → `docs/THIRD_PARTY_ASSETS.md` ergänzen (Quelle,
  Lizenz, Verwendung).
- „Nächste Schritte"-Abschnitt im README ist die Priorität zwischen
  Sessions — vor Beginn einer Aufgabe kurz gegenlesen.

## Sprache

Kommunikation mit Olli auf Deutsch. Code-Kommentare und XML-Doc-Summaries im
Projekt sind aktuell gemischt Deutsch/Englisch (Deutsch für Balance-/GDD-
Begründungen, Englisch für kurze XML-Doc-Summaries) — beim bestehenden Stil
einer Datei bleiben, nicht projektweit vereinheitlichen ohne Rücksprache.
