# 🎮 PROIECT MASTER — „Animal .io"

> Document unic, self-contained. Scop: când Claude pornește pe Cloud PC (sesiune nouă, context gol), citește ÎNTÂI acest fișier și are tot ce-i trebuie ca să continue, fără să refacem discuția de explorare din mai 2026.
> Companion: `setup-joc-unity.md` (pașii de instalare detaliați).

---

## 0. CUM FOLOSEȘTI ACEST DOCUMENT (handoff)
- Pe Cloud PC, creează directorul proiectului (ex: `~/animal-io`), pune acest fișier acolo, pornește `claude` din el.
- Primul prompt pe Cloud PC: „Citește animal-io-PROIECT-MASTER.md și hai să continuăm de unde am rămas."
- ⚠️ Cloud PC = mașină nouă: adu acest fișier acolo (prin GitHub repo sau copiere). Nu există automat.

## 1. CONTEXT RAPID (cine + de ce)
- **User:** Tănase Cristian, solo founder prin ELI-SAMI-TECH SRL. Self-taught dev (~2.5 ani), web/backend.
- **NU e artist 3D** — partea de artă vine din assets gata/AI gen; Claude face cod + montaj + integrare.
- Are deja live: **CreazaApp** (AI app builder) + **FinRomania** (platformă financiară). Cunoaște Supabase, Railway, Firebase, Google Auth, Stripe, Vercel.
- **Acesta e un proiect NOU și separat:** primul lui joc.
- Cost-conscious, s-a ars la Emergent/Base44 cu promisiuni mari. Vrea realism, nu hype.

## 2. VIZIUNEA JOCULUI
Un `.io` cu animale și progresie: pornești ca **șoricel**, crești mâncând, și deblochezi animale tot mai puternice (**pisică → lup → tigru → leu**). Single-player + boți la început (trofee, monede), apoi multiplayer pe echipe (4v4/3v3) cu etape de avansare. Mobil, gratis + reclame.

## 3. CONCEPT & MECANICĂ
- Bază tip **agar.io**: te miști, mănânci hrană + animale mai mici, crești.
- La praguri de mărime/nivel → **devii animal superior** (mai mare, mai rapid, eventual o abilitate).
- Mori dacă te mănâncă unul mai mare. Restart rapid. Partide scurte (2-5 min).

## 4. DIFERENȚIERE (vs clonele agar)
- **Temă animale + progresie** = hook memorabil („de la șoricel la leu").
- **Deblocare animale** prin nivel/mărime.
- **Multiplayer pe echipe** (4v4/3v3), câștigătorii avansează (faza târzie).
- **Single-player-first** ca să rezolve cold-start (lobby gol = ucigașul #1 al .io multiplayer). Asta a fost intuiția lui Cristian — e corectă, o respectăm ca ordine de build.

## 5. STIL VIZUAL
- **2.5D izometric**, orientat după **Clash of Clans** (stilizat, colorat, unghi izometric fix).
- NU 3D liber (scump pe assets+performanță). NU 2D plat (mai puțin „premium").
- 2.5D = sweet spot: arată premium, cost gestionabil, rulează pe orice telefon.

## 6. STACK TEHNIC & ARHITECTURĂ
- **Engine:** Unity 6 Personal (gratis sub $200K/an firmă), pipeline URP (mobil).
- **Claude controlează prin:** Unity MCP (CoplayDev) — creez scene, GameObjects, scripturi C#, Play Mode, citesc consola, screenshot, rulez teste.
- **Mașina de dezvoltare:** Cloud PC Shadow (~$35/lună, RTX 4060-class, 16GB, 512GB) — laptopul lui Cristian (i5-6200U, 30GB liberi) e prea slab. Claude RULEAZĂ pe Cloud PC.
- **Backend (faza 3+):** Railway (server autoritar multiplayer) + Supabase (conturi, monede, trofee, leaderboard). Cristian are Pro pe toate.
- **Monetizare:** Unity Ads/AdMob + Unity IAP.
```
📱 Client Unity (telefon) ──WebSocket(faza3+)──► 🐧 Server Railway ──► 🗄️ Supabase
```

## 7. FAZELE PROIECTULUI
- **Faza 0 — Setup** Cloud PC + Unity + MCP + Claude Code (test MCP: creez cub, screenshot).
- **Faza 1 — MVP single-player** (agar+animale+boți, 2.5D, vs boți). ← ÎNCEPEM AICI
- **Faza 2 — Progresie persistentă** (monede, trofee, shop skill-uri via Supabase) + monetizare.
- **Faza 3 — Multiplayer free-for-all** (server autoritar Railway). ← ZIDUL de dificultate
- **Faza 4 — Echipe + brackets** (4v4/3v3, matchmaking, avansare). ← cel mai greu, ultimul.

## 8. FAZA 1 DETALIATĂ (sub-sesiuni; regula: 1 sesiune = 1 problemă, cu teste)
- **1a** — un animal (șoricel) se mișcă pe hartă 2.5D izometrică, camera urmărește. PLACEHOLDER (formă simplă, fără assets). Test pe telefon devreme.
- **1b** — hrană pe hartă + mecanica de creștere (mănânci → crești).
- **1c** — boți (AI care rătăcesc, pot fi mâncați / te pot mânca).
- **1d** — progresie animale (praguri → pisică→lup→leu; schimbi modelul + stats). AICI intră primele assets reale.
- **1e** — moarte + scor + restart rapid + UI minimal.
- **1f** — polish + build Android + test pe telefon real → **MVP JUCABIL**.
- Notă: placeholder-uri până la 1d ⇒ ZERO cost assets în prima parte.

## 9. ASSETS — strategie & cost
- Faza 1: 3-4 animale (șoricel→pisică→lup→leu), stil 2.5D izometric.
- Surse: pachete din store-uri (multe pachete animale low-poly) + AI gen (Meshy/Tripo) + cleanup Blender de către Claude.
- NU 20 de animale din prima — adăugăm pe parcurs.
- Cost: placeholder-uri = $0 până la 1d; apoi mic (2D/low-poly), crește cu nr. de animale.

## 9b. PIPELINE DE ASSETS (AI + MCP) — decis
> Cheia: TOATE uneltele au MCP ⇒ Claude le operează direct. User-ul NU modelează nimic manual. Claude scrie prompturi → generează → curăță în Blender → importă în Unity.

**Combo recomandat (după reviews G2 / comparații 2026):**
1. **Meshy** (+ Meshy MCP `meshy-dev/meshy-mcp-server`) — PRINCIPAL pt personaje/animale.
   - Plugin nativ Unity (import 1 click), **auto-rigging + 500+ animații** (șoricel→leu se mișcă din prima), export game-ready FBX/GLB/OBJ cu PBR.
   - Free tier 100 credite/lună; plătit de la ~$20.
2. **Blender MCP** (`ahujasid/blender-mcp`, gratis) — HUB de cleanup + asamblare + medii.
   - Trage asset-uri gratis din Poly Haven + Sketchfab, declanșează generare (Rodin), curăță/asamblează modelele Meshy/Tripo pentru Unity.
3. **Tripo** (+ Tripo MCP, opțional) — props rapide ieftine (~10s/model, topologie quad curată). Mai slab la animație/integrare decât Meshy.

**Ce unealtă pt ce asset:**
| Asset | Unealtă | MCP |
|---|---|---|
| Personaje/animale (cu animație) | Meshy | ✅ |
| Props rapide (cutii, obiecte simple) | Tripo | ✅ |
| Copaci, stânci, mediu | Meshy/Tripo + Blender MCP (Poly Haven gratis) | ✅ |
| Mașini / hard-surface | Meshy/Tripo (⚠️ cel mai capricios pt AI — uneori pachet din store e mai rapid) | ✅ |

**Onestitate:** AI 3D 2026 e bun dar cere cleanup (Claude îl face în Blender) — nu e 100% perfect din prima; hard-surface (mașini) e partea capricioasă; calitatea depinde mult de cât de specific e promptul.
Surse: meshy.ai/compare/meshy-vs-tripo · meshy.ai/blog/best-ai-tools-for-3d-game-assets · strayspark.studio (Blender MCP pipeline) · g2.com/products/meshy/reviews

## 10. MONETIZARE (faza 2)
- Reclame: interstițial la moarte + reward video pt bonus (Unity Ads/AdMob).
- IAP: skins animale + skill-uri (Unity IAP).
- Model: gratis + reclame = standard .io, prag de intrare zero.

## 11. COSTURI
- Cloud PC: ~$35/lună (doar cât lucrăm activ).
- Assets: $0 la început (placeholder) → mic/mediu mai târziu.
- Backend: $0 (Pro pe toate).
- Android: $25 o dată (la lansare). iOS: $99/an + necesită Mac (mult mai târziu).
- Tokeni Claude: variabil (joc întreg prin MCP consumă — de ținut minte).
- Unity: $0 până firma trece de $200K/an venit (atunci Pro ~$2200/an).

## 12. REGULI DE LUCRU (din CLAUDE.md global al lui Cristian — RESPECTĂ-LE)
- **1 sesiune = 1 problemă** end-to-end, cu teste, fără shortcut-uri. Nu amesteca 2-3 probleme. Altă problemă văzută → noteaz-o, n-o atinge.
- **Git author:** înainte de primul commit, `git config user.email "Blondu2024@users.noreply.github.com"` (Vercel COMMIT_AUTHOR_REQUIRED; doar forma noreply trece).
- **Push la producție:** NICIODATĂ automat. Întreabă „Dau push?" și așteaptă confirmare. Commit local fără să întrebi = OK.
- **Verifică memory/plan cu grep în cod înainte de recomandări** — fișierele decay; ce scrie aici poate fi depășit, verifică în cod întâi.
- **Token efficiency** pe operații bulk: batch, evită reads duplicate. (Excepție: investiția în teste > economia de tokeni.)

## 13. DECIZII
- [x] Construim pe FAZE, single-player + boți întâi, simplu, apoi adăugăm treptat.
- [x] Stil: 2.5D izometric (Clash of Clans).
- [x] Backend: Railway + Supabase (Cristian are Pro).
- [x] Placeholder-uri întâi, assets reale de la 1d.
- [ ] Câte animale în MVP (recomandat 3-4).
- [ ] Confirmare buget Cloud PC (~$35/lună) la ziua de setup.

## 14. SETUP CLOUD PC (rezumat — detalii în setup-joc-unity.md)
1. Conturi gratis (Unity ✅ făcut, GitHub ✅ are, backend ✅ Pro).
2. Shadow PC (verifică disponibilitate ÎNAINTE de plată; pornire provisioning întâi).
3. Pe Cloud PC: Unity Hub + Unity 6 + modul Android → Node.js → Claude Code → Git (author noreply) → Unity MCP.
4. Test MCP (cub + Play Mode + screenshot). Config: .gitignore Unity, URP mobil.
5. Creează `~/animal-io`, pune acest doc acolo, pornește Claude de acolo.

## 15. CUM RELUĂM PE CLOUD PC (primii pași)
1. Setup complet (secțiunea 14) — Claude ghidează live.
2. Creează directorul proiectului + pune acest fișier în el.
3. Pornește `claude` din director; primul prompt: „Citește animal-io-PROIECT-MASTER.md, continuăm cu sesiunea 1a."
4. Sesiunea 1a: primul animal (placeholder) care se mișcă pe hartă 2.5D izometrică, test pe telefon.

---
*Creat: mai 2026, în faza de explorare (din directorul home, pe laptop). Nimic plătit încă. Plan complet, gata de execuție când Cristian blochează ziua de setup.*
