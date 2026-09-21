using System;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Eine Cutter-Station (GDD 3.2): An ihr haengt eine Staude, darueber
    /// fuellt sich der Schnitt-Balken. Ist er voll, schlaegt der Cutter ab und
    /// die Staude faellt; danach pausiert die Station kurz und haengt eine neue
    /// Zufallsstaude auf.
    ///
    /// Die Station selbst liegt als Objekt in der Szene und ist dort frei
    /// verschiebbar - ihre X-Position ist der Weg, den der Spieler zuruecklegen
    /// muss. Getickt wird sie von ShiftController, damit alle Stationen
    /// gemeinsam anhalten, wenn die Schicht endet.
    /// </summary>
    public class CutterStation : MonoBehaviour
    {
        [Tooltip("Hoehe, in der die Staude haengt und von der sie faellt.")]
        public Transform bunchAnchor;
        [Tooltip("Schnitt-Balken ueber der Station.")]
        public ProgressBarVisual bar;

        /// <summary>Die Station hat abgeschlagen: Staude und Abwurfposition.</summary>
        public event Action<CutterStation, BunchData, Vector3> OnCut;

        BalanceConfig config;
        BananaBunchVisual hangingVisual;
        int day = 1;

        float timer;
        float cutSeconds;
        float regrowTimer;

        public BunchData PendingBunch { get; private set; }
        public bool HasBunch => PendingBunch != null;
        public float Progress => cutSeconds > 0f ? Mathf.Clamp01(timer / cutSeconds) : 0f;
        public Vector3 DropPosition => bunchAnchor != null ? bunchAnchor.position : transform.position;

        public void Initialize(BalanceConfig config, BananaBunchVisual hangingVisual, int day)
        {
            this.config = config;
            this.hangingVisual = hangingVisual;
            this.day = day;
            bar?.Build(config.barWarningFraction);
            GrowNewBunch();
        }

        public void Tick(float dt)
        {
            if (config == null) return;

            if (!HasBunch)
            {
                regrowTimer -= dt;
                if (regrowTimer <= 0f) GrowNewBunch();
                return;
            }

            timer += dt;
            bar?.SetProgress(Progress);

            if (timer >= cutSeconds) Cut();
        }

        void Cut()
        {
            var bunch = PendingBunch;
            Vector3 dropPosition = DropPosition;

            PendingBunch = null;
            timer = 0f;
            regrowTimer = config.regrowSeconds;
            if (hangingVisual != null) hangingVisual.gameObject.SetActive(false);
            bar?.SetVisible(false);

            OnCut?.Invoke(this, bunch, dropPosition);
        }

        void GrowNewBunch()
        {
            PendingBunch = BunchData.GenerateForDay(day);
            cutSeconds = config.RandomCutSeconds();
            timer = 0f;

            if (hangingVisual != null)
            {
                hangingVisual.gameObject.SetActive(true);
                hangingVisual.Build(PendingBunch);
            }
            bar?.SetVisible(true);
            bar?.SetProgress(0f);
        }

        /// <summary>Am Schichtende: Balken anhalten, damit nichts mehr faellt.</summary>
        public void StopForShiftEnd()
        {
            bar?.SetVisible(false);
        }
    }
}
