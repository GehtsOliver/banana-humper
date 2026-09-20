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
        public BananaBunchVisual bunchVisualController;
        public PlayerAnimator playerAnimator;
        public float cutterX;
        public float trailerX;

        [Header("Run-Unlock (Platzhalter bis ProgressionSystem existiert)")]
        public bool runUnlocked = true;

        /// <summary>
        /// TEMPORAER (Nutzerwunsch): reduziert den Trip auf den reinen Core Loop
        /// - kein Auflegen-Minispiel (GDD 3.2), die Staude liegt beim Trip-Start
        /// direkt zentriert auf der Schulter. Fuer echten Content wieder auf
        /// false setzen, dann laeuft wieder das volle Auflegen aus 3.2.
        /// </summary>
        public bool skipPlacement = true;

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
            bunchVisualController?.Build(currentBunch.Length);

            if (skipPlacement)
            {
                // TEMPORAER (Nutzerwunsch): reiner Core Loop, Staude liegt beim
                // Trip-Start bereits mittig auf der Schulter, kein Auflegen-
                // Minispiel. Fuer echten Content wieder auf false setzen.
                CurrentPhase = TripPhase.Placement;
                playerAnimator?.SetWalking(false, false);
                placementOffset = 0f;
            }
            else
            {
                // --- Auflegen ---
                CurrentPhase = TripPhase.Placement;
                placementDone = false;
                playerAnimator?.SetWalking(false, false);
                placement.BeginPlacement();
                while (!placementDone) yield return null;
            }

            balance.BeginTrip(currentBunch, placementOffset);
            SetBunchVisualActive(true);

            // --- Tragen zum Trailer ---
            // Bewegung per A/D (Design-Entscheidung, ersetzt die automatische
            // Bewegung aus GDD 3.1 [A]); Balancieren laeuft parallel per W/S
            // (BalanceController.Tick liest die Vertical-Achse unabhaengig
            // davon). Die Maus ist ausschliesslich fuers Rennen reserviert.
            CurrentPhase = TripPhase.Carrying;
            energyRanOutMidCarry = false;
            float minX = Mathf.Min(cutterX, trailerX);
            float maxX = Mathf.Max(cutterX, trailerX);
            float playerX = cutterX;
            playerAnimator?.SetFacing(trailerX < cutterX);

            while (Mathf.Abs(playerX - trailerX) > 0.05f)
            {
                float dt = Time.deltaTime;

                // E statt rechter Maustaste - Maus ist ausschliesslich fuers
                // Rennen reserviert (Nutzerwunsch).
                if (Input.GetKeyDown(KeyCode.E) && balance.TryBeginReposition())
                {
                    energy.ApplyRepositionCost();
                }

                if (!balance.IsRepositioning)
                {
                    float moveInput = 0f;
                    if (Input.GetKey(KeyCode.D)) moveInput += 1f;
                    if (Input.GetKey(KeyCode.A)) moveInput -= 1f;
                    bool running = runUnlocked && Input.GetMouseButton(0) && moveInput != 0f;

                    balance.Tick(dt, running);
                    energy.ConsumeCarrying(currentBunch.Weight, running, dt);

                    if (moveInput != 0f)
                    {
                        float speed = config.walkSpeed * (running ? config.runSpeedMultiplier : 1f);
                        playerX = Mathf.Clamp(playerX + moveInput * speed * dt, minX, maxX);
                        SetPlayerPosition(playerX);
                        playerAnimator?.SetFacing(moveInput < 0f);
                    }
                    playerAnimator?.SetWalking(moveInput != 0f, running);
                }
                else
                {
                    playerAnimator?.SetWalking(false, false);
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
            SetBunchVisualActive(false);

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
            playerAnimator?.SetFacing(cutterX < startX);
            playerAnimator?.SetWalking(true, false);
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
            playerAnimator?.ShowHurt();
            OnTripFallen?.Invoke();
        }

        void HandleSnapped()
        {
            economy.RegisterFailedTrip(currentBunch);
            energy.ApplySnapPenalty();
            summary.TripsSnapped++;
            bunchVisualController?.SetSnapped();
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
            bunchVisualController?.UpdateStress(balance.Stress / 100f, balance.IsCreaking, balance.IsBending);
        }

        /// <summary>Blendet die Staude aus, solange sie nicht getragen wird (Auflegen, Rueckweg), damit ein
        /// Fallen/Snap nicht als "kaputtes, an der Schulter klebendes Objekt" haengen bleibt.</summary>
        void SetBunchVisualActive(bool active)
        {
            if (bunchVisual == null) return;
            bunchVisual.gameObject.SetActive(active);
            if (active)
            {
                bunchVisual.localRotation = Quaternion.identity;
                bunchVisual.localPosition = new Vector3(0f, bunchVisual.localPosition.y, 0f);
            }
        }
    }
}
