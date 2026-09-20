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

## 2026-09-20 — nach Umstellung auf W/S-Balancieren (Commit c110c24)

**Tester:** Olli
**Config-Stand:** controlStrength 3.5 (erster Schätzwert nach Umstellung von Maus auf W/S)

**Beobachtungen:**
- Neigung fällt zu stark durch den Tastendruck — schon kurzes Halten von
  `W`/`S` schießt weit über die gewünschte Gegenneigung hinaus.

**Bewertung gegen Kap.-12-Kriterien:**
- Spaß am Balancieren allein: noch nicht beurteilbar, Steuerung selbst war
  zu stark, um es einzuschätzen.
- Fails fair/lustig statt unfair: noch nicht beurteilbar.

**Resultierende Änderung:**
- `controlStrength` 3.5 → 1.2 (nächster Commit). Grund: bei gehaltener
  Taste bleibt die Vertical-Achse dauerhaft auf ±1 (anders als ein
  Mausschlag, der sofort abklingt), 3.5 baute dadurch zu schnell Drehimpuls
  auf. Weiter beobachten, ob 1.2 reicht oder noch schwächer werden muss.
