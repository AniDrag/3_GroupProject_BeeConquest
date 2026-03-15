# Beyond the Hive – Code Architecture & Design

**Project:** Group Project 03  
**Team:** Nik (Engineering) & Alexander (Engineering)  
**Art Team:** 4 Artists (with secondary design roles)  
**Duration:** 3 weeks (8–28 September 2025)

---

## Introduction

Our game concept reimagines a popular but stagnant title by bringing it to a new platform with expanded multiplayer and live‑service features. The codebase reflects a strong emphasis on scalability, modularity, and clean separation of concerns – all while working within a tight three‑week deadline.

This document focuses on the engineering decisions behind the player controller, bee AI, and interaction systems. All code is a **collaboration between Nik and Alexander**, with Nik leading the initial implementation of the player controller, bee state machine, and basic AI. Alexander then joined to refine the architecture, design the interaction framework, and optimise the server‑like game manager. Together we iterated on every major system.

---

## Key Systems & Design Decisions

### 1. Player Controller – Movement & Camera

**Files:** `PlayerMovemant.cs`, `PlayerCamera.cs`, `PlayerIngameMenu.cs`  
**Lead:** Nik (initial implementation), finalised together.

The player controller uses a `CharacterController` with custom ground checks and slope handling. Notable features:

- **State‑based movement:** Walking, sprinting, crouching, jumping, falling – each with different acceleration and speed.
- **Slope movement:** `GetSlopeMoveDir()` projects movement onto the slope normal, preventing sliding and allowing natural traversal.
- **Air control:** Reduced acceleration in air for responsive but not arcade‑like control.
- **Camera rotation:** The camera orbits the player, and the player mesh rotates independently (using `orientation` and `playerMesh`) to face movement direction or lock to camera.

**Interesting detail:** The camera uses a **yaw‑only** rotation for the player’s movement direction, while the camera itself has full freedom. This decoupling allows for smooth third‑person controls.

```csharp
// PlayerMovemant.cs – Slope movement
Vector3 GetSlopeMoveDir(Vector3 dir)
{
    return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
}
