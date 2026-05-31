---
name: feedback-platformer-direction
description: "Direcția platformer 3D single-player cu skilluri (jump/dash/țepi/GoalZone) CONFIRMATĂ de user pe 28 mai 2026 — „asta îmi place mult\""
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 59c3f181-e07f-463a-8028-c8449afddb43
---

Pe 28 mai 2026, după ce am construit prototipul platformer în Shooter.unity (WASD+jump+dash+respawn, țepi care omoară, GoalZone galben ca obiectiv per cameră, arcașul Meshy ca personaj), userul a zis explicit: **„ok asta îmi place mult"**. = validare directă a pivotului.

**Why:** userul a oscilat mult între direcții (lup 3D realist → shooter Archero-like → platformer cu skilluri). Asta e prima confirmare clară a unei direcții după pivot. Tratează asta ca anchor — NU re-propune shooter/luptă/inamici dacă nu cere explicit. Continuă să construiești pe stack-ul platformer.

**How to apply:**
- Default la mecanici platformer (jump variants, dash, glide, wall jump, time-stop, magnet, teleport) când propui idei noi.
- Skilluri = SE ADAUGĂ pe parcurs (cum a spus el la pivot „skilluri" la plural). Sistem progresiv: după ce treci camera N primești 1-din-3 skill nou (refolosim ideea ChooseUpgrade din shooter, dar pe skilluri de platformer).
- Inamici / luptă = NU. Single player vs OBSTACOLE / ENIGME, nu vs entități hostile (cel puțin până cere explicit altceva).
- Boss = NU cavaler/entitate de bătut. Camera 10 = cel mai greu nivel de platformer.
- Arcașul Meshy (Idle/Walk) rămâne personajul; Archery_Shot ignorat.
- Harta de 10 camere = niveluri progresive. Camera 1 = tutorial simplu, dificultate crește.

Vezi [[animal-io-web-pivot]] pentru detalii tehnice complete.
