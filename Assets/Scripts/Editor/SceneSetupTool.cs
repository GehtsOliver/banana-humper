using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BananaHumper.Bootstrap;
using BananaHumper.Gameplay;
using BananaHumper.Util;

namespace BananaHumper.EditorTools
{
    /// <summary>
    /// Erzeugt Assets/Scenes/Main.unity im Hybrid-Aufbau (siehe
    /// docs/DECISIONS.md): Kamera, Kulisse, Spielfigur und die Layout-Anker
    /// fuer Cutter/Trailer/Zielmarkierung liegen danach als echte,
    /// verschiebbare Objekte in der Szene. Die Gameplay-Systeme und die
    /// prozeduralen Formen baut weiterhin GameBootstrap zur Laufzeit.
    ///
    /// Nur ein Editor-Werkzeug, nicht Teil des Laufzeit-Codes: Es laeuft einmal
    /// zum Anlegen der Szene. Danach ist die Szene die Quelle der Wahrheit -
    /// ein erneuter Aufruf ueberschreibt Handarbeit und fragt deshalb nach.
    /// </summary>
    public static class SceneSetupTool
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        const float CutterX = 0f;
        const float TrailerX = 12f;

        [MenuItem("BananaHumper/Bootstrap-Szene erzeugen")]
        public static void CreateMainScene()
        {
            if (!Application.isBatchMode && File.Exists(ScenePath))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Bootstrap-Szene neu erzeugen?",
                    $"{ScenePath} existiert bereits und wird komplett neu aufgebaut.\n\n" +
                    "Alle von Hand in der Szene gemachten Aenderungen (verschobene Objekte, " +
                    "angepasste Inspector-Werte) gehen dabei verloren.",
                    "Neu erzeugen", "Abbrechen");
                if (!overwrite) return;
            }

            BuildScene();
        }

        static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera();
            CreateScenery();

            var playerRoot = CreatePlayer(out var playerAnimator, out var bunchVisual);
            var cutterAnchor = CreateAnchor("Cutter", new Vector3(CutterX - 2.6f, GameBootstrap.GroundY, 0f));
            var trailerAnchor = CreateAnchor("Trailer", new Vector3(TrailerX, GameBootstrap.GroundY, 0f));
            var targetMarker = CreateAnchor("TargetMarker", new Vector3(CutterX, 0f, 0f));

            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GameBootstrap>();
            bootstrap.playerRoot = playerRoot;
            bootstrap.playerAnimator = playerAnimator;
            bootstrap.bunchVisual = bunchVisual;
            bootstrap.cutterAnchor = cutterAnchor;
            bootstrap.trailerAnchor = trailerAnchor;
            bootstrap.targetMarker = targetMarker;

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"SceneSetupTool: {ScenePath} im Hybrid-Aufbau erzeugt.");
        }

        static void CreateCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = Mathf.Max(4f, (TrailerX - CutterX) * 0.55f);
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            camGo.transform.position = new Vector3((CutterX + TrailerX) * 0.5f, 0.5f, -10f);
        }

        static Transform CreateAnchor(string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            return go.transform;
        }

        static Transform CreatePlayer(out PlayerAnimator animator, out BananaBunchVisual bunchVisual)
        {
            var playerRoot = new GameObject("Player").transform;
            playerRoot.position = new Vector3(CutterX, 0f, 0f);

            var idleSprite = SpriteFactory.LoadSprite("Art/Player/idle");
            var bodyRenderer = SpriteFactory.CreateSprite("Body", idleSprite, playerRoot,
                new Vector3(0f, GameBootstrap.GroundY + GameBootstrap.PlayerBodyHeight * 0.5f, 0f),
                sortingOrder: 2, worldHeight: GameBootstrap.PlayerBodyHeight);

            animator = playerRoot.gameObject.AddComponent<PlayerAnimator>();
            animator.target = bodyRenderer;
            animator.idleSprite = idleSprite;
            animator.hurtSprite = SpriteFactory.LoadSprite("Art/Player/hurt");
            animator.walkSprites = new[]
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
            bunchVisual = bunchGo.AddComponent<BananaBunchVisual>();

            return playerRoot;
        }

        /// <summary>
        /// Huegel, Baeume, Wolken, Zaun und Grasbueschel aus Kenney "Background
        /// Elements" (CC0, siehe docs/THIRD_PARTY_ASSETS.md), eingefaerbt auf die
        /// GDD-8.1-Palette. Landet als echte Objekte in der Szene, weil es
        /// importierte Sprite-Assets sind - im Gegensatz zu den prozeduralen
        /// Formen, deren Texturen erst zur Laufzeit entstehen und sich deshalb
        /// nicht in einer Szenendatei speichern lassen.
        /// </summary>
        static void CreateScenery()
        {
            var sceneryRoot = new GameObject("Scenery").transform;

            float groundY = GameBootstrap.GroundY;
            float centerX = (CutterX + TrailerX) * 0.5f;
            float spanWidth = (TrailerX - CutterX) + 16f;

            var hillsFar = SpriteFactory.LoadSprite("Art/Background/hills_far");
            if (hillsFar != null)
            {
                float aspect = hillsFar.rect.width / hillsFar.rect.height;
                float height = spanWidth / aspect;
                SpriteFactory.CreateSprite("HillsFar", hillsFar, sceneryRoot, new Vector3(centerX, groundY + height * 0.5f, 0f), -6,
                    worldWidth: spanWidth, tint: new Color(0.34f, 0.5f, 0.34f));
            }

            var hillsNear = SpriteFactory.LoadSprite("Art/Background/hills_near");
            if (hillsNear != null)
            {
                float width = spanWidth * 0.92f;
                float aspect = hillsNear.rect.width / hillsNear.rect.height;
                float height = width / aspect;
                SpriteFactory.CreateSprite("HillsNear", hillsNear, sceneryRoot, new Vector3(centerX, groundY + height * 0.5f, 0f), -5,
                    worldWidth: width, tint: new Color(0.22f, 0.4f, 0.22f));
            }

            var cloud = SpriteFactory.LoadSprite("Art/Background/cloud");
            if (cloud != null)
            {
                float[] cx = { CutterX - 3f, centerX + 1.5f, TrailerX + 3.5f };
                float[] cy = { groundY + 5.4f, groundY + 6.1f, groundY + 5.6f };
                for (int i = 0; i < cx.Length; i++)
                {
                    SpriteFactory.CreateSprite($"Cloud{i}", cloud, sceneryRoot, new Vector3(cx[i], cy[i], 0f), -6,
                        worldWidth: 3f, tint: new Color(1f, 1f, 1f, 0.85f));
                }
            }

            var treeA = SpriteFactory.LoadSprite("Art/Background/tree_a");
            var treeB = SpriteFactory.LoadSprite("Art/Background/tree_b");
            float[] treeX = { CutterX - 2.2f, centerX - 3.2f, centerX + 2.8f, TrailerX + 2.3f };
            Sprite[] treeSprites = { treeA, treeB, treeA, treeB };
            for (int i = 0; i < treeX.Length; i++)
            {
                var sprite = treeSprites[i % treeSprites.Length];
                if (sprite == null) continue;
                float h = 2.1f + (i % 2) * 0.5f;
                SpriteFactory.CreateSprite($"Tree{i}", sprite, sceneryRoot, new Vector3(treeX[i], groundY + h * 0.5f, 0f), -3,
                    worldHeight: h, tint: new Color(0.16f, 0.34f, 0.16f));
            }

            var fence = SpriteFactory.LoadSprite("Art/Background/fence");
            if (fence != null)
            {
                SpriteFactory.CreateSprite("Fence", fence, sceneryRoot, new Vector3(TrailerX + 1.9f, groundY + 0.4f, 0f), -2,
                    worldHeight: 0.8f, tint: new Color(0.42f, 0.3f, 0.17f));
            }

            var grassTuft = SpriteFactory.LoadSprite("Art/Background/grass_tuft");
            if (grassTuft != null)
            {
                var rng = new System.Random(1234);
                for (int i = 0; i < 14; i++)
                {
                    float x = CutterX - 3f + (float)rng.NextDouble() * spanWidth;
                    float h = 0.3f + (float)rng.NextDouble() * 0.25f;
                    SpriteFactory.CreateSprite($"Grass{i}", grassTuft, sceneryRoot, new Vector3(x, groundY + h * 0.5f, 0f), 0,
                        worldHeight: h, flipX: rng.Next(0, 2) == 0, tint: new Color(0.3f, 0.55f, 0.18f));
                }
            }
        }
    }
}
