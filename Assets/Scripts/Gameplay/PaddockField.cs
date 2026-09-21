using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Der Abschnitt des Paddocks, in dem gerade gearbeitet wird (GDD 3.2, 3.5).
    /// Pflanzen und Steine stehen nicht in einer festen Reihe, sondern in einem
    /// Fenster, das mit dem Trailer weiterwandert: Vorne wird nachgepflanzt,
    /// hinten aufgeraeumt.
    ///
    /// Dadurch ist die Reihe faktisch endlos, ohne dass Hunderte Objekte
    /// existieren muessen - und die Crew arbeitet sich sichtbar durch das Feld,
    /// statt auf einem Fleck zu kreisen.
    /// </summary>
    public class PaddockField : MonoBehaviour
    {
        public BalanceConfig config;
        /// <summary>Wie weit das Fenster in Laufrichtung reicht.</summary>
        public float aheadDistance = 16f;
        /// <summary>Wie weit hinter dem Trailer noch Pflanzen stehen bleiben.</summary>
        public float behindDistance = 8f;

        public List<Plant> Plants { get; } = new List<Plant>();
        public List<Obstacle> Obstacles { get; } = new List<Obstacle>();

        /// <summary>Linker und rechter Rand des begehbaren Abschnitts.</summary>
        public float MinX { get; private set; }
        public float MaxX { get; private set; }

        Transform plantsRoot;
        Transform rocksRoot;
        int day = 1;
        int direction = 1;
        int plantCounter;
        int rockCounter;
        float nextPlantX;
        float nextRockX;

        public void Initialize(BalanceConfig config, int day, int direction, float startX)
        {
            this.config = config;
            this.day = day;
            this.direction = direction >= 0 ? 1 : -1;

            // Altes Feld komplett abraeumen: Initialize laeuft bei jeder Schicht,
            // und ohne das blieben die Pflanzen der Vorschicht stehen und das
            // Feld wuerde sich Schicht um Schicht zuwuchern.
            ClearField();

            plantsRoot = new GameObject("Plants").transform;
            plantsRoot.SetParent(transform, false);
            rocksRoot = new GameObject("Rocks").transform;
            rocksRoot.SetParent(transform, false);

            // Hinter dem Trailer anfangen, damit der erste Abschnitt schon
            // bewachsen ist. "Hinten" haengt an der Laufrichtung - bei einer
            // Schicht nach links liegt es rechts vom Trailer.
            nextPlantX = startX - direction * behindDistance;
            nextRockX = nextPlantX;
            UpdateWindow(startX);
        }

        void ClearField()
        {
            foreach (var plant in Plants)
            {
                if (plant != null) Destroy(plant.gameObject);
            }
            foreach (var rock in Obstacles)
            {
                if (rock != null) Destroy(rock.gameObject);
            }
            Plants.Clear();
            Obstacles.Clear();

            if (plantsRoot != null) Destroy(plantsRoot.gameObject);
            if (rocksRoot != null) Destroy(rocksRoot.gameObject);
        }

        /// <summary>Fenster um <paramref name="centerX"/> nachziehen: vorne saeen, hinten aufraeumen.</summary>
        public void UpdateWindow(float centerX)
        {
            float ahead = centerX + direction * aheadDistance;
            float behind = centerX - direction * behindDistance;
            MinX = Mathf.Min(behind, ahead);
            MaxX = Mathf.Max(behind, ahead);

            GrowAhead(ahead);
            RemoveBehind();
        }

        void GrowAhead(float ahead)
        {
            // Solange die vorderste Pflanze noch nicht weit genug im Feld steht,
            // die naechste setzen - Abstand zufaellig, damit kein Raster entsteht.
            while (direction > 0 ? nextPlantX < ahead : nextPlantX > ahead)
            {
                SpawnPlant(nextPlantX);
                nextPlantX += direction * Random.Range(config.plantSpacingMin, config.plantSpacingMax);
            }

            while (direction > 0 ? nextRockX < ahead : nextRockX > ahead)
            {
                // Steine deutlich duenner gesaet als Pflanzen (GDD 3.9).
                if (Random.value < 0.5f) SpawnRock(nextRockX);
                nextRockX += direction * Random.Range(6f, 11f);
            }
        }

        void RemoveBehind()
        {
            for (int i = Plants.Count - 1; i >= 0; i--)
            {
                var plant = Plants[i];
                if (plant == null) { Plants.RemoveAt(i); continue; }
                if (plant.PositionX >= MinX && plant.PositionX <= MaxX) continue;
                // Ein Cutter, der gerade hierher unterwegs ist, sucht sich beim
                // naechsten Tick von selbst ein neues Ziel.
                Plants.RemoveAt(i);
                Destroy(plant.gameObject);
            }

            for (int i = Obstacles.Count - 1; i >= 0; i--)
            {
                var rock = Obstacles[i];
                if (rock == null) { Obstacles.RemoveAt(i); continue; }
                if (rock.PositionX >= MinX && rock.PositionX <= MaxX) continue;
                Obstacles.RemoveAt(i);
                Destroy(rock.gameObject);
            }
        }

        void SpawnPlant(float x)
        {
            var go = new GameObject($"Plant{plantCounter++}");
            go.transform.SetParent(plantsRoot, false);
            go.transform.position = new Vector3(x, Bootstrap.GameBootstrap.GroundY, 0f);

            var anchor = new GameObject("BunchAnchor").transform;
            anchor.SetParent(go.transform, false);
            anchor.localPosition = new Vector3(0f, Bootstrap.GameBootstrap.PlantBunchHeight, 0f);

            var plant = go.AddComponent<Plant>();
            plant.bunchAnchor = anchor;
            plant.bunchVisual = anchor.gameObject.AddComponent<BananaBunchVisual>();
            PaddockVisuals.BuildPlant(go.transform);
            plant.Initialize(day, config.regrowSeconds);

            Plants.Add(plant);
        }

        void SpawnRock(float x)
        {
            // Nicht direkt unter eine Pflanze, sonst liegt ein Stein in der
            // Fallstelle und macht das Fangen unfair. Die Wunschstelle liegt oft
            // zu nah dran, deshalb ein Stueck in beide Richtungen nach einer
            // Luecke suchen, statt den Stein einfach ausfallen zu lassen.
            if (!TryFindGap(ref x)) return;

            var go = new GameObject($"Rock{rockCounter++}");
            go.transform.SetParent(rocksRoot, false);
            go.transform.position = new Vector3(x, Bootstrap.GameBootstrap.GroundY, 0f);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.halfWidth = Random.Range(0.28f, 0.38f);
            obstacle.clearHeight = Random.Range(0.40f, 0.60f);
            PaddockVisuals.BuildRock(obstacle);

            Obstacles.Add(obstacle);
        }

        /// <summary>Sucht ab <paramref name="x"/> die naechste Stelle mit genug Abstand zu allen Pflanzen.</summary>
        bool TryFindGap(ref float x)
        {
            const float Step = 0.4f;
            for (int i = 0; i < 12; i++)
            {
                foreach (int side in new[] { 1, -1 })
                {
                    float candidate = x + side * Step * i;
                    if (!IsClearOfPlants(candidate)) continue;
                    x = candidate;
                    return true;
                }
            }
            return false;
        }

        bool IsClearOfPlants(float x)
        {
            foreach (var plant in Plants)
            {
                if (plant != null && Mathf.Abs(plant.PositionX - x) < config.rockMinDistance) return false;
            }
            return true;
        }

        /// <summary>Wie viele reife Stauden noch im aktuellen Abschnitt haengen (GDD 3.5).</summary>
        public int CountRipeNear(float x, float radius)
        {
            int count = 0;
            foreach (var plant in Plants)
            {
                if (plant != null && plant.HasBunch && Mathf.Abs(plant.PositionX - x) <= radius) count++;
            }
            return count;
        }

        public void Tick(float dt)
        {
            foreach (var plant in Plants)
            {
                if (plant != null) plant.Tick(dt);
            }
        }
    }
}
