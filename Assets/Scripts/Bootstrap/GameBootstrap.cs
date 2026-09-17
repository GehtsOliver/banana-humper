using UnityEngine;
using BananaHumper.Config;
using BananaHumper.Gameplay;
using BananaHumper.UI;
using BananaHumper.Util;

namespace BananaHumper.Bootstrap
{
    /// <summary>
    /// Einziges Objekt, das im Editor in eine leere Szene gelegt werden muss.
    /// Baut Kamera, Graybox-Visuals, alle Gameplay-Systeme und das HUD zur
    /// Laufzeit auf und verdrahtet sie. Siehe README/SETUP.md.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Tooltip("Optional: eigene BalanceConfig-Asset zuweisen. Leer = Default-Werte aus Kapitel 3.6.")]
        public BalanceConfig balanceConfig;

        const float GroundY = -1.5f;
        const float PlayerY = 0f;

        void Start()
        {
            if (balanceConfig == null)
            {
                balanceConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            }

            float cutterX = 0f;
            float trailerX = balanceConfig.distanceToTrailer;

            SetupCamera(cutterX, trailerX);
            SetupGround(cutterX, trailerX);
            var cutterMarker = SpriteFactory.CreateQuad("Cutter", new Color(0.5f, 0.3f, 0.1f), new Vector2(0.6f, 1.6f), null, new Vector3(cutterX, GroundY + 0.8f, 0f));
            var trailerMarker = SpriteFactory.CreateQuad("Trailer", new Color(0.2f, 0.2f, 0.25f), new Vector2(1.4f, 1.0f), null, new Vector3(trailerX, GroundY + 0.5f, 0f));

            var playerRoot = new GameObject("Player").transform;
            playerRoot.position = new Vector3(cutterX, PlayerY, 0f);
            SpriteFactory.CreateQuad("ShoulderSprite", new Color(0.9f, 0.8f, 0.6f), new Vector2(0.5f, 0.5f), playerRoot, Vector3.zero, sortingOrder: 2);
            var bunchVisual = SpriteFactory.CreateQuad("BunchVisual", new Color(0.9f, 0.85f, 0.2f), new Vector2(1.8f, 0.5f), playerRoot, new Vector3(0f, 0.6f, 0f), sortingOrder: 3).transform;

            var targetMarker = SpriteFactory.CreateQuad("TargetMarker", new Color(1f, 0.2f, 0.2f, 0.8f), new Vector2(0.25f, 1.2f), null, new Vector3(cutterX, PlayerY, 0f), sortingOrder: 1);
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
            economy.config = balanceConfig;

            shift.config = balanceConfig;
            shift.placement = placement;
            shift.balance = balance;
            shift.energy = energy;
            shift.economy = economy;
            shift.playerRoot = playerRoot;
            shift.bunchVisual = bunchVisual;
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
            SpriteFactory.CreateQuad("Ground", new Color(0.35f, 0.55f, 0.25f), new Vector2(width, 2f), null, new Vector3(centerX, GroundY - 1f, 0f), sortingOrder: -1);
        }
    }
}
