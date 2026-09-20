# Banana Humper

Umsetzung des GDD (v0.8) für den Kern-Loop, nach Roadmap **Stufe A**
("Kern-Loop validieren und polieren", GDD Kapitel 16.2/16.3): nur die
Balance-Mechanik aus Kapitel 3 plus das Minimum an Ressourcen aus 4.1–4.3,
damit sich Trips wiederholen lassen. Bewusst **nicht** enthalten: Tagesquote,
Verwarnungen, Körper/Shop-Progression, Prestige, Zufallsereignisse, Cutter-
Sprüche, Sound. Das kommt erst, wenn der Kern-Loop laut Roadmap für sich
allein Spaß macht (Kapitel 12, Graybox-Test).

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
Falls die Szene fehlt oder neu erzeugt werden muss: leeres GameObject
anlegen, `GameBootstrap`-Komponente draufziehen, oder das Menü
`BananaHumper > Bootstrap-Szene erzeugen` verwenden.

## Steuerung

Die Maus steuert überall, wo sie zum Einsatz kommt, per **Bewegung/Delta**,
nicht per Cursor-Position – wie ein Besen, den man auf der Hand balanciert
(GDD 3.3). Der Cursor wird während der Schicht automatisch gesperrt und
ausgeblendet (sonst würde man am Rand des Game-View-Fensters hängen
bleiben); beim Schichtende-Screen wird er wieder freigegeben, damit der
Button klickbar ist.

- **Auflegen:** `A`/`D`, um die Schulter unter die rote Zielmarkierung zu
  bringen, bevor die Staude fällt.
- **Tragen - Laufen:** `A`/`D` zum Trailer bzw. zurück (ersetzt die
  automatische Bewegung aus GDD 3.1 [A] - bewusste Design-Entscheidung).
- **Tragen - Balancieren:** zusätzlich, gleichzeitig zum Laufen, Maus
  horizontal bewegen, um die Staude auszugleichen (wie ein Besen auf der
  Hand, GDD 3.3).
- **Linke Maustaste halten (während `A`/`D` gedrückt ist):** rennen
  (schneller, mehr Wackeln, mehr Energieverbrauch).
- **Rechte Maustaste:** umsetzen (kurzer Stillstand, reduziert den
  Auflage-Versatz, kostet Energie).
- Schicht endet automatisch, wenn die Energie leer ist; Button im
  Endscreen startet die nächste Schicht.

## Code-Struktur

| Datei | GDD-Kapitel | Zweck |
|---|---|---|
| `Config/BalanceConfig.cs` | 3.6 | Tuning-Werte als ScriptableObject |
| `Gameplay/BunchData.cs` | 4.3 | Zufälliges Gewicht/Länge pro Staude |
| `Gameplay/PlacementController.cs` | 3.2 | Auflegen, Offset-Berechnung |
| `Gameplay/BalanceController.cs` | 3.4, 3.5 | Pendel-Simulation, Belastung, Snap, Umsetzen |
| `Gameplay/EnergySystem.cs` | 4.2 | Energieverbrauch und Schichtende |
| `Gameplay/EconomySystem.cs` | 4.1, 4.3, 5.1 | Lohn und Erfahrung |
| `Gameplay/ShiftController.cs` | 3.1 | Trip-Ablauf-Statemachine |
| `Gameplay/BananaBunchVisual.cs` | 3.5, 3.8, 8.2 | Prozedurale Bananenstaude (gesund/durchgebogen/gesnappt), Laenge aus Fingerzahl |
| `Gameplay/PlayerAnimator.cs` | 8.1 | Treibt den importierten Kenney-Walk-Zyklus der Spielfigur |
| `UI/HUDController.cs` | 9 | Minimales Schicht-HUD, zur Laufzeit erzeugt |
| `Bootstrap/GameBootstrap.cs` | – | Verdrahtet alles, ersetzt Szenen-Handarbeit |
| `Util/SpriteFactory.cs` | 8.1 | Laedt importierte Sprites und zeichnet kantengeglaettete Vektorformen (abgerundete Rechtecke, Ellipsen) |

Alle Objekte (Kamera, Boden, Kulisse, Cutter, Trailer, Spieler, HUD) werden
von `GameBootstrap` zur Laufzeit erzeugt – es gibt keine handgeschriebene
`.unity`-Szenendatei, weil deren YAML-Format ohne laufenden Editor nicht
zuverlässig zu verifizieren ist. Das hält das Risiko einer kaputten Szene
bei null.

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
