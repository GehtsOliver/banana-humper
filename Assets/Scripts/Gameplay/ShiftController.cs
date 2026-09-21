using System;
using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    public enum CatchQuality
    {
        Perfect,
        Normal,
        Graze,
        Missed
    }

    public struct ShiftSummary
    {
        public int BunchesDelivered;
        public int BunchesMissed;
        public int BunchesDropped;
        public double MoneyEarned;
        public double ExperienceEarned;
    }

    /// <summary>
    /// Der Kern-Loop seit GDD v0.9: mehrere Cutter-Stationen schneiden parallel,
    /// der Spieler muss rechtzeitig unter der fallenden Staude stehen und sie
    /// dann zum mitfahrenden Trailer schleppen. Wer schleppt, kann nicht fangen -
    /// darin liegt die zentrale Entscheidung.
    ///
    /// Dieses Skript taktet alle Beteiligten (Stationen, Trailer, Spieler,
    /// Pendel) statt einer Trip-Statemachine wie bis v0.8. Alles andere
    /// (Tagesquote, Verwarnungen, Koerper/Shop) ist bewusst noch nicht drin -
    /// Stufe A validiert erst diesen Loop.
    /// </summary>
    public class ShiftController : MonoBehaviour
    {
        [Header("Config & Systeme")]
        public BalanceConfig config;
        public BalanceController balance;
        public EnergySystem energy;
        public EconomySystem economy;

        [Header("Welt")]
        public PlayerController player;
        public TrailerController trailer;
        public List<CutterStation> stations = new List<CutterStation>();

        public int Day { get; private set; } = 1;
        public bool IsShiftActive { get; private set; }
        public BunchData CarriedBunch => player != null ? player.CarriedBunch : null;
        /// <summary>Letzte Catch-Bewertung - fuers HUD-Feedback.</summary>
        public CatchQuality LastCatch { get; private set; } = CatchQuality.Missed;

        public event Action OnShiftStarted;
        public event Action<int> OnDelivered;
        public event Action<CatchQuality> OnCaught;
        public event Action OnBunchMissed;
        public event Action OnBunchDropped;
        public event Action<ShiftSummary> OnShiftEnded;

        readonly List<FallingBunch> inFlight = new List<FallingBunch>();
        ShiftSummary summary;
        double moneyAtShiftStart;
        double experienceAtShiftStart;
        float carriedPayoutFactor = 1f;

        /// <summary>
        /// Nach dem Setzen aller Referenzen aufrufen, nicht in Awake: GameBootstrap
        /// weist die Felder erst nach AddComponent zu, Awake feuert aber schon
        /// waehrend AddComponent.
        /// </summary>
        public void Initialize()
        {
            balance.OnDropped += HandleDropped;
            foreach (var station in stations)
            {
                if (station != null) station.OnCut += HandleStationCut;
            }
        }

        void OnDestroy()
        {
            if (balance != null) balance.OnDropped -= HandleDropped;
            foreach (var station in stations)
            {
                if (station != null) station.OnCut -= HandleStationCut;
            }
        }

        public void ResetRun() => economy.ResetRun();

        public void StartShift(int day)
        {
            Day = day;
            moneyAtShiftStart = economy.Money;
            experienceAtShiftStart = economy.Experience;
            summary = new ShiftSummary();
            energy.StartShift();
            player.ClearBunch();

            foreach (var station in stations)
            {
                if (station != null) station.Initialize(config, StationVisual(station), day);
            }

            IsShiftActive = true;
            OnShiftStarted?.Invoke();
        }

        BananaBunchVisual StationVisual(CutterStation station)
        {
            return station.bunchAnchor != null
                ? station.bunchAnchor.GetComponentInChildren<BananaBunchVisual>(true)
                : null;
        }

        void Update()
        {
            if (!IsShiftActive) return;

            float dt = Time.deltaTime;

            foreach (var station in stations)
            {
                if (station != null) station.Tick(dt);
            }
            trailer.Tick(dt);
            player.Tick(dt);

            if (player.IsCarrying) TickCarrying(dt);

            if (energy.IsDepleted) EndShift();
        }

        void TickCarrying(float dt)
        {
            if (Input.GetKeyDown(KeyCode.E) && balance.TryBeginReposition())
            {
                energy.ApplyRepositionCost();
            }

            if (!balance.IsRepositioning)
            {
                balance.Tick(dt, player.IsRunning);
                energy.ConsumeCarrying(player.CarriedBunch.Weight, player.IsRunning, dt);
            }
            else
            {
                player.StandStill();
            }

            player.ApplyCarryPose(balance.Theta, balance.Offset);

            // Nach balance.Tick pruefen: Ein Sturz in diesem Frame hat die
            // Staude bereits abgeraeumt, dann gibt es nichts abzuliefern.
            if (player.IsCarrying && trailer.IsInDeliveryRange(player.PositionX)) Deliver();
        }

        void HandleStationCut(CutterStation station, BunchData bunch, Vector3 dropPosition)
        {
            float shoulderY = player.transform.position.y;
            var falling = FallingBunch.Spawn(bunch, dropPosition, shoulderY, config.fallSeconds, player.shoulderBunch);
            falling.OnImpact += HandleImpact;
            inFlight.Add(falling);
        }

        void HandleImpact(FallingBunch falling)
        {
            falling.OnImpact -= HandleImpact;
            inFlight.Remove(falling);

            CatchQuality quality = EvaluateCatch(falling.ImpactX);
            LastCatch = quality;

            if (quality == CatchQuality.Missed)
            {
                summary.BunchesMissed++;
                falling.ShowCrashAndDestroy(1.2f);
                OnBunchMissed?.Invoke();
                return;
            }

            // Versatz aus der Catch-Qualitaet: sauber gefangen heisst leichter
            // Rest-Weg (GDD 3.3). Vorzeichen zeigt, auf welcher Seite die Staude
            // aufgekommen ist.
            float delta = falling.ImpactX - player.PositionX;
            float offset = Mathf.Clamp(delta / config.catchRadius, -1f, 1f);
            if (quality == CatchQuality.Perfect) offset = 0f;

            carriedPayoutFactor = quality == CatchQuality.Graze ? 1f - config.grazePayoutPenalty : 1f;

            var bunch = falling.Bunch;
            Destroy(falling.gameObject);
            player.TakeBunch(bunch);
            balance.BeginCarry(bunch, offset);
            OnCaught?.Invoke(quality);
        }

        CatchQuality EvaluateCatch(float impactX)
        {
            // Wer schon eine Staude traegt, hat keine Hand frei - das ist der
            // Kern der Entscheidung "schleppen oder fangen" (GDD 3.1).
            if (player.IsCarrying) return CatchQuality.Missed;

            float d = Mathf.Abs(impactX - player.PositionX);
            if (d <= config.perfectCatchWindow) return CatchQuality.Perfect;
            if (d <= config.catchRadius * config.normalCatchFraction) return CatchQuality.Normal;
            if (d <= config.catchRadius) return CatchQuality.Graze;
            return CatchQuality.Missed;
        }

        void Deliver()
        {
            var bunch = player.CarriedBunch;
            balance.StopCarry();
            player.ClearBunch();

            int payout = economy.RegisterDelivery(bunch, carriedPayoutFactor);
            summary.BunchesDelivered++;
            OnDelivered?.Invoke(payout);
        }

        void HandleDropped()
        {
            var bunch = player.CarriedBunch;
            if (bunch == null) return;

            economy.RegisterFailedCarry(bunch);
            energy.ApplyDropPenalty();
            summary.BunchesDropped++;
            player.ShowHurt();
            player.ClearBunch();
            OnBunchDropped?.Invoke();
        }

        void EndShift()
        {
            IsShiftActive = false;
            balance.StopCarry();
            player.ClearBunch();

            foreach (var station in stations)
            {
                if (station != null) station.StopForShiftEnd();
            }
            foreach (var falling in inFlight)
            {
                if (falling != null) Destroy(falling.gameObject);
            }
            inFlight.Clear();

            summary.MoneyEarned = economy.Money - moneyAtShiftStart;
            summary.ExperienceEarned = economy.Experience - experienceAtShiftStart;
            OnShiftEnded?.Invoke(summary);
        }
    }
}
