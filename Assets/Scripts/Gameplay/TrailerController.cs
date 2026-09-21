using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Der Trailer, den der Traktor langsam die Reihe entlangzieht (GDD 3.5,
    /// entspricht dem echten Ablauf aus 1.4). Dadurch aendern sich die Wege
    /// laufend: Eine Station, die eben noch guenstig lag, ist zwei Stauden
    /// spaeter weit weg.
    /// </summary>
    public class TrailerController : MonoBehaviour
    {
        public BalanceConfig config;

        [Header("Grenzen der Reihe")]
        public float minX = -4f;
        public float maxX = 14f;

        int direction = 1;

        public float PositionX => transform.position.x;

        public void Tick(float dt)
        {
            var p = transform.position;
            p.x += direction * config.trailerSpeed * dt;

            // Am Reihenende umkehren (GDD 3.5 [A]) - die Alternative "Reihe zu
            // Ende, Schicht zu Ende" steht dort als offener Punkt.
            if (p.x >= maxX) { p.x = maxX; direction = -1; }
            else if (p.x <= minX) { p.x = minX; direction = 1; }

            transform.position = p;
        }

        public bool IsInDeliveryRange(float playerX)
        {
            return Mathf.Abs(playerX - PositionX) <= config.deliveryRadius;
        }
    }
}
