using System;
using System.Collections;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// "Auflegen" (GDD 3.2): while the Cutter's machete is falling, the player
    /// moves the shoulder (A/D) under a shown centre-of-gravity marker. The
    /// resulting misalignment becomes the persistent `offset` the
    /// BalanceController fights for the rest of the trip. Mouse is intentionally
    /// not used here - it is reserved for running only (see ShiftController).
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
            SetTargetMarkerActive(false);
        }

        public void BeginPlacement()
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(PlacementRoutine());
        }

        /// <summary>
        /// Nur die Zielmarkierung (Schwerpunkt-Anzeige, GDD 3.2) blinkt kurz auf.
        /// shoulderMarker ist die Spielfigur selbst (siehe GameBootstrap) und muss
        /// jederzeit sichtbar bleiben - sie hier mit zu deaktivieren wuerde die
        /// gesamte Spielfigur samt Bananenstaude fuer den Rest des Trips ausblenden.
        /// </summary>
        void SetTargetMarkerActive(bool active)
        {
            if (targetMarker != null) targetMarker.gameObject.SetActive(active);
        }

        IEnumerator PlacementRoutine()
        {
            SetTargetMarkerActive(true);

            float targetOffsetWorld = UnityEngine.Random.Range(-config.placementZoneHalfWidth, config.placementZoneHalfWidth);
            float targetX = cutterWorldX + targetOffsetWorld;
            if (targetMarker != null) targetMarker.position = new Vector3(targetX, targetMarker.position.y, 0f);

            // A/D bewegen die Schulter (Design-Entscheidung, ersetzt Maus-Steuerung
            // aus GDD 3.2 [A]) - die Maus ist ausschliesslich fuers Rennen
            // reserviert (siehe ShiftController).
            float shoulderX = cutterWorldX;
            float elapsed = 0f;
            while (elapsed < config.placementTelegraphSeconds)
            {
                float moveInput = 0f;
                if (Input.GetKey(KeyCode.D)) moveInput += 1f;
                if (Input.GetKey(KeyCode.A)) moveInput -= 1f;
                shoulderX = Mathf.Clamp(shoulderX + moveInput * config.walkSpeed * Time.deltaTime,
                    cutterWorldX - config.placementZoneHalfWidth, cutterWorldX + config.placementZoneHalfWidth);
                if (shoulderMarker != null) shoulderMarker.position = new Vector3(shoulderX, shoulderMarker.position.y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            float rawOffset = (shoulderX - targetX) / Mathf.Max(0.01f, config.placementToleranceWorldUnits);
            float offset = Mathf.Clamp(rawOffset, -1f, 1f);
            bool sweetSpot = Mathf.Abs(offset) < config.sweetSpotOffset;

            SetTargetMarkerActive(false);
            OnPlacementResolved?.Invoke(offset, sweetSpot);
        }
    }
}
