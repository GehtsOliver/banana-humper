using UnityEngine;

namespace BananaHumper.Util
{
    /// <summary>
    /// Erzeugt einfache 1x1-Rechteck-Sprites zur Laufzeit, damit der Graybox-
    /// Prototyp ohne importierte Grafik-Assets lauffaehig ist (GDD 8.1: erst
    /// Platzhalterformen, spaeter finaler Art-Stil).
    /// </summary>
    public static class SpriteFactory
    {
        static Sprite cachedSquare;

        public static Sprite Square()
        {
            if (cachedSquare != null) return cachedSquare;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            cachedSquare = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return cachedSquare;
        }

        public static SpriteRenderer CreateQuad(string name, Color color, Vector2 sizeWorldUnits, Transform parent, Vector3 localPosition, int sortingOrder = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Square();
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            go.transform.localScale = new Vector3(sizeWorldUnits.x, sizeWorldUnits.y, 1f);
            return sr;
        }
    }
}
