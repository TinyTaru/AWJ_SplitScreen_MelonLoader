# AWJ Split Screen + P2 Inject (v0.2.2)

## Important Hotkeys:
F7  - Dump all task/quest states to the log (diagnostic).
F8  - Swap which controller controls which player.
F9  - Toggle multiplayer.
F10 - Flip between horizontal and vertical.

### Player 2 controller mapping:
- Move: Left Stick
- Sprint: Left Stick Click (toggle)
- Look: Right Stick
- Shoot / release: Right Trigger
- Quick build: Left Trigger
- Fixed anchor: Left Bumper
- Moving anchor: Right Bumper
- Delete / cancel: B

Advanced web inputs currently use the controller-only P2 path.

Preferences in:
`<GameFolder>\UserData\MelonPreferences.cfg`
Key settings:
- `P2_GamepadIndex` (default 1 = second pad)
- `FilterP1FromP2Gamepad` (default true)
- `P2_CameraDistance` (default 14, near P1's typical distance)
- `Debug_SpeedLog` (default false; when true, logs P1/P2 average horizontal ground speed per second plus move vector and sprint state — for verifying both players cover the same distance. Test on flat, static ground: standing on moving surfaces inflates the numbers.)

Note on sprint: P2's Left Stick Click now reliably toggles sprint ON and OFF in every game input mode (it previously latched on), and note the game itself latches P1's sprint as a toggle on gamepad — the speed log shows both sprint states if you need to check.

## Installation:
Download the latest Melon Loader installer here: https://melonloader.co/download.html. Note that Melon Loader's website currently only shows 7.0. The installer will update when opened. Once you have downloaded the Melon Loader installer open it and select A Webbing Journey. Make sure you have it set to install version 7.2. Then download the latest release of this mod, un-zip it and copy the DLL into `A Webbing Journey\Mods\`.

## Player 2 progress:
Legend
🟩 = Works on player 2.
🟨 = Partially works on player 2. Likely buggy
🟥 = Isn't implemented on player 2
- Basic webs 🟨
- Advanced webs 🟨
- Movement 🟩
- Camera 🟩
- Jump 🟩
- Emotes 🟥
- Collectables 🟩

Technical Notes:
**Fix 1: Both controllers move P1**
AWJ listens to "any gamepad" via Input System callbacks. This build filters CallbackContext events so P1 ignores input coming from P2's gamepad device.
Toggle:
- `FilterP1FromP2Gamepad` (default true)

**Fix 2: Webs still come from P1**
We keep `P2ShootHeld` updated from both MelonMod.OnUpdate and from WebController.Update/FixedUpdate prefixes, so getters return P2 origin/direction consistently during the whole shoot hold.
