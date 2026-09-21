using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;
using BananaHumper.Gameplay;
using BananaHumper.UI;
using BananaHumper.Util;

namespace BananaHumper.Bootstrap
{
    /// <summary>
    /// Hybrid-Aufbau (siehe docs/DECISIONS.md): Layout und importierte Kunst
    /// liegen als echte Objekte in Assets/Scenes/Main.unity und sind dort im
    /// Editor verschiebbar - insbesondere die Cutter-Stationen, deren Abstaende
    /// das Level-Design des Kern-Loops sind (GDD 3.2). Dieses Skript verdrahtet
    /// zur Laufzeit die Gameplay-Systeme und baut die prozeduralen Formen auf
    /// die Anker aus der Szene.
    ///
    /// Die Szene wird von "BananaHumper > Bootstrap-Szene erzeugen" erzeugt
    /// (Assets/Scripts/Editor/SceneSetupTool.cs) und gehoert danach dir - das
    /// Tool ueberschreibt sie nur nach Rueckfrage.
    ///
    /// Kunst-Quellen (siehe docs/THIRD_PARTY_ASSETS.md fuer Lizenzdetails):
    /// - Spielfigur und Cutter-Posen: Kenney "Toon Characters" (CC0)
    /// - Hintergrund: Kenney "Background Elements" (CC0), eingefaerbt auf die
    ///   GDD-8.1-Palette
    /// - Bananenstaude, Trailer, Cutter-Figur: kein freies CC0-Asset vorhanden,
    ///   deshalb prozedural aus kantengeglaetteten Vektorformen (SpriteFactory).
    ///   Die Cutter-Figur bleibt bewusst ohne konkrete Hautfarbe/Gesichtszuege,
    ///   siehe GDD 1.7 ("Cutter als Bogans, nie ethnisch markiert").
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        /// <summary>Bodenhoehe der Welt - auch das Editor-Tool platziert die Kulisse darauf.</summary>
        public const float GroundY = -1.5f;
        public const float PlayerBodyHeight = 1.6f;

        [Tooltip("Optional: eigene BalanceConfig-Asset zuweisen. Leer = Default-Werte aus Kapitel 3.6.")]
        public BalanceConfig balanceConfig;

        [Header("Szenen-Objekte (legt 'BananaHumper > Bootstrap-Szene erzeugen' an)")]
        public PlayerController player;
        public TrailerController trailer;
        [Tooltip("Cutter-Stationen. Ihre Abstaende sind das Level-Design des Kern-Loops (GDD 3.2).")]
        public List<CutterStation> stations = new List<CutterStation>();
        public CameraController cameraController;

        [Header("Grenzen der Reihe")]
        [Tooltip("Linker Rand. Der rechte Rand waechst mit dem am weitesten entfernten angeheuerten Cutter.")]
        public float rowMinX = -6f;
        [Tooltip("Puffer hinter dem letzten angeheuerten Cutter.")]
        public float rowMarginAfterLastStation = 3f;

        /// <summary>Zur Laufzeit erzeugte Steine (GDD 3.9) - jede Schicht neu gewuerfelt.</summary>
        readonly List<Obstacle> obstacles = new List<Obstacle>();
        float rowMaxX;

        void Start()
        {
            if (balanceConfig == null)
            {
                balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            }

            if (!SceneReferencesComplete()) return;

            // Das Paddock ist nur so gross wie die angeheuerte Mannschaft: Jeder
            // zusaetzliche Cutter verlaengert die Reihe (GDD 5.2).
            rowMaxX = LastHiredStationX() + rowMarginAfterLastStation;

            BuildGround();
            BuildTrailerShape(trailer.transform);
            foreach (var station in stations)
            {
                if (station != null && station.isHired) BuildCutterFigure(station.transform);
                else if (station != null) station.gameObject.SetActive(false);
            }
            SpawnRocks();

            player.config = balanceConfig;
            player.minX = rowMinX;
            player.maxX = rowMaxX;
            player.obstacles = new List<Obstacle>(obstacles);
            player.ClearBunch();

            trailer.config = balanceConfig;
            trailer.minX = rowMinX;
            trailer.maxX = rowMaxX;

            if (cameraController != null)
            {
                cameraController.target = player.transform;
                cameraController.minX = rowMinX;
                cameraController.maxX = rowMaxX;
            }

            var systemsRoot = new GameObject("Systems");
            var balance = systemsRoot.AddComponent<BalanceController>();
            var energy = systemsRoot.AddComponent<EnergySystem>();
            var economy = systemsRoot.AddComponent<EconomySystem>();
            var shift = systemsRoot.AddComponent<ShiftController>();
            var hud = systemsRoot.AddComponent<HUDController>();

            balance.config = balanceConfig;
            energy.config = balanceConfig;
            economy.config = balanceConfig;

            shift.config = balanceConfig;
            shift.balance = balance;
            shift.energy = energy;
            shift.economy = economy;
            shift.player = player;
            shift.trailer = trailer;
            shift.cameraController = cameraController;
            shift.stations = new List<CutterStation>(stations);
            shift.Initialize();

            hud.Bind(shift, balance, energy, economy, balanceConfig);

            shift.ResetRun();
            shift.StartShift(1);
        }

        /// <summary>
        /// Die Szene liefert die Objekte, nicht mehr dieses Skript - fehlt eine
        /// Referenz (z.B. weil jemand GameBootstrap von Hand in eine leere Szene
        /// gelegt hat), gaebe es eine NullReferenceException mitten im Aufbau.
        /// Lieber eine klare Meldung mit dem Weg zur Loesung.
        /// </summary>
        bool SceneReferencesComplete()
        {
            string missing = null;
            if (player == null) missing = nameof(player);
            else if (trailer == null) missing = nameof(trailer);
            else if (stations == null || stations.Count == 0) missing = nameof(stations);
            else if (stations.Contains(null)) missing = $"{nameof(stations)} (leerer Eintrag)";
            else if (!stations.Exists(s => s.isHired)) missing = "mindestens ein angeheuerter Cutter";

            if (missing == null) return true;

            Debug.LogError($"GameBootstrap: Szenen-Referenz '{missing}' fehlt. " +
                           "Szene ueber das Menue 'BananaHumper > Bootstrap-Szene erzeugen' neu anlegen " +
                           "oder die Felder im Inspector zuweisen.", this);
            return false;
        }

        float LastHiredStationX()
        {
            float last = rowMinX + 6f;
            foreach (var station in stations)
            {
                if (station != null && station.isHired) last = Mathf.Max(last, station.transform.position.x);
            }
            return last;
        }

        /// <summary>
        /// Steine pro Schicht neu auswuerfeln (GDD 3.9): nicht zwischen jedem
        /// Cutterpaar einer, sondern ein paar zufaellige - und mit Mindestabstand
        /// zu den Stationen, damit nie einer direkt unter einer Fallstelle liegt.
        /// </summary>
        void SpawnRocks()
        {
            var rocksRoot = new GameObject("Rocks").transform;
            int count = Random.Range(balanceConfig.rockCountMin, balanceConfig.rockCountMax + 1);
            var placed = new List<float>();

            for (int i = 0; i < count; i++)
            {
                // Mehrere Versuche, weil eine Zufallsposition zu nah an einer
                // Station oder einem anderen Stein liegen kann.
                for (int attempt = 0; attempt < 24; attempt++)
                {
                    float x = Random.Range(rowMinX + 1.5f, rowMaxX - 1.5f);
                    if (!IsFarEnough(x, placed)) continue;

                    var go = new GameObject($"Rock{i}");
                    go.transform.SetParent(rocksRoot, false);
                    go.transform.position = new Vector3(x, GroundY, 0f);

                    var obstacle = go.AddComponent<Obstacle>();
                    obstacle.halfWidth = Random.Range(0.28f, 0.38f);
                    obstacle.clearHeight = Random.Range(0.40f, 0.60f);

                    BuildRockShape(obstacle);
                    obstacles.Add(obstacle);
                    placed.Add(x);
                    break;
                }
            }
        }

        bool IsFarEnough(float x, List<float> placed)
        {
            float minDistance = balanceConfig.rockMinDistance;

            foreach (var station in stations)
            {
                if (station == null || !station.isHired) continue;
                if (Mathf.Abs(x - station.DropPosition.x) < minDistance) return false;
            }
            foreach (float other in placed)
            {
                if (Mathf.Abs(x - other) < minDistance) return false;
            }
            return true;
        }

        void BuildGround()
        {
            float margin = 3f;
            float width = Mathf.Abs(rowMaxX - rowMinX) + margin * 2f;
            float centerX = (rowMinX + rowMaxX) * 0.5f;
            SpriteFactory.CreateQuad("Ground", new Color(0.30f, 0.33f, 0.17f), new Vector2(width, 2f), null,
                new Vector3(centerX, GroundY - 1f, 0f), sortingOrder: -1);
        }

        /// <summary>
        /// Prozedurale Cutter-Hintergrundfigur - bewusst ohne konkrete Hautfarbe/
        /// Gesicht (GDD 1.7: nie ethnisch markiert). Entsteht unter dem
        /// Stations-Anker aus der Szene.
        /// </summary>
        void BuildCutterFigure(Transform root)
        {
            var skin = new Color(0.85f, 0.68f, 0.5f);
            var vest = new Color(0.9f, 0.55f, 0.15f);
            var pants = new Color(0.25f, 0.22f, 0.2f);
            var hatColor = new Color(0.55f, 0.4f, 0.2f);
            var blade = new Color(0.8f, 0.82f, 0.85f);

            var figure = new GameObject("CutterFigure").transform;
            figure.SetParent(root, false);

            SpriteFactory.CreateRoundedQuad("Legs", pants, new Vector2(0.5f, 0.7f), 0.3f, figure, new Vector3(0f, 0.35f, 0f), 0);
            SpriteFactory.CreateRoundedQuad("Torso", vest, new Vector2(0.62f, 0.75f), 0.4f, figure, new Vector3(0f, 0.95f, 0f), 1);
            SpriteFactory.CreateEllipse("Head", skin, new Vector2(0.4f, 0.4f), figure, new Vector3(0f, 1.5f, 0f), 2);
            SpriteFactory.CreateEllipse("HatBrim", hatColor, new Vector2(0.62f, 0.18f), figure, new Vector3(0f, 1.62f, 0f), 3);
            SpriteFactory.CreateRoundedQuad("HatTop", hatColor, new Vector2(0.32f, 0.22f), 0.5f, figure, new Vector3(0f, 1.74f, 0f), 3);

            var machete = new GameObject("Machete").transform;
            machete.SetParent(figure, false);
            machete.localPosition = new Vector3(0.42f, 1.05f, 0f);
            machete.localRotation = Quaternion.Euler(0f, 0f, -35f);
            SpriteFactory.CreateRoundedQuad("Blade", blade, new Vector2(0.1f, 0.65f), 0.5f, machete, Vector3.zero, 2);
        }

        /// <summary>
        /// Prozeduraler Stein (GDD 3.9). Die Form richtet sich nach den Werten
        /// der Obstacle-Komponente, damit das, was man sieht, auch das ist,
        /// woran man haengenbleibt.
        /// </summary>
        void BuildRockShape(Obstacle obstacle)
        {
            var stoneColor = new Color(0.45f, 0.44f, 0.42f);
            var shadeColor = new Color(0.33f, 0.32f, 0.31f);

            var shape = new GameObject("RockShape").transform;
            shape.SetParent(obstacle.transform, false);

            float width = obstacle.halfWidth * 2f;
            float height = obstacle.clearHeight;
            // Genau 0..clearHeight hoch: Der sichtbare Stein ist damit exakt das,
            // was man ueberspringen muss - keine unsichtbaren Raender.
            SpriteFactory.CreateEllipse("Stone", stoneColor, new Vector2(width, height),
                shape, new Vector3(0f, height * 0.5f, 0f), 1);
            SpriteFactory.CreateEllipse("Shade", shadeColor, new Vector2(width * 0.45f, height * 0.35f),
                shape, new Vector3(-width * 0.14f, height * 0.6f, 0f), 2);
        }

        /// <summary>Prozeduraler Trailer (rundes Vektor-Composite), unter dem Szenen-Anker.</summary>
        void BuildTrailerShape(Transform root)
        {
            var bodyColor = new Color(0.35f, 0.38f, 0.42f);
            var accentColor = new Color(0.85f, 0.5f, 0.15f);
            var wheelColor = new Color(0.15f, 0.15f, 0.17f);

            var shape = new GameObject("TrailerShape").transform;
            shape.SetParent(root, false);

            SpriteFactory.CreateEllipse("WheelBack", wheelColor, new Vector2(0.42f, 0.42f), shape, new Vector3(-0.55f, 0.21f, 0f), 0);
            SpriteFactory.CreateEllipse("WheelFront", wheelColor, new Vector2(0.42f, 0.42f), shape, new Vector3(0.55f, 0.21f, 0f), 0);
            SpriteFactory.CreateRoundedQuad("Body", bodyColor, new Vector2(1.8f, 0.65f), 0.35f, shape, new Vector3(0f, 0.65f, 0f), 1);
            SpriteFactory.CreateRoundedQuad("Rail", accentColor, new Vector2(1.7f, 0.14f), 0.6f, shape, new Vector3(0f, 1.0f, 0f), 2);
        }
    }
}
