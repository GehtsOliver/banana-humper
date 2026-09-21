using UnityEngine;

namespace BananaHumper.Config
{
    /// <summary>
    /// Tuning-Werte fuer den Kern-Loop (GDD Kapitel 3.6, Stand v0.9).
    /// Eine Asset-Instanz ist die einzige Quelle der Wahrheit, damit sich alles
    /// im Inspector tunen laesst, ohne Code anzufassen.
    ///
    /// Positionen stehen bewusst NICHT hier, sondern in der Szene (Stationen,
    /// Trailer, Reihen-Enden) - siehe docs/DECISIONS.md.
    /// </summary>
    [CreateAssetMenu(fileName = "BalanceConfig", menuName = "BananaHumper/Balance Config")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("Stationen (3.2)")]
        [Tooltip("Zeit, bis der Balken einer Station voll ist und die Staude faellt.")]
        public float cutSecondsMin = 8f;
        public float cutSecondsMax = 14f;
        [Tooltip("Pause, bis eine abgeerntete Station eine neue Staude aufhaengt.")]
        public float regrowSeconds = 3f;
        [Range(0f, 1f)] public float barWarningFraction = 0.7f;

        [Header("Fangen (3.3)")]
        [Tooltip("Falldauer von der Station bis auf Schulterhoehe - das Zeitfenster zum Hinlaufen.")]
        public float fallSeconds = 1.2f;
        [Tooltip("Bis zu diesem Abstand zur Fallstelle wird ueberhaupt gefangen.")]
        public float catchRadius = 1.0f;
        [Tooltip("Innerhalb dieses Abstands ist der Catch perfekt: kein Versatz, voller Lohn.")]
        public float perfectCatchWindow = 0.25f;
        [Tooltip("Ab diesem Anteil des Fangradius ist es nur noch ein Streifer.")]
        [Range(0f, 1f)] public float normalCatchFraction = 0.6f;
        [Tooltip("Lohnabzug fuer einen Streifer (0.25 = -25 %).")]
        [Range(0f, 1f)] public float grazePayoutPenalty = 0.25f;

        [Header("Trailer (3.5)")]
        public float trailerSpeed = 0.4f;
        [Tooltip("Ab diesem Abstand zum Trailer wird automatisch abgeliefert.")]
        public float deliveryRadius = 1.2f;

        [Header("Pendel beim Schleppen (3.4)")]
        // Balancieren ist seit v0.9 nicht mehr die Herausforderung, nur noch
        // spuerbares Gewicht - deshalb hoeheres damping und groesserer maxAngle
        // als in v0.8 (1.2 / 35 Grad).
        public float gravityOverLength = 4.0f;
        public float offsetTorque = 1.5f;
        public float damping = 2.5f;
        // Linke/rechte Maustaste, in BalanceController auf ~0.3s eingeschwungen.
        public float controlStrength = 1.2f;
        public float wobbleWalk = 0.4f;
        public float wobbleRun = 1.0f;
        public float stepFrequency = 4.0f;
        [Range(0f, 1f)] public float wobbleNoise = 0.15f;
        public float maxAngleDeg = 45f;
        [Range(0f, 1f)] public float redWarningFraction = 0.7f;
        [Tooltip("Wie weit der Versatz die Staude sichtbar zur Seite schiebt.")]
        public float offsetVisualShift = 0.75f;

        [Header("Umsetzen (3.4)")]
        public float repositionDuration = 0.6f;
        [Range(0f, 1f)] public float repositionOffsetReduction = 0.6f;
        public float repositionEnergyCost = 5f;
        public float repositionWobbleImpulse = 0.4f;

        [Header("Bewegung")]
        public float walkSpeed = 2.0f;
        public float runSpeedMultiplier = 1.6f;

        [Header("Energie (4.2)")]
        public float startEnergy = 100f;
        // Laufen ohne Staude kostet nichts (4.2): Energie ist ein Budget aus
        // getragenen Kilogramm mal Weg, das macht schwere Stauden zur Abwaegung.
        public float energyPerSecondBase = 2.0f;
        public float energyPerSecondPerWeight = 1f / 40f;
        public float runEnergyMultiplier = 1.6f;
        public float dropEnergyPenalty = 15f;

        [Header("Wirtschaft (4.3, 5.1)")]
        public float payoutPerKg = 0.1f;
        public float experiencePer10Kg = 1f;
        [Range(0f, 1f)] public float failedTripExperienceFraction = 0.5f;

        public float MaxAngleRad => maxAngleDeg * Mathf.Deg2Rad;
        public float RedWarningAngleRad => MaxAngleRad * redWarningFraction;

        public float RandomCutSeconds() => Random.Range(cutSecondsMin, cutSecondsMax);
    }
}
