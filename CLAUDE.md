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

- **Hybrid-Aufbau der Szene, und `.unity` nie von Hand schreiben.**
  `Main.unity` enthält Kamera, Kulisse, Spielfigur und die Layout-Anker
  (Cutter, Trailer, Zielmarkierung, Staude) als echte Objekte — dort darf
  und soll Olli visuell arbeiten. Erzeugt wird die Szene ausschließlich von
  `Assets/Scripts/Editor/SceneSetupTool.cs`, nie durch handgeschriebenes
  YAML (das lässt sich ohne laufenden Editor nicht verifizieren).
  `GameBootstrap` erzeugt zur Laufzeit nur noch die Gameplay-Systeme, das
  HUD und die prozeduralen Formen.
- **Wo gehört ein neues Objekt hin?** Nutzt es ein importiertes Sprite-Asset
  und hat eine feste Position → ins Editor-Tool (landet in der Szene, ist
  verschiebbar). Entsteht seine Textur prozedural über
  `SpriteFactory.CreateRoundedQuad`/`CreateEllipse` → muss zur Laufzeit in
  `GameBootstrap` gebaut werden, denn solche Sprites sind keine Assets und
  können nicht in der Szene gespeichert werden; dann gehört ein Anker-Objekt
  in die Szene, damit die Position trotzdem verschiebbar bleibt.
- **Positionen gehören in die Szene, nicht in `BalanceConfig`.** Doppelte
  Wahrheit vermeiden (deshalb wurde z. B. `distanceToTrailer` entfernt: die
  Trip-Länge ist der Abstand der Anker).
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
