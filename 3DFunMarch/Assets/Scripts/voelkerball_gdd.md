# 🏐 Völkerball PvP – Unity Projektdokumentation

> Gemeinsames Lernprojekt | First-Person Multiplayer | Unity 3D

---

## 📋 Projektübersicht

| Feld | Inhalt |
|---|---|
| Genre | First-Person PvP / Sportspiel |
| Spielerzahl | Multiplayer (lokal/netzwerk, TBD) |
| Engine | Unity 3D |
| Controller | EasyPeasyFirstPersonController (Asset Store) |
| Ziel | Lernprojekt: FPS-Controller-Erweiterung, Physik, Interaktion |

---

## 🧱 Szenen-Setup

- **Player** – `FirstPersonController.cs` + `InputManagerOld.cs` + `ObjectPickUp.cs`
- **Boden** – 3D Cube (skaliert), Layer: `Ground`
- **Ball** – 3D Sphere, Tag: `Ball`, Layer: `Ball`
  - `SphereCollider`
  - `Rigidbody`

---

## ⚙️ Implementierte Mechaniken

### ✅ 1. Ball aufnehmen & werfen – `ObjectPickUp.cs`

**Strategie:** Raycast aus der Kamera + Distanzprüfung über `pickupRange`

| Aktion | Input | Verhalten |
|---|---|---|
| Aufnehmen | LMT drücken | Raycast prüft ob Ball in Sicht & Reichweite → `isKinematic = true`, Ball wird Child der Kamera |
| Halten | (gehalten) | Ball folgt dem GrabPoint (localPosition = Vector3.zero) |
| Werfen | LMT loslassen | `SetParent(null)`, `isKinematic = false`, `AddForce(cam.forward * throwForce, Impulse)` |

**Komponente:** `ObjectPickUp.cs` – eigenständiges MonoBehaviour, kein Eingriff in die State Machine

```
ObjectPickUp.cs
├── TryPickup()    → Raycast aus playerCamera, ballLayer-Filter, Rigidbody aufnehmen
├── CarryBall()    → localPosition = Vector3.zero (Ball sitzt am GrabPoint)
└── ThrowBall()    → SetParent(null), isKinematic = false, AddForce Impulse
```

**Inspector-Felder:**

| Feld | Empfohlener Startwert | Beschreibung |
|---|---|---|
| `pickupRange` | `3` | Max. Raycast-Distanz in Metern |
| `throwForce` | `15` | Wurfkraft (ForceMode.Impulse) |
| `ballLayer` | `Ball` (Layer) | Nur Objekte auf diesem Layer können aufgenommen werden |
| `grabPoint` | Child-Transform der Kamera | GrabPoint-GameObject in Unity positionieren |

**Unity Setup Checkliste:**
- [ ] `ObjectPickUp.cs` dem Player-Objekt hinzufügen
- [ ] Ball-Objekt: Layer = `Ball` setzen
- [ ] In `ObjectPickUp` → Inspector: `ballLayer` auf den `Ball`-Layer setzen
- [ ] Leeres GameObject als Child der PlayerCamera erstellen → `GrabPoint`
- [ ] `GrabPoint` in den `grabPoint`-Slot von `ObjectPickUp` ziehen
- [ ] `GrabPoint` im Scene-View verschieben bis Halteposition passt
- [ ] `throwForce` im Play Mode feinjustieren

---

### 🔧 Controller-Architektur (Referenz)

Der `EasyPeasyFirstPersonController` nutzt eine **State Machine**:

```
FirstPersonController.cs        ← Haupt-MonoBehaviour (partial class)
├── IInputManager               ← Interface für Input-Abstraktion  
├── InputManagerOld.cs          ← Implementierung mit Legacy Input System
├── PlayerBaseState.cs          ← Abstrakte Basisklasse aller States
└── PlayerStateFactory.cs       ← Erzeugt States (Grounded, Jump, Crouch, Slide, ...)
```

**Wichtig:** `ObjectPickUp.cs` greift **nur lesend** auf `fpsController.playerCamera` zu. Die State Machine wird nicht modifiziert – keine Konflikte.

**Input-Hinweis:** `InputManagerOld` verwendet das **Legacy Input System** (`Input.GetAxis`, `Input.GetKey`). `ObjectPickUp.cs` nutzt ebenfalls `Input.GetMouseButtonDown/Up` – konsistent mit dem bestehenden System. Kein `IInputManager`-Eingriff nötig.

---

## 🗂️ Script-Übersicht

| Script | Status | Zuständigkeit |
|---|---|---|
| `FirstPersonController.cs` | ✅ Asset (unverändert) | Bewegung, Kamera, State-Koordination |
| `InputManagerOld.cs` | ✅ Asset (unverändert) | Legacy Input (WASD, Maus, Shift, Ctrl...) |
| `ObjectPickUp.cs` | ✅ Implementiert | Ball Pickup, Carry, Throw via LMT |
| `PlayerHealth.cs` | 🔲 Geplant | Treffer zählen, Ausscheiden |
| `BallHitDetection.cs` | 🔲 Geplant | `OnCollisionEnter` – Spielertreffer vs. Umgebung unterscheiden |
| `GameManager.cs` | ✅ Implementiert | Singleton, Szenen-Reset via `OnPlayerOutOfBounds()` |
| `OutOfBoundsTrigger.cs` | ✅ Implementiert | Trigger-Zone erkennt Spieler, ruft GameManager auf |

---

## 🚀 Nächste Schritte

1. **Testszene verifizieren**
2. **Out-of-Bounds testen** – Trigger-Zone unter der Map platzieren, Spieler hineinfallen lassen
 – `ObjectPickUp.cs` im Play Mode testen, `throwForce` + `GrabPoint`-Position anpassen
3. **`BallHitDetection.cs`** – `OnCollisionEnter` auf dem Ball: Spielertreffer erkennen, Event feuern
4. **`PlayerHealth.cs`** – Treffer empfangen, Ausscheiden verwalten
5. **`GameManager.cs`** – Spielzustand, Teams, Rundenlogik
6. **Multiplayer-Ansatz** planen (Mirror / Unity Netcode for GameObjects)

---

## 💡 Offene Designfragen

- [ ] **Teams:** 2 Teams fix, oder freies FFA?
- [ ] **Respawn:** Ausscheiden pro Runde oder Leben-System?
- [ ] **Map:** Mittellinie als Grenze (klassisch) oder freie Bewegung?
- [ ] **Multiplayer:** Lokal, LAN oder Online?
- [ ] **Ball:** Nur einer, oder mehrere gleichzeitig?
- [ ] **Friendly Fire:** An oder aus?
- [ ] **Ball nach Bodenkontakt:** Sofort wieder aufnehmbar, oder kurze Cooldown-Zeit?

---

## 📝 Entscheidungslog

| Datum | Entscheidung |
|---|---|
| – | Basis-Setup: EasyPeasyFPC (Asset Store), Sphere als Ball |
| – | Input: LMT drücken = aufnehmen, LMT loslassen = werfen |
| – | Pickup-Erkennung: **Raycast** (Kamera-Richtung + `pickupRange`) |
| – | Input-System: **Legacy** (`InputManagerOld`) – kein New Input System nötig |
| – | Controller-Integration: `ObjectPickUp` als separates MB, State Machine unberührt |
| – | Out-of-Bounds: dedizierter Trigger → GameManager.ReloadScene() |
