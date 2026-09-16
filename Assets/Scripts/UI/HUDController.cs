using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BananaHumper.Config;
using BananaHumper.Gameplay;

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
        Image energyFill;
        Image stressFill;
        GameObject endPanel;
        Text endPanelText;

        static Font cachedFont;

        public void Bind(ShiftController shift, BalanceController balance, EnergySystem energy, EconomySystem economy, BalanceConfig config)
        {
            this.shift = shift;
            this.balance = balance;
            this.energy = energy;
            this.economy = economy;
            this.config = config;

            BuildCanvas();

            economy.OnMoneyChanged += m => moneyText.text = $"$ {m:0}";
            shift.OnDelivered += _ => { };
            shift.OnShiftEnded += ShowSummary;
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
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            canvasGo.AddComponent<GraphicRaycaster>();

            var root = canvasGo.transform;

            dayText = CreateText(root, "DayText", new Vector2(20, -20), TextAnchor.UpperLeft, 28);
            moneyText = CreateText(root, "MoneyText", new Vector2(20, -55), TextAnchor.UpperLeft, 28);
            angleText = CreateText(root, "AngleText", new Vector2(20, -90), TextAnchor.UpperLeft, 22);

            energyFill = CreateBar(root, "EnergyBar", new Vector2(20, -130), new Color(0.15f, 0.6f, 0.2f));
            stressFill = CreateBar(root, "StressBar", new Vector2(20, -160), new Color(0.7f, 0.6f, 0.1f));

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
            bgImg.color = new Color(0f, 0f, 0f, 0.5f);

            var fillGo = new GameObject(name + "_Fill");
            fillGo.transform.SetParent(bgGo.transform, false);
            var fillRt = fillGo.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
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

        void ShowSummary(ShiftSummary summary)
        {
            endPanelText.text =
                $"Schicht beendet\n\n" +
                $"Trips: {summary.TripsCompleted}\n" +
                $"Gefallen: {summary.TripsFallen}\n" +
                $"Gesnappt: {summary.TripsSnapped}\n" +
                $"Verdienst: $ {summary.MoneyEarned:0}\n" +
                $"Erfahrung: {summary.ExperienceEarned:0.0}";
            endPanel.SetActive(true);
        }

        void Update()
        {
            if (shift == null) return;

            dayText.text = $"Tag {shift.Day}";

            if (balance != null)
            {
                float angleDeg = balance.Theta * Mathf.Rad2Deg;
                angleText.text = $"Neigung {angleDeg:0.0} deg | Belastung {balance.Stress:0}";
                angleText.color = balance.IsRedWarning ? new Color(0.9f, 0.2f, 0.2f) : Color.white;
            }

            if (energy != null && energyFill != null)
            {
                energyFill.fillAmount = energy.Max > 0f ? energy.Current / energy.Max : 0f;
            }
            if (balance != null && stressFill != null)
            {
                stressFill.fillAmount = balance.Stress / 100f;
            }
        }
    }
}
