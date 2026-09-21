using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BananaHumper.Gameplay;
using BananaHumper.Util;

namespace BananaHumper.UI
{
    /// <summary>
    /// Feierabend-Bildschirm mit beiden Fortschritts-Spuren (GDD 5, 9):
    /// links der Koerper (Erfahrung), rechts die Ausruestung (Geld). Flache
    /// Listen ohne Abhaengigkeiten - kein Skill Tree [E]. Zwei Spalten, weil
    /// zwei getrennte Waehrungen sonst dauernd verwechselt werden.
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
            rect.sizeDelta = new Vector2(760, 470);
            rect.anchoredPosition = new Vector2(250, 0);

            var background = panel.AddComponent<Image>();
            background.sprite = SpriteFactory.Square();
            background.color = new Color(0.06f, 0.09f, 0.06f, 0.92f);

            var title = CreateText(panel.transform, "Title", new Vector2(20, -16), 26, TextAnchor.UpperLeft);
            title.text = "Feierabend";
            moneyLine = CreateText(panel.transform, "Wallet", new Vector2(20, -48), 20, TextAnchor.UpperLeft);
            moneyLine.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 30);

            // Zwei Spalten, weil es zwei Waehrungen sind (GDD 5): links der
            // Koerper, rechts die Ausruestung. Getrennt zu zeigen macht sofort
            // klar, dass Erfahrung nicht fuer Stiefel taugt.
            CreateHeading(panel.transform, new Vector2(20, -84), "Koerper (Erfahrung)");
            CreateHeading(panel.transform, new Vector2(390, -84), "Ausruestung (Geld)");

            float leftY = -118f;
            float rightY = -118f;
            foreach (var definition in UpgradeSystem.Catalogue)
            {
                bool money = definition.Currency == UpgradeCurrency.Money;
                float column = money ? 390f : 20f;
                float rowY = money ? rightY : leftY;
                rows.Add(CreateRow(panel.transform, definition, column, rowY));
                if (money) rightY -= 66f; else leftY -= 66f;
            }

            float y = Mathf.Min(leftY, rightY);
            var next = CreateButton(panel.transform, "NextShift", new Vector2(20, y - 12f), new Vector2(720, 44),
                "Naechste Schicht", new Color(0.2f, 0.5f, 0.25f));
            next.onClick.AddListener(() => OnNextShiftRequested?.Invoke());

            root = panel;
            root.SetActive(false);
        }

        /// <summary>Das erzeugte Panel - die Komponente selbst sitzt auf dem Systems-Objekt.</summary>
        GameObject root;

        Text CreateHeading(Transform parent, Vector2 position, string caption)
        {
            var heading = CreateText(parent, caption, position, 20, TextAnchor.UpperLeft);
            heading.text = caption;
            heading.color = new Color(0.85f, 0.8f, 0.5f);
            return heading;
        }

        Row CreateRow(Transform parent, UpgradeDefinition definition, float x, float y)
        {
            var label = CreateText(parent, $"{definition.Id}_Label", new Vector2(x, y), 16, TextAnchor.UpperLeft);
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(230, 60);

            var button = CreateButton(parent, $"{definition.Id}_Buy", new Vector2(x + 236, y), new Vector2(110, 40),
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
            moneyLine.text = $"Geld: $ {economy.Money:0}          Erfahrung: {economy.Experience:0.0}";

            foreach (var row in rows)
            {
                var definition = UpgradeSystem.Definition(row.id);
                int level = upgrades.LevelOf(row.id);
                bool maxed = upgrades.IsMaxed(row.id);
                bool affordable = upgrades.CanAfford(row.id, economy);

                row.label.text = $"{definition.Name}  (Stufe {level}/{definition.MaxLevel})\n{definition.Effect}";
                row.label.color = maxed ? new Color(0.6f, 0.6f, 0.6f) : Color.white;

                string price = definition.Currency == UpgradeCurrency.Money
                    ? $"$ {upgrades.CostOf(row.id)}"
                    : $"{upgrades.CostOf(row.id)} EP";
                var buttonText = row.button.GetComponentInChildren<Text>();
                buttonText.text = maxed ? "Ausgebaut" : price;

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
