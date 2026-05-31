---
name: animal-io-meshy
description: Meshy AI MCP configurat și funcțional pentru pipeline-ul de assets 3D al animal-io
metadata: 
  node_type: memory
  type: project
  originSessionId: 2b94f2dd-0331-4627-ba9d-a00974bbaa45
---

Serverul MCP **Meshy** (generare assets 3D cu AI) e conectat și funcțional în sesiunea Claude (25 mai 2026, user a băgat cheia API). `meshy_check_balance` → **1100 credite** disponibile.

Acoperă pipeline-ul de assets pentru [[animal-io-setup]] / [[animal-io-web-pivot]]: text→3D, imagine→3D, retexturare, remesh, rigging + animații, export GLB/FBX/OBJ/USDZ etc.

Repere cost (din instrucțiunile serverului):
- text→3D meshy-6 (calitate max): ~20 credite; meshy-5: ~5 credite
- text→3D refine (texturare): 10 credite
- rig: 5 credite (include animații walk + run gratis); animație custom: 3 credite
- remesh: 5; retexture: 10; text→imagine / imagine→imagine: 3–9

⚠️ Regula serverului: confirmă costul cu userul ÎNAINTE de orice generare care consumă credite. Pentru rigging/animație, modelul trebuie generat în `pose_mode="t-pose"`. Pentru web + performanță, preferă low-poly / stylized.

✅ TESTAT end-to-end (25 mai 2026) pe **șoricelul** (primul animal din scară): meshy-6 low-poly stylized, mascotă bipedă în t-pose → text→3D (20) + refine textură (10) + rig cu walk/run (5) = **35 credite** pentru un animal complet animat de bază. A mers fără probleme, ~70s/pas. Confirmat: un animal premium complet = ~35 cr, cum estimasem. Assets în `C:\Users\Shadow\meshy_output\` + contul meshy.ai. Rămase ~1065 credite.

Învățat: preview-ul text→3D vine NEtexturat (gri) — culoarea vine abia la refine; ăsta e designul Meshy (validezi forma înainte să plătești texturarea). Pentru low-poly + meshy-6 nu folosi `model_type=lowpoly` (ignoră ai_model) — folosește meshy-6 standard cu `target_polycount` mic (~12k).

✅ INTEGRAT în prototipul web (vezi [[animal-io-web-pivot]]) — șoricelul GLB riggat a înlocuit sfera-placeholder a player-ului în Babylon.js.

⚠️ Lecție auto-rig Meshy: pe mascotă stylized, skinning-ul leagă urechile/fața de mai multe oase → la animația de mers fața se deformează (vizibil din față, ok din spate). E slăbiciunea cunoscută a auto-rig-ului AI (confirmă secț. 9b din MASTER: „AI 3D cere cleanup"). User a ales să o lase așa deocamdată (e test de pipeline). Fix-uri posibile dacă deranjează: mișcare procedurală rigidă (0 cr) sau re-rig (5 cr, nesigur).

✅ Skill Dash animat (25 mai 2026, +3 cr → 38/1100 folosite): `meshy_animate` action_id=**510** (Standard_Forward_Charge) pe rig-ul șoricelului. Integrat în web: animația vine ca GLB SEPARAT cu schelet propriu (`mouse_dash.glb`), deci în Babylon NU merge să combini clipuri pe un schelet ușor — soluția robustă = încarci un model per animație și comuți vizibilitatea (`setEnabled`) la skill. `startDash`/`endDash` în game.js.

📋 Cum găsești `action_id`-urile (NU există tool de listare): WebFetch pe https://docs.meshy.ai/en/api/animation-library — listează ID-uri numerice. Utile: charge 509-515, jump/leap 13/86/460-472, roll/dodge 156-164, slide 516-519.

🔄 REFĂCUT (25 mai 2026, +38 cr → 76/1100): userul n-a plăcut șoricelul low-poly cartoon, l-a vrut realist/detaliat „la capacitate maximă". Noul șoricel: meshy-6, **150k poligoane**, **PBR on**, biped (păstrat biped fiindcă animațiile sunt umanoide), realist. Rig + walk + Dash(510) re-aplicate. Fișierele din joc (`mouse_walk.glb`/`mouse_dash.glb`) suprascrise cu noul model — codul le ia automat la refresh. Task ids noi: model 019e5f20, refine 019e5f24, rig 019e5f25, anim 019e5f26.

⚠️ Tradeoff de urmărit: 150k+PBR e MULT mai greu pe web decât low-poly — pt zeci de animale pe ecran probabil va trebui remesh/optimizare. PBR poate părea întunecat fără environment/IBL în Babylon (de adăugat dacă e cazul, 0 cr).

✅ DECIZIE FINALĂ (25 mai 2026): după ce a văzut ambele pe hartă, userul a ales **primul șoricel (low-poly cartoon, task 019e5efe/rig 019e5f07/dash 019e5f19)** în locul celui realist — se potrivește cu stilul stilizat al hărții + ușor pe web. Fișierele din joc (`mouse_walk.glb`/`mouse_dash.glb`) au fost readuse la primul șoricel. Cel realist (019e5f20...) rămâne în contul Meshy dar NU se folosește. Stilul de assets pt tot jocul = low-poly stylized cartoon, NU realist.

Următor posibil: boții cu același model (roșiatic), sau următorul animal (pisică, ~35 cr).
