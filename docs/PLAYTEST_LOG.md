# Playtest Log

Protokoll der Graybox-Tests aus Roadmap Stufe A / GDD Kapitel 12
("Kern-Loop validieren"). Ein Eintrag pro Testrunde. Ziel: nachvollziehen
können, welches Feedback zu welcher `BalanceConfig`-Änderung geführt hat.

Kriterien aus GDD Kapitel 12, gegen die getestet wird (Kurzfassung — Details
im GDD nachschlagen):
- Macht das Balancieren allein Spaß, ohne weiteres Spielsystem drumherum?
- Werden Fails (Fallen/Snap) als lustig/fair empfunden statt als unfair?

## Vorlage für einen Eintrag

```
## YYYY-MM-DD — <Commit/Build-Stand>

**Tester:** <wer>
**Config-Stand:** <z. B. Default-BalanceConfig, oder welche Werte geändert>

**Beobachtungen:**
- ...

**Bewertung gegen Kap.-12-Kriterien:**
- Spaß am Balancieren allein: ...
- Fails fair/lustig statt unfair: ...

**Resultierende Änderung:**
- <z. B. controlStrength 0.15 → 0.2, siehe Commit XYZ>
- oder: keine Änderung, weiter beobachten
```

---

(Noch keine Testrunden protokolliert.)
