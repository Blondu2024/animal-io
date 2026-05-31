---
name: animal-io-level1-creatures
description: "Direcție creativă Level 1 (30 mai 2026) — GATA cu zombii generici; temă PRIMATE: inamici = maimuțe-cârlig (gibon agil cu gheară-cârlig), boss = urangutan masiv. Pe brand animal-io."
metadata: 
  node_type: memory
  type: project
  originSessionId: 50025d1a-14ac-4d30-afc5-6cbc5426668d
---

**Decizie 30 mai 2026 — înlocuim zombii din Level 1 cu creaturi originale.** Cristian: „ce-ar fi să nu facem zombie, ceva nou cu vibe de hook". Zombii = clișeu, fără identitate. Direcție aleasă = **temă primate**, pe brand-ul [[animal-io-core-design]] (animale cu skilluri) și legată de mecanica de cârlig:

- **Inamici (pluton)** = **maimuțe-cârlig**: gibon/primat slab, agil, cu o gheară-cârlig de metal; se balansează pe lanțuri/cârlige din tavan, se aruncă spre player și se retrag. „Hook" = mecanica de grappling făcută inamic + verticalitate în luptă.
- **Boss** = **urangutan masiv** (alfa-ul haitei): brațe lungi puternice, lovește cu pumnii (se potrivește cu slam-ul existent din Boss.cs, fără secure). Înlocuiește capsula roșie primitivă.

**Pipeline asset:** Meshy-6 (calitate max, ales de user) → text_to_3d t-pose, formate FBX+GLB → download în `Assets/Meshy/` → Unity import nativ FBX → Mixamo rig (pipeline dovedit [[animal-io-animation-pipeline]]).

**STARE BOSS urangutan (30 mai, sold Meshy ~891):**
- ✅ Model generat + texturat + APROBAT de user (blană roșcat-maronie, față piele, cicatrici). Total 30 cr (preview 20 + refine PBR 10).
- ✅ Model texturat STATIC (fără schelet) la `Assets/Meshy/boss_orangutan_textured.fbx` (single MeshRenderer, UV-uri intacte).
- ✅ User a riguit pe Mixamo + descărcat 3 animații → copiate curat în `Assets/Meshy/Boss/`: `boss_walk.fbx` (Crouched Walking), `boss_attack.fbx` (Jump Attack), `boss_die.fbx` (Mutant Dying). ⚠️ LIPSEȘTE un Idle — de adăugat de pe Mixamo.
- Toate au ACELAȘI schelet Mixamo → clipurile se pot refolosi pe un singur avatar.

**✅ INTEGRAT 30 mai (sesiune lungă):** Toate modelele texturate (material URP din PBR Meshy) + 2 controllere + prefab-uri construite prin `Assets/Editor/AnimalAssembly.cs` (vezi [[animal-io-unity-mcp-pipeline]]). Prefab-uri personaje în `Assets/Resources/` (Boss_Orangutan, Monkey); props în `Assets/Meshy/Props/*.prefab`. COD CABLAT: `BossRoom.SpawnEnemies` instanțiază Resources prefab-urile (+CapsuleCollider, scale bossScale=2.2/monkeyScale=0.9), `Boss.cs` drive Animator (Attack/Die triggers, mârâit, fără secure de cod, flash alb nu roșu), `Zombie.cs` drive Animator (Move loop + Attack). Compilează curat. ⚠️ NETESTAT în Play (Cloud PC îngheață fără focus) — userul testează intrând în camera de boss, calibrăm scări/collidere/weakpoint după.
✅ PROPS PLASATE (30 mai): `AnimalAssembly.PlaceProps()` atașează 16 props pe hazarde (3 țepi, 3 braziere, 1 butoi, 4 cufere, 5 segmente pod) ca copii „_propvis", auto-scalate din bounds, ascunde cubul gri original (mai puțin FireTrap care păstrează flacăra). Idempotent. Confirmat vizual: butoi + braziere cu emission portocaliu arată super în coridor. Scări reglabile în PlaceProps (worldSize per tip). ⚠️ Rula doar OUT of Play mode (EditorSceneManager). NETESTAT în Play complet. ⏭️ Cârlig de atașat la mâna maimuței; calibrare fină scări/poziții după feedback Play.

**PAȘI VECHI (acum acoperiți mai sus):**
1. Aplică materialul texturat (de pe boss_orangutan_textured.fbx) pe SkinnedMeshRenderer-ul modelului riguit (UV identice → se potrivește). ⚠️ FBX Mixamo vine fără textura Meshy.
2. Creează Animator Controller cu stări walk/attack/die (+idle când îl iei).
3. Salvează ca prefab `Assets/Meshy/Boss/Boss_Orangutan.prefab`.
4. `BossRoom.SpawnEnemies` (linia ~48): înlocuiește `CreatePrimitive(Capsule)` roșu cu `Instantiate(prefab)`.
5. `Boss.cs`: leagă Animator de stări (Windup→atac, Slam, Death) + scoate BuildWeapon-ul cu securea (acum lovește cu pumnii). Scale model vs attackRange/slamRadius de recalibrat.
⚠️ De făcut cu user prezent (testează în Play). `execute_code` MCP nefuncțional pe instanța asta.

**STARE MAIMUȚĂ-CÂRLIG (inamic, 30 mai, sold Meshy ~681):**
- ⚠️ LECȚIE RIGGING: prima versiune avea cârligul în mâna dreaptă → Mixamo a dat EROARE (cotul+încheietura acoperite, nu putea pune markerii). Ștearsă. **Regula: personaje pt Mixamo = mâini libere, deschise, depărtate de corp, FĂRĂ obiecte ținute.** (Boss-ul a mers că avea mâinile libere.)
- ✅ REFĂCUTĂ fără cârlig, T-pose curat, mâini deschise, texturată — la `Assets/Meshy/monkey_riggable_textured.fbx` (15 MB). Aprobată de user. Re-cost ~30 cr (preview 20 + refine 10).
- ✅ Mixamo a ACCEPTAT versiunea nouă. User a luat 2 anims → în `Assets/Meshy/Monkey/`: `monkey_attack.fbx` (Dual Weapon Combo) + `monkey_run.fbx` (Running). ⏭️ Mai lipsesc Idle + Death (de luat când vrea — Death util ca să înlocuiască zombie_die din Zombie.cs).
- ⏭️ CÂRLIGUL se atașează SEPARAT în Unity: prop mic parentat la osul mâinii drepte după rigging (de generat un prop „rusty hook" sau modelat simplu). Așa păstrăm identitatea fără să stricăm scheletul.

**Bilanț credite sesiune 30 mai:** ~921 → ~861 (60 cr: boss 30 + maimuță 30). Ambele primate modelate+texturate, pe Meshy-6 (consistență vizuală). Boss riguit, maimuța încă nu.

**PROPS Level 1 (30 mai, statice/fără rig) — toate Meshy-6 texturate PBR, în `Assets/Meshy/Props/`:** `prop_barrel.fbx` (RollingHazard), `prop_spikes.fbx` (SpikeTrap), `prop_chest.fbx` (Interactable), `prop_brazier.fbx` (FireTrap), `prop_bridge_plank.fbx` (CollapseBridge). Cost 150 cr (5×30) → sold ~711. De înlocuit cuburile gri cu ele (drop în scenă + scalare + pe unele păstrezi colliderul-trigger existent). User a cerut „fă obiectele cât fac eu maimuța în Mixamo".

**Motivație:** atacăm grey-blockout-ul direct cu Meshy în loc să așteptăm pe Santy (vezi [[animal-io-mobile-testing]]); hrănește [[cristian-visual-motivation]] + dă conținut original de [[cristian-community-strategy]] build-in-public.
