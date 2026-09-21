using System;
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
    /// Eine Cutter-Station (GDD 3.2). Der Balken ueber ihr ist die **Geduld**
    /// des Cutters, nicht bloss ein Timer:
    /// - Laeuft die Geduld ab, schlaegt er ab, egal wo der Humper steht. Wer
    ///   nicht da ist, verliert die Staude.
    /// - Steht der Humper vorher mit freien Haenden still darunter, schlaegt
    ///   der Cutter sofort ab. Das ist der freiwillige, sichere Weg.
    ///
    /// Jeder Cutter hat ein eigenes Temperament, damit die Stationen
    /// unterschiedlich dringend sind und man priorisieren muss.
    ///
    /// Die Station liegt als Objekt in der Szene und ist dort frei
    /// verschiebbar. Getickt wird sie von ShiftController, damit alle Stationen
    /// gemeinsam anhalten, wenn die Schicht endet.
    /// </summary>
    public class CutterStation : MonoBehaviour
    {
        [Tooltip("Hoehe, in der die Staude haengt und von der sie faellt.")]
        public Transform bunchAnchor;
        [Tooltip("Geduldsbalken ueber der Station.")]
        public ProgressBarVisual bar;
        [Tooltip("Wie geduldig dieser Cutter ist (GDD 3.2).")]
        public CutterTemperament temperament = CutterTemperament.Normal;

        /// <summary>Der Cutter hat abgeschlagen: Staude und Abwurfposition.</summary>
        public event Action<CutterStation, BunchData, Vector3> OnCut;

        BalanceConfig config;
        BananaBunchVisual hangingVisual;
        int day = 1;

        float timer;
        float patienceSeconds;
        float regrowTimer;
        float readyTimer;

        public BunchData PendingBunch { get; private set; }
        public bool HasBunch => PendingBunch != null;
        public float Progress => patienceSeconds > 0f ? Mathf.Clamp01(timer / patienceSeconds) : 0f;
        public Vector3 DropPosition => bunchAnchor != null ? bunchAnchor.position : transform.position;

        public void Initialize(BalanceConfig config, BananaBunchVisual hangingVisual, int day)
        {
            this.config = config;
            this.hangingVisual = hangingVisual;
            this.day = day;
            bar?.Build(config.barWarningFraction);
            GrowNewBunch();

            // Versetzter Start, damit nicht alle Cutter gleichzeitig loslegen
            // und die Schicht in Wellen statt in einem Rhythmus verlaeuft.
            timer = UnityEngine.Random.Range(0f, patienceSeconds * 0.6f);
        }

        /// <summary>
        /// <paramref name="humperReady"/>: Der Humper steht mit freien Haenden
        /// still unter dieser Staude. Der ShiftController entscheidet das, weil
        /// nur er den Tragezustand kennt.
        /// </summary>
        public void Tick(float dt, bool humperReady)
        {
            if (config == null) return;

            if (!HasBunch)
            {
                regrowTimer -= dt;
                if (regrowTimer <= 0f) GrowNewBunch();
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

            timer += dt;
            bar?.SetProgress(Progress);

            if (timer >= patienceSeconds) Cut();
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

        void Cut()
        {
            var bunch = PendingBunch;
            Vector3 dropPosition = DropPosition;

            PendingBunch = null;
            timer = 0f;
            readyTimer = 0f;
            regrowTimer = config.regrowSeconds;
            if (hangingVisual != null) hangingVisual.gameObject.SetActive(false);
            bar?.SetReady(false);
            bar?.SetVisible(false);

            OnCut?.Invoke(this, bunch, dropPosition);
        }

        void GrowNewBunch()
        {
            PendingBunch = BunchData.GenerateForDay(day);
            patienceSeconds = config.RandomPatienceSeconds(TemperamentMultiplier());
            timer = 0f;
            readyTimer = 0f;

            if (hangingVisual != null)
            {
                hangingVisual.gameObject.SetActive(true);
                hangingVisual.Build(PendingBunch);
            }
            bar?.SetVisible(true);
            bar?.SetProgress(0f);
        }

        /// <summary>Am Schichtende: Balken ausblenden, damit nichts mehr faellt.</summary>
        public void StopForShiftEnd()
        {
            bar?.SetVisible(false);
        }
    }
}
