namespace XTFrame
{
    /// <summary>Creation options for an <see cref="XtSurface"/> (xt_surface_desc).</summary>
    public struct XtSurfaceOptions
    {
        /// <summary>Device pixel ratio (0 means 1). Stage 1 hosts pass 1.</summary>
        public float DevicePixelRatio;
        /// <summary>Render premultiplied alpha (false for the default UI shader).</summary>
        public bool Premultiplied;
        /// <summary>Load and run the UI's flow and the bundle flow.</summary>
        public bool RunFlow;
        /// <summary>Let the bundle's text shaper add the system fonts.</summary>
        public bool SystemFonts;
        /// <summary>Instance id given to the context and host ports.</summary>
        public uint InstanceId;

        /// <summary>DevicePixelRatio 1, straight alpha, flow on, no system fonts, instance 0.</summary>
        public static XtSurfaceOptions Default => new XtSurfaceOptions { DevicePixelRatio = 1f, RunFlow = true };
    }

    /// <summary>Timeline play options (xt_play_options).</summary>
    public struct XtPlayOptions
    {
        /// <summary>Play backwards.</summary>
        public bool Reverse;
        /// <summary>Loop.</summary>
        public bool Loop;
        /// <summary>Play once.</summary>
        public bool Once;
        /// <summary>Hold the last frame.</summary>
        public bool Hold;
        /// <summary>Start marker name, or null.</summary>
        public string FromMarker;
        /// <summary>Playback rate.</summary>
        public float Rate;

        /// <summary>Forward, rate 1.</summary>
        public static XtPlayOptions Default => new XtPlayOptions { Rate = 1f };
    }

    /// <summary>One declared port of a surface (xt_port_record), strings copied.</summary>
    public readonly struct XtPortRecord
    {
        /// <summary>Port id (never 0).</summary>
        public readonly uint Id;
        /// <summary>Port name.</summary>
        public readonly string Name;
        /// <summary>Qualified name.</summary>
        public readonly string QualifiedName;
        /// <summary>Owning element id ("" for document ports).</summary>
        public readonly string ElementId;
        /// <summary>Value type.</summary>
        public readonly XtPortValueType ValueType;
        /// <summary>Direction.</summary>
        public readonly XtPortDir Dir;
        /// <summary>True for a document-level port.</summary>
        public readonly bool DocumentLevel;

        internal XtPortRecord(uint id, string name, string qualifiedName, string elementId, XtPortValueType type,
                              XtPortDir dir, bool documentLevel)
        {
            Id = id;
            Name = name;
            QualifiedName = qualifiedName;
            ElementId = elementId;
            ValueType = type;
            Dir = dir;
            DocumentLevel = documentLevel;
        }
    }

    /// <summary>A port event (xt_port_event) with its strings already copied.</summary>
    public readonly struct XtPortEvent
    {
        /// <summary>Event kind.</summary>
        public readonly XtPortEventKind Kind;
        /// <summary>Emit / InputChanged: the port id; Connection: 0.</summary>
        public readonly uint Port;
        /// <summary>Connection: the id <see cref="XtSurface.WatchConnection"/> returned; otherwise 0.</summary>
        public readonly uint ListenerId;
        /// <summary>Emit: qualified port name; Connection: the watched name.</summary>
        public readonly string Name;
        /// <summary>Emit / InputChanged: <see cref="XtPortValueType"/>; Connection: 0 pulse 1 bool 2 number 3 string.</summary>
        public readonly int ValueType;
        /// <summary>Number value.</summary>
        public readonly double Number;
        /// <summary>Bool value.</summary>
        public readonly bool Boolean;
        /// <summary>Text form of the value (always filled).</summary>
        public readonly string Text;
        /// <summary>Instance id (0xFFFFFFFF if none).</summary>
        public readonly uint InstanceId;
        /// <summary>List item index (0xFFFFFFFF if none).</summary>
        public readonly uint ItemIndex;

        internal XtPortEvent(in XtPortEventNative e)
        {
            Kind = (XtPortEventKind)e.kind;
            Port = e.port;
            ListenerId = e.listener_id;
            Name = XtNative.Str(e.name);
            ValueType = e.value_type;
            Number = e.number;
            Boolean = e.boolean != 0;
            Text = XtNative.Str(e.text);
            InstanceId = e.instance_id;
            ItemIndex = e.item_index;
        }
    }

    /// <summary>A flow host event (xt_host_event: sound / haptic / spawn / emit ...), strings copied.</summary>
    public readonly struct XtHostEvent
    {
        /// <summary>FlowHostEventKind value.</summary>
        public readonly int Kind;
        /// <summary>Name.</summary>
        public readonly string Name;
        /// <summary>Volume.</summary>
        public readonly float Volume;
        /// <summary>UI id.</summary>
        public readonly string UiId;
        /// <summary>Instance id.</summary>
        public readonly uint InstanceId;
        /// <summary>Flow node id.</summary>
        public readonly string NodeId;

        internal XtHostEvent(in XtHostEventNative e)
        {
            Kind = e.kind;
            Name = XtNative.Str(e.name);
            Volume = e.volume;
            UiId = XtNative.Str(e.ui_id);
            InstanceId = e.instance_id;
            NodeId = XtNative.Str(e.node_id);
        }
    }

    /// <summary>A timeline event (xt_timeline_event), strings copied.</summary>
    public readonly struct XtTimelineEvent
    {
        /// <summary>TimelineEventKind value.</summary>
        public readonly int Kind;
        /// <summary>Timeline name.</summary>
        public readonly string Timeline;
        /// <summary>Marker name ("" if none).</summary>
        public readonly string Marker;

        internal XtTimelineEvent(in XtTimelineEventNative e)
        {
            Kind = e.kind;
            Timeline = XtNative.Str(e.timeline);
            Marker = XtNative.Str(e.marker);
        }
    }
}
