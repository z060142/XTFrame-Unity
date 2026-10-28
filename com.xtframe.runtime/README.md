# XTFrame Runtime for Unity (`com.xtframe.runtime`)

Runs XTFrame game UIs (HUDs, menus, screens) inside Unity. A published XTFrame bundle in
`StreamingAssets/` is rendered in software into a `Texture2D` that a uGUI `RawImage` shows;
pointer, keyboard and text input go back to the UI, and the UI's ports and connection points are
reachable from C# and from the Inspector.

The package contains no C++. It talks to the XTFrame runtime only through the XTFrame C ABI
(`xt/capi/xtframe.h`, ABI version 1), loaded from a native library you add yourself.

Requirements: Unity 2021.3 LTS or newer; Windows, macOS or Linux (Editor and standalone player).
Android, iOS and WebGL are not supported yet.

## Installation

Either

- add the package by path to `Packages/manifest.json`:

  ```json
  {
    "dependencies": {
      "com.xtframe.runtime": "file:../../XTFrame/Code/Hosts/Unity/com.xtframe.runtime"
    }
  }
  ```

  (the path is relative to the `Packages/` folder), or use *Window > Package Manager > + > Add
  package from disk...* and pick `package.json`; or
- copy the `com.xtframe.runtime` folder into your project's `Packages/` folder.

The package depends on `com.unity.ugui`. The Input System package (`com.unity.inputsystem`) is
optional: when it is installed and enabled in *Player Settings > Active Input Handling*, keyboard and
text input use it; otherwise the legacy Input Manager is used. With neither enabled, keyboard and text
input are off and a warning is logged once.

## Native library

Build XTFrame (the `xt_capi` target, on by default with `XT_BUILD_CAPI`) and copy the library from
`bin/<platform>_<arch>/Release/` into the package:

| Platform | File | Destination |
|---|---|---|
| Windows x86_64 | `xtframe.dll` | `Runtime/Plugins/x86_64/` |
| Linux x86_64 | `libxtframe.so` | `Runtime/Plugins/x86_64/` |
| macOS | `libxtframe.dylib` | `Runtime/Plugins/macOS/` |

After Unity imports the file, select it and, in the Inspector's plugin settings, tick the matching
platform (Standalone with the right OS and CPU) and **Editor**, then press *Apply*. The `.meta` files
are created by Unity; the package does not ship them.

Unity loads a native library once per Editor session. To replace it with a new build, close the
Editor first.

When the library is missing, `XtSurfaceComponent` logs
`XTFrame native library not found (put xtframe into Runtime/Plugins, see the package README)` and
disables itself; the Inspector shows the same hint.

When the library's ABI version does not match the package, the first bundle open throws
`XtException` with the message

```
XTFrame: Incompatible: XTFrame native library ABI version <n> does not match the C# package ABI version 1 (library <version>). Copy a matching xtframe library into Runtime/Plugins.
```

Use a library built from the same XTFrame version as the package.

## Publishing a bundle

Publish the UI project as a directory bundle into `Assets/StreamingAssets/<name>/`, either with the
command-line packer

```
xtframe-pack <UI project directory> -o Assets/StreamingAssets/<name>
```

or with *Publish* in the XTFrame editor. The bundle must be a real directory on disk, which
`StreamingAssets` is on desktop platforms.

## QuickStart

Import the **QuickStart** sample (*Package Manager > XTFrame Runtime > Samples*), add `XtQuickStart`
to an empty GameObject, set **Bundle Path** (relative to `StreamingAssets`) and **Ui Id**, and press
Play. See the sample's README.

To set it up by hand:

1. Create a Canvas (any render mode) with an `EventSystem` in the scene.
2. Add a `RawImage` under it and add **XTFrame > XT Surface** (`XtSurfaceComponent`).
3. Set **Bundle Path** and pick the **Ui Id** from the Inspector's drop-down.

`XtSurfaceComponent` fields:

| Field | Meaning |
|---|---|
| Bundle Path | Bundle directory relative to `StreamingAssets` (absolute paths also work). |
| Ui Id | UI to show. |
| Fit | `Auto` uses the manifest; `Contain` / `Cover` add an `AspectRatioFitter` with the design aspect; `Native` / `Stretch` leave the rect alone. The surface always renders at the rect's pixel size and the UI's uiscale rule scales the content. |
| Run Flow | Load and run the UI's flow and the bundle flow. |
| System Fonts | Let the text shaper add the system fonts. |
| Instance Id | Instance id given to the UI. |
| Hot Reload Seconds | Editor only: poll the bundle for changes while playing (0 = off). Re-publish the bundle and the UI reloads. |
| Keyboard Focus | Send keys even when no text field of the UI has focus. |
| Use Unscaled Time | Advance with `Time.unscaledDeltaTime`. |

From code, `XtSurfaceComponent.Surface` is the live `XtSurface` and `SurfaceCreated` fires when it is
created. `XtBundle` and `XtSurface` can also be used directly, without the component:

```csharp
XtBundle bundle = XtBundle.Acquire(XtSurfaceComponent.ResolveBundlePath("game-ui"));
var surface = new XtSurface(bundle, "hud", 1280, 720, XtSurfaceOptions.Default);
bundle.Release(); // the surface keeps its own reference
// every frame:
surface.Update(Time.unscaledDeltaTime);
surface.Render(); // uploads surface.Texture only when pixels changed
// when done:
surface.Dispose();
```

## Ports

Two kinds of names reach a UI:

- **Declared ports** (`port:` in the UI): `uint id = surface.LookupPort("hp")`, then `SetPort(id, 42.0)`,
  `SetPort(id, true)`, `SetPort(id, "text or JSON")`, `Pulse(id)`, `GetPortNumber/Bool/Text(id)`.
  Out ports raise `XtSurface.PortEvent` with `Kind == Emit`.
- **Connection points** (the manifest's `uis[].ports`, names like `#fire.Click` or
  `document.clock`): `SetConnection(name, value)`, `PulseConnection(name)`, and
  `WatchConnection(name)` for outputs, which raise `PortEvent` with `Kind == Connection` and the
  returned listener id.

`XtSurface` also raises `HostEvent` (flow sound / haptic / spawn / emit events), `TimelineEvent` and
`VisibilityChanged`. All events are raised on the main thread from inside `Update` (or the port call
that caused them); their strings are already copied. Disposing a surface from inside one of its own
events is allowed and takes effect when that native call returns.

**XtPortBridge** (*XTFrame > XT Port Bridge*, next to `XtSurfaceComponent`) wires names to
`UnityEvent`s in the Inspector: each binding has a name, a kind (Number / Bool / String / Pulse) and
the matching `UnityEvent<double>` / `<bool>` / `<string>` / `UnityEvent`. Names starting with `#` or
`document.` are connection points; any other name is a declared port. Its `SetNumber`, `SetBool`,
`SetString` and `Pulse` methods follow the same rule and can be called from scripts, Animator events or
Timeline signals, e.g. `bridge.SetNumber("hp", 42)`.

## Known limitations (stage 1)

- Software rendering only: the whole texture is uploaded with `Texture2D.Apply` on frames whose pixels
  changed; unchanged frames are skipped. GPU rendering through a native rendering plugin is planned.
- Device pixel ratio is 1: the surface renders at the RawImage's size in canvas pixels.
- Windows, macOS and Linux only; Android, iOS and WebGL (where `StreamingAssets` is not a directory)
  are not supported yet.
- One UI document per surface; no stacking several documents on one texture, no drag and drop across
  surfaces.
- No `engine://` external textures, video or scene views.
- No localization or glyph providers, `setToken`, call ports or list insert/remove (set a whole list
  with JSON text instead).
- No IME composition UI; committed text is delivered.
- The cursor shape the UI asks for (`XtSurface.Cursor`) is exposed but not applied to the OS cursor.
- All calls must be made on the main thread.

## License

MIT, see `LICENSE.md`.
