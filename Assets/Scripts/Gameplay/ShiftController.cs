using System;
using System.Collections;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    public enum TripPhase
    {
        Placement,
        Carrying,
        WalkingBack
    }

    public struct ShiftSummary
    {
        public int TripsCompleted;
        public int TripsFallen;
        public int TripsSnapped;
        public double MoneyEarned;
        public double ExperienceEarned;
    }

    /// <summary>
    /// Orchestrates one Trip after another (GDD 3.1) until the Energy budget
    /// for the Schicht runs out (4.2). This is the Kern-Loop: everything else
    /// (Quote, Verwarnungen, Prestige, ...) is deliberately out of scope for
    /// the Stufe-A Graybox and is layered on top later.
    /// </summary>
    public class ShiftController : MonoBehaviour
    {
        [Header("Config & Systeme")]
        public BalanceConfig config;
        public PlacementController placement;
        public BalanceController balance;
        public EnergySystem energy;
        public EconomySystem economy;

        [Header("Visuals")]
        public Transform playerRoot;
        public Transform bunchVisual;
        public float cutterX;
        public float trailerX;

        [Header("Run-Unlock (Platzhalter bis ProgressionSystem existiert)")]
        public bool runUnlocked = true;

        public int Day { get; private set; } = 1;
        public TripPhase CurrentPhase { get; private set; }
        public bool IsShiftActive { get; private set; }

        public event Action OnShiftStarted;
        public event Action<int> OnDelivered;
        public event Action OnTripFallen;
        public event Action OnTripSnapped;
        public event Action<ShiftSummary> OnShiftEnded;

        BunchData currentBunch;
        bool placementDone;
        float placementOffset;
        bool energyRanOutMidCarry;

        ShiftSummary summary;

        /// <summary>
        /// Muss aufgerufen werden, nachdem alle public Referenzen (config, placement,
        /// balance, energy, economy, ...) gesetzt wurden. Nicht in Awake(), weil
        /// GameBootstrap die Referenzen erst nach AddComponent&lt;ShiftController&gt;()
        /// zuweist und Awake() bereits synchron beim AddComponent-Aufruf feuert.
        /// </summary>
        public void Initialize()
        {
            balance.OnFallen += HandleFallen;
            balance.OnSnapped += HandleSnapped;
            placement.OnPlacementResolved += HandlePlacementResolved;
        }

        void OnDestroy()
        {
            balance.OnFallen -= HandleFallen;
            balance.OnSnapped -= HandleSnapped;
            placement.OnPlacementResolved -= HandlePlacementResolved;
        }

        double moneyAtShiftStart;
        double experienceAtShiftStart;

        /// <summary>Setzt Geld/Erfahrung auf 0 - nur beim Start eines neuen Farm-Durchlaufs (Prestige), nicht pro Schicht (4.1).</summary>
        public void ResetRun()
        {
            economy.ResetRun();
        }

        public void StartShift(int day)
        {
            Day = day;
            moneyAtShiftStart = economy.Money;
            experienceAtShiftStart = economy.Experience;
            energy.StartShift();
            summary = new ShiftSummary();
            placement.Setup(cutterX);
            SetPlayerPosition(cutterX);
            IsShiftActive = true;
            OnShiftStarted?.Invoke();
            StartCoroutine(ShiftLoop());
        }

        IEnumerator ShiftLoop()
        {
            while (!energy.IsDepleted)
            {
                yield return StartCoroutine(RunOneTrip());
            }

            IsShiftActive = false;
            summary.MoneyEarned = economy.Money - moneyAtShiftStart;
            summary.ExperienceEarned = economy.Experience - experienceAtShiftStart;
            OnShiftEnded?.Invoke(summary);
        }

        IEnumerator RunOneTrip()
        {
            currentBunch = BunchData.GenerateForDay(Day);

            // --- Auflegen ---
            CurrentPhase = TripPhase.Placement;
            placementDone = false;
            placement.BeginPlacement();
            while (!placementDone) yield return null;

            balance.BeginTrip(currentBunch, placementOffset);

            // --- Tragen zum Trailer ---
            CurrentPhase = TripPhase.Carrying;
            energyRanOutMidCarry = false;
            float distance = 0f;
            float total = Mathf.Abs(trailerX - cutterX);
            float direction = Mathf.Sign(trailerX - cutterX);

            while (distance < total)
            {
                float dt = Time.deltaTime;
                bool running = runUnlocked && Input.GetMouseButton(0);

                if (Input.GetMouseButtonDown(1) && balance.TryBeginReposition())
                {
                    energy.ApplyRepositionCost();
                }

                if (!balance.IsRepositioning)
                {
                    balance.Tick(dt, running);
                    energy.ConsumeCarrying(currentBunch.Weight, running, dt);

                    float speed = config.walkSpeed * (running ? config.runSpeedMultiplier : 1f);
                    distance += speed * dt;
                    SetPlayerPosition(cutterX + direction * distance);
                }

                UpdateBunchVisual();

                if (energy.IsDepleted)
                {
                    energyRanOutMidCarry = true;
                    break;
                }
                if (balance.HasFailed)
                {
                    break;
                }

                yield return null;
            }

            balance.StopTrip();

            if (energyRanOutMidCarry)
            {
                // 4.2: Energie leer -> Schicht endet sofort, ohne Zusatzstrafe.
                yield break;
            }

            if (!balance.HasFailed)
            {
                int payout = economy.RegisterDelivery(currentBunch);
                summary.TripsCompleted++;
                OnDelivered?.Invoke(payout);
            }

            // --- Rueckweg (leer, automatisch, schnell) ---
            CurrentPhase = TripPhase.WalkingBack;
            yield return StartCoroutine(WalkBack());
        }

        IEnumerator WalkBack()
        {
            float elapsed = 0f;
            float startX = playerRoot != null ? playerRoot.position.x : cutterX;
            while (elapsed < config.walkBackSeconds && !energy.IsDepleted)
            {
                float dt = Time.deltaTime;
                energy.ConsumeWalkBack(dt);
                elapsed += dt;
                float t = Mathf.Clamp01(elapsed / config.walkBackSeconds);
                SetPlayerPosition(Mathf.Lerp(startX, cutterX, t));
                yield return null;
            }
            SetPlayerPosition(cutterX);
        }

        void HandlePlacementResolved(float offset, bool sweetSpot)
        {
            placementOffset = offset;
            placementDone = true;
        }

        void HandleFallen()
        {
            economy.RegisterFailedTrip(currentBunch);
            energy.ApplyFallPenalty();
            summary.TripsFallen++;
            OnTripFallen?.Invoke();
        }

        void HandleSnapped()
        {
            economy.RegisterFailedTrip(currentBunch);
            energy.ApplySnapPenalty();
            summary.TripsSnapped++;
            OnTripSnapped?.Invoke();
        }

        void SetPlayerPosition(float x)
        {
            if (playerRoot != null) playerRoot.position = new Vector3(x, playerRoot.position.y, 0f);
        }

        void UpdateBunchVisual()
        {
            if (bunchVisual == null) return;
            bunchVisual.localRotation = Quaternion.Euler(0f, 0f, -balance.Theta * Mathf.Rad2Deg);
            bunchVisual.localPosition = new Vector3(balance.Offset * config.placementToleranceWorldUnits * 0.5f, bunchVisual.localPosition.y, 0f);
        }
    }
}
