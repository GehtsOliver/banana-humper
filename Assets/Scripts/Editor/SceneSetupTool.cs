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

        const float RowMinX = -6f;
        const float RowMaxX = 26f;  // Szenen-Ausdehnung inkl. aller anheuerbaren Cutter; aktiv ist nur bis zum letzten angeheuerten
        const float TrailerX = 5f;   // innerhalb der Reihe, die mit nur zwei Cuttern noch kurz ist
        const float BunchHangHeight = 2.6f;

        /// <summary>
        /// Sechs Stationsplaetze, aber nur die ersten zwei sind angeheuert (GDD
        /// 3.2, 5.2). Die restlichen stehen schon in der Szene und werden
        /// spaeter im Shop freigeschaltet - das Paddock waechst also mit der
        /// Mannschaft, weil GameBootstrap die Reihe am letzten angeheuerten
        /// Cutter enden laesst. Temperamente sind gemischt, damit es von Anfang
        /// an etwas zu priorisieren gibt.
        /// </summary>
        static readonly (float x, CutterTemperament temperament, bool hired)[] Stations =
        {
            (-2.0f, CutterTemperament.Normal, true),
            (3.0f, CutterTemperament.Ungeduldig, true),
            (8.0f, CutterTemperament.Geduldig, false),
            (13.0f, CutterTemperament.Normal, false),
            (18.0f, CutterTemperament.Ungeduldig, false),
            (23.0f, CutterTemperament.Geduldig, false),
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

            var cameraController = CreateCamera();
            CreateScenery();

            var player = CreatePlayer();
            var trailer = CreateTrailer();

            var stationsRoot = new GameObject("Stations").transform;
            var stations = new List<CutterStation>();
            for (int i = 0; i < Stations.Length; i++)
            {
                stations.Add(CreateStation(stationsRoot, i, Stations[i].x, Stations[i].temperament, Stations[i].hired));
            }

            // Steine liegen nicht mehr in der Szene: Sie werden pro Schicht
            // zufaellig gesetzt (GDD 3.9), damit die Wege sich unterscheiden.

            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GameBootstrap>();
            bootstrap.player = player;
            bootstrap.trailer = trailer;
            bootstrap.cameraController = cameraController;
            bootstrap.stations = stations;
            bootstrap.rowMinX = RowMinX;

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log($"SceneSetupTool: {ScenePath} im Hybrid-Aufbau erzeugt.");
        }

        /// <summary>
        /// Feste Zoomstufe statt "ganze Reihe ins Bild": Die Kamera faehrt jetzt
        /// mit (CameraController), damit das Paddock mit jedem angeheuerten
        /// Cutter wachsen kann. Stationen ausserhalb des Bildes zeigt das HUD am
        /// Rand an, sonst gaebe es nichts mehr zu priorisieren (GDD 12).
        /// Solange nur wenige Cutter angeheuert sind, zentriert der Controller
        /// von selbst - man sieht dann ohnehin alles.
        /// </summary>
        static CameraController CreateCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.9f);
            camGo.transform.position = new Vector3(0f, 1.4f, -10f);

            var controller = camGo.AddComponent<CameraController>();
            controller.minX = RowMinX;
            controller.maxX = RowMaxX;
            return controller;
        }

        /// <summary>
        /// Eine Cutter-Station: Anker in der Reihe, darueber die haengende
        /// Staude und der Schnitt-Balken. Die prozedurale Cutter-Figur baut
        /// GameBootstrap zur Laufzeit darunter, ihre Textur waere in einer
        /// Szenendatei nicht speicherbar.
        /// </summary>
        static CutterStation CreateStation(Transform parent, int index, float x, CutterTemperament temperament, bool hired)
        {
            var go = new GameObject($"Station{index}_{temperament}{(hired ? "" : "_NichtAngeheuert")}");
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
            station.temperament = temperament;
            station.isHired = hired;
            return station;
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
