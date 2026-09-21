using UnityEngine;

namespace BananaHumper.Gameplay
{
    /// <summary>
    /// Eine Bananenstaude im Paddock (GDD 3.2). Pflanzen stehen zufaellig
    /// verteilt und sind die Orte, an denen geschnitten wird - die Cutter
    /// wandern zwischen ihnen umher.
    ///
    /// Eine abgeerntete Pflanze braucht Zeit, bis wieder etwas dran haengt.
    /// Das verhindert, dass ein Cutter an derselben Pflanze kleben bleibt, und
    /// schiebt ihn weiter ins Feld.
    /// </summary>
    public class Plant : MonoBehaviour
    {
        [Tooltip("Hoehe, in der die Staude haengt und von der sie faellt.")]
        public Transform bunchAnchor;
        public BananaBunchVisual bunchVisual;

        float regrowSeconds;
        float regrowTimer;
        int day = 1;

        public BunchData Bunch { get; private set; }
        public bool HasBunch => Bunch != null;
        /// <summary>Welcher Cutter diese Pflanze angesteuert hat - verhindert, dass zwei dieselbe nehmen.</summary>
        public Cutter ClaimedBy { get; set; }
        public bool IsFree => HasBunch && ClaimedBy == null;
        public float PositionX => transform.position.x;
        public Vector3 BunchPosition => bunchAnchor != null ? bunchAnchor.position : transform.position;

        public void Initialize(int day, float regrowSeconds)
        {
            this.day = day;
            this.regrowSeconds = regrowSeconds;
            Grow();
        }

        public void Tick(float dt)
        {
            if (HasBunch) return;

            regrowTimer -= dt;
            if (regrowTimer <= 0f) Grow();
        }

        /// <summary>Der Cutter schlaegt ab: Staude weg, Pflanze treibt neu aus.</summary>
        public BunchData Harvest()
        {
            var harvested = Bunch;
            Bunch = null;
            ClaimedBy = null;
            regrowTimer = regrowSeconds;
            if (bunchVisual != null) bunchVisual.gameObject.SetActive(false);
            return harvested;
        }

        void Grow()
        {
            Bunch = BunchData.GenerateForDay(day);
            if (bunchVisual != null)
            {
                bunchVisual.gameObject.SetActive(true);
                bunchVisual.Build(Bunch);
            }
        }
    }
}
