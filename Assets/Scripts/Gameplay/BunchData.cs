using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Eine Staude, die der Cutter auflegt (GDD 4.3). Gewicht und Laenge sind
    /// stufenlos zufaellig (vorher drei feste Stufen kurz/mittel/lang), damit
    /// sich jeder Trip spuerbar anders anfuehlt.
    ///
    /// Die Dicke wird nicht separat gewuerfelt, sondern ergibt sich aus Gewicht
    /// pro Laenge - sonst gaebe es 100-kg-Zwerge und federleichte Riesen. Sie
    /// ist rein optisch; fuer die Physik zaehlen Gewicht und Laenge.
    /// </summary>
    public class BunchData
    {
        /// <summary>
        /// Laengen-Skala, 1.0 = "mittlere" Staude. Alle Tuning-Startwerte aus
        /// GDD 3.6 sind auf diese mittlere Staude geeicht, deshalb verhaelt sie
        /// sich bei 1.0 exakt wie vor der Umstellung auf stufenlose Laengen.
        /// </summary>
        public const float ShortLengthScale = 0.65f;
        public const float MediumLengthScale = 1.0f;
        public const float LongLengthScale = 1.45f;

        /// <summary>GDD 3.4: weightFactor = Gewicht / 50, also ist 50 kg die Referenz-Staude.</summary>
        public const float ReferenceWeight = 50f;

        public readonly float Weight;
        public readonly float LengthScale;
        public readonly float Thickness;

        public BunchData(float weight, float lengthScale)
        {
            Weight = weight;
            LengthScale = lengthScale;
            // Wurzel statt linear: Gewicht waechst mit dem Volumen, eine
            // doppelt so schwere Staude ist nicht doppelt so dick.
            Thickness = Mathf.Clamp(Mathf.Sqrt(weight / lengthScale / ReferenceWeight), 0.7f, 1.4f);
        }

        /// <summary>0 = kuerzeste, 1 = laengste Staude - fuer Optik und Belastungskurve.</summary>
        public float LengthT => Mathf.InverseLerp(ShortLengthScale, LongLengthScale, LengthScale);

        /// <summary>Kurzbeschreibung fuers HUD, damit die stufenlose Variation ablesbar bleibt.</summary>
        public string Description
        {
            get
            {
                string length = LengthT < 0.33f ? "kurz" : LengthT < 0.66f ? "mittel" : "lang";
                string girth = Thickness < 0.9f ? "duenn" : Thickness > 1.15f ? "dick" : "normal";
                return $"{Weight:0} kg, {length}, {girth}";
            }
        }

        public static BunchData GenerateForDay(int day)
        {
            float t = Mathf.Clamp01((day - 1) / 87f);

            // Tag 1 ueberwiegend kurz/mittel, spaeter mehr lange Stauden (4.3).
            // Exponent > 1 zieht die Verteilung Richtung kurz, < 1 Richtung lang.
            float lengthBias = Mathf.Lerp(1.8f, 0.7f, t);
            float lengthRoll = Mathf.Pow(Random.value, lengthBias);
            float lengthScale = Mathf.Lerp(ShortLengthScale, LongLengthScale, lengthRoll);

            // Tag 1: 30-60 kg, steigt bis Tag 88: 60-100 kg (4.3)
            float min = Mathf.Lerp(30f, 60f, t);
            float max = Mathf.Lerp(60f, 100f, t);
            // Laengenkorreliert, damit lange Stauden im Schnitt schwerer sind
            // ("schwere und lange Stauden = mehr Geld", 4.3) - aber nur teil-
            // weise, damit es weiterhin kurz-und-dick und lang-und-duenn gibt.
            float weight = Random.Range(min, max) * Mathf.Lerp(0.85f, 1.15f, lengthRoll);

            return new BunchData(Mathf.Clamp(weight, min, max), lengthScale);
        }

        public int PayoutDollars(BalanceConfig config)
        {
            return Mathf.RoundToInt(Weight * config.payoutPerKg);
        }

        public float ExperienceValue(BalanceConfig config)
        {
            return Weight / 10f * config.experiencePer10Kg;
        }
    }
}
