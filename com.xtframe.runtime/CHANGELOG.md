# Changelog

All notable changes to `com.xtframe.runtime` are documented here.

## [0.7.1] - 2026-11-26

### Changed
- The native library is now built from XTFrame-Backends `v0.21.0` (`backends/runtime/` =
  XTFrame-Runtime `v0.21.0`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild `xt_capi`.
- No C ABI change (`XT_CAPI_ABI_VERSION` 1, `xtframe.h` unchanged since 0.20.0) and no format change
  (IR 3.5): `XtNative` is unchanged and existing bundles keep working.

## [0.7.0] - 2026-11-25

### Added
- `XtNative` declarations for the input channels of XTFrame 0.20.0 (C ABI appended, ABI version
  still 1): `xt_input_channel(string nameUtf8) -> uint` (the process-wide id of a channel name, 0 for
  a null or malformed name), `xt_surface_push_channel(IntPtr s, uint channel, float value,
  float strength, uint flags, uint player) -> XtStatus` and `xt_surface_channel_value(IntPtr s,
  uint channel, uint player) -> float`, plus the constants `XT_CHANNEL_RELATIVE` (1) and
  `XT_CHANNEL_BUTTON` (2). Declarations only: the existing input forwarding is unchanged and nothing
  feeds channels from Unity input yet.

### Changed
- The native library is now built from XTFrame-Backends `v0.20.0` (`backends/runtime/` =
  XTFrame-Runtime `v0.20.0`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild as described for 0.4.0.
- Bundles have to be rebuilt with the 0.20.0 tools: the IR format goes from 3.3 to 3.5.
- Behaviour changes that a host feels:
  - Clock: the runtime clock, document clocks, animation start clocks and paused positions are kept
    in double (a 2 s animation's phase error at 3600.5 s drops from 0.795 s to under 1 us). The C ABI
    signatures are unchanged. `xtTime` is the clock modulo 3600 s, so it wraps to 0 once an hour. A
    document loaded late starts from the current clock instead of jumping its first-frame timelines
    through the whole clock, and `<anim autoplay>` starts at the load clock.
  - The flow `clock` node and `#document.clock` accumulate in double (each tick adds the float `dt`,
    widened, to a double sum; it was a float sum). `timer` and `throttle` are unchanged.
  - CSS animations: a finite `animation-iteration-count` above 1 repeats (it played once) and a
    finite `alternate` runs back; `reverse` / `alternate-reverse` with a finite count keep the delay,
    end on the last pass's end and finish; `reverse infinite` is reversed. A fractional count is
    still truncated. `@timeline` play modes are unchanged.
  - `@keyframes` without `from`, `to` or both: the missing end interpolates toward the property's
    current value, as CSS does (the element froze at the start value). A CSS animation that ends
    without a `forwards` fill puts opacity and the transform components back to their cascade values.
  - uiscale != 1: px grid tracks are scaled like other px lengths, an element under a world anchor
    lands exactly on the given screen point, and spawned instances and virtualized `<repeat>` spacers
    are no longer scaled twice. At uiscale 1 nothing changes.
  - Gamepad: the right stick and both triggers now reach `pad.rx|ry|lt|rt`; navigation, focus and
    scrolling are unchanged.

## [0.6.3] - 2026-11-22

### Changed
- The native library is now built from XTFrame-Backends `v0.19.3` (`backends/runtime/` =
  XTFrame-Runtime `v0.19.3`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild as described for 0.4.0. This is a runtime
  update only: no C ABI addition (ABI version still 1) and no change to `XtNative`.
- Behaviour changes that a host feels:
  - Fonts: within a family the face is chosen by width first (the request is normal width), then
    italic, then weight. Families whose typographic family also holds narrow or condensed faces
    (`"Arial"`, `"Myriad Pro"`, `"Bodoni MT"`, `"Goudy Old Style"`, `"Minion Pro"`) now draw their
    normal-width face instead of a narrow one, so the same text gets wider (`"Arial"` 264 -> 322 px).
    Legacy per-style family names (`"Arial Narrow"`, `"Segoe UI Semibold"`, `"Sitka Text"`) match when
    no face has that typographic family, instead of falling back. `font-stretch` is still not supported.
  - Software renderer: bilinear and trilinear image filtering run on premultiplied texels, as the GPU
    backends filter their textures, so a translucent image edge no longer picks up the colour of
    neighbouring transparent texels. Point sampling and opaque images are unchanged. The SW cache keeps
    a premultiplied level 0 per non-BC image (w x h x 4 more bytes).
  - Runtime: the values of `draggable`, `drop-target`, `autofocus`, `focus-trap` and `key-activation`
    are trimmed of surrounding white space before they are read (`draggable=" false "` is no longer a
    drag source). `Document::appendChild` / `insertBefore` with a pooled `<repeat>` instance as parent
    or child do nothing (a pooled row attached elsewhere crashed on reuse).
  - Bundle writes on Windows: `writeFile()` and the final move of `zipDirectory()` retry for up to
    about 0.25 s when another process briefly holds the target, then fail as before.

## [0.6.2] - 2026-11-21

### Changed
- The native library is now built from XTFrame-Backends `v0.19.2` (`backends/runtime/` =
  XTFrame-Runtime `v0.19.2`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild as described for 0.4.0. This is a runtime
  update only: no C ABI addition (ABI version still 1) and no change to `XtNative`.
- Behaviour changes that a host feels (the first two are decided when a UI is compiled, so the picture
  changes after the UI is recompiled with the new tools):
  - SVG gradients: a linear gradient under a skew, a non-uniform scale not aligned with the gradient,
    or a diagonal `objectBoundingBox` gradient on a non-square shape draws slanted stripes as
    browsers do; with `objectBoundingBox` and a `gradientTransform`, the transform is applied in the
    box's unit space and then the box (it was the box first).
  - `@keyframes` that differ in `custom()` effect parameters now animate them (they compiled to no
    track before).
  - Custom effects on a scene node (a shape, path or group inside an `<svg>`) read `XtInput.local`
    and `XtInput.boxSize` from the node's geometry box (they read 0 before); elements are unchanged.

## [0.6.1] - 2026-11-21

### Changed
- The native library is now built from XTFrame-Backends `v0.19.1` (`backends/runtime/` =
  XTFrame-Runtime `v0.19.1`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild as described for 0.4.0. This is a runtime
  update only: no C ABI addition (ABI version still 1) and no change to `XtNative`.
- Behaviour changes that a host feels:
  - `ui-scale: none` is factor 1 and `uiscaleDpiFloor` is no longer multiplied by the device pixel
    ratio; a host whose screen has a device pixel ratio other than 1 sees its UI scale change
    (nothing changes at 1).
  - An SVG gradient's first stop opacity no longer affects the whole gradient.
  - Custom effects gain the `XtInput.local` and `boxSize` inputs.

## [0.6.0] - 2026-11-20

### Changed
- The native library is now built from XTFrame-Backends `v0.19.0` (`backends/runtime/` =
  XTFrame-Runtime `v0.19.0`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild as described for 0.4.0.

### Added
- `XtNative` declarations for the XTFrame 0.19 measurement and idle C ABI additions (ABI version
  still 1, T-749, T-750, T-753): `xt_surface_frame_record` with `XtFrameRecord` (`xt_frame_record`,
  128 four-byte fields, 512 bytes), `xt_frame_record_csv_header`, `xt_frame_record_csv_row`,
  `xt_surface_mark_frame_start`, `xt_surface_mark_frame_end`, `xt_surface_set_thread_cpu_timing`,
  `xt_surface_set_perf_detail`, `xt_surface_needs_frame`. No high-level C# API yet.

## [0.5.0] - 2026-11-18

### Changed
- The native library is now built from XTFrame-Backends `v0.18.0` (`backends/runtime/` =
  XTFrame-Runtime `v0.18.0`). Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`) and rebuild as described for 0.4.0.

### Added
- `XtNative` declarations for the XTFrame 0.18 world-layer C ABI additions (ABI version still 1,
  T-709, 50.af): `xt_surface_set_world_layers`, `xt_surface_set_world_layer_density`,
  `xt_surface_render_layers_gpu`, `xt_surface_world_layer_count`, `xt_surface_world_layer_info` with
  `XtWorldLayer` (`xt_world_layer`), `xt_surface_world_layer_readback`, `xt_surface_draw_layers_gpu`
  (and `xt_surface_draw_layers_gpu_full` for dest = NULL), `xt_world_layer_orbit_matrix`,
  `xt_world_layer_unproject`, `xt_surface_push_input_ray`. No high-level C# API yet.

## [0.4.0] - 2026-11-14

### Changed
- The native library is now built from XTFrame-Backends `v0.17.0` (the repository's `backends/`
  submodule, which carries XTFrame-Runtime `v0.17.0` as `backends/runtime/`) instead of the
  XTFrame-Runtime mirror: since XTFrame 0.16 only XTFrame-Backends builds `xtframe` with the GPU target.
  Re-fetch the submodules (`git submodule sync --recursive`, then
  `git submodule update --init --recursive`), build with `cd backends`, `cmake --preset vs2022`,
  `cmake --build --preset vs2022-release --target xt_capi`, and copy `xtframe` and `wgpu_native` from
  `backends/bin/<platform>_<arch>/Release/` into `Runtime/Plugins/<platform>/`.
- On Windows `wgpu_native.dll` is now the prebuilt pinned by XTFrame-Backends (wgpu-native b83eb64,
  with memory hints).
- `xt_gpu_target_desc.sample_count` 0 means 1x since XTFrame 0.16 (it was 4x); the `XtNative` comment
  says so. No C# API change.

### Added
- `XtNative` declarations for the XTFrame 0.16 C ABI additions (ABI version still 1):
  `xt_gpu_target_set_image_budget`, `xt_gpu_target_trim`, `xt_gpu_target_memory_stats` with
  `XtGpuMemoryStats` (`xt_gpu_memory_stats`), `xt_gpu_target_preload_ui`, `xt_gpu_target_preload_image`.

## [0.1.0] - 2026-10-28

### Added
- Stage 1 package (T-505): P/Invoke layer mirroring the XTFrame C ABI v1 (`XtNative`).
- `XtBundle` (shared, reference-counted bundle cache), `XtSurface` (software-rendered `Texture2D`,
  input, declared ports, connection points, timelines, events), `XtInput` (key, modifier and wheel
  mapping for the legacy Input Manager and the Input System).
- `XtSurfaceComponent` (uGUI `RawImage` host with fit, pointer and keyboard forwarding, editor hot reload),
  `XtPortBridge` (Inspector `UnityEvent` wiring), custom Inspector, QuickStart sample.
