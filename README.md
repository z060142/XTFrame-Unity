# XTFrame-Unity

The Unity host package for [XTFrame](https://github.com/shereware/XTFrame-Runtime): `com.xtframe.runtime`,
a pure C# UPM package that runs published XTFrame UI bundles through the XTFrame C ABI
(`xt_capi`, shared library `xtframe`). Stage 1 renders through the software renderer into a
`Texture2D` on a uGUI `RawImage`.

- Package: [`com.xtframe.runtime/`](com.xtframe.runtime/) (its README has the install, native-library
  and QuickStart steps).
- Runtime sources the package is built against: the `runtime/` submodule (the public XTFrame runtime
  mirror, pinned to a release tag). Build `xtframe.dll` / `libxtframe.so` / `libxtframe.dylib` from
  it with `-DXT_BUILD_CAPI=ON` and copy the library into `com.xtframe.runtime/Runtime/Plugins/<platform>/`.
- Unity: 2021.3 or newer. Windows, macOS and Linux editor and standalone players (stage 1).

History: the package lived in the XTFrame main repository under `Code/Hosts/Unity/` until 2026-11-01
and was moved here with `git subtree split`, history included.

## Versions

| Package | Runtime (submodule) | ABI |
|---|---|---|
| 0.1.0 | XTFrame-Runtime `v0.13.0-code.2` | `XT_CAPI_ABI_VERSION` 1 |
| 0.1.1 | XTFrame-Runtime `v0.13.1` | `XT_CAPI_ABI_VERSION` 1 |
| 0.2.0 | XTFrame-Runtime `v0.14.0` (IR 3.0: bundles must be republished with 0.14 tools) | `XT_CAPI_ABI_VERSION` 1 |
| 0.2.1 | XTFrame-Runtime `v0.14.0-code.1` (initial show now reaches the visibility listener) | `XT_CAPI_ABI_VERSION` 1 |
| 0.2.2 | XTFrame-Runtime `v0.14.0-code.2` (request_show outside an update notifies at the next update, lifecycle before OnShown) | `XT_CAPI_ABI_VERSION` 1 |
| 0.3.0 | XTFrame-Runtime `v0.15.0` (source format v2; bundles unchanged, document ports named `document.<port>`) | `XT_CAPI_ABI_VERSION` 1 |

## Testing

This repository carries no Unity CI yet (EditMode tests via `-batchmode -runTests` need a licensed
Unity on the runner). Verify on a Unity project: see `com.xtframe.runtime/README.md`, "Known limitations (stage 1)" and the XTFrame design chapter 50 §50.6.3 acceptance list.

## License

MIT, see [LICENSE](LICENSE).
