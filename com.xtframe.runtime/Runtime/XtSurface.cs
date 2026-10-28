using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace XTFrame
{
    /// <summary>
    /// One XTFrame UI rendered in software into a <see cref="Texture2D"/> it owns (xt_surface).
    /// Main thread only. Call <see cref="Dispose"/> when done; a Dispose from inside one of the
    /// surface's own events is deferred until the native call that raised it has returned.
    /// </summary>
    public sealed partial class XtSurface : IDisposable
    {
        private static readonly XtNative.PortEventFn s_portFn = OnPortEvent;
        private static readonly XtNative.HostEventFn s_hostFn = OnHostEvent;
        private static readonly XtNative.TimelineEventFn s_timelineFn = OnTimelineEvent;
        private static readonly XtNative.VisibilityFn s_visibilityFn = OnVisibility;
        private static readonly uint s_inputEventSize = (uint)Marshal.SizeOf<XtInputEvent>();
        private static readonly uint s_playOptionsSize = (uint)Marshal.SizeOf<XtPlayOptionsNative>();

        private IntPtr _handle;
        private XtBundle _bundle;
        private GCHandle _self;
        private Texture2D _texture;
        private int _width;
        private int _height;
        private float _dpr;
        private int _callbackDepth;
        private bool _disposeRequested;
        private readonly List<XtPortRecord> _ports = new List<XtPortRecord>();

        /// <summary>A declared port emitted or changed, or a watched connection point fired.</summary>
        public event Action<XtPortEvent> PortEvent;
        /// <summary>The flow raised a host event.</summary>
        public event Action<XtHostEvent> HostEvent;
        /// <summary>A timeline started, ended or hit a marker.</summary>
        public event Action<XtTimelineEvent> TimelineEvent;
        /// <summary>The UI was shown (true) or hidden (false).</summary>
        public event Action<bool> VisibilityChanged;
        /// <summary>Raised after <see cref="HotReload"/> succeeded (port ids are re-read).</summary>
        public event Action HotReloaded;

        /// <summary>The texture the UI is drawn into (RGBA32, sRGB, no mips, rows top to bottom).</summary>
        public Texture2D Texture => _texture;
        /// <summary>The bundle this surface was created from.</summary>
        public XtBundle Bundle => _bundle;
        /// <summary>Surface width in device pixels.</summary>
        public int Width => _width;
        /// <summary>Surface height in device pixels.</summary>
        public int Height => _height;
        /// <summary>True after the native surface was destroyed.</summary>
        public bool IsDisposed => _handle == IntPtr.Zero;
        /// <summary>The UI id.</summary>
        public string UiId { get; }

        /// <summary>Creates a surface for a UI of an open bundle; takes its own bundle reference.</summary>
        public XtSurface(XtBundle bundle, string uiId, int width, int height, XtSurfaceOptions options)
        {
            if (bundle == null || !bundle.IsOpen)
                throw new XtException(XtStatus.InvalidArgument, "bundle is not open");
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            _dpr = options.DevicePixelRatio > 0f ? options.DevicePixelRatio : 1f;
            var desc = new XtSurfaceDesc
            {
                struct_size = (uint)Marshal.SizeOf<XtSurfaceDesc>(),
                width = width,
                height = height,
                device_pixel_ratio = _dpr,
                premultiplied = options.Premultiplied ? 1 : 0,
                run_flow = options.RunFlow ? 1 : 0,
                system_fonts = options.SystemFonts ? 1 : 0,
                instance_id = options.InstanceId,
            };
            IntPtr h = XtNative.xt_surface_create(bundle.Handle, uiId, ref desc);
            if (h == IntPtr.Zero)
                throw new XtException(XtStatus.NotFound, XtNative.LastError());
            _handle = h;
            _bundle = bundle;
            bundle.Retain();
            UiId = uiId;
            _width = width;
            _height = height;
            try
            {
                _self = GCHandle.Alloc(this);
                IntPtr user = GCHandle.ToIntPtr(_self);
                XtNative.Check(XtNative.xt_surface_set_port_listener(h, s_portFn, user));
                XtNative.Check(XtNative.xt_surface_set_host_event_listener(h, s_hostFn, user));
                XtNative.Check(XtNative.xt_surface_set_timeline_listener(h, s_timelineFn, user));
                XtNative.Check(XtNative.xt_surface_set_visibility_listener(h, s_visibilityFn, user));
                CreateTexture();
                ReadPorts();
            }
            catch
            {
                DisposeNow();
                throw;
            }
        }

        // ---------------------------------------------------------------- frame

        /// <summary>Changes the output size: rebuilds the texture and resizes the native viewport.</summary>
        public void Resize(int width, int height)
        {
            ThrowIfDisposed();
            width = Math.Max(1, width);
            height = Math.Max(1, height);
            if (width == _width && height == _height)
                return;
            XtNative.Check(XtNative.xt_surface_resize(_handle, width, height, _dpr));
            _width = width;
            _height = height;
            DestroyTexture();
            CreateTexture();
        }

        /// <summary>Advances the UI by dt seconds; events are raised from inside this call.</summary>
        public void Update(float dt)
        {
            ThrowIfDisposed();
            XtStatus st = XtNative.xt_surface_update(_handle, dt);
            FlushDeferredDispose();
            XtNative.Check(st);
        }

        /// <summary>Renders into the texture; uploads with Apply only when pixels changed.
        /// Returns true when the texture was uploaded.</summary>
        public unsafe bool Render()
        {
            if (IsDisposed)
                return false;
            NativeArray<byte> data = _texture.GetRawTextureData<byte>();
            IntPtr ptr = (IntPtr)NativeArrayUnsafeUtility.GetUnsafePtr(data);
            XtNative.Check(XtNative.xt_surface_render(_handle, ptr, _width * 4, out XtDamage damage));
            if (damage.changed == 0)
                return false;
            _texture.Apply(false, false);
            return true;
        }

        // ---------------------------------------------------------------- input

        /// <summary>Pointer moved to (x, y) in logical pixels, top-left origin.</summary>
        public void PushPointerMove(float x, float y, byte modifiers = 0)
        {
            XtInputEvent e = NewEvent(XtInputKind.PointerMove);
            e.x = x;
            e.y = y;
            e.modifiers = modifiers;
            Push(ref e);
        }

        /// <summary>Pointer button (0 left, 1 right, 2 middle) pressed; clicks = host click count (0 unknown).</summary>
        public void PushPointerDown(float x, float y, int button, int clicks = 0, byte modifiers = 0)
        {
            XtInputEvent e = NewEvent(XtInputKind.PointerDown);
            e.x = x;
            e.y = y;
            e.button = button;
            e.clicks = (byte)Mathf.Clamp(clicks, 0, 255);
            e.modifiers = modifiers;
            Push(ref e);
        }

        /// <summary>Pointer button released.</summary>
        public void PushPointerUp(float x, float y, int button, int clicks = 0, byte modifiers = 0)
        {
            XtInputEvent e = NewEvent(XtInputKind.PointerUp);
            e.x = x;
            e.y = y;
            e.button = button;
            e.clicks = (byte)Mathf.Clamp(clicks, 0, 255);
            e.modifiers = modifiers;
            Push(ref e);
        }

        /// <summary>Wheel at (x, y); notches, positive y scrolls down.</summary>
        public void PushPointerWheel(float x, float y, float notchesX, float notchesY, byte modifiers = 0)
        {
            XtInputEvent e = NewEvent(XtInputKind.Wheel);
            e.x = x;
            e.y = y;
            e.wheel_x = notchesX;
            e.wheel_y = notchesY;
            e.modifiers = modifiers;
            Push(ref e);
        }

        /// <summary>The pointer left the surface.</summary>
        public void PushPointerLeave()
        {
            XtInputEvent e = NewEvent(XtInputKind.PointerLeave);
            Push(ref e);
        }

        /// <summary>The host took the pointer press away (xt_surface_cancel_pointer).</summary>
        public void PushPointerCancel()
        {
            ThrowIfDisposed();
            XtNative.xt_surface_cancel_pointer(_handle);
        }

        /// <summary>Key pressed or released.</summary>
        public void PushKey(XtKey key, bool down, byte modifiers, bool repeat)
        {
            XtInputEvent e = NewEvent(down ? XtInputKind.KeyDown : XtInputKind.KeyUp);
            e.key = (ushort)key;
            e.modifiers = modifiers;
            e.repeat = (byte)(repeat ? 1 : 0);
            Push(ref e);
        }

        /// <summary>Committed text of any length (the library splits it into code points).</summary>
        public void PushText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;
            XtInputEvent e = NewEvent(XtInputKind.TextInput);
            IntPtr utf8 = Marshal.StringToCoTaskMemUTF8(text);
            try
            {
                e.text = utf8;
                e.text_len = (uint)Encoding.UTF8.GetByteCount(text);
                Push(ref e);
            }
            finally
            {
                Marshal.FreeCoTaskMem(utf8);
            }
        }

        /// <summary>Cursor shape the UI asks for.</summary>
        public XtCursor Cursor => IsDisposed ? XtCursor.Default : (XtCursor)XtNative.xt_surface_cursor(_handle);

        /// <summary>True while a text field has focus.</summary>
        public bool WantsTextInput => !IsDisposed && XtNative.xt_surface_wants_text_input(_handle) != 0;

        /// <summary>The focused text field's caret rect in logical pixels.</summary>
        public bool TryGetTextInputRect(out Rect rect)
        {
            rect = default;
            if (IsDisposed)
                return false;
            if (XtNative.xt_surface_text_input_rect(_handle, out float x, out float y, out float w, out float h) !=
                XtStatus.Ok)
                return false;
            rect = new Rect(x, y, w, h);
            return true;
        }

        /// <summary>Input kind: 0 mouse, 1 gamepad, 2 touch.</summary>
        public void SetInputKind(int kind) => Call(XtNative.xt_surface_set_input_kind(Handle, kind));

        /// <summary>Overrides the ui scale; a value &lt;= 0 returns to the bundle spec rule.</summary>
        public void SetUiScale(float scale) => Call(XtNative.xt_surface_set_ui_scale(Handle, scale));

        /// <summary>The current ui scale.</summary>
        public float UiScale => IsDisposed ? 1f : XtNative.xt_surface_ui_scale(_handle);

        /// <summary>Safe area insets.</summary>
        public void SetSafeArea(float top, float right, float bottom, float left) =>
            Call(XtNative.xt_surface_set_safe_area(Handle, top, right, bottom, left));

        // ---------------------------------------------------------------- lifecycle / appearance

        /// <summary>Shows the UI (plays its intro).</summary>
        public void RequestShow() => Call(XtNative.xt_surface_request_show(Handle));

        /// <summary>Hides the UI (plays its outro).</summary>
        public void RequestHide() => Call(XtNative.xt_surface_request_hide(Handle));

        /// <summary>Visibility without intro/outro.</summary>
        public bool Visible
        {
            get => !IsDisposed && XtNative.xt_surface_is_visible(_handle) != 0;
            set => Call(XtNative.xt_surface_set_visible(Handle, value ? 1 : 0));
        }

        /// <summary>Selects a theme ("" or null clears).</summary>
        public void SetTheme(string name) => Call(XtNative.xt_surface_set_theme(Handle, name ?? string.Empty));

        /// <summary>Selects a language.</summary>
        public void SetLanguage(string lang) => Call(XtNative.xt_surface_set_language(Handle, lang ?? string.Empty));

        /// <summary>Posts a system event (kind, value) to the UI.</summary>
        public void PostSystemEvent(string kind, string value) =>
            Call(XtNative.xt_surface_post_system_event(Handle, kind ?? string.Empty, value ?? string.Empty));

        /// <summary>Reloads the UI after <see cref="XtBundle.Reload"/> reported it changed.</summary>
        public void HotReload()
        {
            Call(XtNative.xt_surface_hot_reload(Handle));
            if (IsDisposed)
                return;
            ReadPorts();
            HotReloaded?.Invoke();
        }

        // ---------------------------------------------------------------- timelines

        /// <summary>Plays a timeline (null options = library defaults).</summary>
        public void PlayTimeline(string scope, string name)
        {
            Call(XtNative.xt_surface_play_timeline(Handle, scope ?? string.Empty, name ?? string.Empty, IntPtr.Zero));
        }

        /// <summary>Plays a timeline with options.</summary>
        public unsafe void PlayTimeline(string scope, string name, XtPlayOptions options)
        {
            IntPtr marker = string.IsNullOrEmpty(options.FromMarker)
                                ? IntPtr.Zero
                                : Marshal.StringToCoTaskMemUTF8(options.FromMarker);
            try
            {
                var o = new XtPlayOptionsNative
                {
                    struct_size = s_playOptionsSize,
                    reverse = options.Reverse ? 1 : 0,
                    loop = options.Loop ? 1 : 0,
                    once = options.Once ? 1 : 0,
                    hold = options.Hold ? 1 : 0,
                    from_marker = marker,
                    rate = options.Rate,
                };
                Call(XtNative.xt_surface_play_timeline(Handle, scope ?? string.Empty, name ?? string.Empty, (IntPtr)(&o)));
            }
            finally
            {
                if (marker != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(marker);
            }
        }

        /// <summary>Stops a timeline.</summary>
        public void StopTimeline(string scope, string name) =>
            Call(XtNative.xt_surface_stop_timeline(Handle, scope ?? string.Empty, name ?? string.Empty));

        /// <summary>Seeks a timeline to t seconds.</summary>
        public void SeekTimeline(string scope, string name, float t) =>
            Call(XtNative.xt_surface_seek_timeline(Handle, scope ?? string.Empty, name ?? string.Empty, t));

        /// <summary>True while the timeline plays.</summary>
        public bool IsTimelinePlaying(string scope, string name) =>
            !IsDisposed &&
            XtNative.xt_surface_is_timeline_playing(_handle, scope ?? string.Empty, name ?? string.Empty) != 0;

        // ---------------------------------------------------------------- declared ports

        /// <summary>The declared ports (re-read after hot reload).</summary>
        public IReadOnlyList<XtPortRecord> Ports => _ports;

        /// <summary>Port id for a name, or 0 (XT_INVALID_PORT).</summary>
        public uint LookupPort(string name) =>
            IsDisposed || string.IsNullOrEmpty(name) ? XtNative.InvalidPort : XtNative.xt_surface_port_lookup(_handle, name);

        /// <summary>Sets a number port.</summary>
        public void SetPort(uint port, double value) => Call(XtNative.xt_surface_port_set_number(Handle, port, value));

        /// <summary>Sets a bool port.</summary>
        public void SetPort(uint port, bool value) => Call(XtNative.xt_surface_port_set_bool(Handle, port, value ? 1 : 0));

        /// <summary>Sets any port from text (struct / list ports take JSON).</summary>
        public void SetPort(uint port, string text) =>
            Call(XtNative.xt_surface_port_set_text(Handle, port, text ?? string.Empty));

        /// <summary>Fires an Event port.</summary>
        public void Pulse(uint port) => Call(XtNative.xt_surface_port_pulse(Handle, port));

        /// <summary>A number port's value.</summary>
        public double GetPortNumber(uint port) => IsDisposed ? 0.0 : XtNative.xt_surface_port_get_number(_handle, port);

        /// <summary>A bool port's value.</summary>
        public bool GetPortBool(uint port) => !IsDisposed && XtNative.xt_surface_port_get_bool(_handle, port) != 0;

        /// <summary>A port's value as text (copied).</summary>
        public string GetPortText(uint port) =>
            IsDisposed ? string.Empty : XtNative.Str(XtNative.xt_surface_port_get_text(_handle, port));

        // ---------------------------------------------------------------- connection points

        /// <summary>Sets a number connection point (manifest uis[].ports name).</summary>
        public void SetConnection(string port, double value, string param = null) =>
            Call(XtNative.xt_surface_connection_set_number(Handle, port, value, param));

        /// <summary>Sets a bool connection point.</summary>
        public void SetConnection(string port, bool value, string param = null) =>
            Call(XtNative.xt_surface_connection_set_bool(Handle, port, value ? 1 : 0, param));

        /// <summary>Sets a string connection point.</summary>
        public void SetConnection(string port, string text, string param = null) =>
            Call(XtNative.xt_surface_connection_set_text(Handle, port, text ?? string.Empty, param));

        /// <summary>Fires a pulse connection point.</summary>
        public void PulseConnection(string port, string param = null) =>
            Call(XtNative.xt_surface_connection_pulse(Handle, port, param));

        /// <summary>Watches an output connection point; events arrive as <see cref="XtPortEventKind.Connection"/>.</summary>
        public uint WatchConnection(string port)
        {
            XtStatus st = XtNative.xt_surface_connection_watch(Handle, port, out uint id);
            XtNative.Check(st);
            return id;
        }

        /// <summary>Stops a watch.</summary>
        public void UnwatchConnection(uint listenerId)
        {
            if (!IsDisposed)
                XtNative.xt_surface_connection_unwatch(_handle, listenerId);
        }

        // ---------------------------------------------------------------- anchor / perf

        /// <summary>Sets a world anchor port's projected position.</summary>
        public void SetWorldAnchor(uint port, float x, float y, float depth, bool onScreen) =>
            Call(XtNative.xt_surface_set_world_anchor(Handle, port, x, y, depth, onScreen ? 1 : 0));

        /// <summary>Turns frame timing collection on or off (off by default).</summary>
        public void SetPerfEnabled(bool enabled)
        {
            if (!IsDisposed)
                XtNative.xt_surface_set_perf_enabled(_handle, enabled ? 1 : 0);
        }

        /// <summary>Last frame's timings.</summary>
        public XtFrameStats FrameStats
        {
            get
            {
                if (IsDisposed)
                    return default;
                XtNative.Check(XtNative.xt_surface_frame_stats(_handle, out XtFrameStats s));
                return s;
            }
        }

        // ---------------------------------------------------------------- dispose

        /// <summary>Destroys the native surface, the texture and the bundle reference. Inside one
        /// of this surface's events the destruction is deferred until the native call returns.</summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;
            if (_callbackDepth > 0)
            {
                _disposeRequested = true;
                return;
            }
            DisposeNow();
        }

        private void DisposeNow()
        {
            _disposeRequested = false;
            if (_handle != IntPtr.Zero)
            {
                XtNative.xt_surface_destroy(_handle);
                _handle = IntPtr.Zero;
            }
            if (_self.IsAllocated)
                _self.Free();
            DestroyTexture();
            _ports.Clear();
            if (_bundle != null)
            {
                _bundle.Release();
                _bundle = null;
            }
            PortEvent = null;
            HostEvent = null;
            TimelineEvent = null;
            VisibilityChanged = null;
            HotReloaded = null;
        }

        private void FlushDeferredDispose()
        {
            if (_disposeRequested && _callbackDepth == 0)
                DisposeNow();
        }

        // ---------------------------------------------------------------- internals

        private IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return _handle;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(XtSurface));
        }

        // Every status-returning call that may raise callbacks goes through here: a Dispose
        // requested from one of those callbacks runs once the native call has returned.
        private void Call(XtStatus status)
        {
            FlushDeferredDispose();
            XtNative.Check(status);
        }

        private static XtInputEvent NewEvent(XtInputKind kind)
        {
            return new XtInputEvent { struct_size = s_inputEventSize, kind = (int)kind, gesture_scale = 1f };
        }

        private void Push(ref XtInputEvent e)
        {
            Call(XtNative.xt_surface_push_input(Handle, ref e));
        }

        private void CreateTexture()
        {
            _texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false, false)
            {
                name = "XTFrame " + UiId,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave,
            };
        }

        private void DestroyTexture()
        {
            if (_texture == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_texture);
            else
                UnityEngine.Object.DestroyImmediate(_texture);
            _texture = null;
        }

        private void ReadPorts()
        {
            _ports.Clear();
            uint n = XtNative.xt_surface_port_count(_handle);
            for (uint i = 0; i < n; ++i)
            {
                XtNative.Check(XtNative.xt_surface_port_record(_handle, i, out XtPortRecordNative r));
                _ports.Add(new XtPortRecord(r.id, XtNative.Str(r.name), XtNative.Str(r.qualified_name),
                                            XtNative.Str(r.element_id), (XtPortValueType)r.value_type,
                                            (XtPortDir)r.dir, r.document_level != 0));
            }
        }
    }
}
