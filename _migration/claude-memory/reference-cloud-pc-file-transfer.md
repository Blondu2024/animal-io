---
name: reference-cloud-pc-file-transfer
description: Cum scoți fișiere de pe Cloud PC-ul (Shadow.tech) al lui Cristian pe PC-ul principal — upload pe host public via curl
metadata: 
  node_type: memory
  type: reference
  originSessionId: 3f267229-4027-44d8-9574-b0bfc02f8e6b
---

Cristian lucrează pe un **Cloud PC (Shadow.tech)** streamuit pe PC-ul lui principal. Filesystem-ul NU e partajat → nu poate muta fișiere (screenshot-uri, build-uri) de pe Cloud PC pe PC-ul principal direct.

**Soluție (30 mai 2026):** urcă fișierul pe un host public anonim via `curl.exe` din PowerShell, dă-i linkul → îl deschide din browserul de pe PC-ul principal/telefon și descarcă.
- ✅ **uguu.se** merge — link DIRECT la imagine, ~3h retenție: `curl.exe -s -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64)" -F "files[]=@C:\path\file.png" "https://uguu.se/upload.php"` (JSON → câmp `url`).
- ✅ **tmpfiles.org** backup, ~1h: `curl.exe -s -F "file=@path" "https://tmpfiles.org/api/v1/upload"` (URL e pagină viewer; direct = inserează `/dl/`).
- ❌ **catbox.moe** = „Invalid uploader", **0x0.st** = upload-uri dezactivate (anti-AI-spam) — nu le folosi.
- Pune User-Agent de browser (multe hosturi blochează UA-ul default de curl).
- E ok să urci public CÂND userul vrea oricum să publice conținutul (ex. poze pt LinkedIn). Pt fișiere private, NU.

Pt un link PERMANENT (nu temporar): build WebGL → itch.io (gratis, cont al userului). Vezi [[cristian-community-strategy]].
