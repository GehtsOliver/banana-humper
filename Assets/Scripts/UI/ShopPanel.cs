using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BananaHumper.Gameplay;
using BananaHumper.Util;

namespace BananaHumper.UI
{
    /// <summary>
    /// Shop am Schichtende (GDD 5.2, 9). Flache Liste ohne Abhaengigkeiten -
    /// kein Skill Tree [E]. Bezahlt wird mit Geld.
    ///
    /// Erscheint zwischen den Schichten statt mitten im Spiel: Der Kern-Loop
    /// soll nicht fuer Menues unterbrochen werden, und Anheuern veraendert die
    /// Groesse des Paddocks - das laesst sich nur sauber zum Schichtstart neu
    /// aufbauen.
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        class Row
        {
            public UpgradeId id;
            public Button button;
            public Text label;
        }

        readonly List<Row> rows = new List<Row>();
        UpgradeSystem upgrades;
        EconomySystem economy;
        Text moneyLine;
        Func<Font> fontProvider;

        public event Action OnNextShiftRequested;

        public void Build(Transform parent, UpgradeSystem upgrades, EconomySystem economy, Func<Font> fontProvider)
        {
            this.upgrades = upgrades;
            this.economy = economy;
            this.fontProvider = fontProvider;

            var panel = new GameObject("ShopPanel");
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(560, 420);
            rect.anchoredPosition = new Vector2(320, 0);

            var background = panel.AddComponent<Image>();
            background.sprite = SpriteFactory.Square();
            background.color = new Color(0.06f, 0.09f, 0.06f, 0.92f);

            var title = CreateText(panel.transform, "Title", new Vector2(20, -16), 26, TextAnchor.UpperLeft);
            title.text = "Shop";
            moneyLine = CreateText(panel.transform, "Money", new Vector2(20, -48), 20, TextAnchor.UpperLeft);

            float y = -84f;
            foreach (var definition in UpgradeSystem.Catalogue)
            {
                rows.Add(CreateRow(panel.transform, definition, y));
                y -= 58f;
            }

            var next = CreateButton(panel.transform, "NextShift", new Vector2(20, y - 12f), new Vector2(520, 44),
                "Naechste Schicht", new Color(0.2f, 0.5f, 0.25f));
            next.onClick.AddListener(() => OnNextShiftRequested?.Invoke());

            root = panel;
            root.SetActive(false);
        }

        /// <summary>Das erzeugte Panel - die Komponente selbst sitzt auf dem Systems-Objekt.</summary>
        GameObject root;

        Row CreateRow(Transform parent, UpgradeDefinition definition, float y)
        {
            var label = CreateText(parent, $"{definition.Id}_Label", new Vector2(20, y), 18, TextAnchor.UpperLeft);
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 52);

            var button = CreateButton(parent, $"{definition.Id}_Buy", new Vector2(396, y), new Vector2(144, 40),
                "Kaufen", new Color(0.25f, 0.35f, 0.5f));
            var row = new Row { id = definition.Id, button = button, label = label };
            button.onClick.AddListener(() =>
            {
                upgrades.TryBuy(row.id, economy);
                Refresh();
            });
            return row;
        }

        public void Show()
        {
            root.SetActive(true);
            Refresh();
        }

        public void Hide() => root.SetActive(false);

        void Refresh()
        {
            moneyLine.text = $"Guthaben: $ {economy.Money:0}";

            foreach (var row in rows)
            {
                var definition = UpgradeSystem.Definition(row.id);
                int level = upgrades.LevelOf(row.id);
                bool maxed = upgrades.IsMaxed(row.id);
                bool affordable = upgrades.CanAfford(row.id, economy);

                row.label.text = $"{definition.Name}  (Stufe {level}/{definition.MaxLevel})\n{definition.Effect}";
                row.label.color = maxed ? new Color(0.6f, 0.6f, 0.6f) : Color.white;

                var buttonText = row.button.GetComponentInChildren<Text>();
                buttonText.text = maxed ? "Ausgebaut" : $"$ {upgrades.CostOf(row.id)}";

                row.button.interactable = affordable;
                var image = row.button.GetComponent<Image>();
                image.color = maxed
                    ? new Color(0.25f, 0.25f, 0.25f)
                    : affordable ? new Color(0.25f, 0.45f, 0.3f) : new Color(0.3f, 0.25f, 0.25f);
            }
        }

        Text CreateText(Transform parent, string name, Vector2 position, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(520, 30);

            var text = go.AddComponent<Text>();
            text.font = fontProvider();
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            return text;
        }

        Button CreateButton(Transform parent, string name, Vector2 position, Vector2 size, string caption, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.AddComponent<Image>();
            image.sprite = SpriteFactory.Square();
            image.color = color;

            var button = go.AddComponent<Button>();

            var caption_ = CreateText(go.transform, "Label", Vector2.zero, 18, TextAnchor.MiddleCenter);
            var captionRect = caption_.GetComponent<RectTransform>();
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.offsetMin = Vector2.zero;
            captionRect.offsetMax = Vector2.zero;
            caption_.text = caption;

            return button;
        }
    }
}
