using System;
using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>Wie lange ein Cutter wartet, bevor er von sich aus abschlaegt (GDD 3.2).</summary>
    public enum CutterTemperament
    {
        Ungeduldig,
        Normal,
        Geduldig
    }

    /// <summary>
    /// Ein Cutter (GDD 3.2). Er steht nicht mehr fest an einem Platz, sondern
    /// wandert durch das Paddock: Nach dem Abschlagen sucht er sich die
    /// naechstgelegene freie Pflanze und laeuft hin.
    ///
    /// Der Balken ueber ihm ist seine **Geduld**, kein Timer:
    /// - Laeuft sie ab, schlaegt er ab, egal wo der Humper steckt.
    /// - Steht der Humper mit freien Haenden still darunter, schlaegt er sofort
    ///   ab. Das ist der freiwillige, sichere Weg.
    /// Waehrend er laeuft, ist nichts zu holen - dadurch entstehen von selbst
    /// Luecken und ein Rhythmus statt Dauerbeschuss.
    ///
    /// Getickt von ShiftController, damit am Schichtende alles gemeinsam steht.
    /// </summary>
    public class Cutter : MonoBehaviour
    {
        enum State { Walking, Working }

        [Tooltip("Wie geduldig dieser Cutter ist (GDD 3.2).")]
        public CutterTemperament temperament = CutterTemperament.Normal;
        [Tooltip("Arbeitet dieser Cutter schon? Nicht angeheuerte bleiben unsichtbar - sie werden spaeter im Shop freigeschaltet (GDD 5.2).")]
        public bool isHired;
        [Tooltip("Geduldsbalken ueber dem Kopf - laeuft mit.")]
        public ProgressBarVisual bar;

        /// <summary>Abgeschlagen: Staude und Abwurfposition.</summary>
        public event Action<Cutter, BunchData, Vector3> OnCut;

        BalanceConfig config;
        List<Plant> plants;
        Plant target;
        State state = State.Walking;

        float patienceTimer;
        float patienceSeconds;
        float readyTimer;

        public bool IsWorking => state == State.Working && target != null && target.HasBunch;
        public float Progress => patienceSeconds > 0f ? Mathf.Clamp01(patienceTimer / patienceSeconds) : 0f;
        /// <summary>Wo die Staude runterkommt - nur sinnvoll, solange er arbeitet.</summary>
        public Vector3 DropPosition => target != null ? target.BunchPosition : transform.position;

        public void Initialize(BalanceConfig config, List<Plant> plants)
        {
            this.config = config;
            this.plants = plants;

            if (!isHired)
            {
                gameObject.SetActive(false);
                return;
            }

            bar?.Build(config.barWarningFraction);
            bar?.SetVisible(false);
            state = State.Walking;
            target = null;
        }

        /// <summary>
        /// <paramref name="humperReady"/>: Der Humper steht mit freien Haenden
        /// still unter dieser Staude. Das entscheidet der ShiftController, weil
        /// nur er den Tragezustand kennt.
        /// </summary>
        public void Tick(float dt, bool humperReady)
        {
            if (config == null || !isHired) return;

            if (target == null || !target.HasBunch)
            {
                ReleaseTarget();
                target = FindNearestFreePlant();
                if (target == null)
                {
                    // Gerade traegt keine Pflanze - warten, statt ins Leere zu laufen.
                    bar?.SetVisible(false);
                    return;
                }
                target.ClaimedBy = this;
                state = State.Walking;
                bar?.SetVisible(false);
            }

            if (state == State.Walking)
            {
                WalkTowardsTarget(dt);
                return;
            }

            if (humperReady)
            {
                readyTimer += dt;
                bar?.SetReady(true);
                if (readyTimer >= config.readyToCutSeconds)
                {
                    Cut();
                    return;
                }
            }
            else
            {
                readyTimer = 0f;
                bar?.SetReady(false);
            }

            patienceTimer += dt;
            bar?.SetProgress(Progress);
            if (patienceTimer >= patienceSeconds) Cut();
        }

        void WalkTowardsTarget(float dt)
        {
            float targetX = target.PositionX;
            var p = transform.position;
            p.x = Mathf.MoveTowards(p.x, targetX, config.cutterWalkSpeed * dt);
            transform.position = p;

            if (Mathf.Abs(p.x - targetX) > 0.02f) return;

            state = State.Working;
            patienceSeconds = config.RandomPatienceSeconds(TemperamentMultiplier());
            patienceTimer = 0f;
            readyTimer = 0f;
            bar?.SetVisible(true);
            bar?.SetReady(false);
            bar?.SetProgress(0f);
        }

        Plant FindNearestFreePlant()
        {
            Plant nearest = null;
            float bestDistance = float.MaxValue;

            foreach (var plant in plants)
            {
                if (plant == null || !plant.IsFree) continue;
                float distance = Mathf.Abs(plant.PositionX - transform.position.x);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                nearest = plant;
            }
            return nearest;
        }

        void Cut()
        {
            var plant = target;
            Vector3 dropPosition = plant.BunchPosition;
            var bunch = plant.Harvest();

            target = null;
            state = State.Walking;
            patienceTimer = 0f;
            readyTimer = 0f;
            bar?.SetReady(false);
            bar?.SetVisible(false);

            OnCut?.Invoke(this, bunch, dropPosition);
        }

        void ReleaseTarget()
        {
            if (target != null && target.ClaimedBy == this) target.ClaimedBy = null;
            target = null;
        }

        public float TemperamentMultiplier()
        {
            switch (temperament)
            {
                case CutterTemperament.Ungeduldig: return config.patienceImpatient;
                case CutterTemperament.Geduldig: return config.patiencePatient;
                default: return config.patienceNormal;
            }
        }

        /// <summary>Am Schichtende: Balken ausblenden, damit nichts mehr faellt.</summary>
        public void StopForShiftEnd()
        {
            bar?.SetVisible(false);
            ReleaseTarget();
        }
    }
}
