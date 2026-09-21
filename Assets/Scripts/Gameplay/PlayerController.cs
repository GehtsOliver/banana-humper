using UnityEngine;
using BananaHumper.Config;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Freie Bewegung in der Reihe (GDD 3.1). Bis v0.8 steckte das Laufen in
    /// der Trip-Statemachine des ShiftControllers; seit v0.9 laeuft der Spieler
    /// durchgehend frei zwischen den Stationen, also gehoert es hierher.
    ///
    /// Getickt wird von ShiftController, damit die Figur am Schichtende steht.
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

        public BunchData CarriedBunch { get; private set; }
        public bool IsCarrying => CarriedBunch != null;
        public bool IsRunning { get; private set; }
        public bool IsMoving { get; private set; }
        public float PositionX => transform.position.x;

        /// <summary>Wo die Staude ohne Versatz sitzt - in der Szene eingestellt, also nicht hart setzen.</summary>
        Vector3 shoulderRestPosition;

        void Awake()
        {
            if (shoulderBunch != null) shoulderRestPosition = shoulderBunch.transform.localPosition;
        }

        public void Tick(float dt)
        {
            float moveInput = 0f;
            if (Input.GetKey(KeyCode.D)) moveInput += 1f;
            if (Input.GetKey(KeyCode.A)) moveInput -= 1f;

            IsMoving = moveInput != 0f;
            IsRunning = IsMoving && Input.GetKey(KeyCode.LeftShift);

            if (IsMoving)
            {
                float speed = config.walkSpeed * (IsRunning ? config.runSpeedMultiplier : 1f);
                var p = transform.position;
                p.x = Mathf.Clamp(p.x + moveInput * speed * dt, minX, maxX);
                transform.position = p;
                animator?.SetFacing(moveInput < 0f);
            }

            animator?.SetWalking(IsMoving, IsRunning);
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
