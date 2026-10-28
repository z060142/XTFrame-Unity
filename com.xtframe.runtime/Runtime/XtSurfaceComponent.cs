using System;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XTFrame
{
    /// <summary>How the RawImage rect follows the UI's design size.</summary>
    public enum XtFit
    {
        /// <summary>Use the manifest's fit (contain / cover / stretch; anything else = native).</summary>
        Auto,
        /// <summary>Leave the rect alone.</summary>
        Native,
        /// <summary>Leave the rect alone (the UI's uiscale rule handles it).</summary>
        Stretch,
        /// <summary>AspectRatioFitter FitInParent with the design aspect.</summary>
        Contain,
        /// <summary>AspectRatioFitter EnvelopeParent with the design aspect.</summary>
        Cover,
    }

    /// <summary>
    /// Shows one XTFrame UI of a bundle in StreamingAssets on a uGUI RawImage: creates the surface
    /// on enable, updates and renders it every frame, forwards uGUI pointer events and (when
    /// focused) the keyboard, and destroys it on disable.
    /// </summary>
    [AddComponentMenu("XTFrame/XT Surface")]
    [RequireComponent(typeof(RawImage))]
    [DisallowMultipleComponent]
    public sealed class XtSurfaceComponent : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler,
                                             IScrollHandler, IPointerExitHandler
    {
        // Stage 1 renders at device pixel ratio 1: logical pixels = surface pixels.
        private const float kDevicePixelRatio = 1f;

        [Tooltip("Bundle directory relative to StreamingAssets (an absolute path also works).")]
        [SerializeField] private string bundlePath = string.Empty;
        [Tooltip("UI id inside the bundle.")]
        [SerializeField] private string uiId = string.Empty;
        [SerializeField] private XtFit fit = XtFit.Auto;
        [Tooltip("Load and run the UI flow and the bundle flow.")]
        [SerializeField] private bool runFlow = true;
        [Tooltip("Let the text shaper add the system fonts.")]
        [SerializeField] private bool systemFonts;
        [SerializeField] private uint instanceId;
        [Tooltip("Editor only: poll the bundle for changes every N seconds while playing (0 = off).")]
        [SerializeField] private float hotReloadSeconds;
        [Tooltip("Send keyboard input even when no text field of the UI has focus.")]
        [SerializeField] private bool keyboardFocus;
        [SerializeField] private bool useUnscaledTime = true;
        [Tooltip("Multiplier from PointerEventData.scrollDelta to wheel notches (1 notch = 1 line). StandaloneInputModule reports lines (keep 1); InputSystemUIInputModule may report raw units (~120 per notch): set 1/120.")]
        [SerializeField] private float wheelScale = 1f;

        private RawImage _rawImage;
        private RectTransform _rect;
        private Canvas _canvas;
        private XtBundle _bundle;
        private XtSurface _surface;
        private readonly KeyboardPoller _keyboard = new KeyboardPoller();
        private bool _resizePending;
        private float _reloadTimer;

        /// <summary>The live surface (null while disabled).</summary>
        public XtSurface Surface => _surface;

        /// <summary>The bundle the surface comes from (null while disabled).</summary>
        public XtBundle Bundle => _bundle;

        /// <summary>Raised from OnEnable once the surface exists.</summary>
        public event Action<XtSurface> SurfaceCreated;

        /// <summary>Bundle directory relative to StreamingAssets (takes effect on the next enable).</summary>
        public string BundlePath { get => bundlePath; set => bundlePath = value ?? string.Empty; }
        /// <summary>UI id (takes effect on the next enable).</summary>
        public string UiId { get => uiId; set => uiId = value ?? string.Empty; }
        /// <summary>Fit mode (takes effect on the next enable).</summary>
        public XtFit Fit { get => fit; set => fit = value; }
        /// <summary>Run the flow (takes effect on the next enable).</summary>
        public bool RunFlow { get => runFlow; set => runFlow = value; }
        /// <summary>Use system fonts (takes effect on the next enable).</summary>
        public bool SystemFonts { get => systemFonts; set => systemFonts = value; }
        /// <summary>Instance id (takes effect on the next enable).</summary>
        public uint InstanceId { get => instanceId; set => instanceId = value; }
        /// <summary>Editor hot reload poll interval in seconds (0 = off).</summary>
        public float HotReloadSeconds { get => hotReloadSeconds; set => hotReloadSeconds = value; }
        /// <summary>Send keyboard input even without a focused text field.</summary>
        public bool KeyboardFocus { get => keyboardFocus; set => keyboardFocus = value; }
        /// <summary>Advance with Time.unscaledDeltaTime instead of Time.deltaTime.</summary>
        public bool UseUnscaledTime { get => useUnscaledTime; set => useUnscaledTime = value; }
        /// <summary>Multiplier applied to scrollDelta before it becomes wheel notches.</summary>
        public float WheelScale { get => wheelScale; set => wheelScale = value; }

        /// <summary>Absolute bundle directory for a StreamingAssets-relative path.</summary>
        public static string ResolveBundlePath(string relativePath)
        {
            return Path.Combine(Application.streamingAssetsPath, relativePath ?? string.Empty);
        }

        /// <summary>Re-reads the bundle and reloads this UI even when the manifest reports no change.</summary>
        public void HotReloadNow()
        {
            if (_bundle == null || _surface == null)
                return;
            try
            {
                string[] ids = _bundle.Reload();
                if (_surface != null && !_surface.IsDisposed && Array.IndexOf(ids, uiId) < 0)
                    _surface.HotReload();
            }
            catch (Exception e)
            {
                Fail(e);
            }
        }

        // ------------------------------------------------------------------ lifecycle

        private void OnEnable()
        {
            _rawImage = GetComponent<RawImage>();
            _rect = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas == null)
            {
                Fail("XtSurfaceComponent needs a Canvas parent.");
                return;
            }
            if (string.IsNullOrEmpty(uiId))
            {
                Fail("XtSurfaceComponent: no UI id set.");
                return;
            }
            try
            {
                _bundle = XtBundle.Acquire(ResolveBundlePath(bundlePath));
                XtUiInfo ui = _bundle.GetUi(uiId);
                if (ui == null)
                {
                    Fail("XtSurfaceComponent: UI '" + uiId + "' is not in bundle '" + bundlePath + "'.");
                    return;
                }
                ApplyFit(ui);
                Vector2Int size = ComputeSize();
                var options = new XtSurfaceOptions
                {
                    DevicePixelRatio = kDevicePixelRatio,
                    Premultiplied = false,
                    RunFlow = runFlow,
                    SystemFonts = systemFonts,
                    InstanceId = instanceId,
                };
                _surface = new XtSurface(_bundle, uiId, size.x, size.y, options);
                _rawImage.texture = _surface.Texture;
                // The software buffer runs top to bottom, Unity textures bottom to top.
                _rawImage.uvRect = new Rect(0f, 1f, 1f, -1f);
                _bundle.Reloaded += OnBundleReloaded;
                _keyboard.Enable();
                _resizePending = false;
                _reloadTimer = 0f;
            }
            catch (DllNotFoundException e)
            {
                Fail("XTFrame native library not found (put xtframe into Runtime/Plugins, see the package README): " +
                     e.Message);
                return;
            }
            catch (EntryPointNotFoundException e)
            {
                Fail("XTFrame native library is too old for this package: " + e.Message);
                return;
            }
            catch (Exception e)
            {
                Fail(e);
                return;
            }
            SurfaceCreated?.Invoke(_surface);
        }

        private void OnDisable()
        {
            Teardown();
        }

        private void OnRectTransformDimensionsChange()
        {
            _resizePending = true;
        }

        private void OnCanvasHierarchyChanged()
        {
            _canvas = GetComponentInParent<Canvas>();
            _resizePending = true;
        }

        private void Update()
        {
            XtSurface s = _surface;
            if (s == null)
                return;
            try
            {
                if (_resizePending)
                {
                    _resizePending = false;
                    Vector2Int size = ComputeSize();
                    if (size.x != s.Width || size.y != s.Height)
                    {
                        s.Resize(size.x, size.y);
                        _rawImage.texture = s.Texture;
                    }
                }

                if (keyboardFocus || s.WantsTextInput)
                    _keyboard.Poll(s);
                else
                    _keyboard.TextThisFrame(); // drop text typed while unfocused
                if (_surface != s || s.IsDisposed)
                    return;

                float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                s.Update(dt);
                if (_surface != s || s.IsDisposed)
                    return; // disabled or disposed from one of the surface's events
                s.Render();

                if (hotReloadSeconds > 0f && Application.isEditor)
                {
                    _reloadTimer += Time.unscaledDeltaTime;
                    if (_reloadTimer >= hotReloadSeconds)
                    {
                        _reloadTimer = 0f;
                        _bundle.Reload(); // OnBundleReloaded handles this UI
                    }
                }
            }
            catch (Exception e)
            {
                Fail(e);
            }
        }

        private void OnBundleReloaded(string[] changedIds)
        {
            if (_surface == null || _surface.IsDisposed || Array.IndexOf(changedIds, uiId) < 0)
                return;
            try
            {
                _surface.HotReload();
                XtUiInfo ui = _bundle.GetUi(uiId);
                if (ui != null)
                    ApplyFit(ui);
            }
            catch (Exception e)
            {
                Fail(e);
            }
        }

        private void Teardown()
        {
            _keyboard.Disable();
            if (_bundle != null)
                _bundle.Reloaded -= OnBundleReloaded;
            if (_surface != null)
            {
                _surface.Dispose(); // deferred by XtSurface when called from one of its events
                _surface = null;
            }
            if (_rawImage != null)
                _rawImage.texture = null;
            if (_bundle != null)
            {
                _bundle.Release();
                _bundle = null;
            }
        }

        // Logs once, tears down and disables the component (no per-frame exceptions).
        private void Fail(string message)
        {
            Debug.LogError("[XTFrame] " + message, this);
            Teardown();
            enabled = false;
        }

        private void Fail(Exception e)
        {
            Fail(e.Message);
        }

        // ------------------------------------------------------------------ layout

        private XtFit EffectiveFit(XtUiInfo ui)
        {
            if (fit != XtFit.Auto)
                return fit;
            switch (ui.Fit)
            {
                case "contain": return XtFit.Contain;
                case "cover": return XtFit.Cover;
                case "stretch": return XtFit.Stretch;
                default: return XtFit.Native; // "native", "center", unknown
            }
        }

        private void ApplyFit(XtUiInfo ui)
        {
            XtFit f = EffectiveFit(ui);
            var fitter = GetComponent<AspectRatioFitter>();
            if (f == XtFit.Contain || f == XtFit.Cover)
            {
                if (fitter == null)
                    fitter = gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = f == XtFit.Contain ? AspectRatioFitter.AspectMode.FitInParent
                                                       : AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = ui.DesignHeight > 0f ? ui.DesignWidth / ui.DesignHeight : 1f;
            }
            else if (fitter != null)
            {
                Destroy(fitter);
            }
        }

        // Rect size in device pixels, rounded, at least 1 x 1.
        private Vector2Int ComputeSize()
        {
            Rect r = _rect.rect;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            return new Vector2Int(Mathf.Max(1, Mathf.RoundToInt(r.width * scale)),
                                  Mathf.Max(1, Mathf.RoundToInt(r.height * scale)));
        }

        // ------------------------------------------------------------------ pointer

        // Screen point -> logical pixels inside the rect, top-left origin.
        private bool ToLogical(PointerEventData eventData, out float x, out float y)
        {
            x = y = 0f;
            Camera cam = null;
            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = eventData.pressEventCamera;
                if (cam == null)
                    cam = eventData.enterEventCamera;
                if (cam == null)
                    cam = _canvas.worldCamera;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, cam,
                                                                         out Vector2 local))
                return false;
            Rect r = _rect.rect;
            // Rect units -> surface pixels (the canvas scale factor), then pixels -> logical.
            float sx = r.width > 0f ? _surface.Width / r.width : 1f;
            float sy = r.height > 0f ? _surface.Height / r.height : 1f;
            x = (local.x - r.xMin) * sx / kDevicePixelRatio;
            y = (r.yMax - local.y) * sy / kDevicePixelRatio;
            return true;
        }

        private static int ButtonIndex(PointerEventData.InputButton b)
        {
            switch (b)
            {
                case PointerEventData.InputButton.Right: return 1;
                case PointerEventData.InputButton.Middle: return 2;
                default: return 0;
            }
        }

        private bool Live => _surface != null && !_surface.IsDisposed;

        /// <summary>uGUI pointer move.</summary>
        public void OnPointerMove(PointerEventData eventData)
        {
            if (!Live || !ToLogical(eventData, out float x, out float y))
                return;
            try { _surface.PushPointerMove(x, y, XtInput.Modifiers()); }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>uGUI pointer down.</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Live || !ToLogical(eventData, out float x, out float y))
                return;
            try { _surface.PushPointerDown(x, y, ButtonIndex(eventData.button), eventData.clickCount, XtInput.Modifiers()); }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>uGUI pointer up.</summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Live || !ToLogical(eventData, out float x, out float y))
                return;
            try { _surface.PushPointerUp(x, y, ButtonIndex(eventData.button), eventData.clickCount, XtInput.Modifiers()); }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>uGUI scroll.</summary>
        public void OnScroll(PointerEventData eventData)
        {
            if (!Live || !ToLogical(eventData, out float x, out float y))
                return;
            Vector2 d = eventData.scrollDelta * wheelScale;
            try { _surface.PushPointerWheel(x, y, d.x, XtInput.WheelNotches(d), XtInput.Modifiers()); }
            catch (Exception e) { Fail(e); }
        }

        /// <summary>uGUI pointer exit.</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!Live)
                return;
            try { _surface.PushPointerLeave(); }
            catch (Exception e) { Fail(e); }
        }
    }
}
