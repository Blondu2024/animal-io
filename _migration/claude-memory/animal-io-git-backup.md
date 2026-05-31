---
name: animal-io-git-backup
description: Backup complet în git (30 mai 2026) pe branch backup-30may-2026 — proiect + memorie + ghid migrare; Cristian face upgrade la Cloud PC nou
metadata: 
  node_type: memory
  type: project
  originSessionId: 50025d1a-14ac-4d30-afc5-6cbc5426668d
---

**Backup 30 mai 2026** — Cristian face upgrade la Cloud PC (ăsta vechi e slab). Tot e urcat pe
**GitHub `Blondu2024/animal-io`, branch `backup-30may-2026`**:
- Proiectul Unity complet (commit 2c0b518) — cod, modele Meshy, scenă, audio, prefab-uri.
- `_migration/claude-memory/` — snapshot al celor 19 fișiere de memorie (commit d827f39).
- `_migration/SETUP.md` — ghid migrare: instalare (Unity 6000.4.8f1, Node, uv, ffmpeg), clonare,
  config MCP (UnityMCP=`uvx --from mcpforunityserver==9.7.1 mcp-for-unity`; Meshy=`npx @meshy-ai/meshy-mcp-server` + MESHY_API_KEY), restaurare memorie.

**EXCLUS din backup:** `Assets/Forest/` (pivot abandonat, .bin >100MB → gitignored), `Library/` (regenerabil),
cheia Meshy API (re-introdusă manual — e în `.claude.json` mcpServers.Meshy.env pe mașina veche).

⚠️ **Snapshot-ul de memorie din repo e de la 30 mai** — dacă se mai lucrează înainte de migrare,
RE-COPIAZĂ memoria în `_migration/claude-memory/` + commit, ca să nu se piardă munca nouă.
Vezi [[animal-io-unity-mcp-pipeline]] pt lecțiile tehnice care fac tot să meargă.
