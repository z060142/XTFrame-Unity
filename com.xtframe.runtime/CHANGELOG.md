# Changelog

All notable changes to `com.xtframe.runtime` are documented here.

## [0.1.0] - 2026-10-28

### Added
- Stage 1 package (T-505): P/Invoke layer mirroring the XTFrame C ABI v1 (`XtNative`).
- `XtBundle` (shared, reference-counted bundle cache), `XtSurface` (software-rendered `Texture2D`,
  input, declared ports, connection points, timelines, events), `XtInput` (key, modifier and wheel
  mapping for the legacy Input Manager and the Input System).
- `XtSurfaceComponent` (uGUI `RawImage` host with fit, pointer and keyboard forwarding, editor hot reload),
  `XtPortBridge` (Inspector `UnityEvent` wiring), custom Inspector, QuickStart sample.
