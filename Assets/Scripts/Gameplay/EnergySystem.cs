using System;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>Energy budget for one Schicht (GDD 4.2).</summary>
    public class EnergySystem : MonoBehaviour
    {
        public BalanceConfig config;

        public event Action OnEnergyDepleted;

        public float Current { get; private set; }
        public float Max { get; private set; }

        bool depletedFired;

        public void StartShift(float maxOverride = -1f)
        {
            Max = maxOverride > 0f ? maxOverride : config.startEnergy;
            Current = Max;
            depletedFired = false;
        }

        public void ConsumeCarrying(float weight, bool isRunning, float dt)
        {
            float perSecond = config.energyPerSecondBase + weight * config.energyPerSecondPerWeight;
            if (isRunning) perSecond *= config.runEnergyMultiplier;
            Spend(perSecond * dt);
        }

        // Laufen ohne Staude kostet seit v0.9 nichts (GDD 4.2): Energie ist ein
        // Budget aus getragenen Kilogramm mal Weg. Genau das macht schwere
        // Stauden zur Abwaegung statt zur automatisch besseren Wahl.
        public void ApplyDropPenalty() => Spend(config.dropEnergyPenalty);
        public void ApplyRepositionCost() => Spend(config.repositionEnergyCost);
        public void ApplyStumbleCost() => Spend(config.stumbleEnergyCost);

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
