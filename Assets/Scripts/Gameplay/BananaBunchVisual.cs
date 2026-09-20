using UnityEngine;
using BananaHumper.Util;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Prozedural gebaute Bananenstaude aus mehreren "Fingern" (kein freies CC0-
    /// Bananen-Asset gefunden, siehe docs/THIRD_PARTY_ASSETS.md). Deckt die drei
    /// Sichtzustaende aus GDD 8.2 ab: gesund, unter Belastung durchgebogen,
    /// gesnappt. Die Finger-Anzahl/-Laenge variiert mit der Staudenlaenge (4.3),
    /// sodass kurze/mittlere/lange Stauden auch optisch unterscheidbar sind.
    /// Kann per <see cref="broomTestMode"/> stattdessen einen Besen anzeigen,
    /// um die Balance-Physik isoliert zu testen.
    /// </summary>
    public class BananaBunchVisual : MonoBehaviour
    {
        static readonly Color HealthyColor = new Color(0.93f, 0.85f, 0.25f);
        static readonly Color BruisedColor = new Color(0.55f, 0.42f, 0.15f);
        static readonly Color StemColor = new Color(0.35f, 0.25f, 0.12f);
        static readonly Color BroomHandleColor = new Color(0.55f, 0.38f, 0.2f);
        static readonly Color BroomBristleColor = new Color(0.8f, 0.68f, 0.3f);

        /// <summary>
        /// TEMPORAER (Nutzerwunsch): zeigt statt der Bananenstaude einen langen
        /// Besen, um die Balance-Physik isoliert zu testen - siehe GDD 3.3
        /// ("wie ein Besen, den man auf der Hand balanciert"). Der Besen pivotiert
        /// an der Schulter genau wie die Staude, ist aber lang und ungeteilt,
        /// damit die Neigung (Theta) auf den ersten Blick sichtbar ist. Fuer
        /// echten Content wieder auf false setzen.
        /// </summary>
        public bool broomTestMode = true;

        Transform[] fingers;
        SpriteRenderer[] fingerRenderers;
        float[] baseAngles;

        /// <summary>Baut die Staude (oder im Testmodus den Besen) neu auf - wird zu Beginn jedes Trips aufgerufen.</summary>
        public void Build(BunchLength length)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            fingers = null;
            fingerRenderers = null;
            baseAngles = null;

            if (broomTestMode)
            {
                BuildBroom();
                return;
            }

            int count = length == BunchLength.Short ? 4 : length == BunchLength.Long ? 7 : 5;
            float spread = length == BunchLength.Short ? 40f : length == BunchLength.Long ? 70f : 55f;
            float fingerLen = length == BunchLength.Short ? 0.5f : length == BunchLength.Long ? 1.0f : 0.75f;

            SpriteFactory.CreateRoundedQuad("Stem", StemColor, new Vector2(0.22f, 0.22f), 0.5f, transform, Vector3.zero, 4);

            fingers = new Transform[count];
            fingerRenderers = new SpriteRenderer[count];
            baseAngles = new float[count];

            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : (float)i / (count - 1);
                float angle = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, t);
                baseAngles[i] = angle;

                var fingerGo = new GameObject($"Finger{i}");
                fingerGo.transform.SetParent(transform, false);
                fingerGo.transform.localPosition = Vector3.zero;
                fingerGo.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

                var sr = SpriteFactory.CreateRoundedQuad("Shape", HealthyColor, new Vector2(0.24f, fingerLen), 0.7f,
                    fingerGo.transform, new Vector3(0f, -fingerLen * 0.45f, 0f), 3);

                fingers[i] = fingerGo.transform;
                fingerRenderers[i] = sr;
            }
        }

        /// <summary>
        /// Langer, ungeteilter Besen statt Bananenstaude (siehe broomTestMode).
        /// Liegt bei Theta=0 horizontal auf der Schulter (Pivot bei (0,0,0),
        /// mittig auf dem Schaft) - wie eine Balancierstange, nicht wie ein
        /// haengender Bananenbund. Rotiert um genau diesen Mittelpunkt mit Theta.
        /// </summary>
        void BuildBroom()
        {
            const float shaftLength = 2.2f;
            const float shaftWidth = 0.09f;

            SpriteFactory.CreateRoundedQuad("Grip", StemColor, new Vector2(0.16f, 0.16f), 0.6f, transform, Vector3.zero, 4);
            SpriteFactory.CreateRoundedQuad("Handle", BroomHandleColor, new Vector2(shaftLength, shaftWidth), 0.3f,
                transform, Vector3.zero, 3);
            SpriteFactory.CreateRoundedQuad("Bristles", BroomBristleColor, new Vector2(shaftLength * 0.22f, shaftWidth * 3.5f), 0.35f,
                transform, new Vector3(shaftLength * 0.48f, 0f, 0f), 4);
        }

        /// <summary>Pro Frame waehrend des Tragens: Finger haengen mit steigender Belastung durch, Farbe wandert Richtung "geprellt" (GDD 3.5).</summary>
        public void UpdateStress(float stress01, bool isCreaking, bool isBending)
        {
            if (fingers == null) return;
            float droopDeg = isBending ? stress01 * 22f : isCreaking ? stress01 * 8f : 0f;
            Color tint = isCreaking ? Color.Lerp(HealthyColor, BruisedColor, stress01) : HealthyColor;

            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i].localRotation = Quaternion.Euler(0f, 0f, baseAngles[i] - droopDeg);
                if (fingerRenderers[i] != null) fingerRenderers[i].color = tint;
            }
        }

        /// <summary>Einmaliger Pose-Wechsel beim "auf der Schulter gesnappt" (GDD 3.8).</summary>
        public void SetSnapped()
        {
            if (fingers == null) return;
            for (int i = 0; i < fingers.Length; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                fingers[i].localRotation = Quaternion.Euler(0f, 0f, baseAngles[i] * 0.3f - 55f * side);
                if (fingerRenderers[i] != null) fingerRenderers[i].color = BruisedColor;
            }
        }
    }
}
