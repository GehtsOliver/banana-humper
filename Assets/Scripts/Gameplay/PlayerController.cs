using System;
using System.Collections.Generic;
using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Freie Bewegung in der Reihe inklusive Springen (GDD 3.1, 3.9). Bis v0.8
    /// steckte das Laufen in der Trip-Statemachine des ShiftControllers; seit
    /// v0.9 laeuft der Spieler durchgehend frei zwischen den Stationen, also
    /// gehoert es hierher.
    ///
    /// Die Figur meldet nur, dass sie gestolpert ist - was das kostet, entscheidet
    /// der ShiftController. Getickt wird von dort, damit die Figur am
    /// Schichtende steht.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public BalanceConfig config;
        [Tooltip("Staude auf der Schulter - beim Fangen eingeblendet, beim Abliefern aus.")]
        public BananaBunchVisual shoulderBunch;
        public PlayerAnimator animator;

        [Header("Grenzen der Reihe")]
        public float minX = -5f;
        public float maxX = 15f;

        [Header("Hindernisse")]
        [Tooltip("Steine in der Reihe (GDD 3.9). Werden von GameBootstrap aus der Szene eingesammelt.")]
        public List<Obstacle> obstacles = new List<Obstacle>();

        /// <summary>Gegen einen Stein gelaufen - der ShiftController haengt die Folgen dran.</summary>
        public event Action OnStumbled;

        public BunchData CarriedBunch { get; private set; }
        public bool IsCarrying => CarriedBunch != null;
        public bool IsRunning { get; private set; }
        public bool IsMoving { get; private set; }
        public bool IsGrounded => heightAboveGround <= 0.001f;
        /// <summary>Hoehe ueber dem Boden - 0 heisst am Boden.</summary>
        public float Height => heightAboveGround;
        public float PositionX => transform.position.x;
        /// <summary>Schulterhoehe am Boden. Stauden fallen immer hierher, auch wenn gerade gesprungen wird.</summary>
        public float GroundY => groundY;

        /// <summary>Wo die Staude ohne Versatz sitzt - in der Szene eingestellt, also nicht hart setzen.</summary>
        Vector3 shoulderRestPosition;
        float groundY;
        float heightAboveGround;
        float verticalVelocity;
        float stumbleTimer;

        void Awake()
        {
            if (shoulderBunch != null) shoulderRestPosition = shoulderBunch.transform.localPosition;
            groundY = transform.position.y;
        }

        public void Tick(float dt)
        {
            if (stumbleTimer > 0f) stumbleTimer -= dt;

            float moveInput = 0f;
            if (Input.GetKey(KeyCode.D)) moveInput += 1f;
            if (Input.GetKey(KeyCode.A)) moveInput -= 1f;

            IsMoving = moveInput != 0f;
            IsRunning = IsMoving && Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            {
                verticalVelocity = config.jumpVelocity;
            }

            TickVertical(dt);

            if (IsMoving)
            {
                float speed = config.walkSpeed * (IsRunning ? config.runSpeedMultiplier : 1f);
                if (stumbleTimer > 0f) speed *= config.stumbleSpeedFactor;

                float nextX = Mathf.Clamp(transform.position.x + moveInput * speed * dt, minX, maxX);
                if (BlockedAt(nextX))
                {
                    // Nicht hart blockieren: Durchstolpern ist lustiger als eine
                    // unsichtbare Wand und passt zu "Scheitern ist lustig" (1.6).
                    Stumble();
                }
                SetPositionX(nextX);
                animator?.SetFacing(moveInput < 0f);
            }

            // In der Luft laeuft der Walk-Zyklus nicht weiter.
            animator?.SetWalking(IsMoving && IsGrounded, IsRunning);
        }

        void TickVertical(float dt)
        {
            if (IsGrounded && verticalVelocity <= 0f)
            {
                heightAboveGround = 0f;
                verticalVelocity = 0f;
                return;
            }

            verticalVelocity -= config.jumpGravity * dt;
            heightAboveGround = Mathf.Max(0f, heightAboveGround + verticalVelocity * dt);

            var p = transform.position;
            p.y = groundY + heightAboveGround;
            transform.position = p;
        }

        void SetPositionX(float x)
        {
            var p = transform.position;
            p.x = x;
            transform.position = p;
        }

        bool BlockedAt(float x)
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle != null && obstacle.Blocks(x, heightAboveGround)) return true;
            }
            return false;
        }

        void Stumble()
        {
            // Cooldown ueber denselben Timer: Solange man noch stolpert, loest
            // derselbe Stein nicht jeden Frame erneut aus.
            if (stumbleTimer > 0f) return;
            stumbleTimer = config.stumbleSeconds;
            OnStumbled?.Invoke();
        }

        public void TakeBunch(BunchData bunch)
        {
            CarriedBunch = bunch;
            if (shoulderBunch != null)
            {
                shoulderBunch.gameObject.SetActive(true);
                shoulderBunch.Build(bunch);
                shoulderBunch.transform.localRotation = Quaternion.identity;
                shoulderBunch.transform.localPosition = shoulderRestPosition;
            }
        }

        /// <summary>Staude ist weg (abgeliefert oder fallen gelassen).</summary>
        public void ClearBunch()
        {
            CarriedBunch = null;
            if (shoulderBunch != null) shoulderBunch.gameObject.SetActive(false);
        }

        /// <summary>Neigung und Versatz der getragenen Staude anzeigen (GDD 3.4).</summary>
        public void ApplyCarryPose(float thetaRad, float offset)
        {
            if (shoulderBunch == null) return;
            var t = shoulderBunch.transform;
            t.localRotation = Quaternion.Euler(0f, 0f, -thetaRad * Mathf.Rad2Deg);
            t.localPosition = shoulderRestPosition + new Vector3(offset * config.offsetVisualShift, 0f, 0f);
        }

        public void ShowHurt() => animator?.ShowHurt();

        public void StandStill() => animator?.SetWalking(false, false);
    }
}
