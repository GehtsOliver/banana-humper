using UnityEngine;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Folgt dem Humper durch die Reihe und klemmt an den Paddock-Raendern, damit
    /// nie ins Leere gefilmt wird. Erst dadurch kann das Paddock groesser sein als
    /// ein Bildschirm (GDD 3.2).
    ///
    /// Weil damit Stationen aus dem Bild wandern, zeigt das HUD deren Geduld am
    /// Bildrand an - ohne das gaebe es nichts mehr zu priorisieren.
    ///
    /// Enthaelt auch das Kamerawackeln (GDD 8.4): Es gehoert hierher, weil es
    /// dieselbe Position beeinflusst und sonst zwei Skripte darum streiten.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public Transform target;

        [Header("Grenzen der Reihe")]
        public float minX = -8f;
        public float maxX = 32f;

        [Tooltip("Wie schnell die Kamera nachzieht. Hoeher = straffer.")]
        public float followLerp = 4f;

        Camera cam;
        float shakeTimer;
        float shakeDuration;
        float shakeStrength;
        float baseY;
        float baseZ;

        void Awake()
        {
            cam = GetComponent<Camera>();
            baseY = transform.position.y;
            baseZ = transform.position.z;
        }

        void LateUpdate()
        {
            // LateUpdate, damit die Kamera die Bewegung dieses Frames sieht und
            // nicht immer einen Frame hinterherhinkt.
            float desiredX = target != null ? target.position.x : transform.position.x;

            float halfWidth = cam.orthographicSize * cam.aspect;
            if (maxX - minX <= halfWidth * 2f)
            {
                // Paddock passt komplett ins Bild - dann gar nicht erst wackeln
                // lassen, sondern mittig stehen bleiben.
                desiredX = (minX + maxX) * 0.5f;
            }
            else
            {
                desiredX = Mathf.Clamp(desiredX, minX + halfWidth, maxX - halfWidth);
            }

            float x = Mathf.Lerp(transform.position.x, desiredX, 1f - Mathf.Exp(-followLerp * Time.deltaTime));
            transform.position = new Vector3(x, baseY, baseZ) + CurrentShakeOffset();
        }

        Vector3 CurrentShakeOffset()
        {
            if (shakeTimer <= 0f) return Vector3.zero;

            shakeTimer -= Time.deltaTime;
            float remaining = shakeDuration > 0f ? Mathf.Clamp01(shakeTimer / shakeDuration) : 0f;
            float amount = shakeStrength * remaining;
            return new Vector3(Random.Range(-amount, amount), Random.Range(-amount, amount), 0f);
        }

        /// <summary>Kurzes Wackeln, z.B. wenn eine Staude im Matsch landet (GDD 8.4).</summary>
        public void Shake(float strength, float seconds)
        {
            // Ein staerkerer Ruckler darf einen schwaecheren ueberschreiben,
            // umgekehrt nicht - sonst schluckt ein Dauerstolpern den Aufprall.
            if (shakeTimer > 0f && strength < shakeStrength) return;
            shakeStrength = strength;
            shakeDuration = seconds;
            shakeTimer = seconds;
        }
    }
}
