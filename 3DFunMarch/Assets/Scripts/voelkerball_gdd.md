# 🏐 Völkerball PvP – Unity Projektdokumentation

> Gemeinsames Lernprojekt | First-Person Multiplayer | Unity 3D

---

## 📋 Projektübersicht

| Feld | Inhalt |
|---|---|
| Genre | First-Person PvP / Sportspiel |
| Spielerzahl | Multiplayer (Host + Clients) |
| Engine | Unity 3D |
| Controller | EasyPeasyFirstPersonController (Asset Store) |
| Netzwerk | Netcode for GameObjects (NGO) + Unity Transport |

---

## 🧱 Szenen-Setup

- **Player Prefab** – `FirstPersonController` + `InputManagerOld` + `ObjectPickUp` + `PlayerKnockback` + `NetworkPlayerSetup` + `NetworkObject` + `NetworkTransform`
- **Ball** – 3D Sphere, Tag: `Ball`, Layer: `Ball` – `SphereCollider` + `Rigidbody` + `BallThrower` + `NetworkBallSync` + `NetworkObject` + `NetworkTransform`
- **Boden** – 3D Cube, Layer: `Ground`
- **Out-of-Bounds-Zone** – Trigger-Collider unterhalb der Map + `OutOfBoundsTrigger`
- **NetworkManager** – `NetworkManager`-Komponente + `UnityTransport` + `NetworkLobbyUI`

---

## ⚙️ Implementierte Mechaniken

### ✅ 1. Ball aufnehmen & werfen – `ObjectPickUp.cs`

| Aktion | Input | Verhalten |
|---|---|---|
| Aufnehmen | LMT drücken | Raycast → `isKinematic = true`, SetParent(grabPoint), `NetworkBallSync.RequestPickup()` |
| Werfen | LMT loslassen | SetParent(null), AddForce, `NetworkBallSync.RequestThrow(force)` |

---

### ✅ 2. Ball-Highlight – `ObjectPickUp.cs`

Raycast trifft Ball → alle Material-Slots werden mit `highlightMaterial` ersetzt. Verlässt der Raycast den Ball → Original-Materials werden wiederhergestellt.

---

### ✅ 3. Knockback – `PlayerKnockback.cs`

`OnControllerColliderHit` erkennt Ballkollision. Wirft den Spieler per `CharacterController.Move()` weg. Werfer-Instanz wird via `BallThrower` ausgeschlossen. Spieler kann mit `playerControlInfluence` minimal gegensteuern.

---

### ✅ 4. Out-of-Bounds Reset – `OutOfBoundsTrigger.cs` + `GameManager.cs`

Spieler betritt Trigger-Zone → `GameManager.OnPlayerOutOfBounds()` → `SceneManager.LoadScene()`.

---

### ✅ 5. Multiplayer – Netcode for GameObjects

#### Architektur

```
NetworkManager (Singleton)
├── UnityTransport          → Verbindungsschicht (UDP, Port 7777)
├── Player Prefab           → wird pro Client automatisch gespawnt
│   ├── NetworkObject       → NGO-Identität
│   ├── NetworkTransform    → Position/Rotation sync (Server Authoritative)
│   └── NetworkPlayerSetup  → deaktiviert Kamera/Input auf Non-Owner-Instanzen
└── Ball
    ├── NetworkObject
    ├── NetworkTransform
    └── NetworkBallSync     → Pickup/Throw-State via ServerRpc + NetworkVariable
```

#### Ownership-Modell

| Objekt | Owner | Schreibrecht |
|---|---|---|
| Player Prefab | jeweiliger Client | Nur Owner bewegt sich |
| Ball | Server | `holderNetworkObjectId` NetworkVariable, Server Authoritative |

#### NetworkPlayerSetup – Inspector Setup

Im Player-Prefab `NetworkPlayerSetup` hinzufügen und folgende Felder zuweisen:

| Feld | Zuweisung |
|---|---|
| `playerCamera` | Camera-Komponente der Spielerkamera |
| `audioListener` | AudioListener der Kamera |
| `localOnlyComponents` | `FirstPersonController`, `ObjectPickUp`, `PlayerKnockback`, `InputManagerOld` |

#### NetworkBallSync – Ablauf

```
Client drückt LMT
  → ObjectPickUp.TryPickup()
    → NetworkBallSync.RequestPickup()          [lokaler Aufruf]
      → PickupServerRpc()                      [→ Server]
        → holderNetworkObjectId.Value = id     [NetworkVariable]
          → OnHolderChanged() auf allen Clients [automatisch]
            → rb.isKinematic = true
            → Owner: SetParent(grabPoint)

Client lässt LMT los
  → ObjectPickUp.ThrowBall()
    → NetworkBallSync.RequestThrow(force)      [lokaler Aufruf]
      → ThrowServerRpc(force)                  [→ Server]
        → holderNetworkObjectId.Value = 0
        → rb.AddForce(force)                   [Server-seitig]
          → NetworkTransform synchronisiert Position
```

#### NetworkLobbyUI

Einfaches OnGUI-Menü auf dem NetworkManager-Objekt. Zeigt Host/Client-Buttons bis eine Verbindung besteht. IP-Adresse im Inspector oder per Textfeld konfigurierbar.

---

## 🗂️ Script-Übersicht

| Script | Status | Zuständigkeit |
|---|---|---|
| `FirstPersonController.cs` | ✅ Asset (unverändert) | Bewegung, Kamera, State-Koordination |
| `InputManagerOld.cs` | ✅ Asset (unverändert) | Legacy Input |
| `ObjectPickUp.cs` | ✅ Implementiert | Pickup, Carry, Throw – ruft NetworkBallSync auf |
| `BallThrower.cs` | ✅ Implementiert | Werfer-Referenz für Knockback-Ausschluss |
| `PlayerKnockback.cs` | ✅ Implementiert | Ballkollision → CharacterController Knockback |
| `OutOfBoundsTrigger.cs` | ✅ Implementiert | Trigger → GameManager |
| `GameManager.cs` | ✅ Implementiert | Singleton, Szenen-Reset |
| `NetworkPlayerSetup.cs` | ✅ Implementiert | Kamera/Input auf Non-Owner deaktivieren |
| `NetworkBallSync.cs` | ✅ Implementiert | Ball-State synchronisieren (ServerRpc + NetworkVariable) |
| `NetworkLobbyUI.cs` | ✅ Implementiert | Host/Client UI |
| `PlayerHealth.cs` | 🔲 Geplant | Treffer zählen, Ausscheiden |

---

## 🚀 Nächste Schritte

1. **Multiplayer testen** – Multiplayer Play Mode (MPPM) aktivieren, Host + Virtual Player
2. **NetworkPlayerSetup** im Inspector verdrahten (Kamera, AudioListener, Script-Array)
3. **Ball als NetworkObject** in NetworkManager Prefab-Liste eintragen (oder in Szene spawnen)
4. **PlayerHealth.cs** – Treffer-Logik netzwerkfähig implementieren
5. **Rundensystem** im GameManager ergänzen

---

## 💡 Offene Designfragen

- [ ] Teams: 2 Teams fix oder FFA?
- [ ] Respawn: Ausscheiden pro Runde oder Leben-System?
- [ ] Map: Mittellinie als Grenze?
- [ ] Ball: Nur einer oder mehrere?
- [ ] Friendly Fire: An oder aus?

---

## 📝 Entscheidungslog

| Datum | Entscheidung |
|---|---|
| – | Basis-Setup: EasyPeasyFPC, Sphere als Ball |
| – | Input: LMT drücken = aufnehmen, LMT loslassen = werfen |
| – | Pickup-Erkennung: Raycast (Kamera-Richtung + pickupRange) |
| – | Input-System: Legacy (InputManagerOld) |
| – | Controller-Integration: ObjectPickUp als separates MB, State Machine unberührt |
| – | Out-of-Bounds: Trigger → GameManager.ReloadScene() |
| – | Multiplayer: NGO, Host + Clients, Server Authoritative Ball |
| – | Werfer-Ausschluss: BallThrower-Komponente auf dem Ball |
