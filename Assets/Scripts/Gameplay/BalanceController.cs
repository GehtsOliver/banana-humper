using System;
using System.Collections;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// The pendulum/stress simulation from GDD 3.4 and 3.5. No Rigidbody -
    /// this is a small hand-rolled model so it stays easy to tune from
    /// BalanceConfig.
    /// </summary>
    public class BalanceController : MonoBehaviour
    {
        public BalanceConfig config;

        public event Action OnFallen;
        public event Action OnSnapped;
        public event Action OnRepositionPerformed;

        BunchData currentBunch;
        float stepPhase;
        float pendingImpulse;

        public float Theta { get; private set; }
        public float Omega { get; private set; }
        public float Stress { get; private set; }
        public float Offset { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsRepositioning { get; private set; }
        public bool HasFailed { get; private set; }

        public bool IsRedWarning => Mathf.Abs(Theta) > config.RedWarningAngleRad;
        public bool IsCreaking => Stress >= config.stressCreakWarning;
        public bool IsBending => Stress >= config.stressBendWarning;

        public void BeginTrip(BunchData bunch, float initialOffset)
        {
            currentBunch = bunch;
            Theta = 0f;
            Omega = 0f;
            Stress = 0f;
            Offset = initialOffset;
            // Zufaellige Start-Phase statt immer 0: sonst liefert Sin(stepPhase)
            // in jedem einzelnen Trip die ersten ~0.75s (bei stepFrequency=4)
            // denselben deterministischen Schubs in dieselbe Richtung, bevor die
            // Schwingung je ins Negative dreht - kombiniert mit der instabilen
            // Sin(Theta)-Schwerkraft-Rueckkopplung kippt die Staude dadurch
            // praktisch immer zur selben Seite, egal wie stark gegengelenkt wird.
            stepPhase = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
            pendingImpulse = 0f;
            IsActive = true;
            IsRepositioning = false;
            HasFailed = false;
        }

        public void StopTrip()
        {
            IsActive = false;
        }

        /// <summary>Advances the simulation by one frame. Call only while carrying.</summary>
        public void Tick(float dt, bool isRunning)
        {
            if (!IsActive || IsRepositioning || currentBunch == null) return;

            float weightFactor = currentBunch.Weight / 50f;
            float wobbleAmplitude = isRunning ? config.wobbleRun : config.wobbleWalk;
            stepPhase += (isRunning ? config.runSpeedMultiplier : 1f) * config.stepFrequency * dt;
            float noise = (UnityEngine.Random.value * 2f - 1f) * config.wobbleNoise;
            float wobble = wobbleAmplitude * (Mathf.Sin(stepPhase) * 0.5f + noise);

            // W/S (Unity-"Vertical"-Achse) statt Maus - Maus ist jetzt
            // ausschliesslich fuers Rennen reserviert (siehe ShiftController).
            float steerInput = Input.GetAxis("Vertical");

            float impulse = pendingImpulse;
            pendingImpulse = 0f;

            float alpha = config.gravityOverLength * Mathf.Sin(Theta) * weightFactor
                        + config.offsetTorque * Offset * weightFactor
                        - config.damping * Omega
                        - config.controlStrength * steerInput
                        + wobble
                        + impulse;

            Omega += alpha * dt;
            Theta += Omega * dt;

            float haltekraft = Mathf.Abs(Offset) + Mathf.Max(0f, Mathf.Abs(Theta) - config.ComfortAngleRad) / config.MaxAngleRad;
            float lengthFactor = config.LengthFactor(currentBunch.Length);
            if (haltekraft > config.haltekraftThreshold)
            {
                Stress += haltekraft * lengthFactor * weightFactor * config.stressRate * dt;
            }
            else
            {
                Stress -= config.recoveryRate * dt;
            }
            Stress = Mathf.Clamp(Stress, 0f, 100f);

            if (Mathf.Abs(Theta) > config.MaxAngleRad)
            {
                Fail(snapped: false);
                return;
            }
            if (Stress >= 100f)
            {
                Fail(snapped: true);
            }
        }

        void Fail(bool snapped)
        {
            IsActive = false;
            HasFailed = true;
            if (snapped) OnSnapped?.Invoke();
            else OnFallen?.Invoke();
        }

        /// <summary>Rechtsklick "Umsetzen" (3.5). Returns false if not currently possible.</summary>
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

        /// <summary>Hook for future random events (Spinne, Regen-Rutscher, GDD Kapitel 7).</summary>
        public void ApplyImpulse(float impulse)
        {
            pendingImpulse += impulse;
        }
    }
}
