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
    /// Der Kern-Loop seit GDD v0.9: Cutter wandern durch das Paddock, schlagen
    /// an den Pflanzen ab, und der Humper muss rechtzeitig unter der fallenden
    /// Staude stehen und sie zum mitfahrenden Trailer schleppen. Wer schleppt,
    /// kann nicht fangen - darin liegt die zentrale Entscheidung.
    ///
    /// Dieses Skript taktet alle Beteiligten (Pflanzen, Cutter, Trailer,
    /// Spieler, Pendel) statt einer Trip-Statemachine wie bis v0.8. Alles andere
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
        public CameraController cameraController;
        public List<Cutter> cutters = new List<Cutter>();
        public PaddockField field;

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
        public event Action OnStumbled;
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
            player.OnStumbled += HandleStumbled;
            foreach (var cutter in cutters)
            {
                if (cutter != null) cutter.OnCut += HandleCut;
            }
        }

        void OnDestroy()
        {
            if (balance != null) balance.OnDropped -= HandleDropped;
            if (player != null) player.OnStumbled -= HandleStumbled;
            foreach (var cutter in cutters)
            {
                if (cutter != null) cutter.OnCut -= HandleCut;
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

            // Richtung pro Schicht auswuerfeln: Mal arbeitet sich die Crew nach
            // rechts durch das Feld, mal nach links (GDD 3.5).
            trailer.BeginShift(player.PositionX + 3f);
            field.Initialize(config, day, trailer.Direction, trailer.PositionX);

            foreach (var cutter in cutters)
            {
                if (cutter != null) cutter.Initialize(config, field.Plants);
            }

            IsShiftActive = true;
            OnShiftStarted?.Invoke();
        }

        void Update()
        {
            if (!IsShiftActive) return;

            float dt = Time.deltaTime;

            field.Tick(dt);

            // Der Trailer zieht nur weiter, wenn der Abschnitt leergeerntet ist
            // (GDD 3.5) - und das Feld wandert mit ihm.
            int ripeNearby = field.CountRipeNear(trailer.PositionX, config.trailerSectionRadius);
            trailer.Tick(dt, ripeNearby);
            field.UpdateWindow(trailer.PositionX);
            ApplyFieldBounds();

            foreach (var cutter in cutters)
            {
                if (cutter != null) cutter.Tick(dt, HumperIsReadyAt(cutter));
            }
            player.Tick(dt);

            if (player.IsCarrying) TickCarrying(dt);

            if (energy.IsDepleted) EndShift();
        }

        /// <summary>
        /// Spieler, Kamera und Steine ziehen mit dem Feld mit - sonst liefe man
        /// gegen die Grenzen des vorherigen Abschnitts.
        /// </summary>
        void ApplyFieldBounds()
        {
            player.minX = field.MinX;
            player.maxX = field.MaxX;
            player.obstacles = field.Obstacles;

            if (cameraController == null) return;
            cameraController.minX = field.MinX;
            cameraController.maxX = field.MaxX;
        }

        /// <summary>
        /// Steht der Humper mit freien Haenden still unter dieser Staude? Dann
        /// schlaegt der Cutter frueher ab (GDD 3.2). Bewusst an "steht still"
        /// geknuepft und nicht bloss an die Naehe - sonst wuerde jedes
        /// Vorbeilaufen unterwegs ungewollt Stauden ausloesen.
        /// </summary>
        bool HumperIsReadyAt(Cutter cutter)
        {
            // Nur wer schon an einer Pflanze steht, kann abschlagen - einen
            // laufenden Cutter kann man nicht anhalten.
            if (!cutter.IsWorking) return false;
            if (player.IsCarrying || player.IsMoving || !player.IsGrounded) return false;
            float reach = config.catchRadius * config.normalCatchFraction;
            return Mathf.Abs(player.PositionX - cutter.DropPosition.x) <= reach;
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

        void HandleCut(Cutter cutter, BunchData bunch, Vector3 dropPosition)
        {
            // Bewusst die Bodenhoehe, nicht die aktuelle Spielerhoehe: Sonst
            // wuerde ein Sprung im falschen Moment die Fallstrecke verkuerzen.
            var falling = FallingBunch.Spawn(bunch, dropPosition, player.GroundY, config.fallSeconds, player.shoulderBunch);
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
                // GDD 8.4: Der Fehlschlag soll wehtun, ohne zu bestrafen.
                cameraController?.Shake(0.25f, 0.3f);
                SplashEffect.Spawn(falling.transform.position, new Color(0.35f, 0.28f, 0.16f), 10, 3.5f);
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

            if (quality == CatchQuality.Perfect)
            {
                SplashEffect.Spawn(player.transform.position, new Color(0.95f, 0.9f, 0.4f), 6, 2.2f, 0.5f);
            }
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

        /// <summary>
        /// Gegen einen Stein gelaufen (GDD 3.9). Ohne Staude kostet es nur
        /// Tempo; mit Staude wackelt sie kraeftig und es kostet Kraft - genau
        /// dann lohnt sich also das Springen.
        /// </summary>
        void HandleStumbled()
        {
            OnStumbled?.Invoke();
            cameraController?.Shake(0.12f, 0.18f);
            if (!player.IsCarrying) return;

            energy.ApplyStumbleCost();
            float direction = UnityEngine.Random.value < 0.5f ? -1f : 1f;
            balance.ApplyImpulse(direction * config.stumbleWobbleImpulse);
        }

        void HandleDropped()
        {
            var bunch = player.CarriedBunch;
            if (bunch == null) return;

            economy.RegisterFailedCarry(bunch);
            energy.ApplyDropPenalty();
            summary.BunchesDropped++;
            player.ShowHurt();
            cameraController?.Shake(0.3f, 0.35f);
            SplashEffect.Spawn(player.transform.position, new Color(0.35f, 0.28f, 0.16f), 10, 3.5f);
            player.ClearBunch();
            OnBunchDropped?.Invoke();
        }

        void EndShift()
        {
            IsShiftActive = false;
            balance.StopCarry();
            player.ClearBunch();

            foreach (var cutter in cutters)
            {
                if (cutter != null) cutter.StopForShiftEnd();
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
