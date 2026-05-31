---
name: animal-io-starter-assets
description: "Pivot 28 mai (seară) — abandonat PlatformerController custom, integrat Unity Starter Assets ThirdPerson + Cinemachine + PlatformerExtras propriu peste pentru Dash/Roll/Crawl/Climb"
metadata: 
  node_type: memory
  type: project
  originSessionId: b4ec02a3-8be3-44fb-b21f-beb8f455740a
---

**Stare la final de sesiune 28 mai 2026 (seară):** după o zi întreagă de tuning pe PlatformerController custom (Rigidbody-based) care nu se simțea bine, **pivot major la Unity Starter Assets ThirdPerson Controller** (oficial Unity, gratis pe Asset Store). Cinemachine 3.1.6 instalat ca dependență.

**Structura nouă în Shooter.unity:**
- `PlayerArmature` (CharacterController + ThirdPersonController + StarterAssetsInputs + PlayerInput + **PlatformerExtras** custom)
- `MainCamera` (Camera + CinemachineBrain + AudioListener)
- `PlayerFollowCamera` (CinemachineVirtualCamera cu Cinemachine3rdPersonFollow și CinemachineBasicMultiChannelPerlin)
- Vechiul Player capsulă + FirstPersonCamera + PlatformerController = ȘTERSE din scenă (fișierele rămân pe disc)

**PlatformerExtras.cs** (`Assets/Scripts/Shooter/PlatformerExtras.cs`) — extinde ThirdPersonController fără să-l atinge:
- **Sprint Shift**: forțează `sai.sprint = Keyboard.current.leftShiftKey.isPressed` în Update (ocolește Input Action buggy)
- **Dash Q**: bumpează `tpc.MoveSpeed/SprintSpeed` la 14, `SpeedChangeRate` la 80, `RotationSmoothTime` la 0.01, forțează `sai.move=(0,1)` și `sai.sprint=true` pe durata 0.25s. TPC face cc.Move el însuși cu viteza nouă. NU dezactivează TPC, NU cheamă cc.Move propriu.
- **Roll W tap-tap**: identic ca dash dar Speed=10, Duration=0.7. Plus **tumblă vizibilă** prin rotirea `Skeleton.localRotation` 360° pe X axis cu lift compensator (`0.9 * (1-cos(angle))`) ca să nu cadă prin podea.
- **Crawl C hold**: scade MoveSpeed/SprintSpeed la 1.5. Vizual = **squash Y la 0.5 pe PlayerArmature.localScale** + CC height la 1.0 + camera coboară la y=0.6 + tranziții lerp smooth (MoveTowards la 5/sec). **User a respins varianta prone-flat** (skeleton rotit 90° pe X) — prefera crouch.
- **Climb E hold + WallInFront raycast**: în LateUpdate (după TPC), un singur `cc.Move(Vector3.up * 3 * dt)`.
- **Respawn Backspace**: cc.enabled=false → teleport spawn → restore tot (scale, scoot, skeleton rotation, transitions).
- Debug HUD activ (DebugHUD bool) jos-stânga, arată Grounded/Crawl/Dash/Roll/Climb + key states + sai.move/sprint + TPC.MoveSpeed/SprintSpeed + cooldowns.

**⚠️ LECȚII CRITICE găsite pe Unity forums (după multă frustrare a userului):**
1. **Sprint type=PassThrough** în Starter Assets `.inputactions` are bug cunoscut Unity — skip-uiește events `started`/`canceled` → callback OnSprint nu se trage. **Fix: schimbat la `Button`.** Fișier: `Assets/StarterAssets/InputSystem/StarterAssets.inputactions`.
2. **Look ScaleVector2 default 0.05** prea mic → mouse super-lent. **Schimbat la 0.5** (10× mai responsiv).
3. **Cinemachine3rdPersonFollow `Damping` default (0.1, 0.25, 0.3)** dădea senzație de lag. **Setat (0,0,0)** pentru urmărire instant.
4. **CinemachineBasicMultiChannelPerlin Noise `AmplitudeGain=0.5`** dădea handheld shake care confunda userul. **Setat 0.**
5. **NU chema `cc.Move()` de mai multe ori pe frame** — al doilea call e parțial respins de collide-and-slide. Plus `Vector3.down * 2` din primul mănâncă deplasarea orizontală prin proiecție pe sol. PATTERNUL CORECT: bumpa MoveSpeed/SprintSpeed și lasă TPC să facă singurul cc.Move. (Asta a rezolvat dash-ul care mergea 0.5m din 2.5m teoretic.)
6. TPC defaults `MoveSpeed=2.0, SprintSpeed=5.335` sunt pentru demo scene scale. **Bumpat la 4 / 8** pentru feel snappy.

**Setări active confirmate live:**
- `MoveSpeed=4, SprintSpeed=8, MouseSensitivity=1, RotationSmoothTime=0.12`
- Cinemachine3rdPersonFollow `Damping=(0,0,0)`, `ShoulderOffset=(1,0,0)`, `CameraDistance=4`, `CameraSide=0.6`
- Input Action `Sprint: Button`, `Look ScaleVector2(0.5, 0.5)`
- PlatformerExtras: Dash 14×0.25=3.5m, Roll 10×0.7=7m, Crawl 1.5m/s

**Pipeline import unitypackage din Asset Store** (lecție Cloud PC): userul nu a vrut să bage cont Asset Store + click manual prin GUI. Soluție = script editor `MCP/Import Starter Assets ThirdPerson` (Assets/Editor/ImportStarterAssets.cs) care cheamă `AssetDatabase.ImportPackage(path, interactive: true)` pe pachetul descărcat la `%APPDATA%\Unity\Asset Store-5.x\Unity Technologies\Unity Essentials\*.unitypackage`. **NU funcționează în play mode**, trebuie stop întâi. Userul a tot intrat în play accidental → multe erori "cannot be used during play mode" pe MCP set_property/save_scene.

**Meshy:** balanță 921 cr (924 → 921). Au plecat 3 cr pe regenerare `archer_crawl.glb` cu **action_id 372 Prone_Reach_Help** — dar Starter Assets character e altă rig, deci clipul Meshy nu se folosește pe PlayerArmature curent. Rămâne pe disc, va fi util când reactivăm arcașul Meshy în altă scenă.

**Următor pas (când reluăm):** userul a zis "salveaza ne auzim maine". Posibile direcții:
- **A) Continuăm cu Invector Third Person Controller plătit (~$40)** — userul considera asta pentru că hack-urile noastre vizuale (skeleton rotate pentru roll, scale squash pentru crouch) nu sunt curate. Invector are anim REALE de crouch/dash/roll out of box. I-am dat link la versiunea FREE Basic Locomotion ca să testeze feel-ul înainte.
- **B) Trecem peste mișcări și mergem pe conținut** — adăugat platforme/obstacole în camerele 2-10, sistem de scoring, UI, build, ca să simtă „progres vizibil" în loc de tuning.
- **C) Pivotează la jocuri mai mici 2-4 săptămâni** — discuție strategică din final de sesiune (vezi [[cristian-strategic-why]]).

**STARE FINALĂ:** scena `Shooter.unity` salvată, compilează curat, toate mișcările funcționează (sprint/jump/dash/roll/crawl/climb/respawn) cu hack-uri vizuale acceptabile pentru prototip. Vezi [[animal-io-web-pivot]] pentru context complet, [[cristian-strategic-why]] pentru de ce continuă cu game dev.
