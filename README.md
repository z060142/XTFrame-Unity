# XTFrame-Unity

The Unity host package for [XTFrame](https://github.com/shereware/XTFrame-Runtime): `com.xtframe.runtime`,
a pure C# UPM package that runs published XTFrame UI bundles through the XTFrame C ABI
(`xt_capi`, shared library `xtframe`). Stage 1 renders through the software renderer into a
`Texture2D` on a uGUI `RawImage`.

- Package: [`com.xtframe.runtime/`](com.xtframe.runtime/) (its README has the install, native-library
  and QuickStart steps).
- Native sources the package is built against: the `backends/` submodule
  ([XTFrame-Backends](https://github.com/shereware/XTFrame-Backends), pinned to a release tag), which
  carries the XTFrame runtime mirror as its own `backends/runtime/` submodule at the tag of the same
  name. Since XTFrame 0.16 the runtime mirror alone builds `xtframe` without the GPU target; the
  complete library (with `xt_gpu_target_*`) is built from XTFrame-Backends. The C ABI header is
  `backends/runtime/Code/Runtime/capi/include/xt/capi/xtframe.h`.
- Building the native library (Windows x64, Visual Studio 2022; the presets turn on `XT_BUILD_CAPI`):

  ```
  git submodule update --init --recursive
  cd backends
  cmake --preset vs2022
  cmake --build --preset vs2022-release --target xt_capi
  ```

  Linux / macOS: `cmake --preset ninja-gcc-release` (or `ninja-clang-release`), then
  `cmake --build --preset ninja-gcc-release --target xt_capi`. The products land in
  `backends/bin/<platform>_<arch>/Release/` (e.g. `backends/bin/win_x64/Release/`, git-ignored):
  `xtframe.dll` / `libxtframe.so` / `libxtframe.dylib` with `wgpu_native.dll` / `libwgpu_native.so` /
  `libwgpu_native.dylib` next to it. Copy both into `com.xtframe.runtime/Runtime/Plugins/<platform>/`
  (`wgpu_native` is needed by `xt_gpu_target_*`; without it the GPU target fails to create and the
  software path still works). On Windows `wgpu_native.dll` is the prebuilt that XTFrame-Backends pins
  in `Code/SDKs/versions_backends.cmake` (wgpu-native commit b83eb64, with memory hints), downloaded and
  hash-checked by the first configure.
- Unity: 2021.3 or newer. Windows, macOS and Linux editor and standalone players (stage 1).

History: the package lived in the XTFrame main repository under `Code/Hosts/Unity/` until 2026-11-01
and was moved here with `git subtree split`, history included.

## Versions

Up to 0.3.7 the submodule was `runtime/` (XTFrame-Runtime); from 0.4.0 it is `backends/`
(XTFrame-Backends, with XTFrame-Runtime as `backends/runtime/`).

| Package | Native source (submodule) | ABI |
|---|---|---|
| 0.1.0 | XTFrame-Runtime `v0.13.0-code.2` | `XT_CAPI_ABI_VERSION` 1 |
| 0.1.1 | XTFrame-Runtime `v0.13.1` | `XT_CAPI_ABI_VERSION` 1 |
| 0.2.0 | XTFrame-Runtime `v0.14.0` (IR 3.0: bundles must be republished with 0.14 tools) | `XT_CAPI_ABI_VERSION` 1 |
| 0.2.1 | XTFrame-Runtime `v0.14.0-code.1` (initial show now reaches the visibility listener) | `XT_CAPI_ABI_VERSION` 1 |
| 0.2.2 | XTFrame-Runtime `v0.14.0-code.2` (request_show outside an update notifies at the next update, lifecycle before OnShown) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.0 | XTFrame-Runtime `v0.15.0` (source format v2; bundles unchanged, document ports named `document.<port>`) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.1 | XTFrame-Runtime `v0.15.0-code.1` (bundle string tables load on open; set_language re-resolves `@key` and posts `languageChanged` itself) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.2 | XTFrame-Runtime `v0.15.1` (`spec.defaultLanguage` in the manifest; `<repeat>` template svg/@key fixes; importer fixes; no ABI or bundle change) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.3 | XTFrame-Runtime `v0.15.1-code.1` (xt_surface_save_state / xt_surface_load_state externs) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.4 | XTFrame-Runtime `v0.15.1-code.2` (GPU target: xt_gpu_target_*, xt_surface_render_gpu; wgpu_native ships beside xtframe) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.5 | XTFrame-Runtime `v0.15.1-code.3` (hidden surfaces release render scratch; xt_surface_trim / xt_surface_set_trim_on_hide) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.6 | XTFrame-Runtime `v0.15.1-code.4` (sample_count on the GPU target; damage-aware SW partial redraw, SIMD compositing, per-target shared GPU textures/pipelines) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.7 | XTFrame-Runtime `v0.15.2` (release: snapshots v2-v4, GPU target with sample_count, trim-on-hide, per-bundle threading; no ABI change) | `XT_CAPI_ABI_VERSION` 1 |
| 0.4.0 | XTFrame-Backends `v0.17.0` (`backends/runtime/` = XTFrame-Runtime `v0.17.0`; GPU target memory: xt_gpu_target_set_image_budget / trim / memory_stats, image preload: xt_gpu_target_preload_ui / preload_image; sample_count 0 now means 1x) | `XT_CAPI_ABI_VERSION` 1 |
| 0.5.0 | XTFrame-Backends `v0.18.0` (`backends/runtime/` = XTFrame-Runtime `v0.18.0`; world layers: xt_surface_set_world_layers / set_world_layer_density / render_layers_gpu / world_layer_count / world_layer_info / world_layer_readback / draw_layers_gpu, xt_world_layer_orbit_matrix / unproject, xt_surface_push_input_ray) | `XT_CAPI_ABI_VERSION` 1 |
| 0.6.0 | XTFrame-Backends `v0.19.0` (`backends/runtime/` = XTFrame-Runtime `v0.19.0`; measurement and idle: xt_surface_frame_record with xt_frame_record, xt_frame_record_csv_header / csv_row, xt_surface_mark_frame_start / mark_frame_end, xt_surface_set_thread_cpu_timing, xt_surface_set_perf_detail, xt_surface_needs_frame) | `XT_CAPI_ABI_VERSION` 1 |
| 0.6.1 | XTFrame-Backends `v0.19.1` (`backends/runtime/` = XTFrame-Runtime `v0.19.1`; runtime update only, no C ABI change) | `XT_CAPI_ABI_VERSION` 1 |
| 0.6.2 | XTFrame-Backends `v0.19.2` (`backends/runtime/` = XTFrame-Runtime `v0.19.2`; runtime update only, no C ABI change) | `XT_CAPI_ABI_VERSION` 1 |

### Upgrading to 0.4.0

The submodule moved from `runtime/` to `backends/`. In an existing clone, after pulling:

```
git submodule sync --recursive
git submodule update --init --recursive
```

(a leftover `runtime/` directory can be deleted). Rebuild the native library from `backends/` as
described above and copy `xtframe` and `wgpu_native` from `backends/bin/<platform>_<arch>/Release/`
into `com.xtframe.runtime/Runtime/Plugins/<platform>/` together; close the Unity Editor first.
A GPU target created with `sample_count` 0 now renders surfaces without MSAA (1x); pass 4 for 4x.

## Testing

This repository carries no Unity CI yet (EditMode tests via `-batchmode -runTests` need a licensed
Unity on the runner). Verify on a Unity project: see `com.xtframe.runtime/README.md`, "Known limitations (stage 1)" and the XTFrame design chapter 50 §50.6.3 acceptance list.

## License

MIT, see [LICENSE](LICENSE).
