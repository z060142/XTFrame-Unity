// P/Invoke mirror of the XTFrame C ABI (xt/capi/xtframe.h, Docs/design/50-c-abi.md §50.3).
// Every declaration here follows the header text one-to-one: same entry point names, same
// parameter order, same field order and widths. Nothing in this file depends on UnityEngine.
//
// Strings passed in are marshalled as NUL-terminated UTF-8 (LPUTF8Str). Every `const char*` the
// library returns is declared as IntPtr and must be copied at once with Marshal.PtrToStringUTF8
// (see Str); the pointer is only valid until the next call on the same object.

using System;
using System.Runtime.InteropServices;

namespace XTFrame
{
    /// <summary>Status codes returned by the native library (values equal xt::Status).</summary>
    public enum XtStatus
    {
        /// <summary>Success.</summary>
        Ok = 0,
        /// <summary>An argument was invalid (bad id, bad UTF-8, surfaces still open, ...).</summary>
        InvalidArgument = 1,
        /// <summary>The compiled UI data was malformed.</summary>
        InvalidIr = 2,
        /// <summary>A named object was not found.</summary>
        NotFound = 3,
        /// <summary>An index was out of range.</summary>
        OutOfRange = 4,
        /// <summary>The call is not supported here (e.g. a forbidden call inside a callback).</summary>
        Unsupported = 5,
        /// <summary>A file could not be read.</summary>
        IoError = 6,
        /// <summary>An internal error (a caught C++ exception).</summary>
        Internal = 7,
        /// <summary>Incompatible data or ABI version.</summary>
        Incompatible = 8,
        /// <summary>Still in use (xt_gpu_target_destroy with surfaces attached).</summary>
        Busy = 9,
    }

    /// <summary>Input event kinds (xt_input_kind; values equal InputEvent::Kind order).</summary>
    public enum XtInputKind
    {
        /// <summary>Pointer moved.</summary>
        PointerMove = 0,
        /// <summary>Pointer button pressed.</summary>
        PointerDown = 1,
        /// <summary>Pointer button released.</summary>
        PointerUp = 2,
        /// <summary>Wheel / scroll.</summary>
        Wheel = 3,
        /// <summary>Key pressed.</summary>
        KeyDown = 4,
        /// <summary>Key released.</summary>
        KeyUp = 5,
        /// <summary>Committed text (UTF-8, any length).</summary>
        TextInput = 6,
        /// <summary>Pointer left the viewport.</summary>
        PointerLeave = 7,
        /// <summary>IME composition began.</summary>
        CompositionStart = 8,
        /// <summary>IME composition text changed.</summary>
        CompositionUpdate = 9,
        /// <summary>IME composition ended.</summary>
        CompositionEnd = 10,
        /// <summary>Gamepad button pressed.</summary>
        GamepadButtonDown = 11,
        /// <summary>Gamepad button released.</summary>
        GamepadButtonUp = 12,
        /// <summary>Gamepad axis moved.</summary>
        GamepadAxisMotion = 13,
        /// <summary>Recognized touch gesture.</summary>
        Gesture = 14,
        /// <summary>The host took the pointer press away.</summary>
        PointerCancel = 15,
    }

    /// <summary>Kind of a port event delivered to <see cref="XtSurface.PortEvent"/>.</summary>
    public enum XtPortEventKind
    {
        /// <summary>A declared out port emitted a value.</summary>
        Emit = 0,
        /// <summary>A declared in port changed.</summary>
        InputChanged = 1,
        /// <summary>A watched connection point fired.</summary>
        Connection = 2,
    }

    /// <summary>Declared port value type (values equal PortValueType, Docs/design/02 §PORT).</summary>
    public enum XtPortValueType
    {
#pragma warning disable 1591 // self-describing names
        Number = 1, String = 2, Bool = 3, Color = 4, Vec2 = 5, Vec3 = 6, Entity = 7, Enum = 8, Struct = 9,
        List = 10, Texture = 11, Event = 12,
#pragma warning restore 1591
    }

    /// <summary>Declared port direction (PortDir).</summary>
    public enum XtPortDir
    {
        /// <summary>Host to UI.</summary>
        In = 0,
        /// <summary>UI to host.</summary>
        Out = 1,
        /// <summary>Call port.</summary>
        Call = 2,
    }

    /// <summary>Cursor shape requested by the UI (values equal xt::Cursor).</summary>
    public enum XtCursor
    {
        /// <summary>Platform default arrow.</summary>
        Default = 0,
        /// <summary>Hand / link.</summary>
        Pointer = 1,
        /// <summary>Text I-beam.</summary>
        Text = 2,
        /// <summary>Move / drag.</summary>
        Move = 3,
    }

    /// <summary>Pointer buttons (values equal xt::PointerButton).</summary>
    public enum XtPointerButton
    {
        /// <summary>Primary button.</summary>
        Left = 0,
        /// <summary>Secondary button.</summary>
        Right = 1,
        /// <summary>Middle button.</summary>
        Middle = 2,
    }

    /// <summary>Modifier bit flags (values equal xt::Modifiers).</summary>
    [Flags]
    public enum XtModifiers : byte
    {
        /// <summary>No modifier.</summary>
        None = 0,
        /// <summary>Shift held.</summary>
        Shift = 1,
        /// <summary>Control held.</summary>
        Ctrl = 2,
        /// <summary>Alt / Option held.</summary>
        Alt = 4,
        /// <summary>Windows / Command held.</summary>
        Super = 8,
    }

    /// <summary>Key codes (values equal xt::KeyCode: USB HID values up to Up, then the T-143 block).</summary>
    public enum XtKey : ushort
    {
        /// <summary>No mapped key.</summary>
        Unknown = 0,
#pragma warning disable 1591 // self-describing key names
        KeyA = 4, KeyB = 5, KeyC = 6, KeyD = 7, KeyE = 8, KeyF = 9, KeyG = 10, KeyH = 11, KeyI = 12,
        KeyJ = 13, KeyK = 14, KeyL = 15, KeyM = 16, KeyN = 17, KeyO = 18, KeyP = 19, KeyQ = 20,
        KeyR = 21, KeyS = 22, KeyT = 23, KeyU = 24, KeyV = 25, KeyW = 26, KeyX = 27, KeyY = 28,
        KeyZ = 29,
        Key1 = 30, Key2 = 31, Key3 = 32, Key4 = 33, Key5 = 34, Key6 = 35, Key7 = 36, Key8 = 37,
        Key9 = 38, Key0 = 39,
        Enter = 40, Escape = 41, Backspace = 42, Tab = 43, Space = 44,
        Delete = 76, Home = 74, End = 77,
        Right = 79, Left = 80, Down = 81, Up = 82,
        // T-143 appended block (sequential, not HID).
        F1 = 83, F2 = 84, F3 = 85, F4 = 86, F5 = 87, F6 = 88, F7 = 89, F8 = 90, F9 = 91, F10 = 92,
        F11 = 93, F12 = 94,
        PageUp = 95, PageDown = 96, Insert = 97,
        ShiftLeft = 98, ShiftRight = 99, ControlLeft = 100, ControlRight = 101,
        AltLeft = 102, AltRight = 103, MetaLeft = 104, MetaRight = 105, CapsLock = 106,
        Minus = 107, Equal = 108, BracketLeft = 109, BracketRight = 110, Backslash = 111,
        Semicolon = 112, Quote = 113, Backquote = 114, Comma = 115, Period = 116, Slash = 117,
        Numpad0 = 118, Numpad1 = 119, Numpad2 = 120, Numpad3 = 121, Numpad4 = 122,
        Numpad5 = 123, Numpad6 = 124, Numpad7 = 125, Numpad8 = 126, Numpad9 = 127,
        NumpadAdd = 128, NumpadSubtract = 129, NumpadMultiply = 130, NumpadDivide = 131,
        NumpadDecimal = 132, NumpadEnter = 133,
#pragma warning restore 1591
    }

    /// <summary>Bundle display spec (xt_bundle_spec; defaults when the bundle has no spec).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XtBundleSpec
    {
        /// <summary>Logical height (default 1080).</summary>
        public float LogicalHeight;
        /// <summary>UiScaleRule: 0 height, 1 width, 2 shortEdge, 3 fit.</summary>
        public int UiScaleRule;
        /// <summary>Default safe area.</summary>
        public float SafeAreaDefault;
        /// <summary>Console safe area.</summary>
        public float SafeAreaConsole;
        /// <summary>Non-zero when the ui scale is clamped.</summary>
        public int UiScaleClamped;
        /// <summary>Lower clamp.</summary>
        public float UiScaleClampMin;
        /// <summary>Upper clamp.</summary>
        public float UiScaleClampMax;
        /// <summary>DPI floor.</summary>
        public float UiScaleDpiFloor;
    }

    /// <summary>Per-frame timings (xt_frame_stats; zero unless perf collection is enabled).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XtFrameStats
    {
        /// <summary>Last update time in milliseconds.</summary>
        public float UpdateMs;
        /// <summary>Last render time in milliseconds.</summary>
        public float RenderMs;
        /// <summary>Draw commands executed.</summary>
        public uint ExecutedDraws;
        /// <summary>Draw commands skipped.</summary>
        public uint SkippedDraws;
    }

    /// <summary>xt_ui_info.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtUiInfoNative
    {
        public IntPtr id;
        public IntPtr name;
        public float design_width;
        public float design_height;
        public IntPtr fit;
        public int has_flow;
        public uint port_count;
        public IntPtr contract;
    }

    /// <summary>xt_port_info.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtPortInfoNative
    {
        public IntPtr name;
        public IntPtr type;
        public int is_input;
    }

    /// <summary>xt_surface_desc.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtSurfaceDesc
    {
        public uint struct_size;
        public int width;
        public int height;
        public float device_pixel_ratio;
        public int premultiplied;
        public int run_flow;
        public int system_fonts;
        public uint instance_id;
    }

    /// <summary>xt_input_event. `text` is an unmanaged UTF-8 copy owned by the caller
    /// (Marshal.StringToCoTaskMemUTF8, freed with Marshal.FreeCoTaskMem after the call).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtInputEvent
    {
        public uint struct_size;
        public int kind;
        public float x;
        public float y;
        public int button;
        public float wheel_x;
        public float wheel_y;
        public ushort key;
        public byte modifiers;
        public byte repeat;
        public IntPtr text;
        public uint text_len;
        public uint composition_cursor;
        public uint composition_sel_len;
        public uint pointer_id;
        public byte gamepad_button;
        public byte gamepad_axis;
        public byte player;
        public byte clicks;
        public float axis_value;
        public byte gesture;
        public byte direction;
        public float gesture_scale;
    }

    /// <summary>xt_window_kind (T-636).</summary>
    public enum XtWindowKind
    {
        None = 0,
        Win32 = 1,
        Cocoa = 2,
        X11 = 3,
        Wayland = 4,
    }

    /// <summary>xt_native_window (64-bit layout: kind at 0, union at 8, size 24).</summary>
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal struct XtNativeWindow
    {
        [FieldOffset(0)] public int kind;
        /// <summary>hwnd | ns_window | x11 Display* | wl_display*</summary>
        [FieldOffset(8)] public IntPtr handle0;
        /// <summary>x11 Window (uint64) | wl_surface*</summary>
        [FieldOffset(16)] public IntPtr handle1;
    }

    /// <summary>xt_gpu_target_desc. struct_size = Marshal.SizeOf&lt;XtGpuTargetDesc&gt;() (64 on 64-bit); backend is a UTF-8 string pointer or IntPtr.Zero.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtGpuTargetDesc
    {
        public uint struct_size;
        public XtNativeWindow window;
        public int width;
        public int height;
        public int vsync;
        public IntPtr backend;
        public int sample_count; // T-647 (50.z): 0 = 1x (the default since v0.16.0, T-666; it was 4x before), 1 = none, 4 = 4x MSAA; read only when struct_size covers it (Marshal.SizeOf = 64 on x64)
    }

    /// <summary>xt_gpu_memory_stats (T-656, 50.ab). struct_size = Marshal.SizeOf&lt;XtGpuMemoryStats&gt;() (64), set by the caller; the library writes only the fields it covers.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtGpuMemoryStats
    {
        public uint struct_size;
        public uint image_textures;
        public ulong image_bytes;
        public uint image_idle_textures;
        public uint attached_surfaces;
        public ulong image_idle_bytes;
        public ulong image_evictions;
        public ulong surface_bytes;
        public ulong transient_bytes;
        public ulong atlas_bytes;
    }

    /// <summary>xt_world_layer (T-709, 50.af). struct_size = Marshal.SizeOf&lt;XtWorldLayer&gt;() (108), set by the caller; the library writes only the fields it covers. world3 is row-major (m[row * 4 + col]), rect is x, y, w, h.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtWorldLayer
    {
        public uint struct_size;
        public uint id;
        public uint order;
        public uint tex_width;
        public uint tex_height;
        /// <summary>16 floats, row-major: layer plane to root box.</summary>
        public unsafe fixed float world3[16];
        /// <summary>4 floats: x, y, w, h on the layer plane (logical px).</summary>
        public unsafe fixed float rect[4];
        public float opacity;
        public byte backface_hidden;
        public byte changed;
        public byte _pad0;
        public byte _pad1;
    }

    /// <summary>xt_frame_record (T-750 / T-753, 57.2 / 57.5): FrameRecord v1, the runtime's per-frame record. 128 four-byte fields = 512 bytes; struct_size = Marshal.SizeOf&lt;XtFrameRecord&gt;(), set by the caller; the library writes only the fields it covers. causes is [cause][kind] flattened row-major, 11 x 4 = 44 entries, and must be allocated (new uint[44]) before the struct is passed in. Field order and types follow the header one-to-one.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtFrameRecord
    {
        public uint struct_size;
        public uint version;
        public uint frame;
        public uint steady;
        public float time_s;
        public uint backend;
        public uint frame_marked;
        public float frame_wall_ms;
        public float frame_cpu_ms;
        public float unaccounted_ms;
        public float input_ms;
        public float input_cpu_ms;
        public float update_ms;
        public float update_cpu_ms;
        public float flow_ms;
        public float flow_cpu_ms;
        public float paint_ms;
        public float paint_cpu_ms;
        public float damage_ms;
        public float damage_cpu_ms;
        public float build_ms;
        public float build_cpu_ms;
        public float submit_ms;
        public float submit_cpu_ms;
        public float wait_ms;
        public float wait_cpu_ms;
        public float update_style_ms;
        public float update_layout_ms;
        public float update_animate_ms;
        public float update_transform_ms;
        public float submit_plan_ms;
        public float submit_upload_ms;
        public float submit_encode_ms;
        public float submit_queue_ms;
        public uint styled_elements;
        public uint laid_out_elements;
        public uint layout_roots;
        public uint layout_full_passes;
        public uint active_animations;
        public uint active_transitions;
        public uint anim_evaluated;
        public uint expr_evaluated;
        public uint set_style_calls;
        public uint flow_signals;
        public uint flow_node_evals;
        public uint flow_sink_writes;
        public uint commands;
        public uint layers;
        public uint effect_layers;
        public uint projections;
        public uint glyphs;
        public uint path_points;
        public uint image_draws;
        public uint culled_offscreen;
        public uint border_dash_hits;
        public uint border_dash_misses;
        public uint bands;
        public uint band_redraws;
        public uint passes;
        public uint draw_calls;
        public uint pipeline_binds;
        public uint bind_groups;
        public uint upload_bytes;
        public uint transient_creates;
        public uint frame_reused;
        public uint effect_cache_hits;
        public uint effect_cache_misses;
        public uint coverage_hits;
        public uint coverage_misses;
        public float idle_paint_ms;
        public uint paint_skipped;
        public uint static_skip_mismatch;
        public uint partial_layout_mismatch;
        public uint arena_nodes;
        public uint command_bytes;
        public uint effect_cache_bytes;
        public uint transient_pool_bytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 44)]
        public uint[] causes;
        public uint redundant_styled;
        public uint redundant_laid_out;
        public uint redundant_commands;
        public uint repainted_unchanged_not_cacheable;
        public uint repainted_unchanged_key_changed;
        public uint repainted_unchanged_budget;
        public uint repainted_unchanged_first_sight;
    }

    /// <summary>xt_rect (device pixels, top-left origin).</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtRect
    {
        public int x;
        public int y;
        public int w;
        public int h;
    }

    /// <summary>xt_damage.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtDamage
    {
        public int full;
        public int changed;
        public uint rect_count;
        /// <summary>16 xt_rect entries (x, y, w, h each) as a fixed buffer, so a Render() allocates nothing.</summary>
        public unsafe fixed int rects[64];

        /// <summary>Rectangle `index` (0..rect_count-1) of the damage list.</summary>
        public unsafe XtRect Rect(int index)
        {
            fixed (int* r = rects)
            {
                int* q = r + index * 4;
                return new XtRect { x = q[0], y = q[1], w = q[2], h = q[3] };
            }
        }
    }

    /// <summary>xt_play_options. `from_marker` is an unmanaged UTF-8 copy owned by the caller.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtPlayOptionsNative
    {
        public uint struct_size;
        public int reverse;
        public int loop;
        public int once;
        public int hold;
        public IntPtr from_marker;
        public float rate;
    }

    /// <summary>xt_port_record.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtPortRecordNative
    {
        public uint id;
        public IntPtr name;
        public IntPtr qualified_name;
        public IntPtr element_id;
        public int value_type;
        public int dir;
        public int document_level;
    }

    /// <summary>xt_port_event.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtPortEventNative
    {
        public int kind;
        public uint port;
        public uint listener_id;
        public IntPtr name;
        public int value_type;
        public double number;
        public int boolean;
        public IntPtr text;
        public uint instance_id;
        public uint item_index;
    }

    /// <summary>xt_host_event.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtHostEventNative
    {
        public int kind;
        public IntPtr name;
        public float volume;
        public IntPtr ui_id;
        public uint instance_id;
        public IntPtr node_id;
    }

    /// <summary>xt_timeline_event.</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct XtTimelineEventNative
    {
        public int kind;
        public IntPtr timeline;
        public IntPtr marker;
    }

    /// <summary>Raw P/Invoke declarations for xtframe.dll / libxtframe.so / libxtframe.dylib.</summary>
    internal static class XtNative
    {
        public const string Lib = "xtframe";
        public const uint AbiVersion = 1; // XT_CAPI_ABI_VERSION
        public const uint InvalidPort = 0; // XT_INVALID_PORT

        // ---- callback types ----
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LogFn(int level, IntPtr message, IntPtr user);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void PortEventFn(IntPtr s, IntPtr e, IntPtr user);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void HostEventFn(IntPtr s, IntPtr e, IntPtr user);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void TimelineEventFn(IntPtr s, IntPtr e, IntPtr user);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VisibilityFn(IntPtr s, int visible, IntPtr user);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UpdateHookFn(IntPtr s, float dt, IntPtr user);

        // ---- library ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_capi_abi_version")]
        public static extern uint xt_capi_abi_version();
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_capi_version_string")]
        public static extern IntPtr xt_capi_version_string();
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_capi_last_error")]
        public static extern IntPtr xt_capi_last_error();
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_capi_set_log")]
        public static extern void xt_capi_set_log(LogFn fn, IntPtr user);

        // ---- bundle ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_open")]
        public static extern IntPtr xt_bundle_open([MarshalAs(UnmanagedType.LPUTF8Str)] string dirUtf8);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_close")]
        public static extern XtStatus xt_bundle_close(IntPtr b);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_id")]
        public static extern IntPtr xt_bundle_id(IntPtr b);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_name")]
        public static extern IntPtr xt_bundle_name(IntPtr b);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_dir")]
        public static extern IntPtr xt_bundle_dir(IntPtr b);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_ui_count")]
        public static extern uint xt_bundle_ui_count(IntPtr b);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_ui_info")]
        public static extern XtStatus xt_bundle_ui_info(IntPtr b, uint index, out XtUiInfoNative info);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_find_ui")]
        public static extern int xt_bundle_find_ui(IntPtr b, [MarshalAs(UnmanagedType.LPUTF8Str)] string uiId);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_ui_port_info")]
        public static extern XtStatus xt_bundle_ui_port_info(IntPtr b, uint uiIndex, uint portIndex,
                                                             out XtPortInfoNative info);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_get_spec")]
        public static extern XtStatus xt_bundle_get_spec(IntPtr b, out XtBundleSpec spec);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_reload")]
        public static extern uint xt_bundle_reload(IntPtr b);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_bundle_changed_ui_id")]
        public static extern IntPtr xt_bundle_changed_ui_id(IntPtr b, uint index);

        // ---- surface ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_create")]
        public static extern IntPtr xt_surface_create(IntPtr b, [MarshalAs(UnmanagedType.LPUTF8Str)] string uiId,
                                                      ref XtSurfaceDesc desc);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_destroy")]
        public static extern void xt_surface_destroy(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_ui_id")]
        public static extern IntPtr xt_surface_ui_id(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_resize")]
        public static extern XtStatus xt_surface_resize(IntPtr s, int width, int height, float devicePixelRatio);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_ui_scale")]
        public static extern XtStatus xt_surface_set_ui_scale(IntPtr s, float scale);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_ui_scale")]
        public static extern float xt_surface_ui_scale(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_safe_area")]
        public static extern XtStatus xt_surface_set_safe_area(IntPtr s, float top, float right, float bottom, float left);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_input_kind")]
        public static extern XtStatus xt_surface_set_input_kind(IntPtr s, int kind);

        // ---- input ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_push_input")]
        public static extern XtStatus xt_surface_push_input(IntPtr s, ref XtInputEvent e);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_cancel_pointer")]
        public static extern void xt_surface_cancel_pointer(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_cursor")]
        public static extern int xt_surface_cursor(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_wants_text_input")]
        public static extern int xt_surface_wants_text_input(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_text_input_rect")]
        public static extern XtStatus xt_surface_text_input_rect(IntPtr s, out float x, out float y, out float w,
                                                                 out float h);

        // ---- frame ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_update")]
        public static extern XtStatus xt_surface_update(IntPtr s, float dt);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_render")]
        public static extern XtStatus xt_surface_render(IntPtr s, IntPtr rgba, int strideBytes, out XtDamage damage);

        // ---- lifecycle and appearance ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_request_show")]
        public static extern XtStatus xt_surface_request_show(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_request_hide")]
        public static extern XtStatus xt_surface_request_hide(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_visible")]
        public static extern XtStatus xt_surface_set_visible(IntPtr s, int visible);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_is_visible")]
        public static extern int xt_surface_is_visible(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_theme")]
        public static extern XtStatus xt_surface_set_theme(IntPtr s, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_language")]
        public static extern XtStatus xt_surface_set_language(IntPtr s, [MarshalAs(UnmanagedType.LPUTF8Str)] string lang);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_post_system_event")]
        public static extern XtStatus xt_surface_post_system_event(IntPtr s,
                                                                   [MarshalAs(UnmanagedType.LPUTF8Str)] string kind,
                                                                   [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_hot_reload")]
        public static extern XtStatus xt_surface_hot_reload(IntPtr s);

        // ---- timelines (opt is a const xt_play_options* that may be NULL) ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_play_timeline")]
        public static extern XtStatus xt_surface_play_timeline(IntPtr s,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string scopePath,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
                                                               IntPtr opt);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_stop_timeline")]
        public static extern XtStatus xt_surface_stop_timeline(IntPtr s,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string scopePath,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_seek_timeline")]
        public static extern XtStatus xt_surface_seek_timeline(IntPtr s,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string scopePath,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string name, float t);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_is_timeline_playing")]
        public static extern int xt_surface_is_timeline_playing(IntPtr s,
                                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string scopePath,
                                                                [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        // ---- declared ports ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_count")]
        public static extern uint xt_surface_port_count(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_record")]
        public static extern XtStatus xt_surface_port_record(IntPtr s, uint index, out XtPortRecordNative record);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_lookup")]
        public static extern uint xt_surface_port_lookup(IntPtr s, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_set_number")]
        public static extern XtStatus xt_surface_port_set_number(IntPtr s, uint port, double value);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_set_bool")]
        public static extern XtStatus xt_surface_port_set_bool(IntPtr s, uint port, int value);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_set_text")]
        public static extern XtStatus xt_surface_port_set_text(IntPtr s, uint port,
                                                               [MarshalAs(UnmanagedType.LPUTF8Str)] string text);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_pulse")]
        public static extern XtStatus xt_surface_port_pulse(IntPtr s, uint port);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_get_number")]
        public static extern double xt_surface_port_get_number(IntPtr s, uint port);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_get_bool")]
        public static extern int xt_surface_port_get_bool(IntPtr s, uint port);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_port_get_text")]
        public static extern IntPtr xt_surface_port_get_text(IntPtr s, uint port);

        // ---- connection points ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_connection_set_number")]
        public static extern XtStatus xt_surface_connection_set_number(IntPtr s,
                                                                       [MarshalAs(UnmanagedType.LPUTF8Str)] string port,
                                                                       double value,
                                                                       [MarshalAs(UnmanagedType.LPUTF8Str)] string param);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_connection_set_bool")]
        public static extern XtStatus xt_surface_connection_set_bool(IntPtr s,
                                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string port,
                                                                     int value,
                                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string param);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_connection_set_text")]
        public static extern XtStatus xt_surface_connection_set_text(IntPtr s,
                                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string port,
                                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string text,
                                                                     [MarshalAs(UnmanagedType.LPUTF8Str)] string param);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_connection_pulse")]
        public static extern XtStatus xt_surface_connection_pulse(IntPtr s,
                                                                  [MarshalAs(UnmanagedType.LPUTF8Str)] string port,
                                                                  [MarshalAs(UnmanagedType.LPUTF8Str)] string param);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_connection_watch")]
        public static extern XtStatus xt_surface_connection_watch(IntPtr s,
                                                                  [MarshalAs(UnmanagedType.LPUTF8Str)] string port,
                                                                  out uint listenerId);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_connection_unwatch")]
        public static extern void xt_surface_connection_unwatch(IntPtr s, uint listenerId);

        // ---- callbacks ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_port_listener")]
        public static extern XtStatus xt_surface_set_port_listener(IntPtr s, PortEventFn fn, IntPtr user);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_host_event_listener")]
        public static extern XtStatus xt_surface_set_host_event_listener(IntPtr s, HostEventFn fn, IntPtr user);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_timeline_listener")]
        public static extern XtStatus xt_surface_set_timeline_listener(IntPtr s, TimelineEventFn fn, IntPtr user);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_visibility_listener")]
        public static extern XtStatus xt_surface_set_visibility_listener(IntPtr s, VisibilityFn fn, IntPtr user);
        // T-561 (50.3): called inside xt_surface_update, after input dispatch, before style resolution.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_update_hook")]
        public static extern XtStatus xt_surface_set_update_hook(IntPtr s, UpdateHookFn fn, IntPtr user);
        // T-635 (50.y): returned pointer is owned by the surface and valid until the next save_state or destroy; read it immediately.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_save_state")]
        public static extern IntPtr xt_surface_save_state(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_load_state")]
        public static extern XtStatus xt_surface_load_state(IntPtr s, [MarshalAs(UnmanagedType.LPUTF8Str)] string json);

        // T-636 (50.z): GPU target; wgpu_native must sit next to xtframe in Plugins
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_create")]
        public static extern IntPtr xt_gpu_target_create(ref XtGpuTargetDesc desc);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_destroy")]
        public static extern XtStatus xt_gpu_target_destroy(IntPtr t);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_resize")]
        public static extern XtStatus xt_gpu_target_resize(IntPtr t, int width, int height);
        /// <param name="clearRgba">4 floats, straight alpha 0..1; null = transparent.</param>
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_begin_frame")]
        public static extern XtStatus xt_gpu_target_begin_frame(IntPtr t, float[] clearRgba);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_end_frame")]
        public static extern XtStatus xt_gpu_target_end_frame(IntPtr t);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_readback")]
        public static extern XtStatus xt_gpu_target_readback(IntPtr t, IntPtr rgba, int stride);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_render_gpu")]
        public static extern XtStatus xt_surface_render_gpu(IntPtr s, IntPtr t, ref XtRect dest);
        /// <summary>xt_surface_render_gpu with dest = NULL (the whole target).</summary>
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_render_gpu")]
        public static extern XtStatus xt_surface_render_gpu_full(IntPtr s, IntPtr t, IntPtr destNull);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_detach_gpu")]
        public static extern XtStatus xt_surface_detach_gpu(IntPtr s);
        // T-641 (50.aa): hidden surfaces release their render scratch by default; set_trim_on_hide(s, 0) keeps it. trim is callback-safe.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_trim")]
        public static extern XtStatus xt_surface_trim(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_trim_on_hide")]
        public static extern XtStatus xt_surface_set_trim_on_hide(IntPtr s, int enabled);
        // T-656 (50.ab): shared image textures of a target; idle budget (default 64 MiB, 0 = destroy when unused), trim (not inside a frame), counters.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_set_image_budget")]
        public static extern XtStatus xt_gpu_target_set_image_budget(IntPtr t, ulong idleBytes);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_trim")]
        public static extern XtStatus xt_gpu_target_trim(IntPtr t);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_memory_stats")]
        public static extern XtStatus xt_gpu_target_memory_stats(IntPtr t, ref XtGpuMemoryStats stats);
        // T-688 (50.ae): synchronous image preload into the target's shared cache; loaded = textures newly uploaded.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_preload_ui")]
        public static extern XtStatus xt_gpu_target_preload_ui(IntPtr t, IntPtr b, [MarshalAs(UnmanagedType.LPUTF8Str)] string uiId,
                                                               out uint loaded);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_gpu_target_preload_image")]
        public static extern XtStatus xt_gpu_target_preload_image(IntPtr t, IntPtr b,
                                                                  [MarshalAs(UnmanagedType.LPUTF8Str)] string asset,
                                                                  out uint loaded);

        // T-709 (50.af): world-layer output. Every function returns Unsupported (count 0) in a build without wgpu, except orbit_matrix and unproject (pure math).
        // Float arrays: boxToDest / outMatrix are 16 floats, origin / direction are 3 floats.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_world_layers")]
        public static extern XtStatus xt_surface_set_world_layers(IntPtr s, int enabled);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_world_layer_density")]
        public static extern XtStatus xt_surface_set_world_layer_density(IntPtr s, float density);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_render_layers_gpu")]
        public static extern XtStatus xt_surface_render_layers_gpu(IntPtr s, IntPtr t);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_world_layer_count")]
        public static extern uint xt_surface_world_layer_count(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_world_layer_info")]
        public static extern XtStatus xt_surface_world_layer_info(IntPtr s, uint index, ref XtWorldLayer layer);
        /// <param name="rgba">tex_width x tex_height RGBA8, premultiplied alpha; stride &gt;= tex_width * 4.</param>
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_world_layer_readback")]
        public static extern XtStatus xt_surface_world_layer_readback(IntPtr s, uint index, IntPtr rgba, int stride);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_draw_layers_gpu")]
        public static extern XtStatus xt_surface_draw_layers_gpu(IntPtr s, IntPtr t, ref XtRect dest, float[] boxToDest,
                                                                 float depthScale);
        /// <summary>xt_surface_draw_layers_gpu with dest = NULL (the whole target).</summary>
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_draw_layers_gpu")]
        public static extern XtStatus xt_surface_draw_layers_gpu_full(IntPtr s, IntPtr t, IntPtr destNull,
                                                                      float[] boxToDest, float depthScale);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_world_layer_orbit_matrix")]
        public static extern XtStatus xt_world_layer_orbit_matrix(float[] outMatrix, float boxW, float boxH, float destW,
                                                                  float destH, float yawDeg, float pitchDeg, float fovDeg);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_world_layer_unproject")]
        public static extern XtStatus xt_world_layer_unproject(float[] boxToDest, float depthScale, float destX, float destY,
                                                               float[] originOut, float[] directionOut);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_push_input_ray")]
        public static extern XtStatus xt_surface_push_input_ray(IntPtr s, ref XtInputEvent e, float[] origin,
                                                                float[] direction);

        // ---- world anchor, perf ----
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_world_anchor")]
        public static extern XtStatus xt_surface_set_world_anchor(IntPtr s, uint port, float x, float y, float depth,
                                                                  int onScreen);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_perf_enabled")]
        public static extern void xt_surface_set_perf_enabled(IntPtr s, int enabled);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_frame_stats")]
        public static extern XtStatus xt_surface_frame_stats(IntPtr s, out XtFrameStats stats);

        // T-750 / T-753 (57.2, 57.5): FrameRecord, frame marks, thread CPU timing, perf detail, needs-frame. ABI version still 1.
        // Perf must be on (xt_surface_set_perf_enabled) for the record. CSV functions: buf is a caller-provided UTF-8 buffer of cap bytes
        // (NUL-terminated, truncated); the return value is the length the whole text needs, like snprintf; buf may be null with cap 0.
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_frame_record")]
        public static extern XtStatus xt_surface_frame_record(IntPtr s, ref XtFrameRecord record);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_frame_record_csv_header")]
        public static extern UIntPtr xt_frame_record_csv_header(byte[] buf, UIntPtr cap);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_frame_record_csv_row")]
        public static extern UIntPtr xt_frame_record_csv_row(ref XtFrameRecord record, byte[] buf, UIntPtr cap);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_mark_frame_start")]
        public static extern XtStatus xt_surface_mark_frame_start(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_mark_frame_end")]
        public static extern XtStatus xt_surface_mark_frame_end(IntPtr s);
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_thread_cpu_timing")]
        public static extern XtStatus xt_surface_set_thread_cpu_timing(IntPtr s, int enabled);
        /// <param name="level">0 (default), 1 or 2; anything else is InvalidArgument.</param>
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_set_perf_detail")]
        public static extern XtStatus xt_surface_set_perf_detail(IntPtr s, int level);
        /// <param name="needsFrame">0: nothing would change, the host may skip update and render; 1: it has to update and render.</param>
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "xt_surface_needs_frame")]
        public static extern XtStatus xt_surface_needs_frame(IntPtr s, out int needsFrame);

        // ---- helpers ----

        /// <summary>Copies a native UTF-8 string at once; NULL becomes "".</summary>
        public static string Str(IntPtr p)
        {
            return p == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUTF8(p);
        }

        /// <summary>The calling thread's last native error message ("" if none).</summary>
        public static string LastError()
        {
            return Str(xt_capi_last_error());
        }

        /// <summary>Throws <see cref="XtException"/> (with the last error text) unless status is Ok.</summary>
        public static void Check(XtStatus status)
        {
            if (status != XtStatus.Ok)
                throw new XtException(status, LastError());
        }

        /// <summary>Compares the loaded library's ABI version with <see cref="AbiVersion"/>.
        /// Throws DllNotFoundException when the library is missing and XtException on mismatch.</summary>
        public static void CheckAbi()
        {
            uint loaded = xt_capi_abi_version();
            if (loaded != AbiVersion)
                throw new XtException(XtStatus.Incompatible,
                                      "XTFrame native library ABI version " + loaded + " does not match the C# package ABI version " +
                                          AbiVersion + " (library " + Str(xt_capi_version_string()) +
                                          "). Copy a matching xtframe library into Runtime/Plugins.");
        }
    }
}
