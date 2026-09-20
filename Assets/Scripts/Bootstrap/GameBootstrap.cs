using UnityEngine;
using BananaHumper.Config;
using BananaHumper.Gameplay;
using BananaHumper.UI;
using BananaHumper.Util;

namespace BananaHumper.Bootstrap
{
    /// <summary>
    /// Einziges Objekt, das im Editor in eine leere Szene gelegt werden muss.
    /// Baut Kamera, Kunst, alle Gameplay-Systeme und das HUD zur Laufzeit auf
    /// und verdrahtet sie. Siehe README.md.
    ///
    /// Kunst-Quellen (siehe docs/THIRD_PARTY_ASSETS.md fuer Lizenzdetails):
    /// - Spielfigur (Poses/Walk-Zyklus): Kenney "Toon Characters" (CC0)
    /// - Hintergrund (Huegel, Baeume, Wolken, Zaun, Gras): Kenney
    ///   "Background Elements" (CC0), flache Silhouetten, per Code eingefaerbt
    ///   auf die GDD-8.1-Palette (Gruen/Gelb/Erdbraun/Himmelblau)
    /// - Bananenstaude, Trailer, Cutter-Figur: es gibt kein freies CC0-Asset
    ///   dafuer (insbesondere fuer die 3 Zustaende gesund/durchgebogen/gesnappt
    ///   der Staude), deshalb prozedural aus kantengeglaetteten Vektorformen
    ///   (SpriteFactory.CreateRoundedQuad/CreateEllipse) gebaut. Die Cutter-
    ///   Figur bleibt bewusst ohne konkrete Hautfarbe/Gesichtszuege abstrakt,
    ///   siehe GDD 1.7 ("Cutter als Bogans, nie ethnisch markiert").
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Optional: eigene BalanceConfig-Asset zuweisen. Leer = Default-Werte aus Kapitel 3.6.")]
        public BalanceConfig balanceConfig;

        const float GroundY = -1.5f;
        const float PlayerY = 0f;
        const float PlayerBodyHeight = 1.6f;

        void Start()
        {
            if (balanceConfig == null)
            {
                balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            }

            float cutterX = 0f;
            float trailerX = balanceConfig.distanceToTrailer;

            SetupCamera(cutterX, trailerX);
            SetupScenery(cutterX, trailerX);
            SetupGround(cutterX, trailerX);
            BuildCutterFigure(cutterX);
            BuildTrailer(trailerX);

            var playerRoot = new GameObject("Player").transform;
            playerRoot.position = new Vector3(cutterX, PlayerY, 0f);

            var idleSprite = SpriteFactory.LoadSprite("Art/Player/idle");
            var bodyRenderer = SpriteFactory.CreateSprite("Body", idleSprite, playerRoot,
                new Vector3(0f, GroundY - PlayerY + PlayerBodyHeight * 0.5f, 0f), sortingOrder: 2, worldHeight: PlayerBodyHeight);

            var playerAnimator = playerRoot.gameObject.AddComponent<PlayerAnimator>();
            playerAnimator.target = bodyRenderer;
            playerAnimator.idleSprite = idleSprite;
            playerAnimator.hurtSprite = SpriteFactory.LoadSprite("Art/Player/hurt");
            playerAnimator.walkSprites = new[]
            {
                SpriteFactory.LoadSprite("Art/Player/walk_0"),
                SpriteFactory.LoadSprite("Art/Player/walk_1"),
                SpriteFactory.LoadSprite("Art/Player/walk_2"),
                SpriteFactory.LoadSprite("Art/Player/walk_3"),
                SpriteFactory.LoadSprite("Art/Player/walk_4"),
                SpriteFactory.LoadSprite("Art/Player/walk_5"),
                SpriteFactory.LoadSprite("Art/Player/walk_6"),
                SpriteFactory.LoadSprite("Art/Player/walk_7"),
            };

            var bunchGo = new GameObject("BunchVisual");
            bunchGo.transform.SetParent(playerRoot, false);
            bunchGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var bunchVisualController = bunchGo.AddComponent<BananaBunchVisual>();
            bunchVisualController.Build(BunchLength.Medium);

            var targetMarker = SpriteFactory.CreateRoundedQuad("TargetMarker", new Color(1f, 0.2f, 0.2f, 0.85f),
                new Vector2(0.25f, 1.2f), 0.6f, null, new Vector3(cutterX, PlayerY, 0f), sortingOrder: 1);
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
            placement.targetMarker = targetMarker.transform;

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
            shift.bunchVisual = bunchGo.transform;
            shift.bunchVisualController = bunchVisualController;
            shift.playerAnimator = playerAnimator;
            shift.cutterX = cutterX;
            shift.trailerX = trailerX;
            shift.Initialize();

            hud.Bind(shift, balance, energy, economy, balanceConfig);

            shift.ResetRun();
            shift.StartShift(1);
        }

        void SetupCamera(float cutterX, float trailerX)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(4f, (trailerX - cutterX) * 0.55f);
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            cam.transform.position = new Vector3((cutterX + trailerX) * 0.5f, 0.5f, -10f);
        }

        void SetupGround(float cutterX, float trailerX)
        {
            float margin = 3f;
            float width = (trailerX - cutterX) + margin * 2f;
            float centerX = (cutterX + trailerX) * 0.5f;
            SpriteFactory.CreateQuad("Ground", new Color(0.30f, 0.33f, 0.17f), new Vector2(width, 2f), null, new Vector3(centerX, GroundY - 1f, 0f), sortingOrder: -1);
        }

        /// <summary>Huegel, Baeume, Wolken, Zaun und Grasbueschel aus Kenney "Background Elements" (CC0), auf die GDD-Palette eingefaerbt.</summary>
        void SetupScenery(float cutterX, float trailerX)
        {
            float centerX = (cutterX + trailerX) * 0.5f;
            float spanWidth = (trailerX - cutterX) + 16f;

            var hillsFar = SpriteFactory.LoadSprite("Art/Background/hills_far");
            if (hillsFar != null)
            {
                float aspect = hillsFar.rect.width / hillsFar.rect.height;
                float height = spanWidth / aspect;
                SpriteFactory.CreateSprite("HillsFar", hillsFar, null, new Vector3(centerX, GroundY + height * 0.5f, 0f), -6,
                    worldWidth: spanWidth, tint: new Color(0.34f, 0.5f, 0.34f));
            }

            var hillsNear = SpriteFactory.LoadSprite("Art/Background/hills_near");
            if (hillsNear != null)
            {
                float width = spanWidth * 0.92f;
                float aspect = hillsNear.rect.width / hillsNear.rect.height;
                float height = width / aspect;
                SpriteFactory.CreateSprite("HillsNear", hillsNear, null, new Vector3(centerX, GroundY + height * 0.5f, 0f), -5,
                    worldWidth: width, tint: new Color(0.22f, 0.4f, 0.22f));
            }

            var cloud = SpriteFactory.LoadSprite("Art/Background/cloud");
            if (cloud != null)
            {
                float[] cx = { cutterX - 3f, centerX + 1.5f, trailerX + 3.5f };
                float[] cy = { GroundY + 5.4f, GroundY + 6.1f, GroundY + 5.6f };
                for (int i = 0; i < cx.Length; i++)
                {
                    SpriteFactory.CreateSprite($"Cloud{i}", cloud, null, new Vector3(cx[i], cy[i], 0f), -6,
                        worldWidth: 3f, tint: new Color(1f, 1f, 1f, 0.85f));
                }
            }

            var treeA = SpriteFactory.LoadSprite("Art/Background/tree_a");
            var treeB = SpriteFactory.LoadSprite("Art/Background/tree_b");
            float[] treeX = { cutterX - 2.2f, centerX - 3.2f, centerX + 2.8f, trailerX + 2.3f };
            Sprite[] treeSprites = { treeA, treeB, treeA, treeB };
            for (int i = 0; i < treeX.Length; i++)
            {
                var sprite = treeSprites[i % treeSprites.Length];
                if (sprite == null) continue;
                float h = 2.1f + (i % 2) * 0.5f;
                SpriteFactory.CreateSprite($"Tree{i}", sprite, null, new Vector3(treeX[i], GroundY + h * 0.5f, 0f), -3,
                    worldHeight: h, tint: new Color(0.16f, 0.34f, 0.16f));
            }

            var fence = SpriteFactory.LoadSprite("Art/Background/fence");
            if (fence != null)
            {
                SpriteFactory.CreateSprite("Fence", fence, null, new Vector3(trailerX + 1.9f, GroundY + 0.4f, 0f), -2,
                    worldHeight: 0.8f, tint: new Color(0.42f, 0.3f, 0.17f));
            }

            var grassTuft = SpriteFactory.LoadSprite("Art/Background/grass_tuft");
            if (grassTuft != null)
            {
                var rng = new System.Random(1234);
                for (int i = 0; i < 14; i++)
                {
                    float x = cutterX - 3f + (float)rng.NextDouble() * spanWidth;
                    float h = 0.3f + (float)rng.NextDouble() * 0.25f;
                    SpriteFactory.CreateSprite($"Grass{i}", grassTuft, null, new Vector3(x, GroundY + h * 0.5f, 0f), 0,
                        worldHeight: h, flipX: rng.Next(0, 2) == 0, tint: new Color(0.3f, 0.55f, 0.18f));
                }
            }
        }

        /// <summary>
        /// Prozedurale Cutter-Hintergrundfigur - bewusst ohne konkrete Hautfarbe/
        /// Gesicht (GDD 1.7: nie ethnisch markiert). Steht deutlich abseits vom
        /// Player-Startpunkt, damit sie klar als Hintergrundfigur an der
        /// Schneidestelle erkennbar ist und nicht wie eine zweite Spielfigur wirkt.
        /// </summary>
        void BuildCutterFigure(float x)
        {
            var root = new GameObject("Cutter").transform;
            root.position = new Vector3(x - 2.6f, GroundY, 0f);

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

        /// <summary>Prozeduraler Trailer (rundes Vektor-Composite statt einzelnem Rechteck).</summary>
        void BuildTrailer(float x)
        {
            var root = new GameObject("Trailer").transform;
            root.position = new Vector3(x, GroundY, 0f);

            var bodyColor = new Color(0.35f, 0.38f, 0.42f);
            var accentColor = new Color(0.85f, 0.5f, 0.15f);
            var wheelColor = new Color(0.15f, 0.15f, 0.17f);

            SpriteFactory.CreateEllipse("WheelBack", wheelColor, new Vector2(0.42f, 0.42f), root, new Vector3(-0.55f, 0.21f, 0f), 0);
            SpriteFactory.CreateEllipse("WheelFront", wheelColor, new Vector2(0.42f, 0.42f), root, new Vector3(0.55f, 0.21f, 0f), 0);
            SpriteFactory.CreateRoundedQuad("Body", bodyColor, new Vector2(1.8f, 0.65f), 0.35f, root, new Vector3(0f, 0.65f, 0f), 1);
            SpriteFactory.CreateRoundedQuad("Rail", accentColor, new Vector2(1.7f, 0.14f), 0.6f, root, new Vector3(0f, 1.0f, 0f), 2);
        }
    }
}
