# Plan joc — „Animal .io" (ideea lui Cristian, reluată mai 2026)

> Concept propriu, recuperat după ~2 ani. NU-l mai uita.
> Stack: Unity 6 (client) + Railway (server multiplayer, faza 3-4) + Supabase (progresie/conturi). Cloud PC pt dezvoltare. Android întâi.

---

## 1. Viziune
Un `.io` cu animale și progresie: pornești ca șoricel, crești și deblochezi animale tot mai puternice (pisică, lup, tigru, leu...). Single-player la început (trofee, monede, vs boți), apoi multiplayer pe echipe (4v4/3v3) cu etape de avansare.

## 2. Diferențiere (ce-l scoate din marea de clone agar)
- **Temă animale + progresie** = hook memorabil („de la șoricel la leu")
- **Deblocare animale** prin nivel
- **Multiplayer pe echipe** (4v4, 3v3), câștigătorii avansează în etapa următoare
- **Single-player-first** ca să rezolve cold-start (lobby gol = ucigașul #1 al .io multiplayer)

## 3. Decizie cheie: 2D vs 3D
- **2D (RECOMANDAT):** animale top-down stilizate, ieftin, rapid, rulează pe orice telefon. Ca agar.io original.
- 3D: mai impresionant dar mult mai mult lucru pe assets + performanță. Lăsat ca opțiune ulterioară.

## 4. Mecanica de bază
- Te miști, mănânci hrană + jucători/boți mai mici, crești.
- La praguri de mărime/nivel → deblochezi/devii animal superior (mai mare, mai rapid, abilitate).
- Mori dacă te mănâncă unul mai mare. Restart rapid.

## 5. Progresie & economie (faza 2)
- Monede (din joc) → cumperi skill-uri / skins / boost-uri
- Trofee (din partide câștigate) → progres pe termen lung
- Nivele care deblochează animale noi

## 6. Arhitectura tehnică
```
📱 Client Unity (telefon) — randare, input, predicție
        ↕ (faza 3+) WebSocket
🐧 Server autoritar Railway — stare joc, anti-cheat, matchmaking echipe
        ↕
🗄️ Supabase — conturi, monede, trofee, deblocări, leaderboard
```

## 7. FAZE de construcție (ordinea corectă — single-player întâi)
- **Faza 0 — Setup:** Cloud PC + Unity + MCP + Claude Code (test MCP)
- **Faza 1 — MVP single-player (spart în sub-sesiuni, incremental):**
  - **1a** — un animal (șoricel) se mișcă pe hartă 2.5D izometrică, camera urmărește (PLACEHOLDER, fără assets)
  - **1b** — hrană + mecanica de creștere (mănânci → crești)
  - **1c** — boți (AI care rătăcesc, pot fi mâncați / te pot mânca)
  - **1d** — progresie animale (la praguri devii pisică→lup→leu; aici intră primele assets reale)
  - **1e** — moarte + scor + restart + UI minimal
  - **1f** — polish + build Android + test pe telefon real → MVP JUCABIL
  - Notă: începem cu placeholder-uri ⇒ NU plătim assets (Meshy/Tripo) până la 1d
- **Faza 2 — Progresie persistentă:** monede, trofee, shop skill-uri (Supabase) + monetizare (reclame/IAP)
- **Faza 3 — Multiplayer free-for-all:** server autoritar Railway, mai mulți jucători live
- **Faza 4 — Echipe + etape:** 4v4/3v3, matchmaking, brackets de avansare (partea grea, când există bază de jucători)

## 8. Assets (animale)
- Faza 1: 3-4 animale low-poly/2D — din store-uri (multe pachete animale) + AI gen (Meshy/Tripo) + cleanup Blender de către Claude
- NU 20 de animale din prima — adăugăm pe parcurs
- Cost assets: mic la început (2D ieftin), crește cu numărul de animale

## 9. Monetizare
- Reclame (interstițial la moarte, reward video pt bonus) — Unity Ads/AdMob
- Skins animale + skill-uri (IAP) — Unity IAP
- Gratis + reclame = standard .io

## 10. Costuri
- Cloud PC: ~$35/lună (cât lucrăm)
- Assets: mic (2D) → mediu dacă 3D / multe animale
- Backend: $0 (Pro pe toate)
- Android: $25 o dată
- Tokeni Claude: variabil

## 11. Decizii
- [x] Construim pe FAZE, single-player + boți întâi, simplu, apoi adăugăm treptat ✅
- [x] Stil vizual: 2.5D izometric, orientat după Clash of Clans ✅
- [ ] Câte animale în MVP (recomand 3-4: șoricel→pisică→lup→leu)?
- [ ] Buget pornire (~$35/lună Cloud PC) — confirmat?

## 12. Status & următorul pas
- Plan = practic bătut în cuie. NIMIC plătit încă.
- Singurul pas care costă: ziua de setup Cloud PC (~$35/lună).
- Putem începe sesiunea 1a chiar cu placeholder-uri (zero cost assets).
