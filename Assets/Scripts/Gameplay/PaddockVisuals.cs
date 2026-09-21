using UnityEngine;
using BananaHumper.Util;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Prozedurale Formen fuer die Dinge, die das Paddock zur Laufzeit
    /// nachwachsen laesst (Pflanzen, Steine). Liegt getrennt von GameBootstrap,
    /// weil PaddockField sie laufend braucht und nicht nur beim Start.
    ///
    /// Alles prozedural, weil es dafuer kein passendes freies CC0-Asset gibt
    /// (siehe docs/THIRD_PARTY_ASSETS.md).
    /// </summary>
    public static class PaddockVisuals
    {
        /// <summary>
        /// Bananenpflanze: Scheinstamm mit ein paar grossen Blattwedeln.
        /// Bewusst schlank und leicht transparent, damit sie die haengende
        /// Staude und den Cutter davor nicht verdeckt.
        /// </summary>
        public static void BuildPlant(Transform root)
        {
            var trunkColor = new Color(0.36f, 0.40f, 0.22f);
            var leafColor = new Color(0.22f, 0.45f, 0.20f, 0.9f);
            float height = Bootstrap.GameBootstrap.PlantBunchHeight + 0.5f;

            var shape = new GameObject("PlantShape").transform;
            shape.SetParent(root, false);

            SpriteFactory.CreateRoundedQuad("Trunk", trunkColor, new Vector2(0.28f, height), 0.3f,
                shape, new Vector3(0f, height * 0.5f, 0f), -1);

            float[] angles = { 35f, -35f, 70f, -70f };
            for (int i = 0; i < angles.Length; i++)
            {
                var leaf = new GameObject($"Leaf{i}").transform;
                leaf.SetParent(shape, false);
                leaf.localPosition = new Vector3(0f, height - 0.05f, 0f);
                leaf.localRotation = Quaternion.Euler(0f, 0f, angles[i]);
                SpriteFactory.CreateEllipse("Blade", leafColor, new Vector2(0.22f, 1.5f), leaf,
                    new Vector3(0f, 0.7f, 0f), -2);
            }
        }

        /// <summary>
        /// Stein (GDD 3.9). Die Form richtet sich exakt nach halfWidth und
        /// clearHeight, damit der sichtbare Stein das ist, woran man
        /// haengenbleibt - keine unsichtbaren Raender.
        /// </summary>
        public static void BuildRock(Obstacle obstacle)
        {
            var stoneColor = new Color(0.45f, 0.44f, 0.42f);
            var shadeColor = new Color(0.33f, 0.32f, 0.31f);

            var shape = new GameObject("RockShape").transform;
            shape.SetParent(obstacle.transform, false);

            float width = obstacle.halfWidth * 2f;
            float height = obstacle.clearHeight;
            SpriteFactory.CreateEllipse("Stone", stoneColor, new Vector2(width, height),
                shape, new Vector3(0f, height * 0.5f, 0f), 1);
            SpriteFactory.CreateEllipse("Shade", shadeColor, new Vector2(width * 0.45f, height * 0.35f),
                shape, new Vector3(-width * 0.14f, height * 0.6f, 0f), 2);
        }
    }
}
