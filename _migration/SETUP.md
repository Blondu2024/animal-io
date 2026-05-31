# Animal-IO — Ghid de migrare pe Cloud PC nou (backup 30 mai 2026)

Tot ce-ți trebuie ca să continui proiectul „ca uns" pe o mașină nouă, chiar dacă
Claude pornește fără memorie. Branch de backup: **`backup-30may-2026`**.

---

## Ce e în acest backup (git)
- ✅ Proiectul Unity complet: `Assets/` (Scripts, Meshy modele/texturi/materiale/prefab-uri,
  Resources, scena `Shooter.unity`, audio), `ProjectSettings/`, `Packages/`.
- ✅ Scriptul de asamblare `Assets/Editor/AnimalAssembly.cs`.
- ✅ Memoria lui Claude: `_migration/claude-memory/*.md` (19 fișiere — decizii, pipeline, lecții).
- ✅ Acest ghid.

## Ce NU e în backup (de refăcut pe mașina nouă)
- `Library/` — se regenerează automat de Unity la prima deschidere (de-aia e ignorat).
- `Assets/Forest/` — pădurea din pivotul abandonat (1.4GB, .bin > 100MB; transfer separat dacă o vrei).
- **Chei secrete** (Meshy API key) — NU se urcă în git; le re-introduci manual (vezi mai jos).

---

## Pași de setup (mașină nouă)

### 1. Instalează
- **Git** (sau PortableGit ca aici).
- **Unity Hub** + **Unity 6000.4.8f1** (EXACT aceeași versiune ca proiectul).
- **Node.js** (pt `npx` — serverul MCP Meshy).
- **Python + uv** (pt `uvx` — serverul MCP UnityMCP). Instalează uv: `https://docs.astral.sh/uv/`.
- *(Opțional)* **ffmpeg portabil** pt pipeline-ul audio Sonniss — era la
  `C:\Users\Shadow\tools\ffmpeg-8.1.1-essentials_build\bin\ffmpeg.exe`.

### 2. Clonează proiectul
```powershell
git clone https://github.com/Blondu2024/animal-io.git
cd animal-io
git checkout backup-30may-2026
```

### 3. Deschide în Unity
Deschide folderul `animal-io` în Unity Hub. La prima deschidere:
- Regenerează `Library/` (durează câteva minute).
- Auto-instalează pachetul **UnityMCP** (`com.coplaydev.unity-mcp`, deja în `Packages/manifest.json`).

### 4. Configurează serverele MCP în Claude
Cele două servere (în config-ul Claude `.claude.json` → `mcpServers`):

**UnityMCP** (control Unity din Claude):
```
command: uvx
args:    --from mcpforunityserver==9.7.1 mcp-for-unity
env:     (niciuna)
```

**Meshy** (generare modele 3D):
```
command: npx
args:    -y @meshy-ai/meshy-mcp-server
env:     MESHY_API_KEY = <CHEIA TA MESHY>   <-- re-introdu cheia din contul meshy.ai
```
> Contul Meshy (credite ~681 rămase la backup) e online, nu se pierde — doar re-bagi API key-ul.

### 5. Restaurează memoria lui Claude
Copiază `_migration/claude-memory/*.md` în folderul de memorie al noului Claude
(`<config>/projects/<workspace>/memory/`). Așa Claude știe instant tot contextul:
direcția jocului, pipeline-ul Meshy→Mixamo→Unity, lecțiile tehnice MCP, etc.
Începe cu `MEMORY.md` (indexul).

---

## Lecții tehnice CRITICE (ca să nu reînveți bătălia)
Vezi `_migration/claude-memory/animal-io-unity-mcp-pipeline.md` pe larg. Pe scurt:
- **`refresh_unity scope=all`** (nu `scope=scripts`) ca să detecteze fișiere `.cs` schimbate.
- **`[DidReloadScripts]`** pt cod de editor — `EditorApplication.update`/`delayCall` sunt înghețate
  pe Cloud PC fără focus pe fereastră. (Și Play îngheață fără focus.)
- **`execute_code` MCP e stricat** aici → folosește scripturi `Assets/Editor/*.cs`.
- **Textură Meshy:** Mixamo pierde textura → `meshy_download_model include_textures=true` dă PNG-uri PBR
  → material URP/Lit → asignează pe SkinnedMeshRenderer.
- **Clipuri Mixamo:** prinse ca sub-assets în FBX → leagă-le în editor cu
  `AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<AnimationClip>()`.

## Stare proiect la backup
Level 1 = temă primate: boss urangutan + maimuțe-cârlig (texturate + animate, cablate în joc),
5 props texturate plasate în coridor, audio complet. Vezi memoria pt detalii + pașii următori
(cârlig la mâna maimuței, calibrare scări, build Android, Level 2).
