# AWJ Split Screen + P2 Inject (v0.2.2)

**Fix 1: Both controllers move P1**
AWJ listens to "any gamepad" via Input System callbacks. This build filters CallbackContext events so P1 ignores input coming from P2's gamepad device.
Toggle:
- `FilterP1FromP2Gamepad` (default true)

**Fix 2: Webs still come from P1**
We keep `P2ShootHeld` updated from both MelonMod.OnUpdate and from WebController.Update/FixedUpdate prefixes, so getters return P2 origin/direction consistently during the whole shoot hold.

P2 controller mapping:
- Move: Left Stick
- Look: Right Stick
- Shoot: Right Trigger
- Attach: A
- Delete: B
- Release: RB

Keyboard fallback:
- Move IJKL, Look N/M, Shoot U, Attach P, Delete O, Release RightCtrl.

Prefs in:
`<GameFolder>\UserData\MelonPreferences.cfg`
Key settings:
- `P2_GamepadIndex` (default 1 = second pad)
- `FilterP1FromP2Gamepad` (default true)

Install:
Build Release; copy `bin\Release\net472\AWJ_SplitScreen.dll` into `<GameFolder>\Mods\` (overwrite old).
