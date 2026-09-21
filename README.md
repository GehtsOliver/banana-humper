# Banana Humper

Umsetzung des GDD (v0.9) für den Kern-Loop, nach Roadmap **Stufe A**
("Kern-Loop validieren und polieren", GDD Kapitel 16.2/16.3): Cutter wandern
durch das Paddock und schlagen an den Pflanzen ab, der Spieler muss
rechtzeitig unter der fallenden Staude stehen und sie zum Trailer schleppen,
der sich abschnittsweise durch die Reihe arbeitet. Bewusst
**nicht** enthalten: Tagesquote, Verwarnungen, Körper/Shop-Progression,
Prestige, Zufallsereignisse, Cutter-Sprüche, Sound. Das kommt erst, wenn der
Kern-Loop laut Roadmap für sich allein Spaß macht (Kapitel 12, Graybox-Test).

Die Kunst ist ein erster Schritt in Richtung Kapitel 8: Spielfigur und
Hintergrund nutzen echte CC0-Vektor-Assets von Kenney.nl statt reiner
Platzhalter-Rechtecke, Details siehe [docs/THIRD_PARTY_ASSETS.md](docs/THIRD_PARTY_ASSETS.md)
und den Abschnitt "Kunst" unten. Finaler, eigener Art-Stil (GDD 8.1) steht
weiterhin aus.

## Umgebung

- **Engine:** Unity 6 (`6000.3.24f1` LTS in `ProjectVersion.txt` – bei Bedarf
  in Unity Hub anpassen, jede 6000.x-Version sollte das Projekt öffnen).
- **Render Pipeline:** Built-in (nicht URP). Das GDD nennt URP als Zielwert;
  für diesen ersten Graybox-Pass wurde bewusst Built-in gewählt, weil es ohne
  Editor-Konfiguration läuft. Umstieg auf URP ist später ein einzelner
  Package-Manager-Schritt (Package Manager → URP installieren → Render
  Pipeline Converter), berührt keinen der Gameplay-Scripts.
- **Input:** klassischer Input Manager (Maus), kein Input-System-Package
  nötig.
- Der Code wurde per `Unity.exe -batchmode -nographics -quit` (lokal
  installierte `6000.3.24f1`) probehalber importiert/kompiliert, und
  `Assets/Scenes/Main.unity` wurde damit erzeugt - keine Compiler-Fehler,
  aber kein Gameplay-Test im Editor (kein Play-Mode-Durchlauf, keine
  visuelle Kontrolle der neuen Sprites/Skalierung). Bitte beim ersten
  Öffnen in Unity trotzdem auf Compiler-Fehler und die Console pruefen.

## Projekt öffnen und spielen

1. Projekt in Unity Hub hinzufügen (diesen Ordner wählen) und öffnen.
2. `Assets/Scenes/Main.unity` öffnen (ein einzelnes `GameBootstrap`-
   GameObject, siehe unten - erzeugt via `BananaHumper/Bootstrap-Szene
   erzeugen` im Editor-Menü, `Assets/Scripts/Editor/SceneSetupTool.cs`).
3. Play drücken.

Optional: eine eigene `BalanceConfig`-Asset anlegen
(`Assets > Create > BananaHumper > Balance Config`) und im
`GameBootstrap`-Inspector zuweisen, um Kapitel-3.6-Werte zu tunen ohne Code
anzufassen. Ohne Zuweisung werden die Default-Werte aus dem GDD verwendet.
Falls die Szene fehlt oder neu aufgebaut werden muss: Menü
`BananaHumper > Bootstrap-Szene erzeugen`. **Achtung:** Das baut die Szene
komplett neu und verwirft alles, was du darin von Hand angepasst hast (es
fragt vorher nach).

### Positionen anpassen

Die Szene ist im **Hybrid-Aufbau** (siehe
[docs/DECISIONS.md](docs/DECISIONS.md)): Layout und importierte Kunst liegen
als echte Objekte in der Szene, die Gameplay-Systeme entstehen weiterhin zur
Laufzeit.

**Direkt in der Szene verschiebbar** (Hierarchy, Edit-Modus, sichtbar):

| Objekt | Bedeutung |
|---|---|
| `Cutters/Cutter0..5` | Temperament und `Is Hired` je Cutter. Start sind zwei; Pflanzen und Steine entstehen zur Laufzeit im Feld-Fenster |
| `Player` | Startposition der Spielfigur |
| `Player > BunchVisual` | Auflagepunkt der Staude auf der Schulter |
| `Trailer` | Startposition; pro Schicht setzt ihn der Loop neu und er zieht abschnittsweise weiter |
| `Scenery/*` | Hügel, Bäume, Wolken, Zaun, Gras |
| `Main Camera` | Bildausschnitt |

Einfach anklicken, verschieben, Szene speichern (`Strg+S`) – fertig. Die
Größe des Arbeitsfensters ergibt sich aus der Mannschaft und steht als
`Paddock Base Width` / `Paddock Width Per Cutter` in der `BalanceConfig`.

**Nicht in der Szene:** Pflanzen und Steine entstehen erst zur Laufzeit im
mitwandernden Feld-Fenster (`PaddockField`) – vor dem Trailer wächst nach,
hinter ihm wird aufgeräumt. Damit ist die Reihe faktisch endlos, ohne dass
hunderte Objekte existieren müssen. Auch Cutter-Figuren, Trailer-Form,
Geduldsbalken und Stauden werden prozedural aus kantengeglätteten
Vektorshapes gebaut (`SpriteFactory`); deren Texturen entstehen zur Laufzeit
und lassen sich nicht in einer Szenendatei speichern.

## Steuerung

- **Laufen:** `A`/`D` durch die Reihe.
- **Rennen:** `Shift` halten (schneller, mehr Wackeln, mehr Energie – aber
  nur mit Staude kostet Laufen überhaupt Energie).
- **Springen:** `Leertaste`. Im Feld liegen zufällig verteilte Steine; dagegenlaufen
  kostet Tempo, und mit Staude zusätzlich Energie und einen kräftigen
  Wackler.
- **Fangen:** rechtzeitig unter der fallenden Staude stehen. Stehst du mit
  freien Händen **still** unter einer hängenden Staude, schlägt der Cutter
  sofort ab – so holst du dir Stauden aktiv, statt auf seine Geduld zu warten. Je mittiger,
  desto besser: perfekt = kein Versatz, Streifer = −25 % Lohn und starker
  Versatz.
- **Abliefern:** mit Staude zum Trailer laufen – passiert automatisch.
- Schicht endet, wenn die Energie leer ist. Dann öffnet sich der **Shop**.

**Balancieren ist abgeschaltet.** Die Staude sitzt fest auf der Schulter;
Maustasten und `E` (umsetzen) haben derzeit keine Funktion. Die Simulation
steckt weiter im Code und lässt sich über `Balancing Enabled` in der
`BalanceConfig` wieder einschalten (siehe
[docs/DECISIONS.md](docs/DECISIONS.md)).

## Fortschritt

Am Schichtende öffnet sich der Feierabend-Bildschirm mit zwei Spalten – zwei
Währungen, die sich bewusst nicht überschneiden:

**Körper (Erfahrung, aus abgelieferten Stauden)**

| Attribut | Effekt pro Stufe | Max | ab |
|---|---|---|---|
| Energie | +20 maximale Energie | 10 | 12 EP |
| Stärke | Energie beim Schleppen −8 % | 10 | 10 EP |
| Ausdauer | Energie beim Laufen/Rennen −8 % | 10 | 10 EP |
| Laufgeschwindigkeit | +6 % Tempo | 8 | 18 EP |

**Ausrüstung (Geld)**

| Artikel | Effekt pro Stufe | Max | ab |
|---|---|---|---|
| Cutter anheuern | Ein Cutter mehr, das Feld wächst mit | 4 | 60 $ |
| Schulterpad | Fangradius +12 % | 5 | 40 $ |
| Gummistiefel | Stolpern kostet −30 % Energie, bremst kürzer | 3 | 80 $ |
| Instant-Kaffee | Start mit +15 Energie über dem Maximum | 3 | 45 $ |

Preise steigen je Stufe um Faktor 1,6 (GDD 4.5). Käufe verändern **nicht**
das `BalanceConfig`-Asset, sondern eine Laufzeitkopie – sonst würde ein Kauf
im Editor die Datei dauerhaft ändern.

## Energie

Alles kostet Energie, gestaffelt nach Anstrengung:

| Anteil | pro Sekunde | gesenkt durch |
|---|---|---|
| Dasein (immer) | 0,15 | – |
| Laufen | +0,6 | Ausdauer |
| Rennen | ×1,8 auf den Laufanteil | Ausdauer |
| Schleppen | +`1,4 + kg/50` | Stärke |

Eine Schicht hält damit am Tag 1 rund 73 Sekunden, mit ausgebauten
Attributen bis etwa 150.

## Code-Struktur

| Datei | GDD-Kapitel | Zweck |
|---|---|---|
| `Config/BalanceConfig.cs` | 3.6 | Tuning-Werte als ScriptableObject |
| `Gameplay/BunchData.cs` | 4.3 | Stufenlos zufälliges Gewicht/Länge pro Staude, Dicke daraus abgeleitet |
| `Gameplay/Cutter.cs` | 3.2 | Wandernder Cutter: Geduld, Abschlagen, Weg zur nächsten Pflanze |
| `Gameplay/Plant.cs` | 3.2 | Bananenpflanze: trägt eine Staude, treibt nach der Ernte neu aus |
| `Gameplay/PaddockField.cs` | 3.2, 3.5 | Wanderndes Feld-Fenster: sät vorne nach, räumt hinten auf |
| `Gameplay/PaddockVisuals.cs` | 8.2 | Prozedurale Formen für Pflanzen und Steine |
| `Gameplay/ProgressBarVisual.cs` | 3.2 | Geduldsbalken über dem Cutter (in der Welt, nicht im HUD) |
| `Gameplay/FallingBunch.cs` | 3.3 | Fallende Staude, meldet den Aufprall |
| `Gameplay/PlayerController.cs` | 3.1 | Freie Bewegung in der Reihe, Tragezustand |
| `Gameplay/TrailerController.cs` | 3.5 | Trailer: zieht erst weiter, wenn der Abschnitt leer ist; Richtung pro Schicht |
| `Gameplay/Obstacle.cs` | 3.9 | Stein in der Reihe, Treffer- und Höhenprüfung |
| `Gameplay/CameraController.cs` | – | Mitfahrende Kamera mit Paddock-Grenzen und Kamerawackeln |
| `Gameplay/SplashEffect.cs` | 8.4 | Prozedurale Dreck-/Bananenspritzer beim Aufprall |
| `Gameplay/BalanceController.cs` | 3.4 | Pendel-Simulation beim Schleppen, Umsetzen |
| `Gameplay/EnergySystem.cs` | 4.2 | Energieverbrauch und Schichtende |
| `Gameplay/EconomySystem.cs` | 4.1, 4.3, 5.1 | Lohn und Erfahrung |
| `Gameplay/ShiftController.cs` | 3.1, 3.3 | Taktet alles, wertet Catch-Qualität aus, Schichtende |
| `Gameplay/BananaBunchVisual.cs` | 3.8, 8.2 | Prozedurale Bananenstaude, Größe/Länge/Dicke aus `BunchData` |
| `Gameplay/PlayerAnimator.cs` | 8.1 | Treibt den importierten Kenney-Walk-Zyklus der Spielfigur |
| `Gameplay/UpgradeSystem.cs` | 5.2, 10.2 | Shop-Stufen, Kosten, Laufzeitkopie der Config |
| `UI/HUDController.cs` | 9 | Minimales Schicht-HUD, zur Laufzeit erzeugt |
| `UI/ShopPanel.cs` | 5.2, 9 | Shop am Schichtende |
| `Bootstrap/GameBootstrap.cs` | – | Verdrahtet die Systeme, baut prozedurale Formen auf die Szenen-Anker |
| `Editor/SceneSetupTool.cs` | – | Erzeugt `Main.unity` (Kamera, Kulisse, Spieler, Anker) – nur Editor |
| `Util/SpriteFactory.cs` | 8.1 | Laedt importierte Sprites und zeichnet kantengeglaettete Vektorformen (abgerundete Rechtecke, Ellipsen) |

Die Aufteilung folgt dem Hybrid-Prinzip (siehe
[docs/DECISIONS.md](docs/DECISIONS.md)):

- **In der Szene** (`Main.unity`, vom Editor-Tool angelegt, danach von Hand
  pflegbar): Kamera, Kulisse, Spielfigur samt Animator-Referenzen, die vier
  Cutter-Stationen und der Trailer.
- **Zur Laufzeit von `GameBootstrap`**: die Gameplay-Systeme mitsamt
  Verdrahtung, das HUD und alle prozeduralen Formen, deren Texturen sich
  nicht als Asset speichern lassen.

Die Szenendatei wird nach wie vor nicht von Hand als YAML geschrieben,
sondern vom Editor-Tool erzeugt – dadurch bleibt der Aufbau reviewbar und
reproduzierbar, ohne dass man auf visuelles Arbeiten verzichten muss.

## Kunst

Zwei Quellen, siehe [docs/THIRD_PARTY_ASSETS.md](docs/THIRD_PARTY_ASSETS.md)
für Lizenzdetails:

- **Importierte CC0-Vektor-Assets** (Kenney.nl, `Assets/Resources/Art/`):
  Spielfigur-Posen/Walk-Zyklus und Hintergrund-Kulisse (Hügel, Bäume, Wolken,
  Zaun, Gras). Die Hintergrund-Elemente sind einfarbige Silhouetten und
  werden zur Laufzeit auf die GDD-8.1-Palette eingefärbt.
- **Prozedurale Vektorformen** (`SpriteFactory.CreateRoundedQuad`/
  `CreateEllipse`, kantengeglättet statt hart) für alles, wofür es kein
  passendes freies Asset gibt: Bananenstaude (3 Sichtzustände), Trailer,
  Cutter-Figur. Die Cutter-Figur bleibt bewusst ohne konkrete Hautfarbe/
  Gesichtszüge (GDD 1.7: "Cutter als Bogans, nie ethnisch markiert").

Weiterhin offen: finaler eigener Art-Stil, Sound/Juice (GDD Kapitel 8).

## Nächste Schritte laut Roadmap (Kapitel 16, Stufe A)

1. **Graybox-Test** mit 3–5 Testern gegen die Kriterien aus Kapitel 12
   durchführen (macht Balancieren allein Spaß? Werden Fails als lustig statt
   unfair empfunden?).
2. Basierend auf Feedback tunen (`BalanceConfig`-Werte) – erste Kandidaten:
   `controlStrength`, `damping`, `stressRate`, `placementToleranceWorldUnits`.
3. Danach erst: finaler Art-Stil, Sound/Juice (Kapitel 8), dann Tagesquote,
   Verwarnungen (4.4, 4.7), Körper/Shop (Kapitel 5), Ereignisse (Kapitel 7).
