# Setup joc — Unity + MCP + Cloud PC (checklist pas cu pas)

> Stack: Unity 6 (gratis) + Unity MCP (eu controlez) + Cloud PC Shadow (~$35/lună) + backend outside (Railway/Supabase). Lansare Android întâi ($25 o dată).
> Regulă bani: conturile gratis ACUM. Shadow (plătit) abia când ai o zi liberă de setup.

---

## FAZA A — Conturi gratis (de pe laptop, ACUM — nu costă nimic)

- [ ] **Unity** — cont gratis pe unity.com (licență Personal, gratis sub $200K venit/an)
- [ ] **GitHub** — ✅ AM DEJA (cont Blondu2024, author noreply configurat)
- [ ] **Backend** (alege unul pt început, ambele au tier free):
  - [ ] Supabase (supabase.com) — DB + auth, ce știu deja
  - [ ] sau Railway (railway.app) — pt serverul de multiplayer
- [ ] (opțional, la nevoie de assets) cont Mixamo (gratis, Adobe) / Meshy / Tripo

## FAZA B — Cloud PC (Shadow) — DOAR când ești gata de ziua de setup

- [ ] Verifică disponibilitatea Shadow în regiunea ta ÎNAINTE să plătești (poate fi coadă de provisioning)
- [ ] Abonament Shadow tier NEO (~$35/lună, RTX 4060-class, 16GB RAM, 512GB SSD)
- [ ] Pornește provisioning-ul PRIMUL (dacă e coadă, începe ceasul cât faci restul)
- [ ] Instalează app-ul Shadow pe laptop → conectează-te la Cloud PC

## FAZA C — Pe Cloud PC (Windows complet) — eu te ghidez la fiecare comandă

- [ ] **Unity Hub** + **Unity 6** (download mare, ~30-60 min) — de pe unity.com/download
- [ ] **Modulul Android Build Support** (SDK + NDK) în timpul instalării Unity
- [ ] **Node.js LTS** (nodejs.org sau `winget install OpenJS.NodeJS.LTS`)
- [ ] **Claude Code** (`npm install -g @anthropic-ai/claude-code` — confirmăm versiunea exactă live)
- [ ] **Git** pe Cloud PC + config author: `git config --global user.email "Blondu2024@users.noreply.github.com"`
- [ ] **Unity MCP** (CoplayDev) — package Unity + Python/uv + config client (facem împreună)

## FAZA D — Test + optimizare (eu fac, tu confirmi)

- [ ] Pornesc `claude` în terminalul de pe Cloud PC
- [ ] TEST MCP: creez un cub în scenă, pornesc Play Mode, citesc consola, fac screenshot
- [ ] Dacă merge → MCP funcțional, pot construi ✅
- [ ] Config proiect: .gitignore Unity, URP pt mobil, setări build Android
- [ ] Creăm directorul proiectului (ex: ~/jocul-meu) → de aici pornești Claude pe viitor

## FAZA E — Începem jocul

- [ ] Stabilim jocul exact (.io multiplayer / casual / altceva)
- [ ] Primul prompt real de joc → lucrez efectiv

---

### Note
- Setup total: ~o zi, din care jumătate e așteptat descărcări.
- Singura necunoscută: coada de provisioning Shadow.
- După setup, fiecare sesiune pornește în câteva secunde.
- Cost pornire: ~$35/lună (Shadow) + $25 Android (o dată) + backend ieftin + tokeni Claude.
