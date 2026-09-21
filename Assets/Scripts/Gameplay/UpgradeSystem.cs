using System;
using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    public enum UpgradeId
    {
        HireCutter,
        ShoulderPad,
        Boots,
        CarryStrap,
        Coffee
    }

    public class UpgradeDefinition
    {
        public UpgradeId Id;
        public string Name;
        public string Effect;
        public int MaxLevel;
        public float BaseCost;
    }

    /// <summary>
    /// Shop-Stufen und ihre Wirkung (GDD 5.2). Gekauft wird mit Geld, die Liste
    /// ist flach ohne Abhaengigkeiten - kein Skill Tree [E].
    ///
    /// Wichtig fuers Verstaendnis: Die Stufen aendern **nicht** das
    /// BalanceConfig-Asset. Stattdessen gibt es eine Laufzeitkopie, die bei
    /// jedem Kauf frisch aus den Basiswerten abgeleitet und dann mit allen
    /// Stufen ueberschrieben wird. Ohne das wuerde ein Kauf im Editor die
    /// Asset-Datei dauerhaft veraendern, und die Effekte wuerden sich bei
    /// jedem Kauf erneut aufaddieren (GDD 10.2, "Stats-Prinzip").
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static readonly UpgradeDefinition[] Catalogue =
        {
            new UpgradeDefinition
            {
                Id = UpgradeId.HireCutter, Name = "Cutter anheuern",
                Effect = "Ein Cutter mehr im Paddock, das Feld wird groesser",
                MaxLevel = 4, BaseCost = 60f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.ShoulderPad, Name = "Schulterpad",
                Effect = "Fangradius +12 % - Stauden lassen sich unsauberer fangen",
                MaxLevel = 5, BaseCost = 40f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.Boots, Name = "Gute Stiefel",
                Effect = "Laufgeschwindigkeit +8 %",
                MaxLevel = 5, BaseCost = 50f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.CarryStrap, Name = "Tragegurt",
                Effect = "Energieverbrauch beim Schleppen -10 %",
                MaxLevel = 5, BaseCost = 70f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.Coffee, Name = "Instant-Kaffee",
                Effect = "+15 Startenergie",
                MaxLevel = 5, BaseCost = 45f,
            },
        };

        public event Action OnUpgradesChanged;

        BalanceConfig baseConfig;
        BalanceConfig runtimeConfig;
        List<Cutter> cutters;
        readonly Dictionary<UpgradeId, int> levels = new Dictionary<UpgradeId, int>();

        /// <summary>Die Werte, mit denen tatsaechlich gespielt wird - nie das Asset selbst.</summary>
        public BalanceConfig RuntimeConfig => runtimeConfig;

        public void Initialize(BalanceConfig baseConfig, List<Cutter> cutters)
        {
            this.baseConfig = baseConfig;
            this.cutters = cutters;

            runtimeConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            foreach (var definition in Catalogue) levels[definition.Id] = 0;

            // Bereits in der Szene angeheuerte Cutter zaehlen nicht als gekaufte
            // Stufe - sie sind die Startmannschaft.
            ApplyAll();
        }

        public int LevelOf(UpgradeId id) => levels.TryGetValue(id, out int level) ? level : 0;

        public static UpgradeDefinition Definition(UpgradeId id)
        {
            foreach (var definition in Catalogue)
            {
                if (definition.Id == id) return definition;
            }
            return null;
        }

        public bool IsMaxed(UpgradeId id)
        {
            var definition = Definition(id);
            if (definition == null) return true;
            if (id == UpgradeId.HireCutter) return NextUnhiredCutter() == null || LevelOf(id) >= definition.MaxLevel;
            return LevelOf(id) >= definition.MaxLevel;
        }

        /// <summary>GDD 4.5: kosten(stufe) = basiskosten * 1,6^stufe.</summary>
        public int CostOf(UpgradeId id)
        {
            var definition = Definition(id);
            if (definition == null) return int.MaxValue;
            return Mathf.RoundToInt(definition.BaseCost * Mathf.Pow(1.6f, LevelOf(id)));
        }

        public bool CanAfford(UpgradeId id, EconomySystem economy)
        {
            return !IsMaxed(id) && economy.Money >= CostOf(id);
        }

        public bool TryBuy(UpgradeId id, EconomySystem economy)
        {
            if (!CanAfford(id, economy)) return false;
            if (!economy.TrySpend(CostOf(id))) return false;

            levels[id] = LevelOf(id) + 1;
            if (id == UpgradeId.HireCutter) HireNextCutter();

            ApplyAll();
            OnUpgradesChanged?.Invoke();
            return true;
        }

        Cutter NextUnhiredCutter()
        {
            foreach (var cutter in cutters)
            {
                if (cutter != null && !cutter.isHired) return cutter;
            }
            return null;
        }

        void HireNextCutter()
        {
            var cutter = NextUnhiredCutter();
            if (cutter == null) return;

            cutter.isHired = true;
            cutter.gameObject.SetActive(true);
            // Figur erst jetzt bauen: Vor dem Anheuern soll da nichts stehen.
            PaddockVisuals.BuildCutter(cutter.transform);
        }

        /// <summary>
        /// Laufzeitwerte komplett neu aus den Basiswerten ableiten und dann alle
        /// Stufen anwenden. Bewusst nicht inkrementell - sonst haengt das
        /// Ergebnis von der Kaufreihenfolge ab und driftet mit jedem Kauf.
        /// </summary>
        void ApplyAll()
        {
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(baseConfig), runtimeConfig);

            runtimeConfig.catchRadius *= Mathf.Pow(1.12f, LevelOf(UpgradeId.ShoulderPad));
            runtimeConfig.walkSpeed *= Mathf.Pow(1.08f, LevelOf(UpgradeId.Boots));

            float strap = Mathf.Pow(0.9f, LevelOf(UpgradeId.CarryStrap));
            runtimeConfig.energyPerSecondBase *= strap;
            runtimeConfig.energyPerSecondPerWeight *= strap;

            runtimeConfig.startEnergy += 15f * LevelOf(UpgradeId.Coffee);
        }
    }
}
