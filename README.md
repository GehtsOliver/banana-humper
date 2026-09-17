# Banana Humper

Umsetzung des GDD (v0.8) für den Kern-Loop, nach Roadmap **Stufe A**
("Kern-Loop validieren und polieren", GDD Kapitel 16.2/16.3): nur die
Balance-Mechanik aus Kapitel 3 plus das Minimum an Ressourcen aus 4.1–4.3,
damit sich Trips wiederholen lassen. Bewusst **nicht** enthalten: Tagesquote,
Verwarnungen, Körper/Shop-Progression, Prestige, Zufallsereignisse, Cutter-
Sprüche, Sound, finaler Art-Stil. Das kommt erst, wenn der Kern-Loop laut
Roadmap für sich allein Spaß macht (Kapitel 12, Graybox-Test).

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
- In dieser Entwicklungsumgebung ist kein Unity/dotnet installiert, der Code
  konnte also nicht im Editor kompiliert/getestet werden. Bitte beim ersten
  Öffnen in Unity auf Compiler-Fehler prüfen.

## Projekt öffnen und spielen

1. Projekt in Unity Hub hinzufügen (diesen Ordner wählen) und öffnen.
2. Eine neue leere Szene anlegen (`File > New Scene > Basic (Built-in)`,
   dann alles außer der Kamera löschen – die Kamera wird ohnehin zur
   Laufzeit neu konfiguriert).
3. Ein leeres GameObject erstellen, `GameBootstrap` (aus
   `Assets/Scripts/Bootstrap`) draufziehen.
4. Optional: eine eigene `BalanceConfig`-Asset anlegen
   (`Assets > Create > BananaHumper > Balance Config`) und im
   `GameBootstrap`-Inspector zuweisen, um Kapitel-3.6-Werte zu tunen ohne
   Code anzufassen. Ohne Zuweisung werden die Default-Werte aus dem GDD
   verwendet.
5. Play drücken.

## Steuerung

Die Maus steuert überall per **Bewegung/Delta**, nicht per Cursor-Position –
wie ein Besen, den man auf der Hand balanciert (GDD 3.3). Der Cursor wird
während der Schicht automatisch gesperrt und ausgeblendet (sonst würde man
am Rand des Game-View-Fensters hängen bleiben); beim Schichtende-Screen wird
er wieder freigegeben, damit der Button klickbar ist.

- **Auflegen:** Maus horizontal bewegen, um die Schulter unter die rote
  Zielmarkierung zu bringen, bevor die Staude fällt.
- **Balancieren:** Maus horizontal bewegen, um die Staude auszugleichen.
- **Linke Maustaste halten:** rennen (schneller, mehr Wackeln, mehr
  Energieverbrauch).
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
| `UI/HUDController.cs` | 9 | Minimales Schicht-HUD, zur Laufzeit erzeugt |
| `Bootstrap/GameBootstrap.cs` | – | Verdrahtet alles, ersetzt Szenen-Handarbeit |
| `Util/SpriteFactory.cs` | 8.1 | Graybox-Rechtecke ohne Art-Assets |

Alle Objekte (Kamera, Boden, Cutter/Trailer-Marker, Spieler, HUD) werden von
`GameBootstrap` zur Laufzeit aus einfachen farbigen Rechtecken erzeugt – es
gibt keine handgeschriebene `.unity`-Szenendatei, weil deren YAML-Format
ohne laufenden Editor nicht zuverlässig zu verifizieren ist. Das hält das
Risiko einer kaputten Szene bei null, kostet aber echten Art-Stil (Kapitel
8) für später.

## Nächste Schritte laut Roadmap (Kapitel 16, Stufe A)

1. **Graybox-Test** mit 3–5 Testern gegen die Kriterien aus Kapitel 12
   durchführen (macht Balancieren allein Spaß? Werden Fails als lustig statt
   unfair empfunden?).
2. Basierend auf Feedback tunen (`BalanceConfig`-Werte) – erste Kandidaten:
   `controlStrength`, `damping`, `stressRate`, `placementToleranceWorldUnits`.
3. Danach erst: finaler Art-Stil, Sound/Juice (Kapitel 8), dann Tagesquote,
   Verwarnungen (4.4, 4.7), Körper/Shop (Kapitel 5), Ereignisse (Kapitel 7).
