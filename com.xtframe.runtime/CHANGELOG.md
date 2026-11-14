# Changelog

All notable changes to `com.xtframe.runtime` are documented here.

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
