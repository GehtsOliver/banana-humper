using System;
using System.Collections;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// "Auflegen" (GDD 3.2): while the Cutter's machete is falling, the player
    /// moves the shoulder under a shown centre-of-gravity marker. The resulting
    /// misalignment becomes the persistent `offset` the BalanceController fights
    /// for the rest of the trip.
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        public BalanceConfig config;
        public Transform shoulderMarker;
        public Transform targetMarker;

        public event Action<float, bool> OnPlacementResolved;

        float cutterWorldX;
        Coroutine running;

        public void Setup(float cutterX)
        {
            cutterWorldX = cutterX;
            if (shoulderMarker != null) shoulderMarker.position = new Vector3(cutterWorldX, shoulderMarker.position.y, 0f);
            SetActive(false);
        }

        public void BeginPlacement()
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(PlacementRoutine());
        }

        void SetActive(bool active)
        {
            // shoulderMarker ist der Spieler selbst (siehe GameBootstrap) - der bleibt
            // immer sichtbar. Nur die rote Zielmarkierung ist reine Auflegen-UI.
            if (targetMarker != null) targetMarker.gameObject.SetActive(active);
        }

        IEnumerator PlacementRoutine()
        {
            SetActive(true);

            float targetOffsetWorld = UnityEngine.Random.Range(-config.placementZoneHalfWidth, config.placementZoneHalfWidth);
            float targetX = cutterWorldX + targetOffsetWorld;
            if (targetMarker != null) targetMarker.position = new Vector3(targetX, targetMarker.position.y, 0f);

            // Relative Mausbewegung wie beim Balancieren (3.3), nicht die absolute
            // Cursorposition - der Cursor ist waehrend der Schicht gesperrt (siehe
            // ShiftController), Input.mousePosition liefert dann keine brauchbaren Werte mehr.
            float shoulderOffset = 0f;
            float shoulderX = cutterWorldX;
            float elapsed = 0f;
            while (elapsed < config.placementTelegraphSeconds)
            {
                float mouseDeltaX = Input.GetAxis("Mouse X");
                shoulderOffset = Mathf.Clamp(shoulderOffset + mouseDeltaX * config.placementMouseSensitivity, -config.placementZoneHalfWidth, config.placementZoneHalfWidth);
                shoulderX = cutterWorldX + shoulderOffset;
                if (shoulderMarker != null) shoulderMarker.position = new Vector3(shoulderX, shoulderMarker.position.y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            float rawOffset = (shoulderX - targetX) / Mathf.Max(0.01f, config.placementToleranceWorldUnits);
            float offset = Mathf.Clamp(rawOffset, -1f, 1f);
            bool sweetSpot = Mathf.Abs(offset) < config.sweetSpotOffset;

            SetActive(false);
            OnPlacementResolved?.Invoke(offset, sweetSpot);
        }
    }
}
