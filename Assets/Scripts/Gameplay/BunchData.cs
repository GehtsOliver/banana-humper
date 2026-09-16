using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    public enum BunchLength
    {
        Short,
        Medium,
        Long
    }

    /// <summary>
    /// One banana bunch a Cutter hands to the player (GDD 4.3).
    /// Weight range widens with the current farm day.
    /// </summary>
    public class BunchData
    {
        public readonly float Weight;
        public readonly BunchLength Length;

        public BunchData(float weight, BunchLength length)
        {
            Weight = weight;
            Length = length;
        }

        public static BunchData GenerateForDay(int day)
        {
            // Tag 1: 30-60 kg, steigt bis Tag 88: 60-100 kg (4.3)
            float t = Mathf.Clamp01((day - 1) / 87f);
            float min = Mathf.Lerp(30f, 60f, t);
            float max = Mathf.Lerp(60f, 100f, t);
            float weight = Random.Range(min, max);

            // Tag 1 überwiegend kurz/mittel, später mehr lange Stauden (4.3)
            float longChance = Mathf.Lerp(0.1f, 0.45f, t);
            float shortChance = Mathf.Lerp(0.45f, 0.2f, t);
            float roll = Random.value;
            BunchLength length = roll < shortChance
                ? BunchLength.Short
                : roll < shortChance + (1f - shortChance - longChance)
                    ? BunchLength.Medium
                    : BunchLength.Long;

            return new BunchData(weight, length);
        }

        public int PayoutDollars(BalanceConfig config)
        {
            return Mathf.RoundToInt(Weight * config.payoutPerKg);
        }

        public float ExperienceValue(BalanceConfig config)
        {
            return Weight / 10f * config.experiencePer10Kg;
        }
    }
}
