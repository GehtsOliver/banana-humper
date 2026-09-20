# Banana Humper – Game Design Document (GDD)

Version 0.8 · 16.09.2026 · ersetzt Design-Dokument v0.2 (Job-Sim) und GDD v0.3–v0.7

**Legende: Entscheidungen**
- **[E]** = Entscheidung von Olli
- **[A]** = Annahme / Vorschlag, noch zu bestätigen

**Legende: Versionen (gegen Scope Creep)**
- **[Demo]** = in der Demo, damit automatisch auch in V1
- **[V1]** = Vollversion zum Steam-Release
- **[V2]** = erstes größeres Update nach dem Release
- **[V3+]** = spätere Ideen, ohne Zusage

**Regeln**
1. Vor dem Release wird nur gebaut, was mit [Demo] oder [V1] markiert ist.
2. Neue Ideen landen in Kapitel 13 mit [V2] oder [V3+]. Hochstufen nach V1 nur als bewusste Entscheidung von Olli, dokumentiert mit [E].
3. Die Zuordnung zu V2 und V3+ ist ein Vorschlag **[A]**.

Ein GDD beschreibt, *was* das Spiel ist und *wie* es funktioniert. Es ist der Anker gegen Scope Creep (ungeplantes Anwachsen des Umfangs).

---

## 1. Vision

### 1.1 Pitch
Nordqueensland, Regenzeit. Du bist ein deutscher Backpacker mit fast leerem Konto und einem Ziel: 88 Tage Farmarbeit für das zweite Visum. Der Job, den das Hostel dir vermittelt, ist der härteste auf der Plantage: **Banana Humper**.

Ein Cutter schlägt mit der Machete die Staude ab, und bis zu 100 kg Bananen krachen auf dein Schulterpad. Liegt sie nicht genau im Gleichgewicht, hältst du sie nur noch mit Kraft, und lange Stauden brechen dir dann direkt auf der Schulter durch. Jetzt trägst du sie durch den Matsch zum Trailer, ohne dass sie kippt. Die Staude steckt in einem Schutz-Bag, und was darin wohnt, merkst du erst, wenn es dir über die Schulter krabbelt: Spinnen, Frösche, Ratten, manchmal eine Schlange. Fällt die Staude oder bricht sie, ist sie „gesnappt“, die Cutter fluchen in bestem australischem Umgangston, und ob dein Tag zählt, entscheidet der Farmer.

Die ersten Tage sind eine Qual. Jeder Schritt kostet Kraft, Schultern und Füße schmerzen, und gearbeitet wird auch im Starkregen. Aber du wirst stärker. Nach ein paar Wochen rennst du mit den Stauden, später schleppst du zwei gleichzeitig. Tag 88: Visum. Und dann auf die nächste Farm, nur besser.

### 1.2 Hook (der Satz, der in einem GIF funktioniert)
Balancieren einer riesigen Bananenstaude auf der Schulter, bis sie runterkracht.

### 1.3 Der Name **[E]**
„Banana Humper“ ist die offizielle Bezeichnung des Jobs in Australien (auch Arbeitsschutz-Unterlagen aus Queensland führen „Banana humping“ als Tätigkeit). Für englische Muttersprachler ist der Name zweideutig. Das ist gewollt und passt zum Ton, bleibt aber der einzige Wink: Spiel, Art und Marketing sind eindeutig harmlos.

Pointe für die Store-Beschreibung: „Yes, that's the real job title.“

### 1.4 Hintergrund: der echte Job
Das Spiel basiert auf Ollis eigener Arbeit als Banana Humper 2016 während Work and Travel in Australien.

**Warum Backpacker dort arbeiten:** Für das zweite Working-Holiday-Visum braucht man 88 Tage Arbeit in bestimmten Branchen, etwa auf Farmen. Die anderen wollen einfach möglichst viel Geld sparen. Vermittelt wurde über ein Backpacker-Hostel.

**Der Arbeitsbeginn:** Morgens fährt jede Crew von der Shed (dem Schuppen der Farm) auf einem Ute (australischer Pick-up) zu dem Paddock (Plantagenfeld), das an diesem Tag ansteht. Welches Paddock dran ist, entscheidet vermutlich der Farmer bzw. Farm Manager.

**Die Crew:** 2 Cutter und 2–7 Backpacker als Humper und Stacker.

| Rolle | Wer | Aufgabe |
|---|---|---|
| Cutter | Locals | Schlagen die Staude mit der Machete ab, sodass sie auf die Schulter des Humpers fällt |
| Humper | Backpacker | Trägt die Staude auf dem Schulterpad zum Trailer |
| Stacker | Backpacker | Nimmt die Staude am Trailer entgegen, öffnet den Schutz-Bag, hakt sie mit einem Fahrradschlauch am Trailer ein |
| Traktorfahrer | Backpacker | Fährt den Trailer, die entspannteste Rolle |

Die Rollen rotierten alle 20–30 Minuten. Erfahrene Backpacker durften später auch cutten.

**Der Alltag:**
- Stauden wogen bis zu 100 kg.
- Die Balance auf der Schulter war anfangs extrem schwer: hoher Kraftverbrauch, Schulter- und Fußschmerzen. Nach etwa vier Wochen war der Körper gewöhnt, und man ist mit den Stauden gerannt.
- Stauden, die fallen, sind „gesnappt“ und wertlos.
- Lange Stauden snappten auch auf der Schulter: Lagen sie nicht perfekt ausbalanciert auf, hielt man sie nur mit Kraft, und sie brachen durch.
- In den Bags lebten Tiere: Spinnen, Schlangen, Ratten, Frösche. Das Öffnen war eklig und machte Angst.
- Die Cutter hatten einen rauen australischen Umgangston gegenüber Backpackern.
- Gearbeitet wurde auch bei Starkregen.
- Verletzungen und Krankheiten unter Backpackern kamen vor.
- Ab Donnerstag wurde im Hostel gefeiert.
- Ein neuer deutscher Backpacker ließ in seinen ersten zwei Tagen viele Stauden fallen oder snappen und wurde von der Farm geworfen.

### 1.5 Von der Realität ins Spiel
| Realität | Im Spiel | Kapitel | Version |
|---|---|---|---|
| Bis zu 100 kg auf der Schulter balancieren | Balance-Physik als Kern-Mechanik | 3 | [Demo] |
| Staude fällt auf die Schulter | Auflegen: Schulter unter den Schwerpunkt bringen | 3.2 | [Demo] |
| Schlecht aufgelegte lange Stauden brechen | Belastung, Snap auf der Schulter | 3.5 | [Demo] |
| Anfangs Schmerzen, nach vier Wochen rennen | Energie, Körper-Stufen über Erfahrung, Rennen freischalten | 4, 5.1 | [Demo] |
| Gesnappte Stauden sind wertlos | Kein Lohn, Energieverlust | 3.6 | [Demo] |
| Schulterpad | Shop-Artikel „Schulterpad“ | 5.2 | [Demo] |
| Tiere in den Bags | Zufallsereignisse | 7 | [Demo] / [V1] |
| Starkregen | Regen-Ereignis mit rutschigem Boden | 7 | [Demo] |
| Rauer Ton der Cutter | Cutter-Sprüche als Sprechblasen | 7 | [Demo] |
| 88 Tage fürs zweite Visum | Tageszähler, Tagesquote, Visum als Prestige | 4.4, 6 | [Demo] / [V1] |
| Geld sparen | Lohn und Shop | 4, 5.2 | [Demo] |
| Cutter, Stacker, Traktorfahrer | Hintergrund-Figuren, spielbar später | 13 | [V1] / [V2] |
| Neuer nach zwei Tagen von der Farm geworfen | Geschichte „Der Neue“ und Verwarnungen mit Rauswurf | 7.1, 4.7 | [Demo] / [V1] |
| Hostel-Partys ab Donnerstag | Später | 13 | [V2] |

### 1.6 Design-Säulen
Jedes Feature muss mindestens eine davon stärken, sonst fliegt es raus.
1. **Vom Wackelnden zum Rennenden:** Der Spieler spürt seine Progression im Körper. Anfangs ist jeder Schritt ein Kampf, später rennt man mit zwei Stauden.
2. **Scheitern ist lustig:** Gesnappte Stauden, Spinnen und fluchende Cutter sind Komik, keine Strafe.
3. **Eine Szene, viel Tiefe:** Alles passiert zwischen Staude und Trailer. Tiefe entsteht über Körper und Shop, nicht über neue Orte.
4. **Alles ist aktiv:** Fortschritt entsteht nur durch eigenes Tragen. Keine Helfer, keine Automatisierung, kein Offline-Fortschritt **[E]**.

### 1.7 Eckdaten
| Punkt | Festlegung |
|---|---|
| Spielname | Banana Humper **[E]** |
| Genre | Rein aktives Incremental Game mit Balance-Physik und Prestige, ohne Idle-Anteil **[E]** |
| Plattform | Steam (Windows) mit früher Steam-Seite und Steam-Demo; Web-Builds für private Tests und Webportale (Kapitel 16) **[E]** |
| Engine | Unity 6 **[E]** |
| Input | Maus (Auflegen, Balancieren) + `A`/`D` (Laufen) **[E]** |
| Sprache | Englisch zum Release **[A]**, Deutsch [V2] **[A]** |
| Monetarisierung | Steam Premium (Einmalkauf), keine Ads, keine In-App-Käufe **[E]** |
| Preis Vollversion | 3–5 € **[A]** |
| Spielzeit Vollversion | 3–4 h **[A]** |
| Spielzeit Demo | 15–25 min **[A]** |
| Perspektive | 2D-Seitenansicht **[A]** (vorher 2.5D, 2D ist für das erste Spiel einfacher) |
| Schauplatz | Fiktive Bananenfarm in Nordqueensland, keine realen Farm- oder Hostelnamen **[E]** |
| Ton | Überzeichnet-komisch, Cartoon-Australien; Cutter als Bogans, nie ethnisch markiert **[E]** |
| Spielfigur | Männlicher deutscher Backpacker, Name offen **[E]** |

### 1.8 Zielgruppe
Spieler von kurzen, befriedigenden Incremental- und Roguelite-Spielen (Vampire Survivors, Balatro), die gern optimieren, aber keine Hardcore-Skill-Spiele wollen. Dazu Streamer, die physikalisches Scheitern und Ekelmomente mögen, sowie ehemalige Work-and-Travel-Backpacker, die den Job wiedererkennen.

### 1.9 Alleinstellungsmerkmale
- **Balance-Physik** als Kern-Mechanik statt reinem Klicken
- **Authentisches Setting** aus echter Erfahrung, das kein anderes Spiel hat
- **Tagesquote und Visum** als Druck und Ziel, direkt aus dem echten Work-and-Travel-Alltag

---

## 2. Spielstruktur

| Ebene | Dauer | Was passiert | Version |
|---|---|---|---|
| **Trip** (Kern-Loop) | 5–10 s | Staude auflegen, zum Trailer balancieren, abliefern, Geld | [Demo] |
| **Schicht** (Run) | 45–90 s | So viele Trips wie die Energie erlaubt. Schicht = 1 Arbeitstag | [Demo] |
| **Farm-Durchlauf** | 60–90 min (1. Farm) | 88 gezählte Arbeitstage, danach Visum und Prestige | [V1] (Demo: Tag 1–10) |
| **Gesamtspiel** | 3–4 h | 3 Farmen mit eigenem Modifikator | [V1] |
| **Endlos-Modus** | offen | Weitere Farmen mit Zufallsmodifikator nach Farm 3 | [V2] |

**Begriffe:**
- **Kern-Loop:** die kleinste Aktion, die sich ständig wiederholt
- **Run:** ein Durchgang, der endet (hier: wenn die Energie leer ist)
- **Prestige:** freiwilliger Neustart mit dauerhaftem Bonus

---

## 3. Kern-Mechanik: Auflegen, Balancieren, Halten

### 3.1 Ablauf eines Trips [Demo]
1. Die Figur steht beim Cutter. Die Staude hat ein zufälliges Gewicht und eine Länge (kurz, mittel, lang).
2. **Auflegen:** Der Cutter schlägt ab, die Staude fällt. Der Spieler bringt die Schulter per Maus unter die Markierung des Schwerpunkts (siehe 3.2).
3. Die Figur wird per **`A`/`D`** Richtung Trailer bewegt **[E]** (ersetzt die ursprüngliche Annahme einer automatischen Bewegung).
4. Gleichzeitig hält der Spieler die Staude mit der **Maus** im Gleichgewicht.
5. **Linke Maustaste halten (während `A`/`D` gedrückt ist) = rennen** (schneller, aber mehr Wackeln und mehr Energieverbrauch). Freigeschaltet über die Körper-Stufe „Rennen“ **[A]**.
6. **Rechte Maustaste = umsetzen:** Die Figur bleibt kurz stehen und rückt die Staude nach (siehe 3.5) **[A]**.
7. Am Trailer wird automatisch abgeliefert: Geld gibt es sofort.
8. Rückweg leer, automatisch und schnell (ca. 2 s) **[A]**.

### 3.2 Auflegen [Demo] **[A]**
- Beim Abschlagen erscheint kurz eine Markierung, wo der Schwerpunkt der fallenden Staude landen wird.
- Der Spieler bewegt die Schulter (Maus horizontal) darunter.
- Beim Aufprall wird der **Auflage-Versatz** `offset` berechnet: Abstand zwischen Schulter und Schwerpunkt, normiert auf −1 bis +1.
- Perfekt (|offset| < 0,1): kurzer „Sweet-Spot“-Juice (Sound, Aufblitzen).
- Der Versatz bleibt bestehen, bis der Spieler umsetzt.

### 3.3 Steuerungsprinzip Balancieren [Demo]
Wie ein Besen, den man auf der Hand balanciert: Kippt die Staude nach links, bewegt der Spieler die Maus nach links, um die Schulter „unter“ das Gewicht zu schieben.

### 3.4 Physik-Modell (eigene Simulation, kein Rigidbody) [Demo] **[A]**
Eine eigene Pendel-Simulation ist leichter zu verstehen und zu tunen als die Unity-Physik.

Zustand: Neigungswinkel `θ` (0 = gerade), Winkelgeschwindigkeit `ω`, Belastung `stress` (0–100).

Pro Frame:
```
α = (gravity / length) * sin(θ) * weightFactor   // Kippen durch Gewicht
    + offsetTorque * offset * weightFactor        // Dauerhafter Zug durch schlechte Auflage
    - damping * ω                                 // natürliche Dämpfung
    - controlStrength * mouseDeltaX               // Spieler-Korrektur
    + wobble(t)                                   // Wackeln durch Schritte
    + eventImpulse                                // Spinne, Rutschen usw.
ω += α * dt
θ += ω * dt
```
- `weightFactor = staudenGewicht / 50` (50 kg = 1,0)
- `wobble(t)` = kleine Sinus-Schwingung im Schritttakt plus Rauschen; beim Rennen stärker
- **Fail „gefallen“:** `|θ| > maxAngle` → Staude fällt

### 3.5 Belastung und Snap auf der Schulter [Demo] **[A]**
Liegt die Staude nicht im Gleichgewicht, hält der Humper sie mit Kraft. Das belastet die Staude, lange Stauden brechen dann auf der Schulter.

```
haltekraft = |offset| + max(0, |θ| - comfortAngle) / maxAngle
if haltekraft > 0,15:
    stress += haltekraft * lengthFactor * weightFactor * stressRate * dt
else:
    stress -= recoveryRate * dt
stress = clamp(stress, 0, 100)
```
- **Fail „auf der Schulter gesnappt“:** `stress >= 100` → Staude bricht durch
- **Warnung:** Ab 60 knarzt die Staude hörbar, ab 80 biegt sie sich sichtbar durch
- **Umsetzen (rechte Maustaste):** 0,6 s Stillstand, `offset` −60 %, −5 Energie, kleiner Wackel-Impuls. Umsetzen ist die aktive Gegenmaßnahme, kostet aber Zeit.
- Kurze Stauden snappen praktisch nie, lange sind das eigentliche Risiko.

### 3.6 Startwerte (zum Tunen, Balance-Config als ScriptableObject) [Demo]
| Parameter | Startwert | Wird verändert durch |
|---|---|---|
| gravity / length | 4,0 | – |
| offsetTorque | 1,5 | Shop „Schulterpad“ |
| damping | 1,2 | Körper „Kraft“ |
| controlStrength | 0,015 | – (Einstellung „Maus-Empfindlichkeit“) |
| wobble Gehen | 0,6 | Körper „Schultergewöhnung“ |
| wobble Rennen | 1,5 | Körper „Schultergewöhnung“ |
| maxAngle | 35° | Shop „Schulterpad“ |
| comfortAngle | 8° | Körper „Schultergewöhnung“ |
| stressRate | 60 pro s | Körper „Kraft“ |
| recoveryRate | 15 pro s | – |
| lengthFactor | kurz 0,3 · mittel 1,0 · lang 1,8 | – |
| Laufgeschwindigkeit | 2,0 m/s | Körper „Beine“ |
| Weg Staude → Trailer | 12 m | Farm-Modifikator |

### 3.7 Schwierigkeit „Mittel“ [Demo] **[E]**
- **Tag 1–5:** Auflegen und Balancieren sind schwer, 1–2 Fehler pro Schicht sind normal.
- **Ab ca. Tag 20:** Mit den ersten Körper-Stufen und Schulterpad fühlt es sich kontrolliert an.
- **Spät:** Rennen mit zwei Stauden ist möglich, Skill bleibt ein Bonus, aber keine Hürde.
- Hilfe: Die Staude färbt sich rot, wenn `|θ|` über 70 % von `maxAngle` liegt.

### 3.8 Gesnappt [Demo]
| | Gefallen | Auf der Schulter gesnappt |
|---|---|---|
| Ursache | Balance verloren | Zu lange mit Kraft gehalten |
| Lohn | keiner | keiner |
| Energie | −15 | −10 |
| Juice | Kamerawackeln, Matsch-Partikel, dumpfer Aufprall, Zeitlupe 0,2 s | Lautes Knacken, Staude knickt in der Mitte, Bananen purzeln |
| Reaktion | Cutter-Spruch | Cutter-Spruch |

---

## 4. Ressourcen und Ökonomie

### 4.1 Ressourcen
| Ressource | Zweck | Gilt für | Version |
|---|---|---|---|
| **Geld ($)** | Shop-Artikel kaufen | Farm-Durchlauf (Reset bei Prestige) | [Demo] |
| **Erfahrung** | Körper-Stufen verbessern | Farm-Durchlauf (Reset bei Prestige) | [Demo] |
| **Energie** | Begrenzt die Schicht | Schicht (voll zu Schichtbeginn) | [Demo] |
| **Arbeitstage** | Fortschritt zum Visum (0/88) | Farm-Durchlauf | [Demo] |
| **Sonnenbrand-Punkte** | Prestige-Währung | Dauerhaft | [V1] |

### 4.2 Energie [Demo] **[A]**
- Start: 100
- Tragen: `(2 + gewicht / 40)` pro Sekunde (50 kg ≈ 3,25/s)
- Rennen: ×1,6
- Leer zurücklaufen: 0,5 pro Sekunde
- Gefallen: −15, auf der Schulter gesnappt: −10, umsetzen: −5
- Energie leer → Schicht endet sofort (Staude auf der Schulter fällt, ohne Zusatzstrafe)
- Ziel Tag 1: ca. 4 Trips pro Schicht

### 4.3 Stauden [Demo]
- Gewicht zufällig, Tag 1: 30–60 kg; steigt bis 60–100 kg an Tag 88
- Länge zufällig: Tag 1 überwiegend kurz und mittel, später mehr lange Stauden **[A]**
- Lohn: `gewicht / 10` $, gerundet (50 kg = 5 $)
- Schwere und lange Stauden = mehr Geld, schwerer zu halten (Risiko gegen Belohnung)

### 4.4 Tagesquote (Druckmittel) [Demo]
- Ein Tag zählt nur, wenn die Quote erfüllt ist. Sonst gibt der Farmer den Tag nicht frei, das Geld bleibt aber.
- Quote: `3 + floor(tag / 5)` Stauden (Tag 1: 3, Tag 88: 20) **[A]**, gesenkt, weil es keine Helfer gibt
- Mit „Zweite Staude“ zählt ein Trip als zwei Stauden.
- Schichtende-Screen zeigt: „Tag zählt“ oder „Farmer sagt nein“.

### 4.5 Kostenkurve [Demo]
`kosten(stufe) = basiskosten * 1,6^stufe` **[A]**, gleich für Körper (Erfahrung) und Shop (Geld)

### 4.6 Balance-Ziele **[A]**
| Zeitpunkt | Ziel | Version |
|---|---|---|
| Nach Schicht 1 | Erste Körper-Stufe verbesserbar | [Demo] |
| Nach Schicht 3 | Erster Shop-Artikel kaufbar | [Demo] |
| Nach 10 min | Rennen freigeschaltet | [Demo] |
| Tag 40 | Zweite Staude freigeschaltet | [V1] |
| 1. Farm | 60–90 min | [V1] |
| 2. und 3. Farm | je 40–60 min dank Prestige-Boni | [V1] |

### 4.7 Verwarnungen und Rauswurf
Wer zu viele Stauden fallen lässt oder snappt, fliegt von der Farm, wie im echten Farmalltag.

**Verwarnungen [Demo] [A]**
- **Schlechte Schicht:** Mindestens 3 Fehler und mehr als 50 % der Trips gefallen oder gesnappt → der Farm Manager spricht eine Verwarnung aus.
- **Gute Schicht:** Quote erfüllt und höchstens 20 % Fehler → eine Verwarnung wird gelöscht.
- **Drei Verwarnungen:** gefeuert.
- Das HUD zeigt die aktuellen Verwarnungen (0–3), der Schichtende-Screen kommentiert jede neue Verwarnung.
- **Schwelle:** Ein normaler Einsteiger (1–2 Fehler pro Schicht, siehe 3.7) darf nie gefeuert werden, nur wer wirklich schlecht oder zu riskant spielt. Im Graybox-Test prüfen.

**Rauswurf [V1] [A]**
Kein Game Over, sondern ein Rückschlag. In der Realität summieren sich die 88 Tage auch über mehrere Arbeitgeber.
- Kurzer, komischer Rauswurf-Screen: zurück im Hostel.
- Neustart auf einer anderen Farm vom selben Typ (gleicher Modifikator, anderer Name).
- **Bleibt erhalten:** gezählte Arbeitstage, Erfahrung, Körper-Stufen, Shop-Artikel
- **Verloren:** 50 % des Geldes (Tage ohne Arbeit)
- Verwarnungen werden auf 0 gesetzt.
- Erster Cutter-Spruch auf der neuen Farm: „Heard about you, mate.“

**In der Demo [Demo]:** Ein Rauswurf beendet die Demo mit „Gefeuert! In der Vollversion geht's auf der nächsten Farm weiter.“, Wishlist-Aufruf und Option zum Neustart.

---

## 5. Fortschritt: Körper und Shop

Zwei getrennte Systeme mit zwei Währungen, **kein Skill Tree** **[E]**. Beide sind einfache Listen mit Stufen, ohne Abhängigkeiten oder Verzweigungen. Alles verbessert das eigene Tragen, keine Automatisierung **[E]**.

| System | Währung | Idee | Version |
|---|---|---|---|
| **Körper** | Erfahrung | Der Körper gewöhnt sich durch Arbeit, der Spieler verteilt die Erfahrung selbst | [Demo] |
| **Shop** | Geld | Ausrüstung und Verpflegung kaufen | [Demo] |

### 5.1 Körper (Erfahrung)
**Erfahrung verdienen [Demo] [A]:** 1 Erfahrung pro 10 kg abgelieferter Staude. Gefallene oder gesnappte Stauden geben 50 % davon für die bis dahin getragene Last. So bringt auch eine schlechte Schicht Fortschritt.

| Körper-Stufe | Effekt pro Stufe | Max | Basiskosten | Version |
|---|---|---|---|---|
| Kraft | damping +10 %, weightFactor −5 %, stressRate −8 % | 10 | 10 Erfahrung | [Demo] |
| Ausdauer | +20 max. Energie | 10 | 12 Erfahrung | [Demo] |
| Schultergewöhnung | wobble −10 %, comfortAngle +1° | 8 | 15 Erfahrung | [Demo] |
| Beine | Laufgeschwindigkeit +8 % | 8 | 18 Erfahrung | [Demo] |
| Rennen | Schaltet Rennen frei | 1 | 40 Erfahrung | [Demo] |
| Augenmaß | Schwerpunkt-Markierung beim Auflegen größer und länger sichtbar | 3 | 30 Erfahrung | [V1] **[A]** |
| Routine | Jede fehlerfreie Abgabe in Folge +3 % Lohn (max. +30 % pro Stufe), Fehler setzt zurück | 3 | 80 Erfahrung | [V1] **[A]** |
| Dickes Fell | Cutter-Beleidigungen geben +1 $ | 1 | 60 Erfahrung | [V1] **[A]** |
| Zweite Staude | Zwei Stauden gleichzeitig (zählt doppelt, doppelter Lohn, weightFactor ×1,7) | 1 | 500 Erfahrung | [V1] |

### 5.2 Shop (Geld)
**Ausrüstung**
| Artikel | Effekt pro Stufe | Max | Basiskosten | Version |
|---|---|---|---|---|
| Schulterpad | maxAngle +3°, offsetTorque −10 % | 5 | 40 $ | [Demo] |
| Handschuhe | Reaktionszeit Spinne +0,5 s | 3 | 50 $ | [Demo] |
| Gummistiefel | Rutsch-Impulse bei Regen −25 % | 3 | 80 $ | [Demo] |
| Akubra-Hut | Energieverbrauch bei Hitze −15 % | 3 | 120 $ | [V1] |
| Wasserflasche | +5 Energie pro Abgabe | 5 | 100 $ | [V1] |

**Verpflegung und Sonstiges**
| Artikel | Effekt pro Stufe | Max | Basiskosten | Version |
|---|---|---|---|---|
| Instant-Kaffee | Schichtstart mit +15 Energie über Maximum | 3 | 45 $ | [Demo] |
| Vegemite-Toast | Lohn +10 % | 5 | 150 $ | [V1] |
| Bier für die Cutter | Schwere Stauden häufiger | 3 | 300 $ | [V1] **[A]** |
| Bier für den Farm Manager | Chance 20 %, dass eine Schicht 2 Tage zählt | 3 | 500 $ | [V1] **[A]** |

---

## 6. Prestige: Visum [V1] **[E]**

### 6.1 Ablauf
1. Tag 88 erreicht → Visum-Screen („Second Year Visa granted“)
2. Sonnenbrand-Punkte werden berechnet: `floor(sqrt(gesamtverdienst / 50))` **[A]**
3. Reset: Geld, Erfahrung, Körper-Stufen, Shop-Artikel, Tage, Verwarnungen
4. Behalten: Sonnenbrand-Punkte und dauerhafte Boni
5. Wechsel auf die nächste Farm

### 6.2 Dauerhafte Boni (Sonnenbrand-Punkte) [V1]
| Bonus | Effekt |
|---|---|
| Braungebrannt | Lohn +15 % pro Stufe |
| Hornhaut | Start mit Schultergewöhnung Stufe 2 |
| Erspartes | Start jeder Farm mit 100 $ pro Stufe **[A]** |
| Farm-Erfahrung | Tagesquote −10 % |
| Rennen im Blut | Rennen von Anfang an |

### 6.3 Farmen **[A]**
| Farm | Modifikator | Version |
|---|---|---|
| 1. Farm (fiktiver Name offen) | Standard | [Demo] (Tag 1–10) / [V1] |
| 2. Regenfarm | Häufiger Regen, rutschiger Boden, mehr Frösche | [V1] |
| 3. Riesenstauden-Farm | +30 % Gewicht, mehr lange Stauden, +40 % Lohn | [V1] |
| Danach | Zufälliger Modifikator, endlos | [V2] |

---

## 7. Ereignisse und Gefahren

### 7.0 Zufallsereignisse
| Ereignis | Auslöser | Wirkung | Gegenmittel | Version |
|---|---|---|---|---|
| Spinne | Zufällig, 15 %/Trip | Krabbelt auf die Schulter; in 1,5 s wegklicken, sonst starker Wackel-Impuls und −10 Energie | Handschuhe | [Demo] |
| Regen | Ganze Schicht, ab Tag 5, 20 %/Tag | Zufällige Rutsch-Impulse | Gummistiefel | [Demo] |
| Schlange | Ab Tag 15, 5 %/Trip | Fällt aus der Staude, starker Schreck-Impuls | – | [V1] |
| Frosch | Zufällig | Harmlos, springt aus der Staude, +2 $ („Glücksfrosch“) | – | [V1] |
| Ratte | Ab Tag 30 | Läuft über den Weg, Figur stolpert | Beine | [V1] |
| Hitzetag | Ab Tag 10 | Energieverbrauch +30 % | Akubra-Hut | [V1] |

Cutter-Sprüche: harmlos-derbe Einzeiler bei Fallen, Snap, Schichtbeginn, Rekord und nach einem Rauswurf. 10 Sprüche [Demo], 30 Sprüche [V1].

### 7.1 Geskriptetes Ereignis: Der Neue [Demo] **[A]**
Am ersten Tag fängt neben dem Spieler ein zweiter deutscher Backpacker an (Hintergrund-Figur, gleiches Sprite wie die Spielfigur in anderer Farbe).
- Tag 1 und 2: Man sieht ihn im Hintergrund immer wieder Stauden fallen lassen, die Cutter fluchen.
- Schichtende Tag 2: „Der Neue aus Hamburg ist raus. 14 gesnappte Stauden in zwei Tagen.“
- Ab Tag 3 ist er weg.
- Zweck: Zeigt ohne Tutorial, dass Fehler Folgen haben, und kündigt die Verwarnungen an (4.7).

---

## 8. Art und Audio

### 8.1 Art-Richtung [Demo] **[A]**
- Flacher Cartoon-Stil, klare Silhouetten, kleine Farbpalette (Grün, Gelb, Erdbraun, Himmelblau)
- Einheitlichkeit vor Detail
- Basis: freie Asset-Pakete (z. B. Kenney.nl, CC0), selbst gestaltet werden nur Figur, Staude, Trailer, Cutter
- Figuren ohne aufwendige Animation: Körper als eine Form, Beine als einfacher Zwei-Bild-Laufzyklus

### 8.2 Asset-Liste
**[Demo]:** Hintergrund Farm 1 (Plantage, 1 Ebene + Himmel), Boden, Staude (3 Längen, Zustand „durchgebogen“, „gebrochen“), Spielfigur, Cutter, Trailer mit Traktor, Schwerpunkt-Markierung, Spinne, Regen-Partikel, UI-Elemente, „Der Neue“ (umgefärbte Spielfigur), Verwarnungs-Symbol.

**[V1]:** Rauswurf-Screen (Hostel), Schlange, Frosch, Ratte, Hitze-Effekt, Hintergründe Farm 2 und 3, Visum-Screen, zweite Staude auf der Schulter.

### 8.3 Audio [Demo] **[E]**
Keine Musik, nur diegetischer Sound (Geräusche, die in der Spielwelt existieren): Zikaden, Traktor-Leerlauf, Schritte im Matsch, Regen, Stöhnen beim Auflegen, Knarzen unter Belastung, Knacken beim Snap, dumpfer Aufprall, Cutter-Gemurmel.

### 8.4 Juice (kleine Effekte, die Aktionen spürbar machen) [Demo]
- Perfektes Auflegen: Aufblitzen, satter Sound
- Abgabe: „+5 $“ schwebt hoch, Münz-Sound, Trailer federt
- Fallen und Snap: siehe 3.8
- Rekord-Trip: kurzes Aufblitzen
- Körper-Stufe oder Shop-Kauf: Button „ploppt“, Sound

---

## 9. Screens und UI

| Screen | Inhalt | Version |
|---|---|---|
| Titel | Spielen, Einstellungen, Beenden | [Demo] |
| Schicht-HUD | Energiebalken, Geld, Tag x/88, Quote x/y, Verwarnungen 0–3, Neigungsanzeige und Belastung an der Staude | [Demo] |
| Schichtende | Stauden, Verdienst, Fehler (gefallen / gesnappt), verdiente Erfahrung, „Tag zählt“ / „Farmer sagt nein“, neue oder gelöschte Verwarnung, Buttons zu Körper und Shop | [Demo] |
| Körper | Liste der Körper-Stufen: Stufe, Erfahrungskosten, Effekt als Kurztext | [Demo] |
| Shop | Liste der Artikel (Ausrüstung, Verpflegung): Stufe, Preis, Effekt als Kurztext | [Demo] |
| Einstellungen | Lautstärke, Maus-Empfindlichkeit, Spielstand löschen | [Demo] |
| Demo-Ende | Nach Tag 10: „Die Vollversion geht bis zum Visum“, Wishlist-Aufruf, Discord-Link | nur [Demo] |
| Demo-Ende nach Rauswurf | „Gefeuert! In der Vollversion geht's auf der nächsten Farm weiter.“, Wishlist-Aufruf, Neustart | nur [Demo] |
| Rauswurf | Zurück im Hostel, Verlust von 50 % Geld, „Nächste Farm“ | [V1] |
| Visum-Screen | Zusammenfassung, Sonnenbrand-Punkte, dauerhafte Boni, „Nächste Farm“ | [V1] |

Tutorial [Demo]: keine Textwand, sondern vier kurze Hinweise im ersten Trip („Schulter unter die Markierung“, „Maus bewegen zum Ausgleichen“, „Knarzt es: Rechtsklick zum Umsetzen“, „Energie leer = Feierabend“).

---

## 10. Technik

### 10.1 Setup
| Punkt | Version |
|---|---|
| Unity 6, 2D-Vorlage (URP), Git mit Unity-.gitignore | [Demo] |
| WebGL-Build für private Tests und Webportale | [Demo] |
| Windows-Build für die Steam-Demo | [Demo] |
| Steamworks-Anbindung, Steam Cloud für Spielstände | [V1] **[A]** |
| Achievements | [V2] **[A]** |

### 10.2 Architektur [Demo] **[A]**
| Baustein | Aufgabe |
|---|---|
| `GameManager` | Zustandswechsel: Titel, Schicht, Schichtende, Körper, Shop, Visum |
| `ShiftController` | Trip-Ablauf, Schichtende, Tageszähler, Quote, Verwarnungen, Rauswurf, Skript „Der Neue“ |
| `PlacementController` | Auflegen, Schwerpunkt-Markierung, Versatz berechnen |
| `BalanceController` | Pendel-Simulation, Belastung, Umsetzen, Maus-Input, Fail-Erkennung |
| `EnergySystem` | Verbrauch, Schichtende-Signal |
| `EconomySystem` | Geld, Erfahrung, Lohn, Kosten |
| `ProgressionSystem` | Körper-Stufen und Shop-Artikel: Stufen, Kauf, Effekte auf Stats anwenden |
| `EventSystem` | Zufallsereignisse auslösen |
| `SaveSystem` | Spielstand als JSON |
| `UIManager` | Screens ein-/ausblenden |

- **ScriptableObjects** (Daten-Dateien im Unity-Editor) für `BalanceConfig`, `ProgressionItem` (mit Typ Körper oder Shop), `EventDefinition`, `FarmDefinition`. Balancing geschieht dann ohne Code-Änderung.
- **Stats-Prinzip:** Basiswerte aus `BalanceConfig`, Körper-Stufen und Shop-Artikel liefern Modifikatoren, ein `PlayerStats`-Objekt berechnet die finalen Werte.
- Geld als `double` (Incremental-Zahlen wachsen schnell).

### 10.3 Spielstand [Demo]
- `SaveData` als serialisierbare C#-Klasse, `JsonUtility`
- Web: `PlayerPrefs` (im Browser dauerhaft gespeichert), Windows: Datei
- Autosave nach jeder Schicht, beim Kauf, beim Rauswurf und beim Visum

---

## 11. Demo-Umfang

Alles mit [Demo] markiert. Zusammengefasst:

| Enthalten | Nicht enthalten |
|---|---|
| Tag 1–10 der 1. Farm | Prestige (nur Teaser auf dem Demo-Ende-Screen) |
| Auflegen, Balancieren, Belastung, Umsetzen, Gehen, Rennen | Farmen 2 und 3 |
| Energie, Geld, Erfahrung, Tagesquote, Verwarnungen, „Der Neue“ | Schlange, Ratte, Hitze, Frosch |
| 5 Körper-Stufen, 4 Shop-Artikel | Zweite Staude und restliche V1-Stufen und -Artikel |
| Spinne, Regen | Achievements, Steam Cloud, Rauswurf mit Farmwechsel |
| Juice, Sound, Einstellungen, Save, Tutorial | |

Ziel: 15–25 min Spielzeit, Spieler will am Ende weiterspielen.

Die Demo erscheint als offizielle Steam-Demo und zusätzlich als Web-Build auf Webportalen (Kapitel 16, Stufe C und D).

---

## 12. Erfolgskriterien

**Graybox-Test (privat, 3–5 Tester)**
- Mindestens 3 von 5 spielen freiwillig länger als 10 Minuten.
- Fallen und Snappen werden als lustig beschrieben, nicht als unfair.
- Tester verstehen Auflegen und Balancieren ohne Erklärung nach 1–2 Trips.
- Kein Tester mit normaler Einsteigerleistung wird gefeuert.
- Wenn nicht erfüllt: Mechanik anpassen (zuerst Belastung entschärfen oder das Auflegen vereinfachen), notfalls Balancieren durch Timing-Klicks ersetzen.

**Demo (öffentlich)**
- Durchschnittliche Spielzeit über 10 Minuten
- Mehr positive als negative Kommentare
- Mindestens ein Spieler fragt nach der Vollversion

---

## 13. Spätere Versionen und Gestrichenes

### 13.1 Nach dem Release **[A]**
| Feature | Version |
|---|---|
| Endlos-Modus mit Zufallsfarmen | [V2] |
| Skill Tree mit Verzweigungen, falls Spieler mehr Tiefe wollen | [V2] |
| Achievements | [V2] |
| Deutsche Sprache | [V2] |
| Stacker-Rolle spielbar | [V2] |
| Traktorfahrer-Rolle und Rollenrotation | [V2] |
| Hostel-Partys (z. B. Donnerstagsparty mit Folgen für die nächste Schicht) | [V2] |
| Cutter-Rolle spielbar | [V3+] |
| Abend-Screen | [V3+] |
| Benannte Story-Figuren | [V3+] |
| Mehrere Enden | [V3+] |
| Modi Visa-Run / Cash-Run | [V3+] |
| Controller-Unterstützung | [V3+] |
| Mobile | [V3+] |

### 13.2 Bewusst gestrichen **[E]**
Diese Punkte kommen in keiner Version, außer Olli entscheidet es neu:
- Helfer, die automatisch tragen
- Automatisierung und Idle-Anteil jeder Art
- Offline-Fortschritt

---

## 14. Risiken
| Risiko | Gegenmaßnahme |
|---|---|
| Balancieren macht keinen Spaß | Go/No-Go nach Graybox, Fallback Timing-Klick |
| Auflegen und Belastung überfordern zusammen mit dem Balancieren | Im Graybox-Test einzeln ein- und ausschaltbar machen, Belastung notfalls nur für lange Stauden |
| Ohne Helfer fehlt spätes Wachstum | Zweite Staude, Routine, Lohn-Multiplikatoren und Prestige-Boni tragen die Progression; beim Balancing prüfen |
| Art dauert zu lange | Asset-Pakete, strikte Asset-Liste (Kapitel 8.2) |
| Scope Creep | Nur [Demo] und [V1] vor dem Release bauen, neue Ideen in Kapitel 13 |
| Name wird von Webportalen abgelehnt | Richtlinien vor dem Demo-Release prüfen, Art und Marketing harmlos halten |
| Motivation bricht ein | Kleine, sichtbare Meilensteine, frühe Steam-Seite |
| Zu früh releast | Gates der Release-Roadmap einhalten (Kapitel 16) |
| Spiel wird geklont (Web-Builds sind leicht zu kopieren) | Klone suchen, Takedown-Anträge stellen (Kapitel 16, Stufe D) |
| Wenig Wishlists nach Seiten-Launch | Normal, Demo abwarten (Kapitel 16.4) |
| Rauswurf frustriert | Tage und Körper bleiben erhalten, Schwelle großzügig, gute Schichten löschen Verwarnungen |
| Echte Namen | Keine realen Farm- oder Hostelnamen |

---

## 15. Offene Punkte
- [ ] Name der Spielfigur
- [ ] Fiktiver Name der ersten Farm
- [ ] itch.io-Namen und Social-Media-Handle „Banana Humper“ sichern
- [ ] Inhaltsrichtlinien der Webportale (z. B. CrazyGames) zum Namen prüfen
- [ ] Bestätigung aller [A]-Punkte, besonders automatisches Laufen (3.1), Auflegen per Maus (3.2) und Umsetzen per Rechtsklick (3.5)
- [ ] Versionszuordnung in Kapitel 13 bestätigen
- [ ] Neue Namen „Routine“, „Dickes Fell“, „Bier für die Cutter“, „Bier für den Farm Manager“ bestätigen
- [ ] Sprache zum Release
- [ ] Preis Vollversion und Launch-Rabatt
- [ ] Termin für Steam Next Fest
- [ ] Schwellen für Verwarnungen (4.7) im Graybox-Test festlegen

---

## 16. Release-Roadmap

Vom ersten Prototyp bis nach dem Release. Die Roadmap folgt einem dokumentierten Indie-Release aus 2026 (Incremental Game, Steam, über 60.000 Wishlists zum Launch, über 100.000 verkaufte Kopien in 72 Stunden) und überträgt dessen Vorgehen auf Banana Humper.

Die Roadmap beschreibt den Prozess, nicht Features. Features und ihre Versionen stehen in Kapitel 2–13.

### 16.1 Grundprinzipien
1. **Die Demo ist das Marketing.** Eine starke Steam-Demo bringt den Großteil der Wishlists. Alles andere unterstützt sie nur.
2. **Steam ist der beste Test.** Ob ein Spiel auf Steam funktioniert, zeigt Steam selbst am zuverlässigsten. Webportale (itch.io, CrazyGames, Armor Games) sind Ergänzung, nicht Hauptkanal.
3. **Erst den Kern-Loop perfektionieren, dann das Spiel darum bauen.** Nur die Kern-Mechanik bauen, polieren und bestätigen. Danach erneut konzipieren, wie daraus ein ganzes Spiel wird.
4. **Das „Warum“ zählt.** Der Spieler braucht einen Grund für sein Tun. Bei Banana Humper: 88 Tage fürs zweite Visum.
5. **Nicht zu früh releasen.** Lieber eine gute Demo und Community-Feedback als ein fixes Datum halten.
6. **Community zuerst.** Feedback aus Demo und Discord fließt direkt ins fertige Spiel.

### 16.2 Übersicht
Zeitangaben relativ und bei 6–12 h pro Woche geschätzt **[A]**. Ein Gate ist ein Prüfpunkt: Die nächste Stufe startet erst, wenn er erfüllt ist.

| Stufe | Inhalt | Dauer (ca.) | Gate |
|---|---|---|---|
| A | Kern-Loop validieren und polieren | 2–3 Monate | Kern-Loop macht auch allein Spaß und sieht nach dem finalen Stil aus |
| B | Steam-Seite live | 2–4 Wochen | Seite ist öffentlich |
| C | Demo bauen | 2–3 Monate | Demo-Umfang (Kapitel 11) fertig und getestet |
| D | Demo-Launch und Wachstum | ca. 2 Monate | Für Steam Next Fest angemeldet, Demo stabil |
| E | Steam Next Fest | 1 Woche | Fest abgeschlossen |
| F | Vorlauf zum Release | 6–8 Wochen | Vollversion fertig, Releasedatum steht |
| G | Release-Woche | 1 Woche | Spiel ist live, keine kritischen Bugs |
| H | Nach dem Release | laufend | – |

---

### Stufe A: Kern-Loop validieren und polieren
**Ziel:** Beweisen, dass Balancieren allein schon Spaß macht, bevor das restliche Spiel entsteht.

1. **Graybox-Prototyp** der Balance (Kriterien in Kapitel 12). Go/No-Go.
2. **Kern-Loop polieren:** Nur Staude auflegen, tragen, abliefern, fallen lassen oder snappen, aber mit finalem Look und Feel: Art-Stil für Figur, Staude, Trailer und Plantage, Sound, Juice.
3. **Erneut testen:** Macht dieser eine Loop in schöner Form Spaß?
4. **Konzept-Klausur:** Ohne Ablenkung (kein Bildschirm, Spaziergänge, Gym) überlegen, wie sich der Loop zum ganzen Spiel entwickelt. Ideen erst sammeln, nicht sofort bewerten. Das Warum, den Schauplatz und den Zusammenhang prüfen.
5. **GDD aktualisieren** mit den Ergebnissen.
6. **Material sammeln:** Eigene Fotos und Videos von der Bananenfarm 2016 raussuchen. Die echte Geschichte ist Marketing-Material **[A]**.

**Nebenbei:** Organisatorisches für Steam klären (siehe 16.3).

### Stufe B: Steam-Seite live
**Ziel:** So früh wie möglich Wishlists sammeln. Wishlists (Wunschlisten-Einträge) sind der wichtigste Indikator für den Launch-Erfolg, weil Steam Spieler beim Release benachrichtigt.

**Assets für die Seite:**
- Capsule-Bilder (die Vorschaubilder im Store) in allen geforderten Formaten
- Mindestens 5 Screenshots
- Kurzer Trailer (30–60 s), Fokus auf Balancieren und Fallenlassen
- Beschreibung inklusive der Pointe „Yes, that's the real job title“ und dem Warum (88 Tage, Visum)
- Tags, z. B. Incremental, Physics, Casual, Funny, Simulation **[A]**
- Status „Coming Soon“, noch ohne exaktes Datum

**Nach dem Livegang:**
- Ankündigung überall: Discord-Server anlegen, Social-Media-Posts, erster Devlog
- Wöchentlich kurze Clips (YouTube Shorts, TikTok): Fails, Spinnen, echte Farm-Fotos
- Trailer an YouTube-Kanäle schicken, die Indie-Trailer zeigen

**Erwartung:** Anfangs nur wenige Wishlists pro Tag (einstellig). Das ist normal und kein Grund zur Sorge. Einzelne Clips oder ein aufgegriffener Trailer bringen kleine Spitzen.

### Stufe C: Demo bauen
**Ziel:** Eine Demo, die man nicht aus der Hand legen will.

- Umfang laut Kapitel 11
- Als **offizielle Steam-Demo** (eigene Demo-App in Steamworks)
- Am Demo-Ende: Wishlist-Aufruf und Link zum Discord
- Im Menü: Feedback-Button zum Discord
- Parallel weiterhin Clips posten und Wishlists sammeln
- **Vor dem Demo-Launch (wenige Tage vorher):** Presse und Trailer-Kanäle anschreiben, Demo-Trailer auf YouTube, Short veröffentlichen

### Stufe D: Demo-Launch und Wachstum bis zum Next Fest
**Ziel:** Mit der Demo auf Steam Aufmerksamkeit und Wishlists aufbauen.

1. **Demo auf Steam veröffentlichen.** In den ersten Tagen die Spielerzahlen beobachten: Landet die Demo in den Trending-Listen, bringt Steam selbst die meisten Spieler.
2. **Reddit:** Posts in passenden Subreddits, z. B. r/incremental_games, r/playmygame, r/IndieGaming (Regeln der jeweiligen Community vorher lesen) **[A]**.
3. **Webportale:** Demo zusätzlich als Web-Build auf itch.io, CrazyGames, Armor Games.
4. **Content-Creator-Outreach:** Über rund zwei Monate hinweg Hunderte YouTuber und Streamer anschreiben, die Incremental-, Casual- oder Physik-Spiele spielen. Richtwert: 50–100 Mails pro Woche mit kurzer, persönlicher Vorlage **[A]**. Creator lesen die Mails, entscheiden aber schnell. Manche spielen sofort, andere Wochen später.
5. **Community:** Feedback aus Discord, Steam-Diskussionen und Reviews sammeln, Bugs fixen, Demo regelmäßig aktualisieren.
6. **Klone beobachten:** Nach Kopien des Spiels suchen (auch in chinesischen Plattformen wie Douyin) und bei Funden Takedown-Anträge stellen.
7. **Für Steam Next Fest anmelden** (Frist in Steamworks beachten).

### Stufe E: Steam Next Fest
Steam Next Fest ist ein mehrtägiges Steam-Event, bei dem Tausende Demos gezeigt werden. Es findet mehrmals im Jahr statt (bisher meist Februar, Juni, Oktober). Jedes Spiel kann nur einmal teilnehmen, deshalb den Termin gezielt wählen.

- Mit aktualisierter Demo teilnehmen
- Kurz vorher: Trailer erneut an Presse und Trailer-Kanäle, Creator gezielt auf das Fest hinweisen
- In den ersten zwei Tagen bekommen alle Spiele ähnlich viele Einblendungen. Was danach passiert, hängt davon ab, wie gut die Demo konvertiert.
- **Keine bezahlte Werbung** (z. B. Reddit Ads). In der Referenz brachten 200 $ nur rund 20 Wishlists, weil während des Fests die Werbepreise hoch sind.
- Wiederholte Reddit-Posts während des Fests bringen wenig.

### Stufe F: Vorlauf zum Release
**Ziel:** Das Spiel fertigstellen und die Aufmerksamkeit bis zum Launch steigern.

1. **Exaktes Releasedatum setzen**, damit das Spiel in Steams Upcoming-Kalender erscheint. Das war in der Referenz der stärkste Wachstumstreiber, rund zwei Monate vor Release **[A]**.
2. **Datum nicht in einen großen Steam-Sale legen und nicht direkt dahinter planen:** Während des Summer Sale war der Kalender ausgeblendet und die Wishlists sanken.
3. Vollversion mit dem Community-Feedback fertigstellen (Kapitel 5–7).
4. Creator-Outreach weiterführen, jetzt mit Hinweis auf das Releasedatum.
5. **Preis festlegen:** Basispreis, regionale Preise (Steam-Vorschläge nutzen) und Launch-Rabatt (Referenz: 30 %) **[A]**.
6. Pressekit erstellen (Beschreibung, Screenshots, Trailer, Logo, Kontakt, die echte Farm-Geschichte).
7. **Letzte zwei Wochen:** Hier wachsen die Wishlists am stärksten (Referenz: bis zu 4.000 pro Tag), besonders ab dem Sonntag vor der Release-Woche, wenn das Spiel auf der Startseite erscheint.

### Stufe G: Release-Woche
- Release mit Launch-Rabatt
- Ankündigung auf allen Kanälen, im Discord und bei Creatorn, die die Demo gespielt haben (mit Steam-Keys für die Vollversion)
- Täglich Reviews, Steam-Diskussionen und Discord lesen und antworten
- Hotfixes bereithalten: Die ersten Tage entscheiden über die Review-Quote

### Stufe H: Nach dem Release
1. **Bugs zuerst:** Zwei Wochen lang Stabilität vor neuen Inhalten.
2. **Transparenz:** Devlog oder Video mit Zahlen und Learnings. Das stärkt die Community.
3. **Keine überhasteten Updates:** Erneut eine Konzept-Klausur, bevor neue Inhalte gebaut werden. Kandidaten für [V2] und [V3+] stehen in Kapitel 13.
4. Community-Wünsche im Discord sammeln und priorisieren.
5. An Steam-Sales teilnehmen.
6. Klone und Piraterie weiter beobachten.

---

### 16.3 Organisatorische Voraussetzungen (vor Stufe B)
- [ ] Steamworks-Account anlegen, Steam-Direct-Gebühr (100 $ pro Spiel) zahlen
- [ ] Steuer- und Bankdaten für Steam hinterlegen
- [ ] Nebentätigkeit mit dem Arbeitgeber und steuerliche Anmeldung klären (Steuerberater fragen)
- [ ] Discord-Server, YouTube-Kanal, TikTok-Account „Banana Humper“ anlegen
- [ ] Steam-Seite muss vor Release mindestens zwei Wochen als „Coming Soon“ online sein (bei früher Seite automatisch erfüllt)

### 16.4 Orientierungswerte aus der Referenz
Keine Zielwerte, sondern Anhaltspunkte zur Einordnung. Das Referenzstudio hatte sieben Jahre Erfahrung, rund 100 Prototypen und eine bestehende Community. Ein Teil des Erfolgs war zudem nicht erklärbar.

| Zeitpunkt | Referenz |
|---|---|
| Steam-Seite, erster Tag | 11 Wishlists |
| Danach bis zur Demo | 0–7 Wishlists pro Tag, einzelne Spitzen bis 39; nach gut zwei Monaten rund 500 gesamt |
| Nach dem Demo-Launch | Von ca. 8 auf bis zu 384 Wishlists pro Tag, über 300 gleichzeitige Spieler am ersten Tag |
| Steam Next Fest | Rund 7.000 zusätzliche Wishlists |
| Letzte zwei Wochen vor Release | Bis zu 4.000 Wishlists pro Tag |
| Release | Über 60.000 Wishlists, Demo mit rund 80.000 Spielern |
| 72 Stunden nach Release | Über 100.000 verkaufte Kopien |

### 16.5 Eigene Ziele **[A]**
| Meilenstein | Ziel |
|---|---|
| Vor dem Demo-Launch | 500 Wishlists |
| Vor Steam Next Fest | 3.000 Wishlists |
| Release | 10.000 Wishlists |

---

## 17. Glossar
| Begriff | Bedeutung |
|---|---|
| Graybox | Prototyp aus grauen Platzhalterformen |
| Game Feel | Wie sich Steuerung und Reaktionen anfühlen |
| Juice | Kleine Effekte (Sound, Wackeln, Partikel), die Aktionen befriedigend machen |
| Kern-Loop | Die kleinste Aktion, die sich wiederholt |
| Run | Ein Durchgang, der endet (hier: Schicht) |
| Prestige | Freiwilliger Neustart mit dauerhaftem Bonus |
| Meta-Progression | Fortschritt, der über Runs hinweg bleibt |
| Scope Creep | Ungeplantes Anwachsen des Umfangs |
| ScriptableObject | Unity-Datendatei, die im Editor bearbeitet wird |
| Diegetischer Sound | Geräusche, die in der Spielwelt selbst existieren |
| Tuning | Feinjustieren von Zahlenwerten, bis es sich gut anfühlt |
| Gate | Prüfpunkt, der erfüllt sein muss, bevor die nächste Stufe startet |
| Wishlist | Eintrag auf der Steam-Wunschliste; Spieler werden beim Release benachrichtigt |
| Capsule | Vorschaubild eines Spiels im Steam-Store |
| Steam-Demo | Kostenlose Probierversion als eigene App auf Steam |
| Steam Next Fest | Mehrtägiges Steam-Event mit Tausenden Demos, mehrmals im Jahr |
| Trending | Steam-Liste mit Spielen, die gerade schnell Spieler gewinnen |
| Upcoming-Kalender | Steam-Übersicht kommender Releases, braucht ein exaktes Datum |
| Content Creator | YouTuber und Streamer, die Spiele zeigen |
| Outreach | Gezieltes Anschreiben von Creatorn und Presse |
| Pressekit | Sammlung von Infos, Bildern und Trailer für Presse und Creator |
| Launch-Rabatt | Preisnachlass in der Release-Woche |
| Regionale Preise | Angepasste Preise je Land |
| Takedown | Antrag an eine Plattform, eine Kopie zu entfernen |
| Devlog | Entwicklungstagebuch, oft als Video |
| Versatz (Offset) | Abstand zwischen Schulter und Schwerpunkt der Staude beim Auflegen |
| Sweet Spot | Der perfekte Treffer, belohnt mit Extra-Effekt |
