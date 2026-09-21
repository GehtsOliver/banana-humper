using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Der Trailer, den der Traktor die Reihe entlangzieht (GDD 3.5, entspricht
    /// dem echten Ablauf aus 1.4).
    ///
    /// Er faehrt **nicht** staendig hin und her, sondern steht, solange im
    /// aktuellen Abschnitt noch genug Stauden haengen. Erst wenn die Bananen
    /// zur Neige gehen, zieht er langsam weiter - und die Crew zieht mit. Die
    /// Richtung wird pro Schicht ausgewuerfelt, damit sich nicht jede Schicht
    /// gleich anfuehlt.
    /// </summary>
    public class TrailerController : MonoBehaviour
    {
        public BalanceConfig config;

        /// <summary>+1 = nach rechts, -1 = nach links. Pro Schicht neu gewuerfelt.</summary>
        public int Direction { get; private set; } = 1;
        public bool IsMoving { get; private set; }
        public float PositionX => transform.position.x;

        public void BeginShift(float startX)
        {
            Direction = Random.value < 0.5f ? -1 : 1;
            IsMoving = false;
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        }

        /// <summary>
        /// <paramref name="ripeNearby"/>: Wie viele Stauden im aktuellen
        /// Abschnitt noch haengen. Das entscheidet der ShiftController, weil nur
        /// er das Feld kennt.
        /// </summary>
        public void Tick(float dt, int ripeNearby)
        {
            IsMoving = ripeNearby < config.trailerAdvanceRipeThreshold;
            if (!IsMoving) return;

            var p = transform.position;
            p.x += Direction * config.trailerSpeed * dt;
            transform.position = p;
        }

        public bool IsInDeliveryRange(float playerX)
        {
            return Mathf.Abs(playerX - PositionX) <= config.deliveryRadius;
        }
    }
}
