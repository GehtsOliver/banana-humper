using UnityEngine;
using BananaHumper.Util;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Kleine Dreck- oder Bananenspritzer bei einem Aufprall (GDD 8.4 "Juice").
    /// Prozedural aus denselben Vektorformen wie der Rest, damit kein
    /// Partikelsystem und keine Textur-Assets noetig sind.
    /// Raeumt sich selbst auf.
    /// </summary>
    public class SplashEffect : MonoBehaviour
    {
        struct Particle
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public Vector2 velocity;
            public float spin;
        }

        Particle[] particles;
        float lifetime;
        float elapsed;

        public static void Spawn(Vector3 position, Color color, int count = 8, float strength = 3f, float lifetime = 0.7f)
        {
            var go = new GameObject("Splash");
            go.transform.position = position;
            var effect = go.AddComponent<SplashEffect>();
            effect.Build(color, count, strength, lifetime);
        }

        void Build(Color color, int count, float strength, float life)
        {
            lifetime = life;
            particles = new Particle[count];

            for (int i = 0; i < count; i++)
            {
                float size = Random.Range(0.08f, 0.2f);
                var sr = SpriteFactory.CreateEllipse($"Bit{i}", color, new Vector2(size, size),
                    transform, Vector3.zero, sortingOrder: 12);

                // Halbkreis nach oben: Dreck spritzt vom Boden weg, nicht hinein.
                float angle = Random.Range(20f, 160f) * Mathf.Deg2Rad;
                float speed = strength * Random.Range(0.5f, 1.2f);

                particles[i] = new Particle
                {
                    transform = sr.transform,
                    renderer = sr,
                    velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                    spin = Random.Range(-360f, 360f),
                };
            }
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);

            for (int i = 0; i < particles.Length; i++)
            {
                var p = particles[i];
                p.velocity.y -= 9f * Time.deltaTime;
                p.transform.localPosition += (Vector3)(p.velocity * Time.deltaTime);
                p.transform.Rotate(0f, 0f, p.spin * Time.deltaTime);

                var c = p.renderer.color;
                c.a = 1f - t;
                p.renderer.color = c;

                particles[i] = p;
            }

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
