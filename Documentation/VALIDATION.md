# Simplified Stage 1 validation — 31 October 2026

The submitted prototype keeps only ranger movement, a third-person camera, a primitive forest, seed collection, planting and a health indicator, with basic instructions and feedback.

## Test environment

Unity **6000.6.5f1**, Linux, existing URP/Input System packages. Your Unity editor already held the repository's project lock; the first command-line attempt therefore aborted before compiling. That editor was left running. Subsequent scene generation, compilation and Unity tests ran in an isolated copy at `/tmp/forest-simplified-validation`.

The validation copy used the committed baseline `ProjectSettings.asset` and `QualitySettings.asset`, rather than incorporating unrelated local settings changes. Generated scene snapshots and final evidence were copied back into this repository. The temporary scene-capture utility is not part of the submitted assets.

## Executed checks

| Check | Actual result | Evidence |
| --- | --- | --- |
| Final simplified C# compilation and scene generation in Unity | Passed, editor exit 0 | Saved Stage01_Greybox scene |
| Edit Mode tests | **3 passed, 0 failed** | [NUnit XML](Evidence/editmode-results.xml) |
| Headless Play Mode tests | **4 passed, 0 failed** | [NUnit XML](Evidence/playmode-headless-results.xml) |
| Graphical Play Mode tests | **4 passed, 0 failed** | [NUnit XML](Evidence/playmode-graphical-results.xml) |
| Intermediate source snapshots, steps 1–7 | Editor/runtime C# sources compile against Unity's compiler references; scene script GUIDs resolve | [Compilation/reference report](Evidence/incremental-compilation.txt) |
| Removed-system reference scan | No litter/reuse/companion/alternate-movement references in submitted gameplay C#, scene or material content | Source and saved scene |
| Game-view screenshot inspection | Opening 40% and completed 100% HUD; no removed objects/UI; three saplings visible | [Start](Evidence/stage01-start.png), [restored](Evidence/stage01-restored.png) |

The three Edit Mode tests verify that planting without a seed does nothing; three seed/plant cycles spend exactly one seed each and cap health at 100%; and a pickup cannot award seeds twice.

The four Play Mode tests load the actual saved scene and verify:

1. W, A, S and D movement, ground contact, perimeter collision, camera follow and arrow-key camera yaw.
2. All three seeds and all three planting plots through E key events, resource counts, hidden pickup/plot markers, visible saplings, health after every action, completion and rejection of duplicate rewards.
3. Rejection of out-of-range interactions, planting without a seed and collecting through a solid obstacle.
4. Camera obstruction handling: the camera moves closer and retains an unobstructed line to its focus.

Movement tests use normal Update methods with explicit frames and a fixed simulated timestep. The full interaction test teleports between targets, while movement is tested separately. This is not a claim of a fully manually walked playthrough. Tests restore modified timing and input/editor settings. Screenshot capture uses a floating landscape Game view with synchronous shader compilation; batch runs skip capture.

## Verification limits and known limitations

- No blocking errors were observed in the tested path. Unity still emits upstream package, graphics-fallback, licensing-refresh and shutdown warnings; these did not prevent the successful runs.
- Intermediate snapshots were checked using Unity's C# compiler/reference assemblies and metadata scans. Every intermediate commit was not separately launched and played in the Unity editor.
- Mouse-drag orbit and Escape release were not manually exercised; keyboard camera yaw, following and obstruction handling were tested.
- No standalone, WebGL or mobile build, clean-machine dependency download, performance profile or complete manual route was run.
- Primitive graphics, no audio/animation, one small scene, no persistence or accessibility/settings work. See [scope and plans](ROADMAP.md).

## Preservation and file scope

The original SampleScene, package manifest/lock and other branches were preserved. Unrelated pre-existing changes were excluded from the rebuilt commits: deleted `Assets/Editor.meta`, `Assets/Editor/HubForceResolve.cs` and its `.meta`, plus modified `ProjectSettings/ProjectSettings.asset` and `ProjectSettings/QualitySettings.asset`.

Changes are limited to `Assets/ForestRecovery`, its root metadata, the prototype entry in `ProjectSettings/EditorBuildSettings.asset`, README, and documentation/evidence. Runtime code consists of five scripts: `RangerMotor`, `RangerCamera`, `RestorationState`, `ForestInteractable` and `ForestMission`. The unused reuse material was removed. The scene has three seeds and three plots, with no discarded-material objects, guards or companion.

See the [script guide](SCRIPT_GUIDE.md) for beginner explanations and the [history notes](HISTORY.md) for the reconstructed local commit sequence and backup reference. Nothing was pushed, force-pushed or merged.
