# Forest Recovery — Stage 1

A small university Unity prototype about restoring a damaged forest. This version deliberately keeps one short loop: **move → collect a seed → plant a tree → improve forest health**.

## Open and play

1. Open this repository in **Unity 6000.6.5f1** (see `ProjectSettings/ProjectVersion.txt`) and wait for compilation.
2. Choose **Forest Recovery → Open Stage 1 Prototype**, or open `Assets/ForestRecovery/Scenes/Stage01_Greybox.unity`. If an older version is already open in Unity, reopen this saved scene to see the simplified objects.
3. Press **Play** and click the Game view. A landscape Game view around 1280×720 is recommended.
4. Approach a golden seed and press **E**. Approach an amber planting circle and press **E** to spend one seed and grow a tree. Repeat for all three plots.

| Control | Action |
| --- | --- |
| WASD | Move relative to the camera |
| Hold right mouse and move mouse | Orbit the third-person camera |
| Left / right arrow | Turn the camera with the keyboard |
| E | Collect a nearby seed or plant on a nearby plot |
| Esc | Release mouse-look; release right mouse before starting it again |

Stop and re-enter Play Mode to reset the prototype. No persistent save system is included.

## What works

- A ranger with ground/obstacle collision and camera-relative WASD movement.
- A following third-person camera with orbit controls and obstacle checks.
- A small editable forest made of primitive ground, trees, stumps, rocks and boundary ridges.
- Three one-use seed pickups and three one-use planting plots. Each planting costs **one seed**.
- A HUD with held seeds, planted trees, the current instruction, interaction feedback and forest health.
- Health starts at **40%**. Collecting a seed does not change health. Each planted tree adds **20 percentage points**: **40 → 60 → 80 → 100%**.

Litter collection, reuse, companion guidance and slower movement mechanics are absent from this version. Later-stage concepts remain [plans](Documentation/ROADMAP.md).

## Learn and demonstrate

Read the [beginner script and viva guide](Documentation/SCRIPT_GUIDE.md). It explains all five runtime scripts, where they are attached, their main methods, safe Inspector changes and a short demonstration route.

The new content is under `Assets/ForestRecovery`. The original `Assets/Scenes/SampleScene.unity` remains intact. The prototype is first in build settings. Existing URP 17.6.0 and Input System 1.20.0 packages are reused; no dependencies were added.

The editor builder creates an editable scene only when its saved file is missing. It refuses to overwrite an existing scene. Normal gameplay uses serialized objects, not a runtime-generated level.

## Tests and evidence

See [validation results and screenshots](Documentation/VALIDATION.md) and [history reconstruction notes](Documentation/HISTORY.md).

Use **Window → General → Test Runner** to run `ForestRecovery.Tests` in EditMode and PlayMode. With Unity closed, set `UNITY_EDITOR` to your installed Unity executable and run the following one at a time:

```bash
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testFilter ForestRecovery.Tests -testResults /tmp/forest-editmode-results.xml -logFile /tmp/forest-editmode.log
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform PlayMode -testFilter ForestRecovery.Tests -testResults /tmp/forest-playmode-results.xml -logFile /tmp/forest-playmode.log
```

For actual Game-view screenshots, omit `-batchmode` and `-nographics` on a machine with a display, or use the Test Runner UI. The full-loop test captures opening and completed images in `Documentation/Evidence`. Batch tests skip image capture. Test-only timing and input/editor settings are restored afterward.
