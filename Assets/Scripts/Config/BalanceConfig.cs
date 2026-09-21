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
        [Header("Cutter-Geduld (3.2)")]
        // Der Balken ueber dem Cutter ist seine Geduld: Laeuft sie ab, schlaegt
        // er ab, egal wo der Humper gerade steht. Steht der Humper vorher
        // bereit, schlaegt er sofort ab - das ist der freiwillige, sichere Weg.
        [Tooltip("Basis-Geduld eines Cutters. Je Station mit dem Temperament multipliziert.")]
        public float patienceSecondsMin = 8f;
        public float patienceSecondsMax = 14f;
        [Tooltip("So lange muss man still unter der Staude stehen, damit der Cutter frueher abschlaegt.")]
        public float readyToCutSeconds = 0.25f;
        [Tooltip("Wie schnell ein Cutter zur naechsten Pflanze geht. Langsamer als der Humper, damit man ihn einholen kann.")]
        public float cutterWalkSpeed = 1.2f;
        [Tooltip("Pause, bis an einer abgeernteten Pflanze wieder etwas haengt.")]
        public float regrowSeconds = 6f;

        [Header("Pflanzen (3.2)")]
        // Abstand zwischen zwei Pflanzen. Zu dicht hat zwei Nachteile: Das Feld
        // wirkt zugewuchert, und zwischen die Pflanzen passt kein Stein mehr
        // (siehe rockMinDistance) - bei Abstand 3.5 liegt die Mitte einer
        // Luecke 1.75 von beiden Pflanzen weg und damit knapp ausserhalb.
        public float plantSpacingMin = 3.5f;
        public float plantSpacingMax = 6.0f;

        [Header("Paddock-Groesse")]
        [Tooltip("Grundbreite der Reihe plus Zuschlag je angeheuertem Cutter - das Feld waechst mit der Mannschaft.")]
        public float paddockBaseWidth = 12f;
        public float paddockWidthPerCutter = 5f;
        [Range(0f, 1f)] public float barWarningFraction = 0.7f;

        [Header("Temperamente (3.2)")]
        [Tooltip("Multiplikator auf die Geduld - kleiner heisst ungeduldiger.")]
        public float patienceImpatient = 0.55f;
        public float patienceNormal = 1.0f;
        public float patiencePatient = 1.7f;

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
        [Tooltip("Tempo, wenn der Trailer weiterzieht. Bewusst langsam - er soll schleichen, nicht davonfahren.")]
        public float trailerSpeed = 0.5f;
        [Tooltip("Unter so vielen reifen Stauden im Abschnitt zieht der Trailer weiter.")]
        public int trailerAdvanceRipeThreshold = 2;
        [Tooltip("Radius, der als 'aktueller Abschnitt' um den Trailer zaehlt.")]
        public float trailerSectionRadius = 9f;
        [Tooltip("Ab diesem Abstand zum Trailer wird automatisch abgeliefert.")]
        public float deliveryRadius = 1.2f;

        [Header("Pendel beim Schleppen (3.4)")]
        [Tooltip("Aus: Die Staude sitzt fest auf der Schulter, kein Wackeln, kein Fallenlassen, " +
                 "keine Maustasten. Der Kern ist Fangen und Route - das Balancieren war zuletzt " +
                 "nur noch Beiwerk und lenkte davon ab (Nutzerentscheidung, siehe docs/DECISIONS.md).")]
        public bool balancingEnabled;

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

        [Header("Springen und Hindernisse (3.9)")]
        [Tooltip("Wie viele Steine pro Schicht zufaellig in der Reihe liegen.")]
        public int rockCountMin = 2;
        public int rockCountMax = 3;
        // Mindestabstand eines Steins zu einer Pflanze. Muss kleiner sein als
        // der halbe kleinste Pflanzenabstand, sonst findet sich nie eine
        // gueltige Stelle und es entstehen gar keine Steine. 1.6 haelt den
        // Stein zugleich ausserhalb des Fangradius (1.0), damit er das Fangen
        // nicht unfair macht.
        public float rockMinDistance = 1.6f;
        // 7.0 bei jumpGravity 20 ergibt 1,22 m Sprunghoehe und 0,7 s Flugzeit.
        // Ueber dem hoechsten Stein (0,6 m) ist man davon rund 0,5 s, im Gehen
        // also etwa 1,0 m Strecke - genug Puffer fuer einen 0,76 m breiten
        // Stein. Mit 6.5 war das Fenster fast genau so breit wie der Stein.
        public float jumpVelocity = 7.0f;
        public float jumpGravity = 20f;
        [Tooltip("Wie lange man nach einem Stolperer gebremst ist.")]
        public float stumbleSeconds = 0.45f;
        [Range(0f, 1f)] public float stumbleSpeedFactor = 0.35f;
        public float stumbleEnergyCost = 4f;
        [Tooltip("Wackel-Impuls auf die getragene Staude beim Stolpern.")]
        public float stumbleWobbleImpulse = 1.6f;

        [Header("Energie (4.2)")]
        // Alles kostet Energie, gestaffelt nach Anstrengung: Dasein am
        // wenigsten, Laufen mehr, Rennen deutlich mehr, Schleppen am meisten.
        // Mit den Startwerten unten und typischer Spielweise (grob halb leer
        // laufen, ein Drittel schleppen) haelt eine Schicht rund 75 Sekunden -
        // passend zu den 45-90 s aus GDD Kapitel 2.
        public float startEnergy = 110f;
        [Tooltip("Grundverbrauch pro Sekunde, auch im Stehen.")]
        public float energyIdlePerSecond = 0.15f;
        [Tooltip("Zusaetzlich pro Sekunde beim Laufen. Sinkt mit Ausdauer.")]
        public float energyWalkPerSecond = 0.6f;
        [Tooltip("Faktor auf den Laufverbrauch beim Rennen.")]
        public float runEnergyMultiplier = 1.8f;
        [Tooltip("Zusaetzlich pro Sekunde mit Staude, unabhaengig vom Gewicht. Sinkt mit Staerke.")]
        public float energyCarryPerSecond = 1.4f;
        [Tooltip("Zusaetzlich pro Sekunde je Kilogramm. Sinkt mit Staerke.")]
        public float energyCarryPerWeight = 1f / 50f;
        public float dropEnergyPenalty = 15f;

        [Header("Wirtschaft (4.3, 5.1)")]
        public float payoutPerKg = 0.1f;
        public float experiencePer10Kg = 1f;
        [Range(0f, 1f)] public float failedTripExperienceFraction = 0.5f;

        public float MaxAngleRad => maxAngleDeg * Mathf.Deg2Rad;
        public float RedWarningAngleRad => MaxAngleRad * redWarningFraction;

        public float RandomPatienceSeconds(float temperamentMultiplier)
        {
            return Random.Range(patienceSecondsMin, patienceSecondsMax) * temperamentMultiplier;
        }
    }
}
