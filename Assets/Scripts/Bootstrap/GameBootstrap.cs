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
        /// <summary>Hoehe, in der die Staude an der Pflanze haengt.</summary>
        public const float PlantBunchHeight = 2.6f;

        [Tooltip("Optional: eigene BalanceConfig-Asset zuweisen. Leer = Default-Werte aus Kapitel 3.6.")]
        public BalanceConfig balanceConfig;

        [Header("Szenen-Objekte (legt 'BananaHumper > Bootstrap-Szene erzeugen' an)")]
        public PlayerController player;
        public TrailerController trailer;
        [Tooltip("Die Cutter. Nicht angeheuerte bleiben aus, bis der Shop sie freischaltet (GDD 5.2).")]
        public List<Cutter> cutters = new List<Cutter>();
        public CameraController cameraController;

        /// <summary>
        /// Das begehbare Fenster waechst mit der angeheuerten Mannschaft
        /// (GDD 5.2): mehr Cutter heisst mehr Feld, nicht bloss mehr Betrieb
        /// auf demselben Fleck. Grenzen und Inhalt verwaltet danach PaddockField,
        /// weil das Fenster mit dem Trailer weiterwandert.
        /// </summary>
        PaddockField field;

        void Start()
        {
            if (balanceConfig == null)
            {
                balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            }

            if (!SceneReferencesComplete()) return;

            var systemsRoot = new GameObject("Systems");
            // Upgrades zuerst: Sie liefern die Laufzeit-Config, mit der alle
            // anderen Systeme rechnen - das Asset selbst bleibt unangetastet.
            var upgrades = systemsRoot.AddComponent<UpgradeSystem>();
            upgrades.Initialize(balanceConfig, cutters);
            var runtimeConfig = upgrades.RuntimeConfig;

            int hiredCount = cutters.FindAll(c => c != null && c.isHired).Count;
            float windowWidth = runtimeConfig.paddockBaseWidth
                              + runtimeConfig.paddockWidthPerCutter * hiredCount;

            field = new GameObject("Paddock").AddComponent<PaddockField>();
            field.aheadDistance = windowWidth * 0.65f;
            field.behindDistance = windowWidth * 0.35f;

            BuildGround(player.PositionX, windowWidth * 6f);
            BuildTrailerShape(trailer.transform);
            SetupCutters(windowWidth);

            player.config = runtimeConfig;
            player.ClearBunch();
            trailer.config = runtimeConfig;

            if (cameraController != null)
            {
                cameraController.target = player.transform;
            }

            var balance = systemsRoot.AddComponent<BalanceController>();
            var energy = systemsRoot.AddComponent<EnergySystem>();
            var economy = systemsRoot.AddComponent<EconomySystem>();
            var shift = systemsRoot.AddComponent<ShiftController>();
            var hud = systemsRoot.AddComponent<HUDController>();

            balance.config = runtimeConfig;
            energy.config = runtimeConfig;
            economy.config = runtimeConfig;

            shift.config = runtimeConfig;
            shift.balance = balance;
            shift.energy = energy;
            shift.economy = economy;
            shift.player = player;
            shift.trailer = trailer;
            shift.cameraController = cameraController;
            shift.cutters = new List<Cutter>(cutters);
            shift.field = field;
            shift.Initialize();

            var shop = systemsRoot.AddComponent<ShopPanel>();
            hud.Bind(shift, balance, energy, economy, runtimeConfig, upgrades);
            hud.AttachShop(shop);

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
            else if (cutters == null || cutters.Count == 0) missing = nameof(cutters);
            else if (cutters.Contains(null)) missing = $"{nameof(cutters)} (leerer Eintrag)";
            else if (!cutters.Exists(c => c.isHired)) missing = "mindestens ein angeheuerter Cutter";

            if (missing == null) return true;

            Debug.LogError($"GameBootstrap: Szenen-Referenz '{missing}' fehlt. " +
                           "Szene ueber das Menue 'BananaHumper > Bootstrap-Szene erzeugen' neu anlegen " +
                           "oder die Felder im Inspector zuweisen.", this);
            return false;
        }

        /// <summary>
        /// Cutter starten verteilt im Feld und suchen sich von dort die
        /// naechste Pflanze. Nicht angeheuerte bleiben komplett aus.
        /// </summary>
        void SetupCutters(float windowWidth)
        {
            var hired = cutters.FindAll(c => c != null && c.isHired);
            for (int i = 0; i < cutters.Count; i++)
            {
                var cutter = cutters[i];
                if (cutter == null) continue;
                if (!cutter.isHired)
                {
                    cutter.gameObject.SetActive(false);
                    continue;
                }

                // Verteilt starten, damit sie nicht alle am selben Fleck
                // losziehen; die naechste Pflanze suchen sie sich selbst.
                int slot = hired.IndexOf(cutter);
                float t = hired.Count > 1 ? (float)slot / (hired.Count - 1) : 0.5f;
                float x = player.PositionX + Mathf.Lerp(-windowWidth * 0.2f, windowWidth * 0.4f, t);
                cutter.transform.position = new Vector3(x, GroundY, 0f);
                PaddockVisuals.BuildCutter(cutter.transform);
            }
        }

        /// <summary>
        /// Boden als breites Band: Er muss das ganze wandernde Fenster
        /// abdecken, deshalb grosszuegig statt passgenau.
        /// </summary>
        void BuildGround(float centerX, float width)
        {
            SpriteFactory.CreateQuad("Ground", new Color(0.30f, 0.33f, 0.17f), new Vector2(width, 2f), null,
                new Vector3(centerX, GroundY - 1f, 0f), sortingOrder: -1);
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
