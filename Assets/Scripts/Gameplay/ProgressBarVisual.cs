using UnityEngine;
using BananaHumper.Util;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Schnitt-Balken ueber einer Cutter-Station (GDD 3.2). Bewusst in der Welt
    /// statt im HUD: Der Blick soll dort bleiben, wo die Entscheidung faellt.
    /// Wird zur Laufzeit gebaut, weil die Formen prozedural sind.
    /// </summary>
    public class ProgressBarVisual : MonoBehaviour
    {
        static readonly Color BackgroundColor = new Color(0f, 0f, 0f, 0.55f);
        static readonly Color NormalColor = new Color(0.55f, 0.8f, 0.35f);
        static readonly Color WarningColor = new Color(0.95f, 0.45f, 0.15f);
        /// <summary>Humper steht bereit, der Cutter schlaegt gleich freiwillig ab (GDD 3.2).</summary>
        static readonly Color ReadyColor = new Color(0.45f, 0.85f, 0.95f);

        const float Width = 1.1f;
        const float Height = 0.16f;

        Transform fill;
        SpriteRenderer fillRenderer;
        float warningFraction = 0.7f;

        public void Build(float warningAt)
        {
            warningFraction = warningAt;

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            SpriteFactory.CreateRoundedQuad("BarBackground", BackgroundColor, new Vector2(Width, Height), 0.5f,
                transform, Vector3.zero, 20);

            // Eigener Pivot-Knoten, damit der Balken von links nach rechts
            // waechst statt aus der Mitte heraus.
            var pivot = new GameObject("FillPivot").transform;
            pivot.SetParent(transform, false);
            pivot.localPosition = new Vector3(-Width * 0.5f, 0f, 0f);

            fillRenderer = SpriteFactory.CreateRoundedQuad("BarFill", NormalColor,
                new Vector2(Width - 0.06f, Height - 0.06f), 0.5f, pivot, new Vector3((Width - 0.06f) * 0.5f, 0f, 0f), 21);
            fill = pivot;

            SetProgress(0f);
        }

        bool ready;

        public void SetProgress(float progress01)
        {
            if (fill == null) return;
            float p = Mathf.Clamp01(progress01);
            fill.localScale = new Vector3(p, 1f, 1f);
            if (fillRenderer != null && !ready)
            {
                fillRenderer.color = p >= warningFraction ? WarningColor : NormalColor;
            }
        }

        /// <summary>Faerbt den Balken um, solange der Humper bereitsteht.</summary>
        public void SetReady(bool isReady)
        {
            if (ready == isReady) return;
            ready = isReady;
            if (fillRenderer != null && ready) fillRenderer.color = ReadyColor;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
