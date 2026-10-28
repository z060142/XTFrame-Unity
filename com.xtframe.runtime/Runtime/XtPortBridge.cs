using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace XTFrame
{
    /// <summary>Which UnityEvent of a <see cref="XtPortBridge.Binding"/> fires.</summary>
    public enum XtBindingKind
    {
        /// <summary>onNumber.</summary>
        Number,
        /// <summary>onBool.</summary>
        Bool,
        /// <summary>onString.</summary>
        String,
        /// <summary>onPulse.</summary>
        Pulse,
    }

    /// <summary>
    /// Wires XTFrame ports to UnityEvents in the Inspector, and offers SetNumber / SetBool /
    /// SetString / Pulse for scripts, Animator and Timeline. A name starting with '#' or
    /// "document." is a connection point (manifest uis[].ports); any other name is a declared port.
    /// </summary>
    [AddComponentMenu("XTFrame/XT Port Bridge")]
    [RequireComponent(typeof(XtSurfaceComponent))]
    public sealed class XtPortBridge : MonoBehaviour
    {
        /// <summary>One port -> UnityEvent wire.</summary>
        [Serializable]
        public class Binding
        {
            /// <summary>Port or connection point name.</summary>
            public string port = string.Empty;
            /// <summary>Which event fires.</summary>
            public XtBindingKind kind = XtBindingKind.Pulse;
            /// <summary>Fired with the number value.</summary>
            public UnityEvent<double> onNumber = new UnityEvent<double>();
            /// <summary>Fired with the bool value.</summary>
            public UnityEvent<bool> onBool = new UnityEvent<bool>();
            /// <summary>Fired with the text value.</summary>
            public UnityEvent<string> onString = new UnityEvent<string>();
            /// <summary>Fired on any event of the port.</summary>
            public UnityEvent onPulse = new UnityEvent();

            [NonSerialized] internal uint portId;
            [NonSerialized] internal uint listenerId;
        }

        [SerializeField] private List<Binding> bindings = new List<Binding>();

        private XtSurfaceComponent _component;
        private XtSurface _surface;
        private readonly Dictionary<string, uint> _portIds = new Dictionary<string, uint>();
        private XtSurface _cacheOwner; // _portIds belong to this surface

        /// <summary>The Inspector bindings.</summary>
        public List<Binding> Bindings => bindings;

        /// <summary>True for a connection point name ('#element.Port' or 'document.name').</summary>
        public static bool IsConnectionName(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                   (name[0] == '#' || name.StartsWith("document.", StringComparison.Ordinal));
        }

        private void OnEnable()
        {
            _component = GetComponent<XtSurfaceComponent>();
            _component.SurfaceCreated += Bind;
            if (_component.Surface != null && !_component.Surface.IsDisposed)
                Bind(_component.Surface);
        }

        private void OnDisable()
        {
            if (_component != null)
                _component.SurfaceCreated -= Bind;
            Unbind();
        }

        private void Bind(XtSurface surface)
        {
            Unbind();
            _surface = surface;
            _surface.PortEvent += OnPortEvent;
            _surface.HotReloaded += OnHotReloaded;
            foreach (Binding b in bindings)
            {
                b.portId = 0;
                b.listenerId = 0;
                if (string.IsNullOrEmpty(b.port))
                    continue;
                try
                {
                    if (IsConnectionName(b.port))
                        b.listenerId = _surface.WatchConnection(b.port);
                    else
                    {
                        b.portId = _surface.LookupPort(b.port);
                        if (b.portId == 0)
                            Debug.LogWarning("[XTFrame] XtPortBridge: no declared port '" + b.port + "'.", this);
                    }
                }
                catch (XtException e)
                {
                    Debug.LogWarning("[XTFrame] XtPortBridge: cannot watch '" + b.port + "': " + e.Message, this);
                }
            }
        }

        private void Unbind()
        {
            if (_surface != null)
            {
                _surface.PortEvent -= OnPortEvent;
                _surface.HotReloaded -= OnHotReloaded;
                if (!_surface.IsDisposed)
                    foreach (Binding b in bindings)
                        if (b.listenerId != 0)
                            _surface.UnwatchConnection(b.listenerId);
            }
            foreach (Binding b in bindings)
            {
                b.portId = 0;
                b.listenerId = 0;
            }
            _portIds.Clear();
            _surface = null;
        }

        // Declared port ids may change when the UI is reloaded; connection watches stay.
        private void OnHotReloaded()
        {
            _portIds.Clear();
            if (_surface == null)
                return;
            foreach (Binding b in bindings)
                if (!string.IsNullOrEmpty(b.port) && !IsConnectionName(b.port))
                    b.portId = _surface.LookupPort(b.port);
        }

        private void OnPortEvent(XtPortEvent e)
        {
            for (int i = 0; i < bindings.Count; ++i)
            {
                Binding b = bindings[i];
                bool match = e.Kind == XtPortEventKind.Connection
                                 ? b.listenerId != 0 && b.listenerId == e.ListenerId
                                 : b.portId != 0 && b.portId == e.Port;
                if (!match)
                    continue;
                switch (b.kind)
                {
                    case XtBindingKind.Number: b.onNumber.Invoke(e.Number); break;
                    case XtBindingKind.Bool: b.onBool.Invoke(e.Boolean); break;
                    case XtBindingKind.String: b.onString.Invoke(e.Text); break;
                    default: b.onPulse.Invoke(); break;
                }
            }
        }

        // ------------------------------------------------------------------ host -> UI

        /// <summary>Sets a number on a declared port or a connection point.</summary>
        public void SetNumber(string port, double value)
        {
            XtSurface s = LiveSurface();
            if (s == null)
                return;
            try
            {
                if (IsConnectionName(port))
                    s.SetConnection(port, value);
                else if (TryPortId(s, port, out uint id))
                    s.SetPort(id, value);
            }
            catch (XtException e)
            {
                Warn(port, e);
            }
        }

        /// <summary>Sets a bool on a declared port or a connection point.</summary>
        public void SetBool(string port, bool value)
        {
            XtSurface s = LiveSurface();
            if (s == null)
                return;
            try
            {
                if (IsConnectionName(port))
                    s.SetConnection(port, value);
                else if (TryPortId(s, port, out uint id))
                    s.SetPort(id, value);
            }
            catch (XtException e)
            {
                Warn(port, e);
            }
        }

        /// <summary>Sets text on a declared port (any type; struct / list take JSON) or a connection point.</summary>
        public void SetString(string port, string value)
        {
            XtSurface s = LiveSurface();
            if (s == null)
                return;
            try
            {
                if (IsConnectionName(port))
                    s.SetConnection(port, value ?? string.Empty);
                else if (TryPortId(s, port, out uint id))
                    s.SetPort(id, value ?? string.Empty);
            }
            catch (XtException e)
            {
                Warn(port, e);
            }
        }

        /// <summary>Fires an Event port or a pulse connection point.</summary>
        public void Pulse(string port)
        {
            XtSurface s = LiveSurface();
            if (s == null)
                return;
            try
            {
                if (IsConnectionName(port))
                    s.PulseConnection(port);
                else if (TryPortId(s, port, out uint id))
                    s.Pulse(id);
            }
            catch (XtException e)
            {
                Warn(port, e);
            }
        }

        private XtSurface LiveSurface()
        {
            if (_component == null)
                _component = GetComponent<XtSurfaceComponent>();
            XtSurface s = _component != null ? _component.Surface : null;
            if (s == null || s.IsDisposed)
            {
                Debug.LogWarning("[XTFrame] XtPortBridge: the surface is not running.", this);
                return null;
            }
            return s;
        }

        private bool TryPortId(XtSurface s, string port, out uint id)
        {
            if (string.IsNullOrEmpty(port))
            {
                id = 0;
                return false;
            }
            if (_cacheOwner != s)
            {
                _portIds.Clear();
                _cacheOwner = s;
            }
            if (!_portIds.TryGetValue(port, out id))
            {
                id = s.LookupPort(port);
                if (id != 0)
                    _portIds[port] = id;
            }
            if (id == 0)
                Debug.LogWarning("[XTFrame] XtPortBridge: no declared port '" + port + "'.", this);
            return id != 0;
        }

        private void Warn(string port, XtException e)
        {
            Debug.LogWarning("[XTFrame] XtPortBridge: '" + port + "': " + e.Message, this);
        }
    }
}
