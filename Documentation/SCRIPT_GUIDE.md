# Beginner script guide and viva notes

This guide describes the simplified prototype as it exists now. The five gameplay scripts are in `Assets/ForestRecovery/Runtime`. The editor builder and test scripts are supporting tools, not extra game mechanics.

## How the pieces fit together

```text
Keyboard → RangerMotor → CharacterController → ranger position
                         ↓
                    RangerCamera follows in LateUpdate

E key → ForestMission finds nearest unused item
      → ForestInteractable.Apply(shared RestorationState)
      → counts change, pickup disappears or sapling appears
      → ForestMission draws updated instructions and health
```

`MonoBehaviour` means a script can be attached to a Unity GameObject. `Awake` runs when the object is initialized. `Update` runs each gameplay frame. `LateUpdate` runs after Update, which is useful for following something that has just moved. `OnGUI` is Unity's immediate-mode interface drawing callback; it can run multiple times per frame, so the game does not collect or plant inside it.

## 1. RangerMotor.cs — move the ranger

**Attached to:** `Ranger / WASD`. Its required `CharacterController` is a capsule-shaped collision component. There is no Rigidbody movement system to combine with it.

**Inspector references:** `cameraTransform` points to Main Camera; `body` points to the primitive ranger model. `walkSpeed` is movement speed in metres per second.

**What happens:**

1. `Awake` stores the CharacterController so it does not have to look it up every frame.
2. `Update` reads the installed Input System's current keyboard. D minus A gives horizontal input; W minus S gives forward input. Opposite keys cancel each other.
3. `Vector3.ClampMagnitude(..., 1)` prevents W+D from moving faster than W alone.
4. A rotation around the vertical Y axis turns that input into the camera's direction. Looking another way changes what W means on the ground; looking up does not make the ranger fly.
5. The visible body gradually turns toward movement using `Quaternion.Slerp`.
6. `fallSpeed` accumulates gravity. Ground contact sets a small downward speed to keep the controller in contact with the ground. Its downward speed is capped.
7. `controller.Move` moves the capsule and resolves collisions. It does not add gravity itself, which is why the script supplies downward movement.

`Time.deltaTime` is the duration of the current frame. Multiplying speed by it makes movement approximately consistent across different frame rates. `Grounded` exposes the controller's contact state for verification.

**Safe experiment:** change Walk Speed to 3 in the Inspector and enter Play Mode. The ranger should move more slowly. There is one movement speed and no alternate movement mode.

## 2. RangerCamera.cs — follow and look around

**Attached to:** `Main Camera`. `target` is the ranger Transform. `distance`, `yaw` and `pitch` control how far away the camera sits and which direction it looks.

**What happens:** `LateUpdate` reads right-mouse movement or left/right arrows, calculates a point slightly above the ranger, and positions the camera behind that point. Yaw rotates around the ranger; pitch tilts the camera. Pitch is limited so it cannot flip over.

The focus position is smoothed with `Vector3.Lerp`, so small ranger movements do not jerk the view. The first frame uses the real target position immediately instead of smoothing from the world origin.

A `Physics.SphereCast` checks the path from the focus to the desired camera position. It is like moving a small ball along that line. If it hits a solid obstacle, the camera moves closer instead of going through it. The ranger is on Unity's **Ignore Raycast** layer, so this check does not hit the ranger itself. Leaves and pickup markers have no colliders; trunks, rocks, ground and ridges do.

Holding right mouse captures the cursor. Escape releases it and prevents recapture until right mouse is released. `OnDisable` also releases the cursor when the camera is disabled or Play Mode stops.

**Safe experiment:** set Distance to 6 in the Inspector. This changes the normal camera distance; obstacle checks may bring it closer.

## 3. RestorationState.cs — the shared rules and numbers

**Attached to:** nothing. This is a plain C# class created by ForestMission. That makes the resource rules easy to test without running a whole scene.

| Member | Meaning |
| --- | --- |
| `TargetTrees` | Three trees are needed for completion |
| `StartingHealth` | Initial health is 40% |
| `Seeds` | Seeds currently held; decreases when planting |
| `SeedsCollected` | Total seeds picked up; does not decrease |
| `TreesPlanted` | Number of successful plantings |
| `Complete` | True when all target trees are planted |
| `Health` | Calculated percentage based on planted trees |

`{ get; private set; }` lets other scripts read a number while restricting changes to this class. Other scripts ask the class to collect or plant rather than editing counts freely.

`CollectSeed()` adds a held seed and increments the collected total. It returns false after three seeds have been collected. `Plant()` returns false if there is no seed or the mission is complete; otherwise it spends exactly one seed and increments planted trees. These methods return a bool so the caller knows whether an action really happened.

Health is computed, not stored independently:

```csharp
StartingHealth + (100 - StartingHealth) * TreesPlanted / TargetTrees
```

With the current constants, that is `40 + 20 * TreesPlanted`. A pickup alone leaves health at 40%. One, two and three planted trees give 60%, 80% and 100%. Keeping health derived from the count avoids forgetting to update two different values.

**Safe experiment:** explain the formula before changing it. Changing `TargetTrees` also requires enough seed objects and planting plots in the scene; changing the constant alone does not create objects.

## 4. ForestInteractable.cs — one seed or one planting plot

**Attached to:** each `Seed 1–3` and `Planting plot 1–3` root object.

`ForestAction` is an enum: a named choice between `Seed` and `Plant`. Unity serializes enum values as numbers, so do not reorder the enum without updating existing scene components.

`availableVisual` is the golden marker or amber circle. `restoredVisual` is the planted sapling and green patch; seed objects leave this empty. `Used` starts false. `Label` provides the interaction prompt.

`Apply(state)` does four things:

1. Rejects an already-used object or a missing shared state.
2. Asks the state to collect a seed or plant a tree.
3. Only after success, marks the object used and hides its available visual.
4. Shows its restored visual when one exists.

This order matters. A failed planting must leave the amber circle available. A successful interaction must not award another seed or tree when E is pressed again.

**Safe experiment:** select a planting plot before Play Mode and inspect its visual references. Do not enable the planted visual in the saved starting scene; it should start disabled.

## 5. ForestMission.cs — connect input, objects and HUD

**Attached to:** `Stage 1 / mission + HUD`.

`ranger` points to the movement component. `interactables` is the scene's array of three seed roots and three plot roots. `interactionRange` is 2.2 metres. All objects use the same `State` created in `Awake`, so there is one shared inventory and mission progress.

`Update` finds the nearby item and reacts to E being **pressed this frame**. Holding E does not repeatedly fire the interaction every frame.

`FindNearby()` loops through the array, skips missing or used objects, compares distances, and keeps the nearest candidate within the interaction range. A `Physics.Linecast` checks that a solid object is not between the ranger and the target. Pickup and plot markers have no colliders to block this line.

`TryInteract()` finds the candidate again, calls its `Apply(State)`, and updates feedback according to success. It returns false if the player is too far away or cannot plant. The shared state owns the resource rules; this method owns player-facing feedback.

`OnGUI()` displays held seeds, planted trees, calculated health, controls, the current objective and the E prompt. It scales a 1280×720 layout to the current Game view. `Style` makes readable label styles, and `Panel` draws dark backgrounds while restoring the previous GUI colour. The health bar width uses `Health / 100f`; the `f` makes it floating-point division rather than integer division.

**Safe experiment:** set Interaction Range to 3 in the Inspector and test how far away E works. Returning it to 2.2 keeps the submitted interaction distance.

## Editor helper and tests

`ForestPrototypeBuilder.cs` is inside an Editor folder and an editor-only assembly. It places primitive shapes, assigns materials, connects references, saves the scene and keeps it first in build settings. The menu opens the saved scene; it only builds when the file is missing and never silently overwrites it. Methods such as `AddEnvironment`, `AddRanger`, `AddSeeds` and `AddPlanting` make the construction order readable. None of these methods needs to run in a built player.

An `.asmdef` file groups scripts into an assembly. The runtime assembly references the existing Input System. The editor assembly references runtime code; test assemblies additionally use Unity's test framework. `.meta` files hold Unity asset identities; keep them with their assets so scene references remain valid.

Edit Mode tests check the plain resource rules and one-use pickup behavior. Play Mode tests load the actual scene and inject keyboard state. Movement tests advance individual frames with fixed simulated time; the full interaction test teleports between targets to isolate collection and planting. It is not evidence of a fully manually walked playthrough. Screenshot capture is test tooling, not gameplay code.

Public component field values are serialized into the scene. Changing a default in C# does not necessarily replace an existing Inspector value. Use the Inspector for existing components and save intentional scene changes.

## A short viva demonstration

1. Open the Stage 1 scene, press Play and show **Seeds 0 / Planted 0 / Health 40%**.
2. Show WASD, mouse orbit and camera follow. Explain CharacterController collision and deltaTime.
3. Collect a golden seed with E. Show the marker disappearing and held seeds becoming 1; health stays 40%.
4. Approach an amber circle and press E. Show a sapling, held seeds returning to 0 and health becoming 60%.
5. Repeat for the other two seeds and plots to reach 100%.
6. Stop and re-enter Play Mode to show a fresh state. Explain that progress is in memory, not saved.

Be clear that the Git history was reorganized into learning-sized snapshots of assisted work. It does not establish a historical manual implementation order or contributions by other students. Use the actual test report when discussing verification.
