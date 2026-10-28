using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XTFrame;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// QuickStart: put this on an empty GameObject, fill in the bundle path (relative to
/// StreamingAssets) and the UI id, press Play. It builds a Screen Space Overlay Canvas with a
/// full-screen RawImage + XtSurfaceComponent, and an EventSystem when the scene has none.
/// </summary>
public sealed class XtQuickStart : MonoBehaviour
{
    [Tooltip("Bundle directory relative to StreamingAssets, e.g. game-ui.")]
    public string bundlePath = "game-ui";

    [Tooltip("UI id inside the bundle, e.g. hud.")]
    public string uiId = "hud";

    [Tooltip("Send the keyboard to the UI even without a focused text field.")]
    public bool keyboardFocus = true;

    [Tooltip("Editor only: hot reload poll interval in seconds (0 = off).")]
    public float hotReloadSeconds = 2f;

    /// <summary>The created component (after Start).</summary>
    public XtSurfaceComponent Surface { get; private set; }

    private void Start()
    {
        var canvasGo = new GameObject("XTFrame Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Inactive while configuring: XtSurfaceComponent opens its surface in OnEnable.
        var surfaceGo = new GameObject("XT Surface", typeof(RectTransform));
        surfaceGo.SetActive(false);
        surfaceGo.transform.SetParent(canvasGo.transform, false);
        var rt = (RectTransform)surfaceGo.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        surfaceGo.AddComponent<RawImage>();
        Surface = surfaceGo.AddComponent<XtSurfaceComponent>();
        Surface.BundlePath = bundlePath;
        Surface.UiId = uiId;
        Surface.KeyboardFocus = keyboardFocus;
        Surface.HotReloadSeconds = hotReloadSeconds;
        Surface.SurfaceCreated += s => Debug.Log("[XTFrame] QuickStart: '" + s.UiId + "' is running at " +
                                                 s.Width + " x " + s.Height + ".");
        surfaceGo.SetActive(true);

        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
