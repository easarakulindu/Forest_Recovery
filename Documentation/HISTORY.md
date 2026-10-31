# Local history reconstruction

The original feature branch had three local commits:

```text
202aab7 docs: add prototype setup roadmap and Unity validation evidence
b6949a0 test: verify ranger controls and complete restoration loop
fe331d6 feat: add playable Stage 1 forest restoration greybox
f215c15 Initial check-in
```

Before rewriting, a read-only `git ls-remote origin` check showed only `main` at `f215c15` (and remote HEAD at the same commit). None of the three feature commits was advertised by the remote. No push or remote mutation was performed.

The original tip is retained exactly by **`backup/stage-one-greybox-before-simplify-20261031`**, pointing to `202aab76d41f5b79e0c656f25a21fcb94a765f73`. Inspect it with:

```bash
git log backup/stage-one-greybox-before-simplify-20261031 --oneline
git diff backup/stage-one-greybox-before-simplify-20261031 feature/stage-one-greybox -- Assets/ForestRecovery
```

## New feature sequence

The feature branch is rebuilt directly from the unchanged initial check-in, in this order:

1. `chore: establish Stage 1 Unity scene structure` — saved camera/light scene, scene-opening builder and build entry; SampleScene retained.
2. `feat: add basic greybox forest environment` — ground, ridges, trees, stumps, rocks and their materials.
3. `feat: add basic WASD ranger movement` — CharacterController, visible ranger, camera-relative movement and runtime assembly.
4. `feat: add third-person camera follow and orbit` — following camera, mouse/keyboard orbit and obstruction checks.
5. `feat: add seed collection and interaction prompts` — seed rules, pickups, shared mission state, proximity/E interaction and instructions.
6. `feat: add seed-funded tree planting` — one seed per plot, one-use planting, sapling visuals and completion.
7. `feat: display forest health from planted trees` — calculated 40–100% health, health bar and planting feedback.
8. `test: verify simplified Stage 1 gameplay` — appropriate rule and real-scene integration tests.
9. `docs: explain simplified prototype and record validation` — setup, viva guide, limitations/plans, actual reports and replacement screenshots.

These are **reconstructed incremental snapshots of assisted changes**, created during the simplification task. They do not claim that the game was originally implemented manually in this order, or attribute work to other students. The original author identity is retained for the feature work; new commits use their actual creation time rather than invented historical dates. The initial commit and backup objects keep their original author metadata unchanged.

Scene snapshots 1–7 were assembled progressively with Unity. Their runtime/editor sources were compiled against Unity's references, and their scene script references were checked. Only the final simplified scene received the complete Unity gameplay test pass. The editor builder evolves with each commit and does not refer to scripts introduced in a later step.

Unrelated working-tree changes were recorded before editing and left outside the new history. The active branch was updated only after constructing and checking the new chain; other existing branches and remote refs were not moved. No destructive checkout or stash pop was needed.

For the exact final graph and author metadata, use:

```bash
git log --graph --decorate --oneline feature/stage-one-greybox main backup/stage-one-greybox-before-simplify-20261031
git log --format=fuller main..feature/stage-one-greybox
```
