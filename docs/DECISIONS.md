# Decisions Log

Bewusste technische/design-nahe Entscheidungen, die vom GDD abweichen oder
nicht offensichtlich aus dem Code hervorgehen, mit Datum und Begründung.
Kleinteilige Code-Rationale bleibt als Kommentar im Code — hier stehen nur
Entscheidungen, die jemand (auch zukünftiges Claude) sonst falsch vermuten
oder versehentlich rückgängig machen würde.

Format pro Eintrag: Datum, Entscheidung, Begründung, ggf. Alternative die
verworfen wurde.

---

## 2026-09-21 — Zwei Feld-Bugs: wucherndes Paddock, nie ein Stein

Beide aus dem Umbau auf `PaddockField` und beide mit derselben Ursache-Art —
eine Annahme, die beim Schreiben plausibel wirkte und in der Praxis nie
zutraf.

**Pflanzen häuften sich Schicht um Schicht.** `Initialize` legt bei jedem
Schichtstart neue Wurzelobjekte an, räumte aber die Pflanzen der Vorschicht
nicht ab — die blieben in der Liste und in der Szene stehen. Jetzt räumt
`ClearField()` zuerst auf.

**Es konnte nie ein Stein entstehen.** `rockMinDistance` verlangte 2,2
Abstand zu *jeder* Pflanze, während Pflanzen nur 2,2–4,0 auseinanderstanden.
Die Mitte einer Lücke liegt damit höchstens 2,0 von der nächsten Pflanze
weg — die Bedingung war mathematisch unerfüllbar. Konsequenz: Der Abstand
muss kleiner sein als der halbe kleinste Pflanzenabstand. Jetzt 1,6 bei
Pflanzenabstand 3,5–6,0, und der Stein-Spawn sucht zusätzlich in der
Umgebung nach einer freien Lücke, statt bei der ersten Kollision aufzugeben.

**Daraus als Regel:** Wenn zwei Streuparameter gegeneinander arbeiten
(Dichte des einen begrenzt den Platz des anderen), gehört die Beziehung als
Kommentar an beide Werte — sonst kippt eine Änderung an einem still das
andere Feature.

Gegengerechnet nach dem Fix: 22-m-Fenster mit 2 Cuttern enthält 4–5 Pflanzen
und 2–3 Steine, 42-m-Fenster mit 6 Cuttern 8–10 Pflanzen. Steine werden jetzt
in 100 % der Versuche platziert statt in 0 %.

Nebenbei behoben: Bei einer Schicht nach links begann die Aussaat auf der
falschen Seite des Trailers (`startX - behindDistance` statt
richtungsabhängig).

## 2026-09-21 — Zwei Währungen, neues Energiemodell

**Erfahrung kauft Attribute, Geld kauft Ausrüstung** ([E] von Olli, entspricht
der Trennung aus GDD 5). Attribute: Energie, Stärke, Ausdauer,
Laufgeschwindigkeit. Ausrüstung: Cutter anheuern, Schulterpad, Gummistiefel,
Instant-Kaffee.

**Die beiden Listen überschneiden sich bewusst nicht.** Tempo und
Energieeffizienz kommen ausschließlich aus dem Körper; Ausrüstung kauft
Dinge, die der Körper nicht kann (mehr Kollegen, Fangradius, Schutz vor
Stolperern, Startpolster). Wären beide Währungen für dieselben Effekte gut,
wäre die Trennung nur Buchhaltung. Deshalb sind die früheren Artikel „Gute
Stiefel" (Tempo) und „Tragegurt" (Schlepp-Energie) entfallen — die decken
jetzt Laufgeschwindigkeit und Stärke ab.

**Energiemodell umgebaut [E]:** Vorher kostete nur das Schleppen Energie
(begründet als „Budget aus Kilogramm mal Weg"). Jetzt kostet alles,
gestaffelt: Dasein 0,15/s, Laufen +0,6/s, Rennen ×1,8 darauf, Schleppen
+`1,4 + kg/50`. Dadurch ist die Schicht in Echtzeit begrenzt und Umwege
kosten auch ohne Staude etwas.

**Stärke und Ausdauer greifen an genau diesen beiden Hälften**, nicht an
Config-Werten: `EnergySystem` hat dafür `StrengthFactor` und
`StaminaFactor`. Das macht die Wahl zwischen ihnen zu einer Aussage über den
Spielstil — schwere Stauden wollen Stärke, viel Strecke will Ausdauer.

Gerechnete Schichtlänge (35 % schleppen, 40 % laufen, 10 % rennen, 15 %
stehen, 43-kg-Staude): 73 s ohne Attribute, 106 s bei Stufe 5, 149 s bei
Stufe 10; eine 90-kg-Staude drückt auf 60 s. GDD Kapitel 2 nennt 45–90 s pro
Schicht, das passt, und der Ausbau ist spürbar.

**Gummistiefel = Schutz vor Verletzungen**, umgesetzt als gedämpfter
Stolperer (Energie und Bremszeit). Ein echtes Verletzungssystem gibt es
nicht, und ich wollte keines unaufgefordert erfinden — das Stolpern ist die
nächstliegende vorhandene Entsprechung.

## 2026-09-21 — Shop gebaut, Balancieren abgeschaltet

**Shop (GDD 5.2)** am Schichtende, bezahlt mit Geld, flache Liste ohne
Abhängigkeiten (kein Skill Tree, so steht es im GDD). Fünf Einträge: Cutter
anheuern, Schulterpad (Fangradius), Gute Stiefel (Tempo), Tragegurt
(Energie beim Schleppen), Instant-Kaffee (Startenergie). Die GDD-Artikel aus
v0.8 zielten auf `maxAngle`, `offsetTorque` und Ereignisse, die es nicht
(mehr) gibt — Kapitel 5.2 ist entsprechend umgeschrieben, 5.1 (Körper) ist
als noch-nicht-gebaut markiert.

**Zwischen den Schichten, nicht mittendrin:** Der Loop soll nicht für Menüs
unterbrochen werden, und Anheuern ändert die Größe des Paddocks — das lässt
sich nur beim Schichtstart sauber neu aufbauen.

**Upgrades ändern das BalanceConfig-Asset nicht.** `UpgradeSystem` hält eine
Laufzeitkopie, die bei jedem Kauf frisch aus den Basiswerten abgeleitet und
dann mit allen Stufen überschrieben wird (`JsonUtility.FromJsonOverwrite`).
Ohne das hätte ein Kauf im Editor die Asset-Datei dauerhaft verändert, und
die Effekte hätten sich bei jedem Kauf erneut aufaddiert. Entspricht dem
"Stats-Prinzip" aus GDD 10.2.

**Balancieren abgeschaltet [E] von Olli.** `balancingEnabled` steht auf
false: Die Staude sitzt fest, kein Wackeln, kein Fallenlassen, keine
Maustasten. Der Kern ist Fangen und Route geworden; das Balancieren war
daneben nur noch Beiwerk, das vom Blick auf die Geduldsbalken ablenkte. Der
Code bleibt vollständig erhalten und lässt sich über den Schalter
zurückholen — deshalb ein Schalter statt Löschen.

## 2026-09-21 — Pflanzen und wandernde Cutter, Trailer zieht abschnittsweise weiter

Zwei Änderungen, die die Struktur des Paddocks umdrehen ([E] von Olli):

**Cutter sind keine Stationen mehr.** Vorher *war* ein Cutter ein fester
Platz mit hängender Staude. Jetzt stehen **Pflanzen** willkürlich verteilt
im Feld, und die Cutter **wandern zur nächstgelegenen freien Pflanze**,
sobald sie abgeschlagen haben. Eine abgeerntete Pflanze treibt erst nach
`regrowSeconds` neu aus, was die Cutter weiter ins Feld schiebt statt sie an
einer Pflanze kleben zu lassen. Eine Pflanze wird von höchstens einem Cutter
beansprucht (`ClaimedBy`), sonst würden zwei dieselbe ansteuern.

Nebeneffekt für den Loop: Während ein Cutter läuft, ist bei ihm nichts zu
holen. Dadurch entstehen Lücken und ein Rhythmus, ohne dass dafür etwas
eigens eingebaut werden musste.

**Der Trailer fährt nicht mehr Pendel.** Er steht, solange im Umkreis genug
reife Stauden hängen, und zieht erst weiter, wenn der Abschnitt leergeerntet
ist. Die Richtung wird pro Schicht ausgewürfelt. Das Feld ist dadurch
faktisch endlos: `PaddockField` hält ein Fenster um den Trailer, sät vorne
nach und räumt hinten auf — inklusive der Steine. Spieler-, Kamera- und
Hindernisgrenzen ziehen jeden Frame mit.

**Warum ein Fenster und keine feste Reihe:** Eine echte 88-Tage-Reihe wären
hunderte Objekte, von denen fast alle außerhalb des Bildes stehen. Das
Fenster liefert dasselbe Erlebnis („wir arbeiten uns durch das Feld") mit
konstant wenigen Objekten.

## 2026-09-21 — Geduld, anheuerbare Cutter, mitfahrende Kamera, Juice

Vier zusammenhängende Änderungen nach der ersten Spielrunde („Core Loop
macht Spaß"):

**Der Balken ist die Geduld des Cutters**, kein Timer mehr. Läuft sie ab,
schlägt er ab, egal wo man steht; steht man mit freien Händen still
darunter, schlägt er sofort ab. Aus einer Uhr, der man hinterherläuft, wird
damit eine Verhandlung: Man kann Stauden aktiv abrufen und die Reihenfolge
selbst bestimmen, zahlt aber mit Wartezeit. Bewusst an „steht still"
geknüpft, nicht an bloße Nähe — sonst löst jedes Vorbeilaufen Stauden aus.
Abrufen garantiert einen perfekten Catch ([E]): sicher-aber-langsam gegen
riskant-aber-schnell ist die klarere Rollenverteilung.

**Start mit nur 2 Cuttern** ([E]), weitere über den Shop anheuerbar (GDD
5.2). Das Paddock endet am letzten angeheuerten Cutter und wächst mit der
Mannschaft — dadurch fühlt sich die Farm nie leer an, und das Wachstum ist
spürbar statt nur eine Zahl. Die Stationsplätze liegen schon in der Szene,
`isHired` schaltet sie frei.

**Mitfahrende Kamera** statt „ganze Reihe im Bild", weil das Paddock sonst
nicht wachsen könnte. Damit die Priorisierung nicht am Bildrand endet, zeigt
das HUD für Stationen außerhalb des Bildes eine Randanzeige mit Richtung und
Geduld. Solange wenige Cutter angeheuert sind, zentriert die Kamera von
selbst — man sieht dann ohnehin alles.

**Steine werden pro Schicht zufällig gesetzt** (2–3 statt einer zwischen
jedem Cutterpaar), mit Mindestabstand zu den Stationen. Das ersetzt die
vorherige Festlegung „Steine liegen fest in der Szene, die Strecke soll
lernbar sein" ([E] von Olli).

**HUD-Bug:** Der Energiebalken war unsichtbar, weil das `Image` auf
`type = Filled` stand, aber kein Sprite hatte — ein gefülltes Image ohne
Sprite zeichnet nichts. Gilt für jedes UI-Image in diesem Projekt.

## 2026-09-21 — Umsetzung des neuen Kern-Loops im Code

GDD v0.9 ist jetzt implementiert. Neue Bausteine: `CutterStation`,
`ProgressBarVisual`, `FallingBunch`, `PlayerController`,
`TrailerController`. `ShiftController` ist keine Trip-Statemachine mehr,
sondern taktet Stationen, Trailer, Spieler und Pendel und wertet die
Catch-Qualität aus. `PlacementController` wurde gelöscht, Belastung und
Snap sind aus `BalanceController` entfernt.

**Zwei Entscheidungen, die im GDD offen waren:**
- **Bewegung gehört nicht mehr in den ShiftController.** Bis v0.8 steckte
  das Laufen in der Trip-Coroutine. Da der Spieler jetzt durchgehend frei
  unterwegs ist, liegt es in `PlayerController` mit eigenem Tick.
- **Wer schon trägt, fängt nicht.** `ShiftController.EvaluateCatch` gibt für
  einen tragenden Spieler immer „verpasst“ zurück. Das ist die zentrale
  Spannung des Loops und bewusst eine harte Regel statt eines Malus.

**Energie:** Laufen ohne Staude kostet nichts (GDD 4.2). Damit ist Energie
faktisch ein Budget aus getragenen Kilogramm mal Weg — genau der Regler, der
schwere Stauden zur Abwägung macht statt zur automatisch besseren Wahl.

**Noch nicht drin:** Verwarnungen/Rauswurf (GDD 4.7) und Tagesquote. Der
Loop muss erst für sich tragen (Stufe A).

## 2026-09-21 — Kern-Loop-Wechsel: Fangen statt Balancieren (GDD v0.9)

Der Kern ist nicht mehr das Balancieren einer Staude, sondern: mehrere
Cutter-Stationen mit Schnitt-Balken, rechtzeitig drunterstehen und fangen,
zum mitfahrenden Trailer schleppen. Energie begrenzt die Schicht und
skaliert mit dem Gewicht. Balancieren bleibt abgeschwächt beim Schleppen,
Belastung und Snap entfallen ersatzlos.

**Begründung:** [E] von Olli. Die zentrale Entscheidung ist jetzt *welche
Staude hole ich mir* statt *halte ich diese eine gerade* — wer schleppt,
kann nicht fangen, und schwere Stauden werden dadurch zur Wette statt zur
automatisch besseren Wahl. Nebeneffekt: näher am echten Job aus GDD 1.4
(mehrere Cutter, Traktor zieht den Trailer mit).

**Entschieden in Rücksprache:**
- Balancieren bleibt abgespeckt beim Schleppen (nicht komplett raus)
- Verpasste Staude: Geld weg **und** Verwarnung — dafür wird das bereits
  spezifizierte System aus GDD 4.7 genutzt statt einer zweiten
  Reputationsleiste; im HUD heißt es „Ruf bei den Cuttern“
- Immer nur eine Staude gleichzeitig tragen
- Zusatzfeatures: Catch-Qualität (perfekt/normal/Streifer) und mitfahrender
  Trailer. Bewusst *nicht* gewählt: unterschiedliche Balken-Tempi als
  eigenes Feature, Tagesquote (bleibt als [Demo] im GDD, aber kein Fokus)
- Belastung/Snap: raus, weil sie langes Schieflaufen bestraften — eine
  Herausforderung, die es nicht mehr gibt
- GDD zuerst umschreiben, dann coden

**Was das kostet:** Die über mehrere Runden getunte Balance-Steuerung
(Maustasten, `controlStrength`, Snap-Kurven) verliert ihre zentrale Rolle.
Bewusst in Kauf genommen.

**Noch offen (in GDD 15 eingetragen):** Kapitel 5 (Körper/Shop) verbessert
Balance-Werte wie `comfortAngle` und `maxAngle`, die kaum noch zählen -
sinnvoller wären Lauftempo, Energieeffizienz und Fangradius. Kapitel 7
(Zufallsereignisse) zielte ebenfalls aufs Balancieren.

## 2026-09-21 — Stufenlose Stauden statt drei Längenstufen, Länge wirkt aufs Pendel

Die Staudenlänge war ein Enum mit drei Stufen (`kurz/mittel/lang`). Jetzt
sind Gewicht und Länge stufenlos zufällig (GDD 4.3), und die Dicke ergibt
sich aus Gewicht pro Länge statt separat gewürfelt zu werden — sonst gäbe
es 100-kg-Zwerge. Gewicht ist teilweise mit der Länge korreliert, weil GDD
4.3 „schwere und lange Stauden = mehr Geld" verlangt; die Korrelation ist
bewusst unvollständig, damit es weiterhin lang-und-dünn und kurz-und-dick
gibt.

**Physik:** GDD 3.4 schreibt `α = (gravity / length) * sin(θ) * weightFactor`,
aber die Länge ging nie ein — `gravityOverLength` war eine Konstante. Jetzt
teilt die Länge diesen Term tatsächlich. Ergebnis sind zwei unterschiedliche
Risikoprofile statt nur „groß = schwer":

| Staude | Kippen | Snap bei Dauerbelastung |
|---|---|---|
| klein & leicht (30 kg, kurz) | 2,1 | nach 18,5 s |
| lang & dünn (35 kg) | 1,1 (träge) | nach 2,6 s |
| kurz & dick (60 kg) | 4,2 (zappelig) | nach 9,3 s |
| groß & schwer (100 kg, lang) | 3,2 | nach 0,9 s |

Lange Stauden kippen also träger (langes Pendel), brechen aber schnell —
genau das, was GDD 3.5 beschreibt („Kurze Stauden snappen praktisch nie,
lange sind das eigentliche Risiko"). Kleine, leichte Stauden sind in beiden
Dimensionen die einfachsten ([E] von Olli).

**Begründung für Kalibrierung auf `LengthScale = 1.0`:** Alle Startwerte aus
GDD 3.6 sind auf eine mittlere Staude geeicht. Bei Länge 1,0 rechnet die
Simulation deshalb exakt wie vorher, damit das mühsam getunte
`controlStrength` gültig bleibt.

**Noch nicht spielgetestet** — die Zahlen oben stammen aus einer
Monte-Carlo-Rechnung, nicht aus dem Editor.

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
