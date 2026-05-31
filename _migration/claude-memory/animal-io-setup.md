---
name: animal-io-setup
description: Starea setup-ului Faza 0 pentru proiectul de joc animal-io pe Cloud PC Shadow
metadata: 
  node_type: memory
  type: project
  originSessionId: 8c5a3612-90aa-48d2-bfdc-51cacc51a7aa
---

Proiectul `animal-io` (joc .io cu animale, Unity 6 / URP / Android, vezi planul din repo) e în Faza 0 — setup mediu de dezvoltare.

- **Mașina curentă (C:\Users\Shadow) ESTE Cloud PC-ul Shadow**, nu laptopul: AMD EPYC, GPU NVIDIA RTX 2000 Ada, 16 GB RAM, ~484 GB liberi pe C:, mediu virtualizat (QXL). Deci instalarea Unity aici e corectă.
- **Git** instalat ca **PortableGit** la `C:\Users\Shadow\PortableGit` (adăugat în PATH user). `winget` eșuează cu „Access denied" / exit 5 în sesiunea non-interactivă a lui Claude — instalările care cer UAC trebuie rulate de user cu prefixul `!`.
- Repo `animal-io` clonat la `C:\Users\Shadow\animal-io` (conține doar fișiere de plan, fără cod încă).
- Git author setat global: `Blondu2024@users.noreply.github.com` (cerință Vercel — doar forma noreply trece).
- **Unity Hub 3.18.0** instalat la `C:\Program Files\Unity Hub` (user a rulat installer-ul GUI + UAC; winget nu merge aici).
- **Unity 6000.4.8f1** (Unity 6 LTS) instalat la `C:\Program Files\Unity\Hub\Editor\6000.4.8f1` cu **Android Build Support** (SDK+NDK+OpenJDK) — adăugat de Claude prin Hub CLI `install-modules` (a mers FĂRĂ elevare). Are și WebGL/Windows standalone (inofensiv).
- Contul Windows `Shadow` E în grupul Administrators → UAC funcționează pt installerele GUI; sesiunea Claude rulează ne-elevat.
- **Node.js v24.16.0 LTS** + npm 11.13.0 instalate portabil la `C:\Users\Shadow\nodejs` (în PATH user, fără admin — același tipar ca Git).
- **uv/uvx 0.11.16** instalat la `C:\Users\Shadow\.local\bin` (gestionează Python automat).
- **Proiect Unity URP creat** direct în `C:\Users\Shadow\animal-io` (copiat template-ul `3d-cross-platform` URP din editor, peste repo). `.gitignore` Unity adăugat. Licență Unity Personal activă.
- **Unity MCP (CoplayDev v9.7.1)** instalat: package în proiect (`com.coplaydev.unity-mcp`), server Python `mcpforunityserver==9.7.1` rulat prin uvx. ÎNREGISTRARE CORECTĂ (25 mai 2026, după ce config-ul derivase greșit pe `http://127.0.0.1:8080/mcp` la scope local pe folderul animal-io — server HTTP inexistent, deci mort): re-înregistrat la **scope user**, transport **stdio**, cu `claude mcp add UnityMCP --scope user --transport stdio -- "C:\Users\Shadow\.local\bin\uvx.exe" --from "mcpforunityserver==9.7.1" mcp-for-unity`. Stdio = Claude pornește singur serverul, merge din ORICE folder. `claude mcp list` → UnityMCP ✓ Connected. Bridge Unity stdio pe port **6400** (vezi nota de mai jos despre transport — NU 38000). ⚠️ Tool-urile MCP se încarcă DOAR la pornirea Claude Code — după (re)înregistrare e nevoie de un restart Claude o singură dată.
- ⚠️ IMPORTANT: Unity rezolvă pachetele git DOAR dacă `git` e în PATH-ul procesului care lansează editorul; lansează editorul dintr-un shell cu PATH-ul actualizat (PortableGit). Claude (`claude.exe`) e la `C:\Users\Shadow\.local\bin`.
- FAZA 0 COMPLETĂ. Următor: după restart Claude Code (ca să încarce tool-urile UnityMCP), test MCP — creez cub, Play Mode, screenshot. Apoi Faza 1a: șoricel placeholder care se mișcă pe hartă 2.5D izometrică.
- ⚠️⚠️ CAUZA REALĂ a lui `instance_count: 0` — REZOLVATĂ DEFINITIV (26 mai 2026, după reverse-engineering al pachetului): **nepotrivire de transport.** Partea Claude era pe **stdio**, dar partea **Unity** defaulta pe **HTTP** (`EditorConfigurationCache.cs`: `GetBool("MCPForUnity.UseHttpTransport", true)` — default TRUE!), iar cheia nu era setată în registry. Unity HTTP + Claude stdio = nu se întâlnesc niciodată; bridge-ul stdio nu pornește, nu se scrie status file, instances=0. Memoria veche zicea „port 38000" = GREȘIT (38000 e un port intern Unity fără handshake MCP; bridge-ul real e pe **6400**). Re-deschiderea Unity / `/mcp` Reconnect NU repară asta (cauza e de partea Unity).
- ✅ FIX APLICAT (persistă): setat în registry `HKCU:\Software\Unity Technologies\Unity Editor 5.x` cheia `MCPForUnity.UseHttpTransport_h3850471145 = 0` (DWORD; stdio). `ShouldAutoStartBridge()` = `!UseHttpTransport`, iar constructorul `[InitializeOnLoad] StdioBridgeHost` pornește bridge-ul SINGUR la fiecare încărcare dacă e stdio. Deci de-acum: **deschizi Unity → bridge-ul stdio auto-pornește pe 6400 → scrie `~/.unity-mcp/unity-mcp-status-*.json` → serverul Claude îl descoperă** (TTL 5s). Nu mai e nevoie de reconnect manual decât dacă Claude Code a pornit înainte de Unity (atunci `/mcp` → UnityMCP → Reconnect, fiindcă tool-urile se încarcă la pornirea Claude).
- 🔑 Hash-ul cheilor EditorPrefs Unity (registry, sufix `_h<n>`) = **DJB2-XOR**: `h=5381; pt fiecare byte ASCII: h=((h*33)&0xFFFFFFFF) XOR byte`. Verificat pe 8 chei cunoscute. Util dacă vreau să setez alte EditorPrefs din afara Unity.
- ⚠️ ORDINE recomandată: deschide Unity ÎNTÂI, apoi pornește Claude Code din `animal-io`. Verifică legătura cu resursa `mcpforunity://instances` (trebuie `instance_count >= 1`, port 6400). Lansează Unity dintr-un shell cu PortableGit în PATH (rezolvarea pachetelor git).
