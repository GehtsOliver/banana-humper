using System.Collections.Generic;
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
    /// docs/DECISIONS.md): Kamera, Kulisse, Spielfigur, Trailer und die
    /// Cutter-Stationen liegen danach als echte, verschiebbare Objekte in der
    /// Szene. Die Stationsabstaende sind das Level-Design des Kern-Loops
    /// (GDD 3.2) und genau deshalb im Editor einstellbar. Die Gameplay-Systeme
    /// und die prozeduralen Formen baut weiterhin GameBootstrap zur Laufzeit.
    ///
    /// Nur ein Editor-Werkzeug, nicht Teil des Laufzeit-Codes: Es laeuft einmal
    /// zum Anlegen der Szene. Danach ist die Szene die Quelle der Wahrheit -
    /// ein erneuter Aufruf ueberschreibt Handarbeit und fragt deshalb nach.
    /// </summary>
    public static class SceneSetupTool
    {
        const string ScenePath = "Assets/Scenes/Main.unity";

        const float RowMinX = -4f;
        const float RowMaxX = 14f;
        const float TrailerX = 12f;
        const float BunchHangHeight = 2.6f;

        /// <summary>Startaufstellung der Stationen (GDD 3.2: 4 Stueck, unterschiedlich weit auseinander).</summary>
        static readonly float[] StationX = { -2.5f, 1.5f, 5.5f, 10f };

        /// <summary>
        /// Steine (GDD 3.9), bewusst zwischen den Stationen statt darauf: Sie
        /// sollen die Laufwege stoeren, nicht das Fangen unmoeglich machen.
        /// Unterschiedliche Groessen, damit nicht jeder Sprung gleich aussieht.
        /// </summary>
        static readonly (float x, float halfWidth, float clearHeight)[] Rocks =
        {
            (-0.6f, 0.30f, 0.45f),
            (3.4f, 0.38f, 0.60f),
            (7.9f, 0.28f, 0.40f),
            (11.6f, 0.34f, 0.55f),
        };

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

            var player = CreatePlayer();
            var trailer = CreateTrailer();

            var stationsRoot = new GameObject("Stations").transform;
            var stations = new List<CutterStation>();
            for (int i = 0; i < StationX.Length; i++)
            {
                stations.Add(CreateStation(stationsRoot, i, StationX[i]));
            }

            var obstaclesRoot = new GameObject("Obstacles").transform;
            var obstacles = new List<Obstacle>();
            for (int i = 0; i < Rocks.Length; i++)
            {
                obstacles.Add(CreateRock(obstaclesRoot, i, Rocks[i]));
            }

            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GameBootstrap>();
            bootstrap.player = player;
            bootstrap.trailer = trailer;
            bootstrap.stations = stations;
            bootstrap.obstacles = obstacles;
            bootstrap.rowMinX = RowMinX;
            bootstrap.rowMaxX = RowMaxX;

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
            cam.orthographicSize = Mathf.Max(4f, (RowMaxX - RowMinX) * 0.34f);
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            camGo.transform.position = new Vector3((RowMinX + RowMaxX) * 0.5f, 0.9f, -10f);
        }

        /// <summary>
        /// Eine Cutter-Station: Anker in der Reihe, darueber die haengende
        /// Staude und der Schnitt-Balken. Die prozedurale Cutter-Figur baut
        /// GameBootstrap zur Laufzeit darunter, ihre Textur waere in einer
        /// Szenendatei nicht speicherbar.
        /// </summary>
        static CutterStation CreateStation(Transform parent, int index, float x)
        {
            var go = new GameObject($"Station{index}");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, GameBootstrap.GroundY, 0f);

            var bunchAnchor = new GameObject("HangingBunch").transform;
            bunchAnchor.SetParent(go.transform, false);
            bunchAnchor.localPosition = new Vector3(0f, BunchHangHeight, 0f);
            bunchAnchor.gameObject.AddComponent<BananaBunchVisual>();

            var barGo = new GameObject("CutBar");
            barGo.transform.SetParent(go.transform, false);
            barGo.transform.localPosition = new Vector3(0f, BunchHangHeight + 0.9f, 0f);
            var bar = barGo.AddComponent<ProgressBarVisual>();

            var station = go.AddComponent<CutterStation>();
            station.bunchAnchor = bunchAnchor;
            station.bar = bar;
            return station;
        }

        /// <summary>
        /// Ein Stein als verschiebbares Szenen-Objekt. Die Form baut
        /// GameBootstrap zur Laufzeit aus halfWidth/clearHeight - was man sieht,
        /// ist damit genau das, woran man haengenbleibt.
        /// </summary>
        static Obstacle CreateRock(Transform parent, int index, (float x, float halfWidth, float clearHeight) spec)
        {
            var go = new GameObject($"Rock{index}");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(spec.x, GameBootstrap.GroundY, 0f);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.halfWidth = spec.halfWidth;
            obstacle.clearHeight = spec.clearHeight;
            return obstacle;
        }

        static TrailerController CreateTrailer()
        {
            var go = new GameObject("Trailer");
            go.transform.position = new Vector3(TrailerX, GameBootstrap.GroundY, 0f);
            var trailer = go.AddComponent<TrailerController>();
            trailer.minX = RowMinX;
            trailer.maxX = RowMaxX;
            return trailer;
        }

        static PlayerController CreatePlayer()
        {
            var playerRoot = new GameObject("Player").transform;
            playerRoot.position = new Vector3(0f, 0f, 0f);

            var idleSprite = SpriteFactory.LoadSprite("Art/Player/idle");
            var bodyRenderer = SpriteFactory.CreateSprite("Body", idleSprite, playerRoot,
                new Vector3(0f, GameBootstrap.GroundY + GameBootstrap.PlayerBodyHeight * 0.5f, 0f),
                sortingOrder: 2, worldHeight: GameBootstrap.PlayerBodyHeight);

            var animator = playerRoot.gameObject.AddComponent<PlayerAnimator>();
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
            var bunchVisual = bunchGo.AddComponent<BananaBunchVisual>();

            var player = playerRoot.gameObject.AddComponent<PlayerController>();
            player.animator = animator;
            player.shoulderBunch = bunchVisual;
            player.minX = RowMinX;
            player.maxX = RowMaxX;
            return player;
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
            float centerX = (RowMinX + RowMaxX) * 0.5f;
            float spanWidth = (RowMaxX - RowMinX) + 16f;

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
                float[] cx = { RowMinX - 3f, centerX + 1.5f, RowMaxX + 1.5f };
                float[] cy = { groundY + 5.4f, groundY + 6.1f, groundY + 5.6f };
                for (int i = 0; i < cx.Length; i++)
                {
                    SpriteFactory.CreateSprite($"Cloud{i}", cloud, sceneryRoot, new Vector3(cx[i], cy[i], 0f), -6,
                        worldWidth: 3f, tint: new Color(1f, 1f, 1f, 0.85f));
                }
            }

            var treeA = SpriteFactory.LoadSprite("Art/Background/tree_a");
            var treeB = SpriteFactory.LoadSprite("Art/Background/tree_b");
            float[] treeX = { RowMinX - 2.2f, centerX - 3.2f, centerX + 2.8f, RowMaxX + 0.3f };
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
                SpriteFactory.CreateSprite("Fence", fence, sceneryRoot, new Vector3(RowMaxX - 0.1f, groundY + 0.4f, 0f), -2,
                    worldHeight: 0.8f, tint: new Color(0.42f, 0.3f, 0.17f));
            }

            var grassTuft = SpriteFactory.LoadSprite("Art/Background/grass_tuft");
            if (grassTuft != null)
            {
                var rng = new System.Random(1234);
                for (int i = 0; i < 14; i++)
                {
                    float x = RowMinX - 3f + (float)rng.NextDouble() * spanWidth;
                    float h = 0.3f + (float)rng.NextDouble() * 0.25f;
                    SpriteFactory.CreateSprite($"Grass{i}", grassTuft, sceneryRoot, new Vector3(x, groundY + h * 0.5f, 0f), 0,
                        worldHeight: h, flipX: rng.Next(0, 2) == 0, tint: new Color(0.3f, 0.55f, 0.18f));
                }
            }
        }
    }
}
