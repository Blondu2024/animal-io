---
name: animal-io-audio
description: "Subsistem AUDIO joc (30 mai 2026) — GameAudio + ProceduralAudio, sunete reale Sonniss GDC 2024 extrase cu ffmpeg portabil în Resources/Audio, toate hook-urile legate"
metadata: 
  node_type: memory
  type: project
  originSessionId: e502629b-9136-4ed0-b761-622fe82b3ce0
---

**Audio adăugat 30 mai 2026.** Userul a ales sunete reale gratis din **Sonniss GDC Game Audio Bundle** (vezi [[animal-io-animation-pipeline]] pt context milestone L1 fun).

**Arhitectură (2 scripturi noi în Assets/Scripts):**
- `GameAudio.cs` = singleton, auto-creat de `LevelSequencer.Start` (ca CurrencyManager/DeathManager). Încarcă TOATE clipurile din `Assets/Resources/Audio/` (cheie = nume fișier lowercase). API static: `GameAudio.Play(name,pos,vol,pitch)`, `Play2D`, `StartLoop/StopLoop`, `StartAmbient`. Ce nume NU are fișier real → fallback procedural (`ProceduralAudio.For(name)`), deci un .wav real pus ulterior cu numele corect ÎNLOCUIEȘTE automat sinteticul. Footsteps = ticker în Update (doar dacă există fișier real `footstep`, ca să nu enerveze). Tot 2D (spatialBlend=0, joc de telefon).
- `ProceduralAudio.cs` = sintetizează placeholdere (pluck/thud/boom/growl/whoosh/rumble/drone/fire/fanfare/blip) mapate pe keyword. Throwaway.

**Pipeline sunete reale (REUTILIZABIL — ffmpeg portabil):**
- ffmpeg NU era instalat → descărcat build static portabil (fără admin/UAC) la `C:\Users\Shadow\tools\ffmpeg-8.1.1-essentials_build\bin\ffmpeg.exe` (de pe gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip, ~104MB).
- Userul a descărcat **părțile 1–4 din 9** ale Sonniss GDC2024 în `C:\Users\Shadow\Downloads\Sonniss.com-GDC2024-GameAudioBundle{1..4}of9.zip` (~11GB, încă .zip).
- Sonniss = bibliotecă brută, fișiere LUNGI multi-take, nume pro cu prefixe (AMB/WEAP/GORE/DSGN/WOOD/UI...). NU game-ready → **trebuie tăiate**.
- Metoda (fără a dezarhiva 11GB): `System.IO.Compression.ZipFile.OpenRead` → enumerează catalogul (rapid), index scris în `C:\Users\Shadow\sonniss_index.txt` (366 .wav în părțile 1-4). Grep pe index după keyword pt candidați. Apoi extrag DOAR entry-urile alese (`ExtractToFile`) → ffmpeg tai+normalizez → `Assets/Resources/Audio/<event>.wav`. ffmpeg: one-shot `silenceremove=start_periods=1:start_threshold=-45dB,loudnorm=I=-14:TP=-1.5 -t DUR -ac 1 -ar 44100`; loop `-ss S -t DUR loudnorm=I=-18`.
- ⚠️ NU pot ASCULTA fișierele → pick-uri după nume/categorie, userul confirmă feel-ul în joc și zice ce să schimb (candidați alternativi în index).

**18 sunete reale puse (mapare event ← sursă Sonniss):** arrow_hit←Gore Splatter, weakpoint_hit←Axe Flesh Hit, armor_clink←Metal Shield Block, zombie_die←Creature Growl (DavidDumais), boss_die←GRWL ROAR ANGRY (Chupapsound), boss_slam←Modern Cinematic Impact boom, boss_windup←Whoosh Pass SF Low, wall_smash←Wood Crash Debris, bridge_collapse←Alien Tripod rock collapse rumble, crumble←Wood Snap, chest_open←Squeaky Gate hinge, coin_pickup←UI Click, superarrow_pickup←UI Select Plastic, heart_pickup←Metallic Bell, level_complete←Strings Section Riser, ambient_dungeon←Cave Design (LOOP), portal_hum←Haunted Metal eerie tone (LOOP). + footstep←Starter Assets Concrete_01. Arcul folosește `bow_release.wav` deja existent în Resources/.
**Încă PROCEDURALE (n-am găsit sursă bună în părțile 1-4):** `player_hurt` (grunt uman), `barrel_roll` (rostogolire — la vol 1.5 după feedback "prea încet"), `fire` (foc), `spike` (țepi), `boss_roar` (răcnet). De minat în părțile 3-4/5-9 sau ElevenLabs.

**Hook-uri legate (toate):** PlayerCombat(bow_shoot), Arrow(arrow_hit/weakpoint_hit), Zombie(zombie_die), PlayerHealth(player_hurt), Boss(windup/slam/armor_clink/die + boss_roar la spawn & periodic în Approach), RollingHazard(barrel_roll loop start/stop), IntroCinematic(wall_smash + barrel_roll loop), Interactable(chest_open), Coin(coin_pickup), Pickup(heart/superarrow), LevelSequencer(StartAmbient + level_complete), FireTrap(fire fwoosh la aprindere), SpikeTrap(spike la ridicare), CollapseBridge(bridge_collapse la prăbușire). Compilează curat.

**Test live 30 mai — feedback Cristian + fixuri aplicate:** (1) FireTrap/SpikeTrap/CollapseBridge NU aveau hook deloc → adăugate. (2) BUG: `bridge_collapse.wav` exista dar nu era apelat niciodată → acum apelat (sunet REAL). (3) barrel prea încet → vol 0.85→1.5. (4) Boss părea mut pt că windup/slam se declanșează doar la corp-la-corp, dar userul îl omoară cu arcul de la distanță → adăugat boss_roar la spawn + periodic.
**Test 2 (30 mai):** foc/țepi tot inaudibile — cauza = sintezele procedurale `Fire()`/`Thud()` aveau amplitudine ~0.03 + LoopFade mânca atacul. Rescrise: Fire=fwoosh cu atac rapid (fără LoopFade), Thud=corp ×4 + click transient. Confirmat 3×FireTrap + 3×SpikeTrap în scenă (componente OK, deci era doar volum sinteză). ⚠️ `execute_code` MCP nu merge pe instanța asta (CodeDom → "filename too long" la mono; Roslyn neinstalat) — folosește find_gameobjects by_component pt diagnoză.
**Camera boss = atmosferă proprie (cerut de Cristian "sunet strident"):** sinteză nouă `Tension()` (drone tritone dezacordat ~8Hz beat + sub-bas), keyword tension/strident/alarm/siren. `BossRoom.EnterFight` face StartAmbient("boss_tension"), revine la "ambient_dungeon" pe Win/OnReset-dacă-ieși (flag tensionOn evită blip pe morți coridor).
Rămas de minat sunet REAL pt: fire, spike, boss_roar, boss_tension (drone). Restul procedurale: player_hurt, barrel_roll.
