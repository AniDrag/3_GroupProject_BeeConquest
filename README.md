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
**Lead:** Nik (initial implementation), finalised together with Alexander.

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
```
---
### 2.Bee AI – State Machine & Behaviour
**Files:** `BeeStateMachine.cs`, `BeeStates.cs`, `BasicBee.cs`
**Lead:** Nik (initial implementation), finalised together with Alexander.

The bee AI is built on a classic State pattern, making it easy to add new behaviours. Each state (Idle, Move, Chase, Collect, Combat) is a separate class inheriting from BeeStates. The state machine handles transitions and delegates update calls.

**Why this works**:
    - States are self‑contained and can be tested independently.
    - Adding a new behaviour (e.g., "Flee") requires only a new class and a transition rule.
    - The BasicBee class holds shared data (stats, destination, player reference) that states can access.
    - Collaboration note: Nik built the initial state machine and basic states; Alexander introduced the Game_Manager integration for server‑side collection timing and optimised the destination‑arrival logic.

```csharp
// BeeStateMachine.cs – Core state machine
public class BeeStateMachine : MonoBehaviour
{
    public BeeStates currentState { get; private set; }
    public void ChangeState(BeeStates newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}
```
**Bee collection workflow:**
    - Bee requests a field cell from `Game_Manager.cs`.
    - Game_Manager returns a random cell within radius.
    - Bee calculates travel time + collection time, and enqueues a CollectionData with an absolute trigger time.
    - When the timer expires, `Game_Manager.cs` grants pollen and notifies the cell to reduce durability.
    - This deferred collection model ensures that bees don't instantly collect pollen, they wait for travel/animation time, making the game feel more realistic.

```csharp
// BasicBee.cs – Requesting a cell
public void GetDestinationData()
{
    if (player.currentField != null)
        Game_Manager.instance.Bee_CellRequest(this);
    else
        Game_Manager.instance.Bee_IdleMove(this);
}
```
---
### 3. Interaction System – Unified Interface
**Files: ** `IInteract.cs`, `WorldInteractor.cs`, `BeeShopCharacter.cs`, `ConvertingBay.cs`, `ItemPickup.cs`, `MoveOnInteract.cs`, `Ability.cs`.
**Lead: ** Alexander, with Nik contributing to specific interactables.

The interaction system is built around the IInteract interface, allowing any object to define how it should be interacted with. A WorldInteractor (trigger collider on the player) detects nearby IInteract objects, cycles through them, and calls Interact() when the player presses the key.

**Key design choices:**
    **- Interaction types: **d
        - OnKeyPress, 
        - WhenInRange,    
        - OnKeyHold, 
        - etc., 
    give flexibility.
    **- Text feedback: **
        - The UI shows the current interactable's name, and the player can cycle if multiple are nearby.
    **- ecoupling: **
        - The player doesn't need to know what an object does – it just calls Interact().
    **- interesting detail: **
        - The Ability class (which spawns from bees) implements IInteract and plays a smooth animation when picked up – rotating to face the camera and rising up before disappearing. This small touch adds polish.

```csharp
// IInteract.cs – The contract
public interface IInteract
{
    void Interact(GameObject interactor);
    void DeInteract(GameObject interactor);
    string GetInteractionText();
    bool CanInteract(GameObject interactor);
    InteractionType Type();
}
```
---
### 4. Game Manager as Server
**File:** `Game_Manager.cs`.
**Lead:** Alexander, with Nik.

Although the game is single‑player (for now), the `Game_Manager.cs` is designed as a server authority that could easily be extended to multiplayer. It:

Maintains dictionaries of players, fields, and pending collection events.

Processes a queue of CollectionData with absolute trigger times (based on travel + collection speed).

Aggregates damage/pollen numbers into clusters (ServerLabelAgregator) to reduce UI spam.

Exposes static events (OnFixedTick) for systems that need fixed‑time updates.

**Why this matters:** By treating the local game as a client of a virtual server, we ensure that all game state is centralised and consistent, a perfect foundation for future multiplayer.

```csharp
// Game_Manager.cs – Enqueuing collection
public void GenerateCollectionData(BasicBee bee, FieldCellData cell, int durabilityDamage)
{
    float travel = bee.GetTravelTime(cell.transform.position);
    float total = travel + bee.modedPollinCollectionSpeed;
    CollectionData data = new CollectionData()
    {
        collectAmount = durabilityDamage,
        playerID = bee.playerID,
        fieldCellID = cell.ID,
        field = bee.player.currentField,
        triggerTime = Time.time + total
    };
    collectionDatas.Add(data);
}
```
---
### 5. Buff & Stat System
**Files:** `Stats.cs`, `Buff.cs`.
**Lead:** Nik (base), Alexander (extended with buff stacking and modifiers).

The `Stats.cs` class manages base stats (vit, str, dex, etc.) and derived stats (max health, speed, etc.). It also maintains a dictionary of active Buffs. Each buff can modify a stat via flat modifiers and multipliers.

**Key innovation:** GetModifiedStat() recomputes effective stats on the fly by applying all relevant buffs. This allows bees to dynamically change speed, collection power, etc., based on abilities or field effects.

```csharp
// Stats.cs – Applying buffs
public float GetModifiedStat(StatType stat, float baseValue)
{
    float flat = 0f;
    float mult = 1f;
    foreach (var buff in buffs.Values)
    {
        if (buff.TargetStat == stat)
        {
            flat += buff.FlatModifier;
            mult *= buff.Multiplier;
        }
    }
    return (baseValue + flat) * mult;
}
```
---
### 6. Pollen Conversion Bay
**File:** `ConvertingBay.cs`
**Lead:** Alexander, with Nik integrating bee commands.

The conversion bay is an interactive object that lets players turn pollen into honey. When activated, it commands all bees to return to the hive, then periodically sums their stats to generate honey. This demonstrates how the interaction system and bee AI work together.

**Interesting detail:** Bees have a playerComand flag that overrides their normal behaviour – they ignore fields and move to the hive until the bay is deactivated.

```csharp
// ConvertingBay.cs – Conversion logic
void GeneratePollin()
{
    long convertionAmount = 0;
    foreach (var bee in convertingBees)
        if (bee.atDestination)
            convertionAmount += (bee.GetBeeDex * bee.GetBeeStr);
    Game_Manager.instance.ConvertPolinToHoney(convertionAmount * systemLevel, registeredPlayer.playerID);
}
```
---
### 7. Field Cells with Buffs
**Files:** `FieldCell.cs`, `FieldBuff.cs`.
**Lead:** Alexander, with Nik contributing some ideas in architexture.

Field cells are the core resource nodes. Each cell has durability, a pollin multiplier, and a list of active buffs. Buffs can be applied (e.g., by player abilities) and expire after a duration, notifying the `Game_Manager.cs` so it can sync with clients.

**Why this is cool:** Cells can be temporarily boosted or weakened, adding strategic depth.

```csharp
// FieldCell.cs – Buff expiration
private void FixedUpdate()
{
    for (int i = activeBuffs.Count - 1; i >= 0; i--)
    {
        if (activeBuffs[i].IsExpired(Time.time))
        {
            RemoveBuff(activeBuffs[i]);
            Game_Manager.instance?.OnBuffExpired(this, activeBuffs[i]);
            activeBuffs.RemoveAt(i);
        }
    }
}
```
---
### Collaboration Note
All code in this project is the result of close collaboration between me(Nik) and Alexander.

I implemented the initial player controller, the bee state machine and AI, and the core bee logic.

Alexander designed the interaction framework, the server‑style Game_Manager, the buff system, and the field mechanics.

Together we refined every system – from the camera controls to the collection queue – ensuring they worked seamlessly as a whole.

This partnership allowed us to build a robust, extensible codebase in just three weeks and support eachothers weak points in the process.
---
### Final Thoughts
The architecture we built prioritises modularity, scalability, and clean separation of concerns. The state machine for bees, the interface‑driven interaction system, and the server‑like game manager all contribute to a codebase that is easy to extend and debug. We're proud of what we accomplished in such a short time, and we look forward to building on this foundation.
