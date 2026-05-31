---
name: animal-io-core-design
description: Diferențiatorul/identitatea de bază a jocului animal-io (ideea ținută 2 ani de Cristian)
metadata: 
  node_type: memory
  type: project
  originSessionId: 08f0c08d-bdd8-418c-9003-781403a86d19
---

**Diferențiatorul animal-io** (Cristian și-a amintit-o pe 25 mai 2026, e ideea originală de acum ~2 ani): **fiecare animal are un SKILL unic** + **fiecare hartă e din ce în ce mai grea**. Urci pe o **scară de lumi/hărți tot mai dure**, stăpânind animale care joacă DIFERIT.

**De ce contează:** rezolvă slăbiciunea genului .io (jocurile .io sunt plate, fără capăt, fără progres). Identitatea = **progres + călătorie prin lumi tot mai grele**. Fantezie: te ridici în lanțul trofic (șoricel→leu) prin etape tot mai dure.

**Cum se aplică (principii de design agreate):**
- Evoluția schimbă **CUM joci** (stil/skill nou), nu doar numerele/mărimea.
- Skill-urile sunt **animal-flavored** (dash șoricel, salt iepure, venin/scuipat în loc de „arme", răget leu) — NU pistoale; păstrează fantezia intuitivă pt copii.
- „Câștigătorii avansează în altă etapă" = **urcatul pe scara de hărți**.
- Verbe mereu prezente: **miști (mouse) + mănânci + 1 abilitate (tastă)**. Puține, simple.
- Inima jocului = luptă/abilități (gen „Brawl Stars × lanțul trofic"), NU doar creștere de masă ca agar.io clasic.
- **MODEL B confirmat (25 mai 2026):** animalele = PERSONAJE pe care le DEȚII (ca brawlerii din Brawl Stars), nu doar trepte de evoluție în meci. Echipe 5v5 MIXTE (nu toți au lei — unii câine etc.). Un animal poate avea MAI MULTE skilluri, cumpărate/câștigate. „Șoricel→leu" = progresul CONTULUI prin hărți tot mai grele, nu un meci de 5 min.
- **Ordine moduri:** (a) scară single-player PvE (se construiește prima, fără server) → (b) aceeași scară devine tiere ranked PvP pe echipe (Faza 4-5).
- **Combat = Varianta 2 „animal brawler"** (Brawl Stars cu animale): țintești + lovești cu abilitatea animalului = INIMA jocului; mâncatul/power-up-urile pe hartă = secundar (vindecare/întărire).
- **⭐ REGULA DE ECHILIBRU (fundație, bătută în cuie 25 mai 2026):** HĂRȚILE = TIER-URI DE PUTERE. Te lupți DOAR în tier-ul tău. Echipă = specii mixte din ACELAȘI tier (roluri diferite, putere egală). Tier mic = șoricel/iepure/vulpe (egale între ele); tier mare = leu/elefant/tigru (egale între ele). Urci tier-uri → deblochezi animale mai puternice (fantezia „șoricel→leu" păstrată în absolut), DAR 5 șoareci nu întâlnesc niciodată 5 lei (hărți diferite). NU amesteca câine+leu în aceeași echipă = rupt. Identic SP (boți din tier) și MP (matchmaking pe tier). Șoricelul e viabil în tier-ul lui (viteză+dash+ascundere+skilluri). David-vs-Goliat = doar mod special, nu regulă.
- **Garda anti-încurcătură:** animale & skilluri construite CA DATE (registre), nu hardcodate. Design complet pe hârtie, build minimal pe etape. Vezi `animal-io-web\DESIGN.md` + `ROADMAP.md`.

Vezi [[animal-io-web-pivot]] (stack/roadmap) și [[user-cristian-dev]].
