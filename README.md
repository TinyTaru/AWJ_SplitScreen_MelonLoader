# AWJ Split Screen + P2 Inject (v0.6.5)

## Warning this mod is in beta. Expect bugs.

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
1. Download the Melon Loader installer. [Windows Download](https://github.com/LavaGang/MelonLoader/releases/download/v0.7.2/MelonLoader.Installer.exe) [macOS download](https://github.com/LavaGang/MelonLoader/releases/download/v0.7.2/MelonLoader.macOS.x64.zip). MacOS in untested and may not work.

2. Open the installer and select A Webbing Journey. Make sure you have it set to install version 7.2.
<img width="502" height="732" alt="image" src="https://github.com/user-attachments/assets/d106bea7-9edb-40f2-aa3f-f32f89a249b8" />

3. Download the latest release of the mod [here](https://github.com/TinyTaru/AWJ_SplitScreen_MelonLoader/releases/download/latest/AWJ_SplitScreen.zip).
4. Go to steam and click on A Webbing Journey in your library and then the gear on the right. Select `Browse local files`.
<img width="647" height="586" alt="image" src="https://github.com/user-attachments/assets/d99195bb-b41c-483b-8331-cb7ac59891ed" />

5. Un-zip the file you downloaded and copy the DLL file into the mods folder of the game that Melon Loader added.
<img width="1018" height="630" alt="image" src="https://github.com/user-attachments/assets/9b53c380-4f15-4cc2-a7c6-093e1759c064" />

6. Launch the game as you normally would and once you're in level press F9 to start multiplayer. Make sure you have two controllers plugged in.

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
