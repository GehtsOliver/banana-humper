using UnityEngine;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Ein Stein in der Reihe (GDD 3.9). Liegt als Objekt in der Szene und ist
    /// dort frei verschiebbar - wo Hindernisse stehen, ist Level-Design.
    ///
    /// Kein Collider, kein Rigidbody: Das Spiel rechnet durchgehend mit einer
    /// eigenen, flachen Simulation (X-Position plus Sprunghoehe), und dafuer
    /// reicht ein Abstands- und Hoehenvergleich.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        [Tooltip("Halbe Breite: So nah muss man dran sein, damit es zaehlt.")]
        public float halfWidth = 0.35f;
        [Tooltip("Ab dieser Sprunghoehe ueber dem Boden ist der Stein ueberwunden.")]
        public float clearHeight = 0.55f;

        public float PositionX => transform.position.x;

        /// <summary>Trifft der Spieler den Stein? <paramref name="height"/> ist die Hoehe ueber dem Boden.</summary>
        public bool Blocks(float playerX, float height)
        {
            return height < clearHeight && Mathf.Abs(playerX - PositionX) <= halfWidth;
        }
    }
}
