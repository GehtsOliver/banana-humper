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

        public int RegisterDelivery(BunchData bunch)
        {
            int payout = bunch.PayoutDollars(config);
            AddMoney(payout);
            AddExperience(bunch.ExperienceValue(config));
            return payout;
        }

        /// <summary>Gefallene/gesnappte Staude: kein Lohn, aber 50% Erfahrung fuer die getragene Last (5.1).</summary>
        public void RegisterFailedTrip(BunchData bunch)
        {
            AddExperience(bunch.ExperienceValue(config) * config.failedTripExperienceFraction);
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
