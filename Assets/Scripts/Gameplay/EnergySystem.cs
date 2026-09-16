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

        public void ConsumeWalkBack(float dt)
        {
            Spend(config.walkBackEnergyPerSecond * dt);
        }

        public void ApplyFallPenalty() => Spend(config.fallEnergyPenalty);
        public void ApplySnapPenalty() => Spend(config.snapEnergyPenalty);
        public void ApplyRepositionCost() => Spend(config.repositionEnergyCost);

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
