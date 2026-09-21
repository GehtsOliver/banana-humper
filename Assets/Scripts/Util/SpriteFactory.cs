using System.Collections.Generic;
using UnityEngine;

namespace BananaHumper.Util
{
    /// <summary>
    /// Baut Sprite-Objekte auf (GDD 8.1) - wird sowohl vom Editor-Tool beim
    /// Anlegen der Szene als auch von GameBootstrap zur Laufzeit benutzt. Zwei
    /// Quellen:
    /// - <see cref="LoadSprite"/> laedt importierte CC0-Vektor-Kunst (Kenney.nl,
    ///   siehe docs/THIRD_PARTY_ASSETS.md) aus Resources/Art.
    /// - <see cref="CreateRoundedQuad"/>/<see cref="CreateEllipse"/> zeichnen
    ///   kantengeglaettete Vektorformen (abgerundete Rechtecke, Ellipsen) fuer
    ///   Elemente, fuer die es keine passenden freien Assets gibt (Bananenstaude,
    ///   Trailer, Cutter-Figur) - immer noch "Vektor" statt hartem Rechteck.
    ///
    /// Wichtig fuer den Hybrid-Aufbau: Die prozeduralen Formen erzeugen ihre
    /// Textur zur Laufzeit. Solche Sprites sind keine Assets und lassen sich
    /// deshalb nicht in einer Szenendatei speichern - Objekte daraus muessen
    /// beim Start gebaut werden, waehrend <see cref="LoadSprite"/>-Objekte
    /// (echte Assets) dauerhaft in der Szene liegen koennen.
    /// </summary>
    public static class SpriteFactory
    {
        static Sprite cachedSquare;
        static readonly Dictionary<string, Sprite> loadedSpriteCache = new Dictionary<string, Sprite>();
        static readonly Dictionary<(int w, int h, int radiusTenths), Texture2D> roundedTexCache = new Dictionary<(int, int, int), Texture2D>();
        static readonly Dictionary<(int w, int h), Texture2D> ellipseTexCache = new Dictionary<(int, int), Texture2D>();

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

        /// <summary>Laedt ein importiertes Sprite aus Assets/Resources/Art/... (ohne Endung, z.B. "Art/Player/idle"). Gibt null zurueck statt zu werfen, falls das Asset (noch) nicht importiert ist.</summary>
        public static Sprite LoadSprite(string resourcePath)
        {
            if (loadedSpriteCache.TryGetValue(resourcePath, out var cached)) return cached;
            var sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite == null) Debug.LogWarning($"SpriteFactory: Resource '{resourcePath}' nicht gefunden - Objekt bleibt unsichtbar.");
            loadedSpriteCache[resourcePath] = sprite;
            return sprite;
        }

        /// <summary>
        /// Platziert ein importiertes Sprite in der Welt. Genau eine von worldWidth/worldHeight
        /// angeben, die andere Achse folgt dem Seitenverhaeltnis der Bildquelle.
        /// </summary>
        public static SpriteRenderer CreateSprite(string name, Sprite sprite, Transform parent, Vector3 localPosition, int sortingOrder = 0, float worldWidth = -1f, float worldHeight = -1f, bool flipX = false, Color? tint = null)
        {
            if (sprite == null) return null;

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.flipX = flipX;
            if (tint.HasValue) sr.color = tint.Value;

            Vector2 native = sprite.bounds.size;
            float scale = 1f;
            if (worldHeight > 0f && native.y > 0f) scale = worldHeight / native.y;
            else if (worldWidth > 0f && native.x > 0f) scale = worldWidth / native.x;
            go.transform.localScale = new Vector3(scale, scale, 1f);
            return sr;
        }

        /// <summary>Kantengeglaettetes abgerundetes Rechteck als eigenstaendiges Sprite (statt hartem Quad).</summary>
        public static SpriteRenderer CreateRoundedQuad(string name, Color color, Vector2 worldSize, float cornerRadius01, Transform parent, Vector3 localPosition, int sortingOrder = 0)
        {
            const int texW = 48;
            int texH = Mathf.Clamp(Mathf.RoundToInt(texW * worldSize.y / Mathf.Max(0.01f, worldSize.x)), 8, 256);
            float radiusPx = Mathf.Min(texW, texH) * 0.5f * Mathf.Clamp01(cornerRadius01);
            var key = (texW, texH, Mathf.RoundToInt(radiusPx * 10f));
            if (!roundedTexCache.TryGetValue(key, out var tex))
            {
                tex = BuildRoundedRectTexture(texW, texH, radiusPx);
                roundedTexCache[key] = tex;
            }
            float ppu = texW / Mathf.Max(0.01f, worldSize.x);
            var sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), ppu);
            return CreateFlatSprite(name, sprite, color, parent, localPosition, sortingOrder);
        }

        /// <summary>Kantengeglaettete Ellipse als eigenstaendiges Sprite.</summary>
        public static SpriteRenderer CreateEllipse(string name, Color color, Vector2 worldSize, Transform parent, Vector3 localPosition, int sortingOrder = 0)
        {
            const int texW = 48;
            int texH = Mathf.Clamp(Mathf.RoundToInt(texW * worldSize.y / Mathf.Max(0.01f, worldSize.x)), 8, 256);
            var key = (texW, texH);
            if (!ellipseTexCache.TryGetValue(key, out var tex))
            {
                tex = BuildEllipseTexture(texW, texH);
                ellipseTexCache[key] = tex;
            }
            float ppu = texW / Mathf.Max(0.01f, worldSize.x);
            var sprite = Sprite.Create(tex, new Rect(0, 0, texW, texH), new Vector2(0.5f, 0.5f), ppu);
            return CreateFlatSprite(name, sprite, color, parent, localPosition, sortingOrder);
        }

        static SpriteRenderer CreateFlatSprite(string name, Sprite sprite, Color color, Transform parent, Vector3 localPosition, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return sr;
        }

        static Texture2D BuildRoundedRectTexture(int w, int h, float radius)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float dx = Mathf.Max(0f, Mathf.Max(radius - px, px - (w - radius)));
                    float dy = Mathf.Max(0f, Mathf.Max(radius - py, py - (h - radius)));
                    float alpha = 1f;
                    if (dx > 0f && dy > 0f)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Clamp01(radius - dist + 0.5f);
                    }
                    pixels[y * w + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        static Texture2D BuildEllipseTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[w * h];
            float rx = w * 0.5f, ry = h * 0.5f;
            float cx = w * 0.5f, cy = h * 0.5f;
            float edgePx = 1.2f / Mathf.Min(rx, ry);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f - cx) / rx;
                    float ny = (y + 0.5f - cy) / ry;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    float alpha = Mathf.Clamp01((1f - d) / edgePx);
                    pixels[y * w + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
