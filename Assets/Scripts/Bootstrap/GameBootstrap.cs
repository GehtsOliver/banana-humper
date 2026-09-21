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
    /// Editor verschiebbar. Dieses Skript verdrahtet zur Laufzeit nur noch die
    /// Gameplay-Systeme und baut die prozeduralen Formen (Cutter, Trailer,
    /// Boden, Zielmarkierung) auf die in der Szene platzierten Anker.
    ///
    /// Die Szene wird von "BananaHumper > Bootstrap-Szene erzeugen" erzeugt
    /// (Assets/Scripts/Editor/SceneSetupTool.cs) und gehoert danach dir -
    /// Positionsaenderungen in der Szene ueberleben, das Tool ueberschreibt sie
    /// nur nach Rueckfrage.
    ///
    /// Kunst-Quellen (siehe docs/THIRD_PARTY_ASSETS.md fuer Lizenzdetails):
    /// - Spielfigur (Poses/Walk-Zyklus): Kenney "Toon Characters" (CC0)
    /// - Hintergrund (Huegel, Baeume, Wolken, Zaun, Gras): Kenney
    ///   "Background Elements" (CC0), flache Silhouetten, eingefaerbt auf die
    ///   GDD-8.1-Palette (Gruen/Gelb/Erdbraun/Himmelblau)
    /// - Bananenstaude, Trailer, Cutter-Figur: es gibt kein freies CC0-Asset
    ///   dafuer (insbesondere fuer die 3 Zustaende gesund/durchgebogen/gesnappt
    ///   der Staude), deshalb prozedural aus kantengeglaetteten Vektorformen
    ///   (SpriteFactory.CreateRoundedQuad/CreateEllipse) gebaut. Die Cutter-
    ///   Figur bleibt bewusst ohne konkrete Hautfarbe/Gesichtszuege abstrakt,
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
        [Tooltip("Wurzel der Spielfigur - ihre X-Position wird waehrend der Schicht gesetzt.")]
        public Transform playerRoot;
        public PlayerAnimator playerAnimator;
        [Tooltip("Staude an der Schulter. Verschieben = Auflagepunkt aendern.")]
        public BananaBunchVisual bunchVisual;
        [Tooltip("Startpunkt: hier faellt die Staude auf die Schulter. Die prozedurale Cutter-Figur entsteht beim Start darunter.")]
        public Transform cutterAnchor;
        [Tooltip("Zielpunkt des Trips. Die prozedurale Trailer-Form entsteht beim Start darunter.")]
        public Transform trailerAnchor;
        [Tooltip("Rote Zielmarkierung fuers Auflegen (GDD 3.2).")]
        public Transform targetMarker;

        void Start()
        {
            if (balanceConfig == null)
            {
                balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            }

            if (!SceneReferencesComplete()) return;

            float cutterX = cutterAnchor.position.x;
            float trailerX = trailerAnchor.position.x;

            BuildGround(cutterX, trailerX);
            BuildCutterFigure(cutterAnchor);
            BuildTrailer(trailerAnchor);
            BuildTargetMarker(targetMarker);
            targetMarker.gameObject.SetActive(false);

            var systemsRoot = new GameObject("Systems");
            var placement = systemsRoot.AddComponent<PlacementController>();
            var balance = systemsRoot.AddComponent<BalanceController>();
            var energy = systemsRoot.AddComponent<EnergySystem>();
            var economy = systemsRoot.AddComponent<EconomySystem>();
            var shift = systemsRoot.AddComponent<ShiftController>();
            var hud = systemsRoot.AddComponent<HUDController>();

            placement.config = balanceConfig;
            placement.shoulderMarker = playerRoot;
            placement.targetMarker = targetMarker;

            balance.config = balanceConfig;
            energy.config = balanceConfig;
            // TEMPORAER (Nutzerwunsch): Schicht endet nie, damit sich Trips beim
            // Testen beliebig oft wiederholen lassen. Vor einem echten Balance-
            // Test (GDD Kapitel 12) diese Zeile wieder entfernen.
            energy.InfiniteEnergy = true;
            economy.config = balanceConfig;

            shift.config = balanceConfig;
            shift.placement = placement;
            shift.balance = balance;
            shift.energy = energy;
            shift.economy = economy;
            shift.playerRoot = playerRoot;
            shift.bunchVisual = bunchVisual.transform;
            shift.bunchVisualController = bunchVisual;
            shift.playerAnimator = playerAnimator;
            shift.cutterX = cutterX;
            shift.trailerX = trailerX;
            shift.Initialize();

            hud.Bind(shift, balance, energy, economy, balanceConfig);

            shift.ResetRun();
            shift.StartShift(1);
        }

        /// <summary>
        /// Die Szene liefert die Objekte, nicht mehr dieses Skript - fehlt eine
        /// Referenz (z.B. weil jemand GameBootstrap von Hand in eine leere Szene
        /// gelegt hat), waere die Folge eine NullReferenceException mitten im
        /// Aufbau. Lieber eine klare Meldung mit dem Weg zur Loesung.
        /// </summary>
        bool SceneReferencesComplete()
        {
            string missing = null;
            if (playerRoot == null) missing = nameof(playerRoot);
            else if (playerAnimator == null) missing = nameof(playerAnimator);
            else if (bunchVisual == null) missing = nameof(bunchVisual);
            else if (cutterAnchor == null) missing = nameof(cutterAnchor);
            else if (trailerAnchor == null) missing = nameof(trailerAnchor);
            else if (targetMarker == null) missing = nameof(targetMarker);

            if (missing == null) return true;

            Debug.LogError($"GameBootstrap: Szenen-Referenz '{missing}' ist nicht gesetzt. " +
                           "Szene ueber das Menue 'BananaHumper > Bootstrap-Szene erzeugen' neu anlegen " +
                           "oder die Felder im Inspector zuweisen.", this);
            return false;
        }

        void BuildGround(float cutterX, float trailerX)
        {
            float margin = 3f;
            float width = Mathf.Abs(trailerX - cutterX) + margin * 2f;
            float centerX = (cutterX + trailerX) * 0.5f;
            SpriteFactory.CreateQuad("Ground", new Color(0.30f, 0.33f, 0.17f), new Vector2(width, 2f), null,
                new Vector3(centerX, GroundY - 1f, 0f), sortingOrder: -1);
        }

        /// <summary>
        /// Prozedurale Cutter-Hintergrundfigur - bewusst ohne konkrete Hautfarbe/
        /// Gesicht (GDD 1.7: nie ethnisch markiert). Entsteht unter dem Anker aus
        /// der Szene, dessen Position bestimmt, wo die Figur steht.
        /// </summary>
        void BuildCutterFigure(Transform root)
        {
            var skin = new Color(0.85f, 0.68f, 0.5f);
            var vest = new Color(0.9f, 0.55f, 0.15f);
            var pants = new Color(0.25f, 0.22f, 0.2f);
            var hatColor = new Color(0.55f, 0.4f, 0.2f);
            var blade = new Color(0.8f, 0.82f, 0.85f);

            SpriteFactory.CreateRoundedQuad("Legs", pants, new Vector2(0.5f, 0.7f), 0.3f, root, new Vector3(0f, 0.35f, 0f), 0);
            SpriteFactory.CreateRoundedQuad("Torso", vest, new Vector2(0.62f, 0.75f), 0.4f, root, new Vector3(0f, 0.95f, 0f), 1);
            SpriteFactory.CreateEllipse("Head", skin, new Vector2(0.4f, 0.4f), root, new Vector3(0f, 1.5f, 0f), 2);
            SpriteFactory.CreateEllipse("HatBrim", hatColor, new Vector2(0.62f, 0.18f), root, new Vector3(0f, 1.62f, 0f), 3);
            SpriteFactory.CreateRoundedQuad("HatTop", hatColor, new Vector2(0.32f, 0.22f), 0.5f, root, new Vector3(0f, 1.74f, 0f), 3);

            var machete = new GameObject("Machete").transform;
            machete.SetParent(root, false);
            machete.localPosition = new Vector3(0.42f, 1.05f, 0f);
            machete.localRotation = Quaternion.Euler(0f, 0f, -35f);
            SpriteFactory.CreateRoundedQuad("Blade", blade, new Vector2(0.1f, 0.65f), 0.5f, machete, Vector3.zero, 2);
        }

        /// <summary>Prozeduraler Trailer (rundes Vektor-Composite statt einzelnem Rechteck), unter dem Szenen-Anker.</summary>
        void BuildTrailer(Transform root)
        {
            var bodyColor = new Color(0.35f, 0.38f, 0.42f);
            var accentColor = new Color(0.85f, 0.5f, 0.15f);
            var wheelColor = new Color(0.15f, 0.15f, 0.17f);

            SpriteFactory.CreateEllipse("WheelBack", wheelColor, new Vector2(0.42f, 0.42f), root, new Vector3(-0.55f, 0.21f, 0f), 0);
            SpriteFactory.CreateEllipse("WheelFront", wheelColor, new Vector2(0.42f, 0.42f), root, new Vector3(0.55f, 0.21f, 0f), 0);
            SpriteFactory.CreateRoundedQuad("Body", bodyColor, new Vector2(1.8f, 0.65f), 0.35f, root, new Vector3(0f, 0.65f, 0f), 1);
            SpriteFactory.CreateRoundedQuad("Rail", accentColor, new Vector2(1.7f, 0.14f), 0.6f, root, new Vector3(0f, 1.0f, 0f), 2);
        }

        void BuildTargetMarker(Transform root)
        {
            SpriteFactory.CreateRoundedQuad("Shape", new Color(1f, 0.2f, 0.2f, 0.85f),
                new Vector2(0.25f, 1.2f), 0.6f, root, Vector3.zero, sortingOrder: 1);
        }
    }
}
