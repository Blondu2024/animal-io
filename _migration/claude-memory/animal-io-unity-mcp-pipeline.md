---
name: animal-io-unity-mcp-pipeline
description: "Lecții tehnice CRITICE Unity-MCP pe Cloud PC (30 mai 2026) — cum rulezi cod de editor + bagi modele Meshy texturate+animate în joc, ocolind limitările fără focus"
metadata: 
  node_type: memory
  type: reference
  originSessionId: 50025d1a-14ac-4d30-afc5-6cbc5426668d
---

**Context:** asamblarea modelelor Meshy (boss/maimuță/props) în joc prin Unity-MCP pe Cloud PC. Multe operații au ziduri; iată ce MERGE:

**1. ⚠️ `refresh_unity scope=scripts` NU detectează fișiere `.cs` modificate pe disc** (refresh_triggered:false). Folosește **`scope=all` + `compile=request` + `mode=force`** → recompilează real (refresh_triggered:true). Asta bloca tot la început.

**2. ⚠️ Pe Cloud PC FĂRĂ focus pe fereastră, `EditorApplication.update` și `delayCall` NU rulează** (frozen). Pentru automatizare de editor folosește **`[DidReloadScripts]`** — rulează în pump-ul de reload declanșat de MCP, fără focus. (Și Play îngheață fără focus — vezi [[animal-io-animation-pipeline]].)

**3. ⚠️ `execute_menu_item` NU rulează menu items CUSTOM** ("no menu named..."). Doar built-in Unity.

**4. ⚠️ `execute_code` MCP e stricat** pe instanța asta (CodeDom "filename too long" la mono; Roslyn neinstalat). `unity_reflect` doar inspectează, nu execută.
→ **SOLUȚIA pt cod arbitrar de Unity:** scrie un script `Assets/Editor/*.cs` cu `[DidReloadScripts]` care face treaba; declanșează cu `refresh_unity scope=all compile=request`. Vezi `Assets/Editor/AnimalAssembly.cs`.

**5. TEXTURĂ (Mixamo pierde textura Meshy):** `meshy_download_model format=fbx include_textures=true save_to=<abs>` → dă PNG-uri PBR separate (base_color/normal/metallic/roughness/emission). Copiezi base+normal în Assets → `manage_asset modify properties={"textureType":"NormalMap"}` pe normal → `manage_material create` (URP/Lit) → `set_material_shader_property _BaseMap/_BumpMap` → `assign_material_to_renderer` (target = SkinnedMeshRenderer prin search_method by_id; renderul e pe copil „Mesh_0", nu pe root).

**6. ANIMATOR (clipuri Mixamo prinse ca sub-assets în FBX — MCP nu le leagă):** în editor script, `AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>().First()` → `state.motion = clip`. MERGE. Controller-ul boss = stări Move(loop)/Attack(trigger)/Die(AnyState trigger). Loop pe locomotion via `ModelImporter.defaultClipAnimations[i].loopTime=true` + SaveAndReimport.

**7. SCREENSHOT ca „ochi":** `manage_camera screenshot batch=orbit view_target=<obj> include_image=true` se încadrează automat pe bounds. Screenshot pozițional fix ratează ușor obiectul.

**STARE 30 mai:** ✅ `Boss_Orangutan.prefab` construit (texturat + Animator + controller cu clipuri). ⚠️ avatar=False de reparat (Generic merge totuși prin ierarhie). RĂMAS: wire `BossRoom.SpawnEnemies`+`Boss.cs` la prefab/animator; maimuța (același flux, lipsesc texturi+material+controller); props (texturi+plasare). Vezi [[animal-io-level1-creatures]].
