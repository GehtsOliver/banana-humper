# Banana Humper

Umsetzung des GDD (v0.9) für den Kern-Loop, nach Roadmap **Stufe A**
("Kern-Loop validieren und polieren", GDD Kapitel 16.2/16.3): mehrere
Cutter-Stationen schneiden parallel, der Spieler muss rechtzeitig unter der
fallenden Staude stehen und sie zum mitfahrenden Trailer schleppen. Bewusst
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
| `Stations/Station0..3` | **Das Level-Design:** Die Abstände zwischen den Stationen bestimmen, wie weit man laufen muss – der wichtigste Tuning-Hebel des Kern-Loops |
| `Obstacles/Rock0..3` | Steine zum Überspringen. Höhe und Breite stehen im `Obstacle`-Inspector; die sichtbare Form richtet sich danach |
| `Player` | Startposition der Spielfigur |
| `Player > BunchVisual` | Auflagepunkt der Staude auf der Schulter |
| `Trailer` | Startposition des Trailers (fährt zur Laufzeit die Reihe entlang) |
| `Scenery/*` | Hügel, Bäume, Wolken, Zaun, Gras |
| `Main Camera` | Bildausschnitt |

Einfach anklicken, verschieben, Szene speichern (`Strg+S`) – fertig. Kein
Code, keine Inspector-Zahlen abtippen. Die Reihen-Grenzen, in denen sich
Spieler und Trailer bewegen dürfen, stehen als `Row Min X` / `Row Max X` im
`GameBootstrap`-Inspector.

**Noch nicht sichtbar im Edit-Modus:** Die Cutter-Figuren, die Trailer-Form,
die Schnitt-Balken und die Stauden werden prozedural aus kantengeglätteten
Vektorshapes gebaut (`SpriteFactory`), und deren Texturen entstehen erst zur
Laufzeit – solche Sprites lassen sich nicht in einer Szenendatei speichern.
Verschieben funktioniert trotzdem (der Anker bestimmt die Position), man
sieht das Ergebnis nur erst beim Drücken von Play.

## Steuerung

- **Laufen:** `A`/`D` durch die Reihe.
- **Rennen:** `Shift` halten (schneller, mehr Wackeln, mehr Energie – aber
  nur mit Staude kostet Laufen überhaupt Energie).
- **Springen:** `Leertaste`. In der Reihe liegen vier Steine; dagegenlaufen
  kostet Tempo, und mit Staude zusätzlich Energie und einen kräftigen
  Wackler.
- **Fangen:** rechtzeitig unter der fallenden Staude stehen. Je mittiger,
  desto besser: perfekt = kein Versatz, Streifer = −25 % Lohn und starker
  Versatz.
- **Tragen - Ausgleichen:** linke/rechte Maustaste halten, um die Staude
  gerade zu halten (verzeihend ausgelegt, nicht mehr die Herausforderung).
- **`E`:** umsetzen (kurzer Stillstand, reduziert den Versatz, kostet
  Energie).
- **Abliefern:** mit Staude zum Trailer laufen – passiert automatisch.
- Schicht endet automatisch, wenn die Energie leer ist.

## Code-Struktur

| Datei | GDD-Kapitel | Zweck |
|---|---|---|
| `Config/BalanceConfig.cs` | 3.6 | Tuning-Werte als ScriptableObject |
| `Gameplay/BunchData.cs` | 4.3 | Stufenlos zufälliges Gewicht/Länge pro Staude, Dicke daraus abgeleitet |
| `Gameplay/CutterStation.cs` | 3.2 | Eine Station: Balken füllen, abschlagen, nachwachsen |
| `Gameplay/ProgressBarVisual.cs` | 3.2 | Schnitt-Balken über der Station (in der Welt, nicht im HUD) |
| `Gameplay/FallingBunch.cs` | 3.3 | Fallende Staude, meldet den Aufprall |
| `Gameplay/PlayerController.cs` | 3.1 | Freie Bewegung in der Reihe, Tragezustand |
| `Gameplay/TrailerController.cs` | 3.5 | Mitfahrender Trailer, Ablieferung |
| `Gameplay/Obstacle.cs` | 3.9 | Stein in der Reihe, Treffer- und Höhenprüfung |
| `Gameplay/BalanceController.cs` | 3.4 | Pendel-Simulation beim Schleppen, Umsetzen |
| `Gameplay/EnergySystem.cs` | 4.2 | Energieverbrauch und Schichtende |
| `Gameplay/EconomySystem.cs` | 4.1, 4.3, 5.1 | Lohn und Erfahrung |
| `Gameplay/ShiftController.cs` | 3.1, 3.3 | Taktet alles, wertet Catch-Qualität aus, Schichtende |
| `Gameplay/BananaBunchVisual.cs` | 3.8, 8.2 | Prozedurale Bananenstaude, Größe/Länge/Dicke aus `BunchData` |
| `Gameplay/PlayerAnimator.cs` | 8.1 | Treibt den importierten Kenney-Walk-Zyklus der Spielfigur |
| `UI/HUDController.cs` | 9 | Minimales Schicht-HUD, zur Laufzeit erzeugt |
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
