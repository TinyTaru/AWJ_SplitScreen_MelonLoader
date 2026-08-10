# Fix P2 wind trail in A Webbing Journey split-screen mod

Please diagnose and implement a reliable fix for the Player 2 wind-trail visual in this MelonLoader mod.

## Environment

- Game: **A Webbing Journey**
- Unity: **6000.3.10f1**
- Loader: **MelonLoader 0.7.2**, Mono
- Mod project: .NET Framework 4.7.2
- P1 is the original game player. `PlayerSpider_P2` is the split-screen mod's cloned player.

## Goal

At high speed, P2 must show the same thin, white wind wisps as P1. The effect must follow P2's spider/body, not either camera and not P1. Both players must be able to have the effect simultaneously.

## What is known

1. The game script `_Scripts.Effects.WindParticleSystem` is decompiled in this package. It gets a `ParticleSystem` on its own GameObject and only toggles its `EmissionModule.enabled` based on P1 Rigidbody speed; it also drives global wind audio.
2. Disabling P1's `WindParticleSystem` makes P1's visible wind trail disappear. This is the correct game object to mirror.
3. Unity Explorer shows P1's and P2's `ParticleSystemRenderer.renderMode` as `None`.
4. P1's renderer reports `trailMaterial = "Wind_Line"`; its normal renderer material reports Unity's `Hidden/InternalErrorShader`. This strongly suggests the visual is produced by the Particle System **Trails module**, not its regular particle mesh renderer.
5. A previous attempt forced `renderMode = Mesh` and manually emitted particles. That produced magenta/purple cube squares, so this is definitively incorrect and has been reverted.
6. The current code keeps one cloned P2 `WindParticleSystem`, reparents it to P2's Rigidbody transform, disables the original P1-bound `WindParticleSystem` behaviour, and uses `P2WindTrail` to toggle its copied ParticleSystem's emission based on P2 speed. The current approach still does not make P2's wisps visible.
7. An earlier lifecycle bug was fixed: after reparenting, the code had rediscovered by camera hierarchy and cloned a system every frame. It now retains a single `p2WindSystem` reference.

## Included files

- `UpdateFixMod.cs`: current implementation. Focus on `EnsureP2WindTrail` and nested `P2WindTrail`.
- `WindParticleSystem.decompiled.cs`: exact decompiled game controller.
- `Latest.log`: current game runtime log.
- `screenshots/`: Unity Explorer proof of P2's object location and P1 renderer properties, plus the invalid purple-cube result.

## Requested work

1. Explain why merely cloning/reparenting the `WindParticleSystem` and toggling emission does not render P2's trail.
2. Determine all Particle System modules/properties that P1 needs for the visible trail—especially the Trails module, simulation space, renderer/trail material, and any camera/render-layer dependencies.
3. Provide a concrete C# patch for `UpdateFixMod.cs` that mirrors P1's actual visible behavior for P2 without creating a fake particle effect.
4. Preserve P1 behavior and global wind audio. Do not force `ParticleSystemRenderer.renderMode = Mesh` and do not use a manually-created substitute material/mesh.
5. Recommend any one-time Unity Explorer inspection that would decisively validate the patch, if needed.

Favor inspecting the real runtime P1 system and cloning/copying all relevant module values over assumptions.
