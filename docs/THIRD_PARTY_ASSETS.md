# Third-Party Assets

Alle importierten Grafik-Assets unter `Assets/Resources/Art/` stammen von
[Kenney.nl](https://kenney.nl) und stehen unter **CC0 1.0** (Public Domain):
frei nutzbar in privaten und kommerziellen Projekten, Namensnennung nicht
verpflichtend (aber willkommen).

| Ordner | Kenney-Paket | Quelle | Verwendung |
|---|---|---|---|
| `Assets/Resources/Art/Player/` | [Toon Characters](https://kenney.nl/assets/toon-characters) – "Male adventurer" | `idle`, `hurt`, `walk_0`..`walk_7` Posen | Spielfigur (`PlayerAnimator.cs`) |
| `Assets/Resources/Art/Background/` | [Background Elements](https://kenney.nl/assets/background-elements) – "Flat"-Variante | `hills_far`/`hills_near`/`tree_a`/`tree_b`/`grass_tuft`/`cloud`/`fence` | Plantagen-Kulisse (`GameBootstrap.SetupScenery`) |

Die "Flat"-Variante der Background-Elements-Assets ist eine einheitliche
helle Silhouette ohne Eigenfarbe – sie wird zur Laufzeit per
`SpriteRenderer.color` auf die GDD-8.1-Palette (Grün, Gelb, Erdbraun,
Himmelblau) eingefärbt.

## Bewusst nicht mit fertigen Assets gelöst

Für die Bananenstaude, den Trailer und die Cutter-Hintergrundfigur wurde
kein passendes freies CC0-Asset gefunden (insbesondere fehlt ein Bananen-
Sprite mit den drei Zuständen gesund/durchgebogen/gesnappt aus GDD 3.5/3.8).
Diese Elemente werden stattdessen prozedural aus kantengeglätteten
Vektorformen gebaut (`SpriteFactory.CreateRoundedQuad`/`CreateEllipse`,
`BananaBunchVisual.cs`) statt aus harten Rechtecken. Die Cutter-Figur bleibt
außerdem bewusst ohne konkrete Hautfarbe/Gesichtszüge abstrakt, siehe
GDD 1.7: "Cutter als Bogans, nie ethnisch markiert".

## Lizenztext

Der Original-Lizenztext beider Pakete (`License.txt` aus dem jeweiligen
Kenney-Download):

```
License: (Creative Commons Zero, CC0)
http://creativecommons.org/publicdomain/zero/1.0/

This content is free to use in personal, educational and commercial projects.
Support us by crediting Kenney or www.kenney.nl (this is not mandatory)
```
