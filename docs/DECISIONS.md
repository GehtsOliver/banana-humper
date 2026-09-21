# Decisions Log

Bewusste technische/design-nahe Entscheidungen, die vom GDD abweichen oder
nicht offensichtlich aus dem Code hervorgehen, mit Datum und Begründung.
Kleinteilige Code-Rationale bleibt als Kommentar im Code — hier stehen nur
Entscheidungen, die jemand (auch zukünftiges Claude) sonst falsch vermuten
oder versehentlich rückgängig machen würde.

Format pro Eintrag: Datum, Entscheidung, Begründung, ggf. Alternative die
verworfen wurde.

---

## 2026-09-21 — Hybrid: Layout in der Szene, Systeme weiter im Code

Ersetzt den vorherigen "alles zur Laufzeit"-Ansatz (siehe Eintrag unten).
`Main.unity` enthält jetzt Kamera, Kulisse, Spielfigur (inkl.
`PlayerAnimator` mit zugewiesenen Sprites) und Layout-Anker für Cutter,
Trailer, Zielmarkierung und Staude als echte, im Editor verschiebbare
Objekte. `GameBootstrap` erzeugt nur noch die Gameplay-Systeme, das HUD und
die prozeduralen Formen und liest die Positionen aus der Szene.

**Begründung:** [E] von Olli. Der ursprüngliche Grund für die
Volltgenerierung war, dass Claude ohne laufenden Editor keine Szenendatei
verifizieren kann — das galt aber nur fürs *Schreiben von YAML*. Die Szene
wird weiterhin nicht von Hand geschrieben, sondern von
`Assets/Scripts/Editor/SceneSetupTool.cs` erzeugt, also aus reviewbarem
Code. Damit bleibt die Reproduzierbarkeit erhalten, und Olli kann trotzdem
visuell arbeiten statt Zahlen in Inspector-Felder zu tippen.

**Grenze des Umbaus:** Die prozeduralen Formen (Bananenstaude, Cutter,
Trailer, Boden, Zielmarkierung) erzeugen ihre Textur zur Laufzeit über
`SpriteFactory`. Solche Sprites sind keine Assets und lassen sich nicht in
einer Szenendatei speichern — deshalb liegen dafür nur leere Anker in der
Szene (verschiebbar, aber erst im Play-Modus sichtbar). Nächster möglicher
Schritt, falls das stört: die Shape-Texturen einmalig als echte
PNG/Sprite-Assets backen, dann wären auch diese Objekte im Edit-Modus
sichtbar.

**Folgeänderung:** `BalanceConfig.distanceToTrailer` wurde entfernt — die
Trip-Länge ergibt sich jetzt aus dem Abstand der Cutter-/Trailer-Anker.
Beides parallel zu pflegen wäre eine Falle gewesen (Wert ändern, Trailer
bleibt trotzdem stehen).

## 2026-09 — Keine handgeschriebene `.unity`-Szenendatei (überholt, siehe Eintrag oben)

Alle Szenenobjekte werden von `GameBootstrap.cs` zur Laufzeit erzeugt statt
in einer `.unity`-YAML-Datei zu liegen.

**Begründung:** `.unity`-Dateien lassen sich ohne laufenden Unity-Editor
nicht zuverlässig verifizieren (kaputtes YAML fällt erst beim Öffnen auf).
Runtime-Erzeugung ist vollständig code-review- und testbar.

**Alternative verworfen:** Szene von Hand in Unity bauen und einchecken —
üblicher für größere Projekte, aber hier bewusst vermieden, solange Claude
ohne Editor-Zugriff Code beisteuert.

## 2026-09 — Built-in Render Pipeline statt URP

GDD nennt URP als Zielwert, Graybox nutzt Built-in.

**Begründung:** Built-in läuft ohne Editor-Konfiguration/Package-Installation.
Umstieg auf URP ist später ein einzelner Package-Manager-Schritt (Package
Manager → URP installieren → Render Pipeline Converter) und betrifft keinen
Gameplay-Code.

## 2026-09 — Manuelles Laufen (A/D) statt Auto-Movement

GDD 3.1 [A] sah automatische Bewegung zum Trailer vor. Umgesetzt wurde
stattdessen manuelles Laufen mit `A`/`D`, gleichzeitig zum Balancieren.

**Begründung:** [E] von Olli — macht das Tragen aktiver statt einer reinen
Balance-Warteschleife.

## 2026-09-20 — Balancieren per W/S statt Maus, Maus nur noch fürs Rennen

GDD 3.3 sah Maus-Bewegung fürs Balancieren vor ("wie ein Besen, den man auf
der Hand balanciert"). Umgesetzt wurde stattdessen `W`/`S` (Unity-
"Vertical"-Achse). Als Folge wanderte auch "Umsetzen" von der rechten
Maustaste auf `E`, damit die Maus ausschließlich fürs Rennen (linke
Maustaste halten) zuständig bleibt.

**Begründung:** [E] von Olli. `BalanceConfig.controlStrength` wurde dabei
von 0.15 (getunt für `Input.GetAxis("Mouse X")`-Werte im Bereich ~1–15) auf
3.5 angepasst, weil die Tastatur-Achse nur bis ±1 ausschlägt, dafür aber
bei gehaltener Taste dauerhaft auf 1 bleibt statt wie ein Mausschlag sofort
wieder abzuklingen — ungetesteter erster Schätzwert, siehe
`docs/PLAYTEST_LOG.md`.

**Zurückgerollt** (siehe nächster Eintrag) — nach Test war die Neigung mit
gehaltener Taste zu stark, `controlStrength` wurde erst auf 1.2 gesenkt,
dann bat Olli direkt wieder um Maus-Steuerung.

## 2026-09-20 — Balancieren zurück auf Maus, jetzt linke/rechte Maustaste statt Bewegung

Nach dem W/S-Zwischenschritt (siehe oben) wollte Olli das Balancieren doch
wieder auf der Maus haben, diesmal aber als Tastendruck (linke/rechte
Maustaste halten) statt als Bewegungs-Delta wie ursprünglich in GDD 3.3.
Rennen wanderte dafür von "linke Maustaste halten" auf `Shift` (die Maus
ist ab der Carrying-Phase vollständig fürs Balancieren reserviert),
"Umsetzen" bleibt auf `E`.

**Begründung:** [E] von Olli. `Input.GetMouseButton` liefert sofort volles
±1 ohne Anlaufzeit (anders als Unity's Achsen-Smoothing beim W/S-Versuch) -
um das gerade erst behobene Überschießen nicht mit dem Tastenwechsel wieder
einzuführen, glättet `BalanceController.Tick` den Tastendruck jetzt selbst
über `steerSmoothed` (~0.3s Anlaufzeit, dieselbe Größenordnung wie vorher
die Vertical-Achse). `controlStrength` bleibt vorerst bei 1.2, mit dieser
Steuerung aber noch ungetestet.

## 2026-09 — Cutter-Figur ohne konkrete Hautfarbe/Gesichtszüge

Bewusst abstrakt gehalten (prozedurale Form statt Figuren-Asset mit
Gesicht).

**Begründung:** GDD 1.7 — „Cutter als Bogans, nie ethnisch markiert".
