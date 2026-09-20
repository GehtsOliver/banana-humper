# Decisions Log

Bewusste technische/design-nahe Entscheidungen, die vom GDD abweichen oder
nicht offensichtlich aus dem Code hervorgehen, mit Datum und Begründung.
Kleinteilige Code-Rationale bleibt als Kommentar im Code — hier stehen nur
Entscheidungen, die jemand (auch zukünftiges Claude) sonst falsch vermuten
oder versehentlich rückgängig machen würde.

Format pro Eintrag: Datum, Entscheidung, Begründung, ggf. Alternative die
verworfen wurde.

---

## 2026-09 — Keine handgeschriebene `.unity`-Szenendatei

Alle Szenenobjekte werden von `GameBootstrap.cs` zur Laufzeit erzeugt statt
in einer `.unity`-YAML-Datei zu liegen.

**Begründung:** `.unity`-Dateien lassen sich ohne laufenden Unity-Editor
nicht zuverlässig verifizieren (kaputtes YAML fällt erst beim Öffnen auf).
Runtime-Erzeugung ist vollständig code-review- und testbar.

**Alternative verworfen:** Szene von Hand in Unity bauen und einchecken —
üblicher für größere Projekte, aber hier bewusst vermieden, solange Claude
ohne Editor-Zugriff Code beisteuert.

## 2026-09 — Built-in Render Pipeline statt URP

GDD nennt URP als Zielwert, Graybox nutzt Built-in.

**Begründung:** Built-in läuft ohne Editor-Konfiguration/Package-Installation.
Umstieg auf URP ist später ein einzelner Package-Manager-Schritt (Package
Manager → URP installieren → Render Pipeline Converter) und betrifft keinen
Gameplay-Code.

## 2026-09 — Manuelles Laufen (A/D) statt Auto-Movement

GDD 3.1 [A] sah automatische Bewegung zum Trailer vor. Umgesetzt wurde
stattdessen manuelles Laufen mit `A`/`D`, gleichzeitig zum Balancieren.

**Begründung:** [E] von Olli — macht das Tragen aktiver statt einer reinen
Balance-Warteschleife.

## 2026-09 — Cutter-Figur ohne konkrete Hautfarbe/Gesichtszüge

Bewusst abstrakt gehalten (prozedurale Form statt Figuren-Asset mit
Gesicht).

**Begründung:** GDD 1.7 — „Cutter als Bogans, nie ethnisch markiert".
