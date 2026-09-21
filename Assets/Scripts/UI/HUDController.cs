using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BananaHumper.Config;
using BananaHumper.Gameplay;
using BananaHumper.Util;

namespace BananaHumper.UI
{
    /// <summary>
    /// Minimales Schicht-HUD (GDD Kapitel 9), komplett zur Laufzeit erzeugt
    /// damit kein Szenen-/Prefab-Setup im Editor noetig ist.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        BalanceConfig config;
        ShiftController shift;
        BalanceController balance;
        EnergySystem energy;
        EconomySystem economy;

        Text dayText;
        Text moneyText;
        Text angleText;
        Text bunchText;
        Text catchText;
        Text energyText;
        Image energyFill;
        float catchTextTimer;
        GameObject endPanel;
        Text endPanelText;

        RectTransform canvasRect;
        readonly List<StationMarker> stationMarkers = new List<StationMarker>();
        readonly List<FloatingLabel> floatingLabels = new List<FloatingLabel>();

        static Font cachedFont;

        /// <summary>
        /// Geduldsanzeige fuer eine Station, die gerade nicht im Bild ist. Ohne
        /// sie waere mit der mitfahrenden Kamera nicht mehr zu erkennen, wo es
        /// dringend wird - und damit gaebe es nichts mehr zu priorisieren.
        /// </summary>
        class StationMarker
        {
            public Cutter cutter;
            public RectTransform root;
            public Image fill;
            public Text arrow;
        }

        class FloatingLabel
        {
            public RectTransform rect;
            public Text text;
            public Vector3 worldPosition;
            public float elapsed;
        }

        public void Bind(ShiftController shift, BalanceController balance, EnergySystem energy, EconomySystem economy, BalanceConfig config)
        {
            this.shift = shift;
            this.balance = balance;
            this.energy = energy;
            this.economy = economy;
            this.config = config;

            BuildCanvas();

            economy.OnMoneyChanged += m => moneyText.text = $"$ {m:0}";
            shift.OnCaught += ShowCatchFeedback;
            shift.OnBunchMissed += () => FlashCatchText("Verpasst!", new Color(0.95f, 0.3f, 0.25f));
            shift.OnBunchDropped += () => FlashCatchText("Fallen gelassen!", new Color(0.95f, 0.3f, 0.25f));
            shift.OnStumbled += () => FlashCatchText("Stein! Spring drueber (Leertaste)", new Color(0.95f, 0.75f, 0.2f));
            shift.OnShiftEnded += ShowSummary;
            shift.OnDelivered += ShowPayoutLabel;
        }

        void ShowPayoutLabel(int payout)
        {
            var worldPosition = shift.trailer != null
                ? shift.trailer.transform.position + Vector3.up * 1.6f
                : Vector3.zero;
            SpawnFloatingText(worldPosition, $"+{payout} $", new Color(0.45f, 0.95f, 0.5f));
        }

        /// <summary>Aufsteigender Text an einer Weltposition (GDD 8.4).</summary>
        public void SpawnFloatingText(Vector3 worldPosition, string message, Color color)
        {
            var text = CreateText(canvasRect, "Floating", Vector2.zero, TextAnchor.MiddleCenter, 26);
            text.text = message;
            text.color = color;

            var rect = text.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(220, 40);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0.5f, 0.5f);

            floatingLabels.Add(new FloatingLabel { rect = rect, text = text, worldPosition = worldPosition });
        }

        void UpdateFloatingLabels(float dt)
        {
            const float Lifetime = 1.1f;
            var cam = Camera.main;

            for (int i = floatingLabels.Count - 1; i >= 0; i--)
            {
                var label = floatingLabels[i];
                label.elapsed += dt;
                float t = label.elapsed / Lifetime;

                if (t >= 1f || cam == null)
                {
                    Destroy(label.rect.gameObject);
                    floatingLabels.RemoveAt(i);
                    continue;
                }

                label.worldPosition += Vector3.up * 1.2f * dt;
                Vector3 screen = cam.WorldToScreenPoint(label.worldPosition);
                label.rect.anchoredPosition = ScreenToCanvas(screen);

                var c = label.text.color;
                c.a = 1f - t * t;
                label.text.color = c;
            }
        }

        Vector2 ScreenToCanvas(Vector3 screenPoint)
        {
            // Der Canvas skaliert mit der Aufloesung (ScaleWithScreenSize),
            // deshalb muss die Bildschirmposition auf Canvas-Einheiten umgerechnet
            // werden statt direkt als Pixel zu gelten.
            float scaleX = canvasRect.rect.width / Mathf.Max(1f, Screen.width);
            float scaleY = canvasRect.rect.height / Mathf.Max(1f, Screen.height);
            return new Vector2(screenPoint.x * scaleX, screenPoint.y * scaleY);
        }

        static Font GetFont()
        {
            if (cachedFont != null) return cachedFont;
            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return cachedFont;
        }

        void BuildCanvas()
        {
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<StandaloneInputModule>();
            }

            var canvasGo = new GameObject("HUD Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            canvasRect = canvasGo.GetComponent<RectTransform>();
            var root = canvasGo.transform;

            dayText = CreateText(root, "DayText", new Vector2(20, -20), TextAnchor.UpperLeft, 28);
            moneyText = CreateText(root, "MoneyText", new Vector2(20, -55), TextAnchor.UpperLeft, 28);
            angleText = CreateText(root, "AngleText", new Vector2(20, -90), TextAnchor.UpperLeft, 22);
            bunchText = CreateText(root, "BunchText", new Vector2(20, -120), TextAnchor.UpperLeft, 22);
            catchText = CreateText(root, "CatchText", new Vector2(20, -150), TextAnchor.UpperLeft, 26);
            catchText.text = string.Empty;

            energyText = CreateText(root, "EnergyText", new Vector2(20, -185), TextAnchor.UpperLeft, 22);
            energyFill = CreateBar(root, "EnergyBar", new Vector2(20, -215), new Color(0.15f, 0.6f, 0.2f));

            BuildStationMarkers(root);
            BuildEndPanel(root);

            dayText.text = "Tag 1";
            moneyText.text = "$ 0";
        }

        Text CreateText(Transform parent, string name, Vector2 anchoredPos, TextAnchor anchor, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(400, 32);

            var text = go.AddComponent<Text>();
            text.font = GetFont();
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.text = name;
            return text;
        }

        Image CreateBar(Transform parent, string name, Vector2 anchoredPos, Color fillColor)
        {
            var bgGo = new GameObject(name + "_Background");
            bgGo.transform.SetParent(parent, false);
            var bgRt = bgGo.AddComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0, 1);
            bgRt.anchorMax = new Vector2(0, 1);
            bgRt.pivot = new Vector2(0, 1);
            bgRt.anchoredPosition = anchoredPos;
            bgRt.sizeDelta = new Vector2(220, 20);
            var bgImg = bgGo.AddComponent<Image>();
            // Ohne Sprite zeichnet ein Image mit Type.Filled gar nichts - genau
            // daran war der Energiebalken vorher unsichtbar. SpriteFactory.Square
            // liefert ein echtes 1-Farb-Sprite als Untergrund.
            bgImg.sprite = SpriteFactory.Square();
            bgImg.color = new Color(0f, 0f, 0f, 0.55f);

            var fillGo = new GameObject(name + "_Fill");
            fillGo.transform.SetParent(bgGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = SpriteFactory.Square();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;
            return fillImg;
        }

        void BuildEndPanel(Transform parent)
        {
            endPanel = new GameObject("EndPanel");
            endPanel.transform.SetParent(parent, false);
            var rt = endPanel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(420, 260);
            var bg = endPanel.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);

            endPanelText = CreateText(endPanel.transform, "EndText", new Vector2(20, -20), TextAnchor.UpperLeft, 22);
            var textRt = endPanelText.GetComponent<RectTransform>();
            textRt.sizeDelta = new Vector2(380, 180);

            var buttonGo = new GameObject("RestartButton");
            buttonGo.transform.SetParent(endPanel.transform, false);
            var btnRt = buttonGo.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0f);
            btnRt.anchorMax = new Vector2(0.5f, 0f);
            btnRt.pivot = new Vector2(0.5f, 0f);
            btnRt.anchoredPosition = new Vector2(0, 20);
            btnRt.sizeDelta = new Vector2(220, 44);
            var btnImg = buttonGo.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.5f, 0.25f);
            var button = buttonGo.AddComponent<Button>();

            var btnText = CreateText(buttonGo.transform, "Label", Vector2.zero, TextAnchor.MiddleCenter, 22);
            var btnTextRt = btnText.GetComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;
            btnText.text = "Naechste Schicht";
            btnText.alignment = TextAnchor.MiddleCenter;

            button.onClick.AddListener(() =>
            {
                endPanel.SetActive(false);
                shift.StartShift(shift.Day + 1);
            });

            endPanel.SetActive(false);
        }

        /// <summary>
        /// Je eine Randanzeige pro Station. Sichtbar wird sie nur, wenn die
        /// Station gerade aus dem Bild gewandert ist - dann zeigt sie Richtung
        /// und Geduld, damit die Priorisierung nicht am Bildrand endet.
        /// </summary>
        void BuildStationMarkers(Transform root)
        {
            foreach (var cutter in shift.cutters)
            {
                if (cutter == null || !cutter.isHired) continue;

                var markerGo = new GameObject($"Marker_{cutter.name}");
                markerGo.transform.SetParent(root, false);
                var rect = markerGo.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 0.5f);
                rect.anchorMax = new Vector2(0, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(120, 22);

                var bg = markerGo.AddComponent<Image>();
                bg.sprite = SpriteFactory.Square();
                bg.color = new Color(0f, 0f, 0f, 0.55f);

                var fillGo = new GameObject("Fill");
                fillGo.transform.SetParent(markerGo.transform, false);
                var fillRect = fillGo.AddComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = new Vector2(2, 2);
                fillRect.offsetMax = new Vector2(-2, -2);
                var fill = fillGo.AddComponent<Image>();
                fill.sprite = SpriteFactory.Square();
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;

                var arrow = CreateText(markerGo.transform, "Arrow", Vector2.zero, TextAnchor.MiddleCenter, 20);
                var arrowRect = arrow.GetComponent<RectTransform>();
                arrowRect.anchorMin = Vector2.zero;
                arrowRect.anchorMax = Vector2.one;
                arrowRect.offsetMin = Vector2.zero;
                arrowRect.offsetMax = Vector2.zero;

                markerGo.SetActive(false);
                stationMarkers.Add(new StationMarker { cutter = cutter, root = rect, fill = fill, arrow = arrow });
            }
        }

        void UpdateStationMarkers()
        {
            var cam = Camera.main;
            if (cam == null) return;

            float leftStack = 0f;
            float rightStack = 0f;

            foreach (var marker in stationMarkers)
            {
                var cutter = marker.cutter;
                if (cutter == null || !cutter.isHired || !cutter.IsWorking)
                {
                    marker.root.gameObject.SetActive(false);
                    continue;
                }

                Vector3 screen = cam.WorldToScreenPoint(cutter.DropPosition);
                bool offLeft = screen.x < 0f;
                bool offRight = screen.x > Screen.width;

                if (!offLeft && !offRight)
                {
                    marker.root.gameObject.SetActive(false);
                    continue;
                }

                marker.root.gameObject.SetActive(true);
                marker.fill.fillAmount = cutter.Progress;
                marker.fill.color = cutter.Progress >= config.barWarningFraction
                    ? new Color(0.95f, 0.45f, 0.15f)
                    : new Color(0.55f, 0.8f, 0.35f);
                marker.arrow.text = offLeft ? "◀" : "▶";

                // Mehrere Stationen auf derselben Seite untereinander stapeln,
                // sonst liegen sie deckungsgleich uebereinander.
                float stack = offLeft ? leftStack++ : rightStack++;
                float x = offLeft ? 80f : canvasRect.rect.width - 80f;
                marker.root.anchoredPosition = new Vector2(x, -stack * 28f);
            }
        }

        void ShowCatchFeedback(CatchQuality quality)
        {
            switch (quality)
            {
                case CatchQuality.Perfect:
                    FlashCatchText("Perfekt gefangen!", new Color(0.4f, 0.95f, 0.4f));
                    break;
                case CatchQuality.Graze:
                    FlashCatchText("Streifer - Staude beschaedigt", new Color(0.95f, 0.75f, 0.2f));
                    break;
                default:
                    FlashCatchText("Gefangen", Color.white);
                    break;
            }
        }

        void FlashCatchText(string message, Color color)
        {
            catchText.text = message;
            catchText.color = color;
            catchTextTimer = 1.5f;
        }

        void ShowSummary(ShiftSummary summary)
        {
            endPanelText.text =
                $"Schicht beendet\n\n" +
                $"Abgeliefert: {summary.BunchesDelivered}\n" +
                $"Verpasst: {summary.BunchesMissed}\n" +
                $"Fallen gelassen: {summary.BunchesDropped}\n" +
                $"Verdienst: $ {summary.MoneyEarned:0}\n" +
                $"Erfahrung: {summary.ExperienceEarned:0.0}";
            endPanel.SetActive(true);
        }

        void Update()
        {
            if (shift == null) return;

            dayText.text = $"Tag {shift.Day}";

            var bunch = shift.CarriedBunch;
            bunchText.text = bunch != null ? $"Staude: {bunch.Description}" : "Leere Schulter - naechster Balken?";

            if (balance != null)
            {
                bool carrying = bunch != null;
                angleText.text = carrying ? $"Neigung {balance.Theta * Mathf.Rad2Deg:0.0} deg" : string.Empty;
                angleText.color = balance.IsRedWarning ? new Color(0.9f, 0.2f, 0.2f) : Color.white;
            }

            if (catchTextTimer > 0f)
            {
                catchTextTimer -= Time.deltaTime;
                if (catchTextTimer <= 0f) catchText.text = string.Empty;
            }

            UpdateStationMarkers();
            UpdateFloatingLabels(Time.deltaTime);

            if (energy != null && energyFill != null)
            {
                float fraction = energy.Max > 0f ? energy.Current / energy.Max : 0f;
                energyFill.fillAmount = fraction;
                energyFill.color = fraction < 0.25f
                    ? new Color(0.85f, 0.35f, 0.15f)
                    : new Color(0.15f, 0.6f, 0.2f);
                energyText.text = $"Energie {energy.Current:0} / {energy.Max:0}";
            }
        }
    }
}
