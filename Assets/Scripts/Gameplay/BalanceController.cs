using System;
using System.Collections;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Pendel-Simulation der getragenen Staude (GDD 3.4). Seit v0.9 nur noch
    /// spuerbares Gewicht, nicht mehr die Kern-Herausforderung: Belastung und
    /// Snap sind ersatzlos entfallen, die Werte sind bewusst verzeihend.
    /// Kein Rigidbody - ein kleines eigenes Modell bleibt leichter zu tunen.
    /// </summary>
    public class BalanceController : MonoBehaviour
    {
        public BalanceConfig config;

        public event Action OnDropped;
        public event Action OnRepositionPerformed;

        BunchData currentBunch;
        float stepPhase;
        float pendingImpulse;
        float steerSmoothed;

        public float Theta { get; private set; }
        public float Omega { get; private set; }
        public float Offset { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsRepositioning { get; private set; }

        public bool IsRedWarning => Mathf.Abs(Theta) > config.RedWarningAngleRad;

        /// <summary>Startet das Tragen. <paramref name="initialOffset"/> kommt aus der Catch-Qualitaet (3.3).</summary>
        public void BeginCarry(BunchData bunch, float initialOffset)
        {
            currentBunch = bunch;
            Theta = 0f;
            Omega = 0f;
            Offset = initialOffset;
            // Zufaellige Start-Phase statt immer 0: sonst liefert Sin(stepPhase)
            // in jedem Trip die ersten ~0.75s (bei stepFrequency=4) denselben
            // deterministischen Schubs in dieselbe Richtung, und die Staude
            // kippt praktisch immer zur selben Seite.
            stepPhase = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            pendingImpulse = 0f;
            steerSmoothed = 0f;
            IsActive = true;
            IsRepositioning = false;
        }

        public void StopCarry()
        {
            IsActive = false;
        }

        /// <summary>Einen Frame weiterrechnen. Nur waehrend des Tragens aufrufen.</summary>
        public void Tick(float dt, bool isRunning)
        {
            if (!IsActive || IsRepositioning || currentBunch == null) return;

            float weightFactor = currentBunch.Weight / BunchData.ReferenceWeight;
            // GDD 3.4: "gravity / length" - lange Stauden kippen traeger
            // (langes Pendel), kurze schneller und zappeliger.
            float gravityTerm = config.gravityOverLength / currentBunch.LengthScale;
            float wobbleAmplitude = isRunning ? config.wobbleRun : config.wobbleWalk;
            stepPhase += (isRunning ? config.runSpeedMultiplier : 1f) * config.stepFrequency * dt;
            float noise = (UnityEngine.Random.value * 2f - 1f) * config.wobbleNoise;
            float wobble = wobbleAmplitude * (Mathf.Sin(stepPhase) * 0.5f + noise);

            // Linke/rechte Maustaste. Input.GetMouseButton liefert sofort volles
            // +-1 ohne Ramp, deshalb hier von Hand eingeschwungen - ohne das
            // schiesst ein einzelner Klick die Neigung sofort ueber.
            float steerTarget = 0f;
            if (Input.GetMouseButton(0)) steerTarget -= 1f;
            if (Input.GetMouseButton(1)) steerTarget += 1f;
            steerSmoothed = Mathf.MoveTowards(steerSmoothed, steerTarget, 3f * dt);

            float impulse = pendingImpulse;
            pendingImpulse = 0f;

            float alpha = gravityTerm * Mathf.Sin(Theta) * weightFactor
                        + config.offsetTorque * Offset * weightFactor
                        - config.damping * Omega
                        - config.controlStrength * steerSmoothed
                        + wobble
                        + impulse;

            Omega += alpha * dt;
            Theta += Omega * dt;

            if (Mathf.Abs(Theta) > config.MaxAngleRad)
            {
                IsActive = false;
                OnDropped?.Invoke();
            }
        }

        /// <summary>"Umsetzen" per E (3.4). Gibt false zurueck, wenn es gerade nicht geht.</summary>
        public bool TryBeginReposition()
        {
            if (!IsActive || IsRepositioning) return false;
            StartCoroutine(RepositionRoutine());
            return true;
        }

        IEnumerator RepositionRoutine()
        {
            IsRepositioning = true;
            Offset *= (1f - config.repositionOffsetReduction);
            pendingImpulse += (UnityEngine.Random.value * 2f - 1f) * config.repositionWobbleImpulse;
            OnRepositionPerformed?.Invoke();
            yield return new WaitForSeconds(config.repositionDuration);
            IsRepositioning = false;
        }

        /// <summary>Haken fuer Zufallsereignisse (Spinne, Regen-Rutscher, GDD Kapitel 7).</summary>
        public void ApplyImpulse(float impulse)
        {
            pendingImpulse += impulse;
        }
    }
}
