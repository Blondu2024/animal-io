---
name: animal-io-mobile-testing
description: "Decizie testare mobil (30 mai 2026) — target Android nativ (build din Windows), NU iOS; testul amânat ~1 zi până Cristian e în Romania cu telefon Android"
metadata: 
  node_type: memory
  type: project
  originSessionId: 50025d1a-14ac-4d30-afc5-6cbc5426668d
---

**Decizie 30 mai 2026 — testarea pe telefon.** Cristian a întrebat „pe ce mergem: next level / testare telefon / vizual de la Santy". Recomandare dată: testare telefon = cel mai mare necunoscut de de-riscat (jocul e gândit mobile dar n-a atins niciodată un device real), Santy în paralel, Level 2 pe hold.

**Constrângere descoperită:** Cristian are **iOS, nu Android**, iar mașina e Cloud PC Windows fără Mac (vezi [[animal-io-setup]]). → build nativ iOS imposibil (cere Mac+Xcode+Apple Dev $99/an); Unity Remote imposibil (nu bagi iPhone fizic în Cloud PC). Singura cale iOS-azi era WebGL→itch→Safari (compromis: perf mai mică, audio cere tap).

**Rezolvare:** Cristian pleacă în Romania (de pe ~31 mai 2026) unde **va avea acces la un telefon Android** → a zis „să mai așteptăm". Decizie bună: **APK Android nativ se face direct din Unity pe Windows** (fără Mac/WebGL), testează perf+touch real. Deci target de test = **Android**, amânat ~1 zi.

**Plan când ajunge:** build APK → instalează pe Android → jucabil pe loc. Prep posibil ÎNAINTE ca testul să fie instant: legat controale touch (proiectul are deja scaffold **StarterAssets Mobile** = joystick + butoane on-screen, dar trebuie cablat la PlatformerExtras/arc custom, nu doar la ThirdPersonController).

**Santy** = persoană care face vizual plătit pentru joc (detalii brief încă de definit); task „rulează-fără-mine" bun de pornit în paralel — leagă de [[cristian-community-strategy]] (build-in-public) și [[cristian-visual-motivation]].
