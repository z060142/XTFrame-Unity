using System;
using System.Collections.Generic;
using System.IO;
using AOT;
using UnityEngine;

namespace XTFrame
{
    /// <summary>One connection point of a UI (manifest uis[].ports row).</summary>
    public sealed class XtPortInfo
    {
        /// <summary>Connection point name, e.g. "#fire.Click" or "document.clock".</summary>
        public string Name { get; }
        /// <summary>pulse | bool | number | string.</summary>
        public string Type { get; }
        /// <summary>True for an input (host to UI) connection point.</summary>
        public bool IsInput { get; }

        internal XtPortInfo(string name, string type, bool isInput)
        {
            Name = name;
            Type = type;
            IsInput = isInput;
        }
    }

    /// <summary>One UI of a bundle (xt_ui_info), copied into managed memory when the bundle opens.</summary>
    public sealed class XtUiInfo
    {
        /// <summary>UI id.</summary>
        public string Id { get; }
        /// <summary>Display name.</summary>
        public string Name { get; }
        /// <summary>Design width in logical pixels.</summary>
        public float DesignWidth { get; }
        /// <summary>Design height in logical pixels.</summary>
        public float DesignHeight { get; }
        /// <summary>Manifest fit: native | stretch | contain | cover | center.</summary>
        public string Fit { get; }
        /// <summary>True when the UI has a flow.</summary>
        public bool HasFlow { get; }
        /// <summary>Contract hash (may be "").</summary>
        public string Contract { get; }
        /// <summary>The UI's connection points.</summary>
        public IReadOnlyList<XtPortInfo> Ports { get; }

        internal XtUiInfo(string id, string name, float w, float h, string fit, bool hasFlow, string contract,
                          IReadOnlyList<XtPortInfo> ports)
        {
            Id = id;
            Name = name;
            DesignWidth = w;
            DesignHeight = h;
            Fit = fit;
            HasFlow = hasFlow;
            Contract = contract;
            Ports = ports;
        }
    }

    /// <summary>
    /// A read-only XTFrame bundle directory (xt_bundle). Bundles are shared per absolute path and
    /// reference counted: <see cref="Acquire"/> adds a reference, <see cref="Release"/> drops one,
    /// and the native bundle closes when the last reference goes. Main thread only.
    /// </summary>
    public sealed class XtBundle : IDisposable
    {
        private static readonly Dictionary<string, XtBundle> s_cache = new Dictionary<string, XtBundle>();
        private static readonly XtNative.LogFn s_logFn = OnNativeLog; // kept alive for the native side
        private static bool s_abiChecked;
        private static bool s_hooksInstalled;
        private static readonly string[] s_noIds = new string[0];

        private IntPtr _handle;
        private int _refCount;
        private readonly string _key;
        private List<XtUiInfo> _uis = new List<XtUiInfo>();
        private XtBundleSpec _spec;

        /// <summary>Raised by <see cref="Reload"/> with the ids of the UIs that changed.</summary>
        public event Action<string[]> Reloaded;

        /// <summary>Bundle id from the manifest.</summary>
        public string Id { get; private set; }
        /// <summary>Bundle display name.</summary>
        public string Name { get; private set; }
        /// <summary>Bundle directory as the library sees it.</summary>
        public string Directory { get; private set; }
        /// <summary>The bundle's UIs (copied at open and after each reload).</summary>
        public IReadOnlyList<XtUiInfo> Uis => _uis;
        /// <summary>The bundle spec (defaults when the bundle has none).</summary>
        public XtBundleSpec Spec => _spec;
        /// <summary>True while the native bundle is open.</summary>
        public bool IsOpen => _handle != IntPtr.Zero;

        internal IntPtr Handle => _handle;

        private XtBundle(string key, IntPtr handle)
        {
            _key = key;
            _handle = handle;
            Refresh();
        }

        /// <summary>
        /// Opens (or shares) the bundle at an absolute directory and adds a reference. The first call
        /// checks the native ABI version. Throws DllNotFoundException when the native library is
        /// missing and <see cref="XtException"/> on ABI mismatch or open failure.
        /// </summary>
        public static XtBundle Acquire(string absoluteDir)
        {
            if (string.IsNullOrEmpty(absoluteDir))
                throw new XtException(XtStatus.InvalidArgument, "bundle directory is empty");
            if (!s_abiChecked)
            {
                XtNative.CheckAbi();
                s_abiChecked = true;
                XtNative.xt_capi_set_log(s_logFn, IntPtr.Zero);
                if (!s_hooksInstalled)
                {
                    s_hooksInstalled = true;
#if UNITY_EDITOR
                    // Play-mode exit is not a shutdown in the editor; a domain reload is.
                    UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += ShutdownAll;
#else
                    Application.quitting += ShutdownAll;
#endif
                }
            }

            string key = Path.GetFullPath(absoluteDir);
            if (s_cache.TryGetValue(key, out XtBundle cached) && cached.IsOpen)
            {
                cached._refCount++;
                return cached;
            }

            IntPtr h = XtNative.xt_bundle_open(key);
            if (h == IntPtr.Zero)
                throw new XtException(XtStatus.IoError, XtNative.LastError());
            var bundle = new XtBundle(key, h) { _refCount = 1 };
            s_cache[key] = bundle;
            return bundle;
        }

        /// <summary>Adds a reference (used by each <see cref="XtSurface"/>).</summary>
        internal void Retain()
        {
            if (!IsOpen)
                throw new ObjectDisposedException(nameof(XtBundle));
            _refCount++;
        }

        /// <summary>Drops one reference; the last one closes the native bundle.</summary>
        public void Release()
        {
            if (!IsOpen || _refCount <= 0)
                return;
            if (--_refCount > 0)
                return;
            Close();
        }

        /// <summary>Same as <see cref="Release"/>.</summary>
        public void Dispose()
        {
            Release();
        }

        /// <summary>Index of a UI id, or -1.</summary>
        public int FindUi(string uiId)
        {
            if (!IsOpen || uiId == null)
                return -1;
            return XtNative.xt_bundle_find_ui(_handle, uiId);
        }

        /// <summary>The managed info for a UI id, or null.</summary>
        public XtUiInfo GetUi(string uiId)
        {
            for (int i = 0; i < _uis.Count; ++i)
                if (_uis[i].Id == uiId)
                    return _uis[i];
            return null;
        }

        /// <summary>
        /// Re-reads the manifest and returns the ids of the UIs that changed; each affected
        /// <see cref="XtSurface"/> should then call <see cref="XtSurface.HotReload"/>. Also raises
        /// <see cref="Reloaded"/> so every surface sharing this bundle hears about it.
        /// </summary>
        public string[] Reload()
        {
            if (!IsOpen)
                throw new ObjectDisposedException(nameof(XtBundle));
            uint n = XtNative.xt_bundle_reload(_handle);
            string[] ids = n == 0 ? s_noIds : new string[n];
            for (uint i = 0; i < n; ++i)
                ids[i] = XtNative.Str(XtNative.xt_bundle_changed_ui_id(_handle, i));
            Refresh();
            if (n > 0)
                Reloaded?.Invoke(ids);
            return ids;
        }

        private void Refresh()
        {
            Id = XtNative.Str(XtNative.xt_bundle_id(_handle));
            Name = XtNative.Str(XtNative.xt_bundle_name(_handle));
            Directory = XtNative.Str(XtNative.xt_bundle_dir(_handle));
            XtNative.Check(XtNative.xt_bundle_get_spec(_handle, out _spec));

            uint count = XtNative.xt_bundle_ui_count(_handle);
            var uis = new List<XtUiInfo>((int)count);
            for (uint i = 0; i < count; ++i)
            {
                XtNative.Check(XtNative.xt_bundle_ui_info(_handle, i, out XtUiInfoNative info));
                // Copy every string of `info` before the next call on the bundle invalidates them.
                string id = XtNative.Str(info.id);
                string name = XtNative.Str(info.name);
                string fit = XtNative.Str(info.fit);
                string contract = XtNative.Str(info.contract);
                var ports = new List<XtPortInfo>((int)info.port_count);
                for (uint p = 0; p < info.port_count; ++p)
                {
                    XtNative.Check(XtNative.xt_bundle_ui_port_info(_handle, i, p, out XtPortInfoNative pi));
                    ports.Add(new XtPortInfo(XtNative.Str(pi.name), XtNative.Str(pi.type), pi.is_input != 0));
                }
                uis.Add(new XtUiInfo(id, name, info.design_width, info.design_height, fit, info.has_flow != 0,
                                     contract, ports));
            }
            _uis = uis;
        }

        private void Close()
        {
            XtStatus st = XtNative.xt_bundle_close(_handle);
            if (st != XtStatus.Ok)
            {
                // Surfaces are still open on it (should not happen: each surface holds a reference).
                Debug.LogError("[XTFrame] xt_bundle_close failed: " + st + " " + XtNative.LastError());
                _refCount = 1;
                return;
            }
            _handle = IntPtr.Zero;
            _refCount = 0;
            if (s_cache.TryGetValue(_key, out XtBundle b) && b == this)
                s_cache.Remove(_key);
        }

        // Called before a domain reload or at quit: the native log hook must not keep a pointer to
        // a delegate of an unloaded domain. Bundles still open are closed best effort.
        private static void ShutdownAll()
        {
            if (!s_abiChecked)
                return;
            try
            {
                XtNative.xt_capi_set_log(null, IntPtr.Zero);
                var open = new List<XtBundle>(s_cache.Values);
                foreach (XtBundle b in open)
                    if (b.IsOpen && XtNative.xt_bundle_close(b._handle) == XtStatus.Ok)
                        b._handle = IntPtr.Zero;
                s_cache.Clear();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            s_abiChecked = false;
        }

        [MonoPInvokeCallback(typeof(XtNative.LogFn))]
        private static void OnNativeLog(int level, IntPtr message, IntPtr user)
        {
            try
            {
                string text = "[XTFrame] " + XtNative.Str(message);
                if (level >= 2)
                    Debug.LogError(text);
                else if (level == 1)
                    Debug.LogWarning(text);
                else
                    Debug.Log(text);
            }
            catch
            {
                // Never let a managed exception unwind into native code.
            }
        }
    }
}
