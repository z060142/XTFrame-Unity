using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XTFrame.Editor
{
    /// <summary>Inspector for <see cref="XtSurfaceComponent"/>: UI picker, design info, port tables, reload.</summary>
    [CustomEditor(typeof(XtSurfaceComponent))]
    public sealed class XtSurfaceComponentEditor : UnityEditor.Editor
    {
        private XtBundle _bundle;
        private string _bundleDir;
        private string _error;
        private bool _perfEnabled;

        private void OnDisable()
        {
            ReleaseBundle();
        }

        /// <summary>Repaint every frame while playing so stats stay live.</summary>
        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        /// <summary>Draws the inspector.</summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.Update();
            SerializedProperty pathProp = serializedObject.FindProperty("bundlePath");
            SerializedProperty uiProp = serializedObject.FindProperty("uiId");
            var component = (XtSurfaceComponent)target;

            string dir = XtSurfaceComponent.ResolveBundlePath(pathProp.stringValue);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Bundle", EditorStyles.boldLabel);
            if (string.IsNullOrEmpty(pathProp.stringValue) || !Directory.Exists(dir))
            {
                ReleaseBundle();
                EditorGUILayout.HelpBox("Bundle folder not found: " + dir, MessageType.Info);
                return;
            }
            OpenBundle(dir);
            if (_error != null)
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
                return;
            }

            // UI picker.
            int count = _bundle.Uis.Count;
            var ids = new string[count];
            int current = -1;
            for (int i = 0; i < count; ++i)
            {
                ids[i] = _bundle.Uis[i].Id;
                if (ids[i] == uiProp.stringValue)
                    current = i;
            }
            if (count == 0)
            {
                EditorGUILayout.HelpBox("The bundle has no UIs.", MessageType.Warning);
            }
            else
            {
                int picked = EditorGUILayout.Popup("UI", current, ids);
                if (picked >= 0 && picked != current)
                {
                    uiProp.stringValue = ids[picked];
                    serializedObject.ApplyModifiedProperties();
                    current = picked;
                }
                if (current < 0)
                    EditorGUILayout.HelpBox("UI id '" + uiProp.stringValue + "' is not in this bundle.", MessageType.Warning);
            }

            if (current >= 0)
                DrawUiInfo(_bundle.Uis[current]);

            if (Application.isPlaying && component.Surface != null && !component.Surface.IsDisposed)
                DrawLiveSurface(component.Surface);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open bundle folder"))
                    EditorUtility.RevealInFinder(dir);
                using (new EditorGUI.DisabledScope(!Application.isPlaying || component.Surface == null))
                {
                    if (GUILayout.Button("Hot reload"))
                        component.HotReloadNow();
                }
            }
        }

        private static void DrawUiInfo(XtUiInfo ui)
        {
            EditorGUILayout.LabelField("Design size", ui.DesignWidth + " x " + ui.DesignHeight);
            EditorGUILayout.LabelField("Fit (manifest)", string.IsNullOrEmpty(ui.Fit) ? "-" : ui.Fit);
            EditorGUILayout.LabelField("Has flow", ui.HasFlow ? "yes" : "no");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connection points (" + ui.Ports.Count + ")", EditorStyles.boldLabel);
            foreach (XtPortInfo p in ui.Ports)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.SelectableLabel(p.Name, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUILayout.Label(p.Type, GUILayout.Width(60));
                    GUILayout.Label(p.IsInput ? "in" : "out", GUILayout.Width(30));
                }
            }
        }

        private void DrawLiveSurface(XtSurface surface)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Declared ports (" + surface.Ports.Count + ")", EditorStyles.boldLabel);
            foreach (XtPortRecord r in surface.Ports)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(r.Id.ToString(), GUILayout.Width(80));
                    EditorGUILayout.SelectableLabel(r.QualifiedName, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    GUILayout.Label(r.ValueType.ToString(), GUILayout.Width(60));
                    GUILayout.Label(r.Dir.ToString(), GUILayout.Width(40));
                    GUILayout.Label(surface.GetPortText(r.Id), GUILayout.Width(100));
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Frame", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Surface", surface.Width + " x " + surface.Height + ", ui scale " + surface.UiScale);
            bool perf = EditorGUILayout.Toggle("Collect frame stats", _perfEnabled);
            if (perf != _perfEnabled)
            {
                _perfEnabled = perf;
                surface.SetPerfEnabled(perf);
            }
            try
            {
                XtFrameStats s = surface.FrameStats;
                EditorGUILayout.LabelField("Update / render ms", s.UpdateMs.ToString("0.00") + " / " + s.RenderMs.ToString("0.00"));
                EditorGUILayout.LabelField("Draws executed / skipped", s.ExecutedDraws + " / " + s.SkippedDraws);
            }
            catch (XtException e)
            {
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
            }
        }

        private void OpenBundle(string dir)
        {
            if (_bundleDir == dir && (_error != null || (_bundle != null && _bundle.IsOpen)))
                return;
            ReleaseBundle();
            _bundleDir = dir;
            try
            {
                _bundle = XtBundle.Acquire(dir);
            }
            catch (DllNotFoundException)
            {
                _error = "XTFrame native library not found. Copy xtframe.dll / libxtframe.so / libxtframe.dylib into " +
                         "Runtime/Plugins (see the package README) and enable it for the Editor.";
            }
            catch (EntryPointNotFoundException e)
            {
                _error = "The XTFrame native library is too old for this package: " + e.Message;
            }
            catch (XtException e)
            {
                _error = e.Message;
            }
        }

        private void ReleaseBundle()
        {
            if (_bundle != null)
                _bundle.Release();
            _bundle = null;
            _bundleDir = null;
            _error = null;
        }
    }
}
