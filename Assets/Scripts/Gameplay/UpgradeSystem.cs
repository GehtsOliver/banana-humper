using System;
using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    public enum UpgradeId
    {
        // Attribute (Erfahrung, GDD 5.1)
        MaxEnergy,
        Strength,
        Stamina,
        Legs,
        // Ausruestung (Geld, GDD 5.2)
        HireCutter,
        ShoulderPad,
        RubberBoots,
        Coffee
    }

    /// <summary>Zwei getrennte Waehrungen, zwei getrennte Listen (GDD 5).</summary>
    public enum UpgradeCurrency
    {
        Experience,
        Money
    }

    public class UpgradeDefinition
    {
        public UpgradeId Id;
        public UpgradeCurrency Currency;
        public string Name;
        public string Effect;
        public int MaxLevel;
        public float BaseCost;
    }

    /// <summary>
    /// Fortschritt in zwei Spuren (GDD 5): **Erfahrung** verbessert den Koerper
    /// (Attribute), **Geld** kauft Ausruestung. Beide sind flache Listen ohne
    /// Abhaengigkeiten - kein Skill Tree [E].
    ///
    /// Die Attribute greifen genau dort, wo ihr Name es verspricht:
    /// Staerke senkt die Schlepp-Kosten, Ausdauer die Lauf-Kosten.
    ///
    /// Wichtig: Stufen aendern **nicht** das BalanceConfig-Asset. Es gibt eine
    /// Laufzeitkopie, die bei jedem Kauf frisch aus den Basiswerten abgeleitet
    /// und dann mit allen Stufen ueberschrieben wird. Ohne das wuerde ein Kauf
    /// im Editor die Asset-Datei dauerhaft veraendern, und die Effekte wuerden
    /// sich bei jedem Kauf erneut aufaddieren (GDD 10.2, "Stats-Prinzip").
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        public static readonly UpgradeDefinition[] Catalogue =
        {
            new UpgradeDefinition
            {
                Id = UpgradeId.MaxEnergy, Currency = UpgradeCurrency.Experience, Name = "Energie",
                Effect = "+20 maximale Energie", MaxLevel = 10, BaseCost = 12f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.Strength, Currency = UpgradeCurrency.Experience, Name = "Staerke",
                Effect = "Energieverbrauch beim Schleppen -8 %", MaxLevel = 10, BaseCost = 10f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.Stamina, Currency = UpgradeCurrency.Experience, Name = "Ausdauer",
                Effect = "Energieverbrauch beim Laufen und Rennen -8 %", MaxLevel = 10, BaseCost = 10f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.Legs, Currency = UpgradeCurrency.Experience, Name = "Laufgeschwindigkeit",
                Effect = "+6 % Tempo", MaxLevel = 8, BaseCost = 18f,
            },

            new UpgradeDefinition
            {
                Id = UpgradeId.HireCutter, Currency = UpgradeCurrency.Money, Name = "Cutter anheuern",
                Effect = "Ein Cutter mehr, das Feld wird groesser", MaxLevel = 4, BaseCost = 60f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.ShoulderPad, Currency = UpgradeCurrency.Money, Name = "Schulterpad",
                Effect = "Fangradius +12 %", MaxLevel = 5, BaseCost = 40f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.RubberBoots, Currency = UpgradeCurrency.Money, Name = "Gummistiefel",
                Effect = "Stolpern kostet -30 % Energie und bremst kuerzer", MaxLevel = 3, BaseCost = 80f,
            },
            new UpgradeDefinition
            {
                Id = UpgradeId.Coffee, Currency = UpgradeCurrency.Money, Name = "Instant-Kaffee",
                Effect = "Schichtstart mit +15 Energie ueber dem Maximum", MaxLevel = 3, BaseCost = 45f,
            },
        };

        public event Action OnUpgradesChanged;

        BalanceConfig baseConfig;
        EnergySystem energy;
        List<Cutter> cutters;
        readonly Dictionary<UpgradeId, int> levels = new Dictionary<UpgradeId, int>();

        /// <summary>Die Werte, mit denen tatsaechlich gespielt wird - nie das Asset selbst.</summary>
        public BalanceConfig RuntimeConfig { get; private set; }

        /// <summary>Energie ueber dem Maximum zum Schichtstart (Instant-Kaffee).</summary>
        public float StartEnergyBonus => 15f * LevelOf(UpgradeId.Coffee);
        /// <summary>Faktor auf die Stolper-Strafe (Gummistiefel).</summary>
        public float StumbleFactor => Mathf.Pow(0.7f, LevelOf(UpgradeId.RubberBoots));

        public void Initialize(BalanceConfig baseConfig, EnergySystem energy, List<Cutter> cutters)
        {
            this.baseConfig = baseConfig;
            this.energy = energy;
            this.cutters = cutters;

            RuntimeConfig = ScriptableObject.CreateInstance<BalanceConfig>();
            foreach (var definition in Catalogue) levels[definition.Id] = 0;

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
            if (id == UpgradeId.HireCutter && NextUnhiredCutter() == null) return true;
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
            if (IsMaxed(id)) return false;
            var definition = Definition(id);
            double available = definition.Currency == UpgradeCurrency.Money ? economy.Money : economy.Experience;
            return available >= CostOf(id);
        }

        public bool TryBuy(UpgradeId id, EconomySystem economy)
        {
            if (!CanAfford(id, economy)) return false;

            var definition = Definition(id);
            bool paid = definition.Currency == UpgradeCurrency.Money
                ? economy.TrySpend(CostOf(id))
                : economy.TrySpendExperience(CostOf(id));
            if (!paid) return false;

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
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(baseConfig), RuntimeConfig);

            // Attribute (Erfahrung)
            RuntimeConfig.startEnergy += 20f * LevelOf(UpgradeId.MaxEnergy);
            RuntimeConfig.walkSpeed *= Mathf.Pow(1.06f, LevelOf(UpgradeId.Legs));

            // Ausruestung (Geld)
            RuntimeConfig.catchRadius *= Mathf.Pow(1.12f, LevelOf(UpgradeId.ShoulderPad));

            // Staerke und Ausdauer wirken nicht auf Config-Werte, sondern als
            // Faktoren im EnergySystem - dort sitzt die Unterscheidung zwischen
            // Lauf- und Schlepp-Kosten.
            if (energy != null)
            {
                energy.StrengthFactor = Mathf.Pow(0.92f, LevelOf(UpgradeId.Strength));
                energy.StaminaFactor = Mathf.Pow(0.92f, LevelOf(UpgradeId.Stamina));
            }
        }
    }
}
