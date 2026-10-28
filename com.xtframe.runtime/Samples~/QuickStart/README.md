# XTFrame QuickStart

1. Publish a bundle into `Assets/StreamingAssets/` (for example
   `xtframe-pack Examples/demos/game-ui -o Assets/StreamingAssets/game-ui`, or Publish from the
   XTFrame editor), and make sure the native library is in the package's `Runtime/Plugins/` folder.
2. Create an empty GameObject, add `XtQuickStart`, and set **Bundle Path** (relative to
   `StreamingAssets`, e.g. `game-ui`) and **Ui Id** (e.g. `hud`).
3. Press Play. The script builds a Screen Space Overlay Canvas with a full-screen `RawImage` and an
   `XtSurfaceComponent` (plus an `EventSystem` if the scene has none) and the UI appears.
