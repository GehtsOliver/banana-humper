using System;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>Geld und Erfahrung fuer den aktuellen Farm-Durchlauf (GDD 4.1, 4.3, 5.1).</summary>
    public class EconomySystem : MonoBehaviour
    {
        public BalanceConfig config;

        public event Action<double> OnMoneyChanged;
        public event Action<double> OnExperienceChanged;

        public double Money { get; private set; }
        public double Experience { get; private set; }

        public void ResetRun()
        {
            Money = 0;
            Experience = 0;
            OnMoneyChanged?.Invoke(Money);
            OnExperienceChanged?.Invoke(Experience);
        }

        /// <summary><paramref name="payoutFactor"/> kommt aus der Catch-Qualitaet: ein Streifer beschaedigt die Staude (GDD 3.3).</summary>
        public int RegisterDelivery(BunchData bunch, float payoutFactor = 1f)
        {
            int payout = Mathf.RoundToInt(bunch.PayoutDollars(config) * payoutFactor);
            AddMoney(payout);
            AddExperience(bunch.ExperienceValue(config));
            return payout;
        }

        /// <summary>
        /// Fallen gelassene Staude: kein Lohn, aber 50 % Erfahrung fuer die bis
        /// dahin getragene Last (GDD 5.1). Verpasste Stauden landen hier bewusst
        /// nicht - die hat man nie getragen.
        /// </summary>
        public void RegisterFailedCarry(BunchData bunch)
        {
            AddExperience(bunch.ExperienceValue(config) * config.failedTripExperienceFraction);
        }

        /// <summary>Kauf von Ausruestung (GDD 5.2). Gibt false zurueck, wenn das Geld nicht reicht.</summary>
        public bool TrySpend(double amount)
        {
            if (amount > Money) return false;
            Money -= amount;
            OnMoneyChanged?.Invoke(Money);
            return true;
        }

        /// <summary>Koerper-Stufe verbessern (GDD 5.1). Erfahrung ist die zweite Waehrung.</summary>
        public bool TrySpendExperience(double amount)
        {
            if (amount > Experience) return false;
            Experience -= amount;
            OnExperienceChanged?.Invoke(Experience);
            return true;
        }

        void AddMoney(double amount)
        {
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }

        void AddExperience(double amount)
        {
            Experience += amount;
            OnExperienceChanged?.Invoke(Experience);
        }
    }
}
