using UnityEngine;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Treibt die importierten Kenney-"Male Adventurer"-Posen (CC0, siehe
    /// docs/THIRD_PARTY_ASSETS.md) an. Bewusst kein Animator-Controller,
    /// sondern ein simpler Frame-Wechsel per Skript: Die Posen sind Einzel-
    /// Sprites, und ein Animator waere fuer den reinen Walk-Zyklus mehr
    /// Verwaltung als Nutzen. Die Sprite-Referenzen werden in der Szene
    /// zugewiesen (siehe Assets/Scripts/Editor/SceneSetupTool.cs).
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        public SpriteRenderer target;
        public Sprite idleSprite;
        public Sprite[] walkSprites;
        public Sprite hurtSprite;

        const float BaseFrameRate = 8f;

        float frameTimer;
        int frameIndex;
        bool walking;
        float speedMultiplier = 1f;

        public void SetWalking(bool isWalking, bool isRunning)
        {
            walking = isWalking;
            speedMultiplier = isRunning ? 1.6f : 1f;
        }

        public void SetFacing(bool facingLeft)
        {
            if (target != null) target.flipX = facingLeft;
        }

        /// <summary>Kurze Reaktionspose bei "gefallen" (GDD 3.8) - wird von der naechsten Bewegung ueberschrieben.</summary>
        public void ShowHurt()
        {
            if (target != null && hurtSprite != null) target.sprite = hurtSprite;
        }

        void Update()
        {
            if (target == null) return;
            if (!walking || walkSprites == null || walkSprites.Length == 0)
            {
                if (idleSprite != null) target.sprite = idleSprite;
                frameTimer = 0f;
                frameIndex = 0;
                return;
            }

            frameTimer += Time.deltaTime * BaseFrameRate * speedMultiplier;
            if (frameTimer >= 1f)
            {
                frameTimer -= 1f;
                frameIndex = (frameIndex + 1) % walkSprites.Length;
            }
            // Fallback auf idleSprite statt eines moeglichen null-Eintrags in
            // walkSprites - sonst blinkt die Figur pro Frame unsichtbar, falls
            // eine einzelne Walk-Pose nicht geladen werden konnte.
            var frame = walkSprites[frameIndex];
            target.sprite = frame != null ? frame : idleSprite;
        }
    }
}
