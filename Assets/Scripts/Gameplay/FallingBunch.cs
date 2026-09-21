using System;
using UnityEngine;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Die abgeschlagene Staude auf dem Weg nach unten (GDD 3.3). Die Falldauer
    /// ist das Zeitfenster, in dem der Spieler unter ihr stehen muss.
    ///
    /// Das Objekt entscheidet nicht selbst ueber Treffer oder Fehlschlag - es
    /// meldet nur den Aufprall. Ob gefangen wurde, weiss nur der
    /// ShiftController (er kennt Spieler und Tragezustand).
    /// </summary>
    public class FallingBunch : MonoBehaviour
    {
        public event Action<FallingBunch> OnImpact;

        public BunchData Bunch { get; private set; }
        public float ImpactX => transform.position.x;

        float elapsed;
        float duration;
        float startY;
        float targetY;
        bool impacted;

        public static FallingBunch Spawn(BunchData bunch, Vector3 from, float targetY, float seconds, BananaBunchVisual visualPrefabSource)
        {
            var go = new GameObject("FallingBunch");
            go.transform.position = from;

            var visual = go.AddComponent<BananaBunchVisual>();
            // Testmodus der Schulter-Staude uebernehmen, damit die fallende
            // Staude im Graybox-Test nicht ploetzlich anders aussieht.
            if (visualPrefabSource != null) visual.broomTestMode = visualPrefabSource.broomTestMode;
            visual.Build(bunch);

            var falling = go.AddComponent<FallingBunch>();
            falling.Bunch = bunch;
            falling.startY = from.y;
            falling.targetY = targetY;
            falling.duration = Mathf.Max(0.05f, seconds);
            return falling;
        }

        void Update()
        {
            if (impacted) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Quadratisch statt linear: sieht nach Schwerkraft aus und laesst
            // dem Spieler oben mehr Reaktionszeit als kurz vor dem Aufprall.
            float fallen = t * t;
            var p = transform.position;
            p.y = Mathf.Lerp(startY, targetY, fallen);
            transform.position = p;
            transform.Rotate(0f, 0f, 40f * Time.deltaTime);

            if (t >= 1f)
            {
                impacted = true;
                OnImpact?.Invoke(this);
            }
        }

        /// <summary>Verpasst: kurz am Boden liegen lassen, damit der Fehlschlag sichtbar ist.</summary>
        public void ShowCrashAndDestroy(float seconds)
        {
            var visual = GetComponent<BananaBunchVisual>();
            visual?.SetSnapped();
            transform.rotation = Quaternion.Euler(0f, 0f, 78f);
            Destroy(gameObject, seconds);
        }
    }
}
