using UnityEngine;
using BananaHumper.Gameplay;

namespace BananaHumper.Config
{
    /// <summary>
    /// Tuning values for the pendulum/stress simulation (GDD Kapitel 3.6).
    /// One asset instance acts as the single source of truth so the mechanic
    /// can be tuned in the Inspector without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "BananaHumper/Balance Config")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("Pendel-Physik (3.4)")]
        public float gravityOverLength = 4.0f;
        public float offsetTorque = 1.5f;
        public float damping = 1.2f;
        public float controlStrength = 0.015f;
        public float wobbleWalk = 0.6f;
        public float wobbleRun = 1.5f;
        public float stepFrequency = 4.0f;
        [Range(0f, 1f)] public float wobbleNoise = 0.15f;

        [Header("Winkel (Grad)")]
        public float maxAngleDeg = 35f;
        public float comfortAngleDeg = 8f;
        [Range(0f, 1f)] public float redWarningFraction = 0.7f;

        [Header("Belastung / Snap (3.5)")]
        public float haltekraftThreshold = 0.15f;
        public float stressRate = 60f;
        public float recoveryRate = 15f;
        public float stressCreakWarning = 60f;
        public float stressBendWarning = 80f;

        [Header("Längenfaktor (3.5)")]
        public float lengthFactorShort = 0.3f;
        public float lengthFactorMedium = 1.0f;
        public float lengthFactorLong = 1.8f;

        [Header("Umsetzen (3.5)")]
        public float repositionDuration = 0.6f;
        [Range(0f, 1f)] public float repositionOffsetReduction = 0.6f;
        public float repositionEnergyCost = 5f;
        public float repositionWobbleImpulse = 0.4f;

        [Header("Auflegen (3.2)")]
        public float placementTelegraphSeconds = 0.8f;
        public float placementToleranceWorldUnits = 1.5f;
        public float placementZoneHalfWidth = 2.0f;
        [Range(0f, 1f)] public float sweetSpotOffset = 0.1f;

        [Header("Bewegung")]
        public float walkSpeed = 2.0f;
        public float runSpeedMultiplier = 1.6f;
        public float distanceToTrailer = 12f;
        public float walkBackSeconds = 2.0f;

        [Header("Energie (4.2)")]
        public float startEnergy = 100f;
        public float energyPerSecondBase = 2.0f;
        public float energyPerSecondPerWeight = 1f / 40f;
        public float runEnergyMultiplier = 1.6f;
        public float walkBackEnergyPerSecond = 0.5f;
        public float fallEnergyPenalty = 15f;
        public float snapEnergyPenalty = 10f;

        [Header("Wirtschaft (4.3, 5.1)")]
        public float payoutPerKg = 0.1f;
        public float experiencePer10Kg = 1f;
        [Range(0f, 1f)] public float failedTripExperienceFraction = 0.5f;

        public float MaxAngleRad => maxAngleDeg * Mathf.Deg2Rad;
        public float ComfortAngleRad => comfortAngleDeg * Mathf.Deg2Rad;
        public float RedWarningAngleRad => MaxAngleRad * redWarningFraction;

        public float LengthFactor(BunchLength length)
        {
            switch (length)
            {
                case BunchLength.Short: return lengthFactorShort;
                case BunchLength.Long: return lengthFactorLong;
                default: return lengthFactorMedium;
            }
        }
    }
}
