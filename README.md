# AWJ Split Screen + P2 Inject (v0.2.2)

## Important Hotkeys:
F9  - Toggle multiplayer.
F10 - Flip between horizontal and vertical.

### Player 2 controller mapping:
- Move: Left Stick
- Look: Right Stick
- Shoot: Left Trigger
- Attach/Grapple: Right Trigger
- Delete: B
- Release: RB

Keyboard fallback:
- Move IJKL, Look N/M, Shoot U, Attach P, Delete O, Release RightCtrl.

Preferences in:
`<GameFolder>\UserData\MelonPreferences.cfg`
Key settings:
- `P2_GamepadIndex` (default 1 = second pad)
- `FilterP1FromP2Gamepad` (default true)

## Installation:
Download the latest Melon Loader installer here: https://melonloader.co/download.html. Note that Melon Loader's website currently only shows 7.0. The installer will update when opened. Once you have downloaded the Melon Loader imstaller open it and select A Webbing Jouney. Make sure you have it set to install vertion 7.2. Then download the latest release of this mod, un-zip it and copy the DLL into `A Webbing Jouney\Mods\`.

## Player 2 progress:
Legend
🟩 = Works on player 2.
🟨 = Partially works on player 2. Likely buggy
🟥 = Isn't implemented on player 2
- Basic webs 🟨
- Advanced webs 🟥
- Movement 🟩
- Camera 🟩
- Jump 🟩
- Collectables 🟩

Technical Notes:
**Fix 1: Both controllers move P1**
AWJ listens to "any gamepad" via Input System callbacks. This build filters CallbackContext events so P1 ignores input coming from P2's gamepad device.
Toggle:
- `FilterP1FromP2Gamepad` (default true)

**Fix 2: Webs still come from P1**
We keep `P2ShootHeld` updated from both MelonMod.OnUpdate and from WebController.Update/FixedUpdate prefixes, so getters return P2 origin/direction consistently during the whole shoot hold.
