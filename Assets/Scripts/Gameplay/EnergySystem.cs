using System;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Energiebudget einer Schicht (GDD 4.2). Alles kostet Energie, gestaffelt
    /// nach Anstrengung:
    ///
    ///   Dasein  &lt;  Laufen  &lt;  Rennen  &lt;  Schleppen
    ///
    /// Die Lauf-Anteile sinken mit der **Ausdauer**, der Schlepp-Anteil mit der
    /// **Staerke** (GDD 5.1) - beide Attribute greifen also an genau der
    /// Stelle, die ihr Name verspricht.
    /// </summary>
    public class EnergySystem : MonoBehaviour
    {
        public BalanceConfig config;

        public event Action OnEnergyDepleted;

        public float Current { get; private set; }
        public float Max { get; private set; }

        /// <summary>Faktor auf alle Lauf-Kosten, kommt aus der Ausdauer (1 = ungeuebt).</summary>
        public float StaminaFactor { get; set; } = 1f;
        /// <summary>Faktor auf alle Schlepp-Kosten, kommt aus der Staerke.</summary>
        public float StrengthFactor { get; set; } = 1f;

        bool depletedFired;

        public void StartShift(float maxOverride = -1f)
        {
            Max = maxOverride > 0f ? maxOverride : config.startEnergy;
            Current = Max;
            depletedFired = false;
        }

        /// <summary>Verbrauch eines Frames aus dem aktuellen Zustand der Figur.</summary>
        public void ConsumeTick(bool isMoving, bool isRunning, bool isCarrying, float weight, float dt)
        {
            float perSecond = config.energyIdlePerSecond;

            if (isMoving)
            {
                float movement = config.energyWalkPerSecond;
                if (isRunning) movement *= config.runEnergyMultiplier;
                perSecond += movement * StaminaFactor;
            }

            if (isCarrying)
            {
                float carry = config.energyCarryPerSecond + weight * config.energyCarryPerWeight;
                perSecond += carry * StrengthFactor;
            }

            Spend(perSecond * dt);
        }

        public void ApplyDropPenalty() => Spend(config.dropEnergyPenalty);
        public void ApplyRepositionCost() => Spend(config.repositionEnergyCost);
        public void ApplyStumbleCost(float factor = 1f) => Spend(config.stumbleEnergyCost * factor);

        void Spend(float amount)
        {
            if (amount <= 0f) return;
            Current = Mathf.Max(0f, Current - amount);
            if (Current <= 0f && !depletedFired)
            {
                depletedFired = true;
                OnEnergyDepleted?.Invoke();
            }
        }

        public bool IsDepleted => Current <= 0f;
    }
}
