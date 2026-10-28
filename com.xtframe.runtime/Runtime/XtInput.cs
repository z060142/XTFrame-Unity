// Unity input -> XTFrame input values.
//
// Backends: the Input System package is used when it is installed (asmdef versionDefines sets
// XT_INPUT_SYSTEM) and enabled in Player Settings (ENABLE_INPUT_SYSTEM); otherwise the legacy
// Input Manager (ENABLE_LEGACY_INPUT_MANAGER). With neither, keyboard and text input are off and
// a warning is logged once. Pointer input always comes from uGUI's EventSystem.

#if XT_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM
#define XT_USE_INPUT_SYSTEM
#elif ENABLE_LEGACY_INPUT_MANAGER
#define XT_USE_LEGACY_INPUT
#endif

using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if XT_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace XTFrame
{
    /// <summary>Conversions from Unity input to XTFrame key codes, modifiers and wheel notches.</summary>
    public static class XtInput
    {
        private static readonly KeyValuePair<KeyCode, XtKey>[] s_legacyTable =
        {
            new KeyValuePair<KeyCode, XtKey>(KeyCode.A, XtKey.KeyA), new KeyValuePair<KeyCode, XtKey>(KeyCode.B, XtKey.KeyB),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.C, XtKey.KeyC), new KeyValuePair<KeyCode, XtKey>(KeyCode.D, XtKey.KeyD),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.E, XtKey.KeyE), new KeyValuePair<KeyCode, XtKey>(KeyCode.F, XtKey.KeyF),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.G, XtKey.KeyG), new KeyValuePair<KeyCode, XtKey>(KeyCode.H, XtKey.KeyH),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.I, XtKey.KeyI), new KeyValuePair<KeyCode, XtKey>(KeyCode.J, XtKey.KeyJ),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.K, XtKey.KeyK), new KeyValuePair<KeyCode, XtKey>(KeyCode.L, XtKey.KeyL),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.M, XtKey.KeyM), new KeyValuePair<KeyCode, XtKey>(KeyCode.N, XtKey.KeyN),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.O, XtKey.KeyO), new KeyValuePair<KeyCode, XtKey>(KeyCode.P, XtKey.KeyP),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Q, XtKey.KeyQ), new KeyValuePair<KeyCode, XtKey>(KeyCode.R, XtKey.KeyR),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.S, XtKey.KeyS), new KeyValuePair<KeyCode, XtKey>(KeyCode.T, XtKey.KeyT),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.U, XtKey.KeyU), new KeyValuePair<KeyCode, XtKey>(KeyCode.V, XtKey.KeyV),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.W, XtKey.KeyW), new KeyValuePair<KeyCode, XtKey>(KeyCode.X, XtKey.KeyX),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Y, XtKey.KeyY), new KeyValuePair<KeyCode, XtKey>(KeyCode.Z, XtKey.KeyZ),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha1, XtKey.Key1), new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha2, XtKey.Key2),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha3, XtKey.Key3), new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha4, XtKey.Key4),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha5, XtKey.Key5), new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha6, XtKey.Key6),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha7, XtKey.Key7), new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha8, XtKey.Key8),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha9, XtKey.Key9), new KeyValuePair<KeyCode, XtKey>(KeyCode.Alpha0, XtKey.Key0),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Return, XtKey.Enter), new KeyValuePair<KeyCode, XtKey>(KeyCode.Escape, XtKey.Escape),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Backspace, XtKey.Backspace), new KeyValuePair<KeyCode, XtKey>(KeyCode.Tab, XtKey.Tab),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Space, XtKey.Space), new KeyValuePair<KeyCode, XtKey>(KeyCode.Delete, XtKey.Delete),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Home, XtKey.Home), new KeyValuePair<KeyCode, XtKey>(KeyCode.End, XtKey.End),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.RightArrow, XtKey.Right), new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftArrow, XtKey.Left),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.DownArrow, XtKey.Down), new KeyValuePair<KeyCode, XtKey>(KeyCode.UpArrow, XtKey.Up),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.F1, XtKey.F1), new KeyValuePair<KeyCode, XtKey>(KeyCode.F2, XtKey.F2),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.F3, XtKey.F3), new KeyValuePair<KeyCode, XtKey>(KeyCode.F4, XtKey.F4),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.F5, XtKey.F5), new KeyValuePair<KeyCode, XtKey>(KeyCode.F6, XtKey.F6),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.F7, XtKey.F7), new KeyValuePair<KeyCode, XtKey>(KeyCode.F8, XtKey.F8),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.F9, XtKey.F9), new KeyValuePair<KeyCode, XtKey>(KeyCode.F10, XtKey.F10),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.F11, XtKey.F11), new KeyValuePair<KeyCode, XtKey>(KeyCode.F12, XtKey.F12),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.PageUp, XtKey.PageUp), new KeyValuePair<KeyCode, XtKey>(KeyCode.PageDown, XtKey.PageDown),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Insert, XtKey.Insert),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftShift, XtKey.ShiftLeft), new KeyValuePair<KeyCode, XtKey>(KeyCode.RightShift, XtKey.ShiftRight),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftControl, XtKey.ControlLeft), new KeyValuePair<KeyCode, XtKey>(KeyCode.RightControl, XtKey.ControlRight),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftAlt, XtKey.AltLeft), new KeyValuePair<KeyCode, XtKey>(KeyCode.RightAlt, XtKey.AltRight),
            // LeftApple / RightApple are aliases of LeftCommand / RightCommand (same enum value).
            new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftCommand, XtKey.MetaLeft), new KeyValuePair<KeyCode, XtKey>(KeyCode.RightCommand, XtKey.MetaRight),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftWindows, XtKey.MetaLeft), new KeyValuePair<KeyCode, XtKey>(KeyCode.RightWindows, XtKey.MetaRight),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.CapsLock, XtKey.CapsLock),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Minus, XtKey.Minus), new KeyValuePair<KeyCode, XtKey>(KeyCode.Equals, XtKey.Equal),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.LeftBracket, XtKey.BracketLeft), new KeyValuePair<KeyCode, XtKey>(KeyCode.RightBracket, XtKey.BracketRight),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Backslash, XtKey.Backslash), new KeyValuePair<KeyCode, XtKey>(KeyCode.Semicolon, XtKey.Semicolon),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Quote, XtKey.Quote), new KeyValuePair<KeyCode, XtKey>(KeyCode.BackQuote, XtKey.Backquote),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Comma, XtKey.Comma), new KeyValuePair<KeyCode, XtKey>(KeyCode.Period, XtKey.Period),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Slash, XtKey.Slash),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad0, XtKey.Numpad0), new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad1, XtKey.Numpad1),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad2, XtKey.Numpad2), new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad3, XtKey.Numpad3),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad4, XtKey.Numpad4), new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad5, XtKey.Numpad5),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad6, XtKey.Numpad6), new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad7, XtKey.Numpad7),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad8, XtKey.Numpad8), new KeyValuePair<KeyCode, XtKey>(KeyCode.Keypad9, XtKey.Numpad9),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.KeypadPlus, XtKey.NumpadAdd), new KeyValuePair<KeyCode, XtKey>(KeyCode.KeypadMinus, XtKey.NumpadSubtract),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.KeypadMultiply, XtKey.NumpadMultiply), new KeyValuePair<KeyCode, XtKey>(KeyCode.KeypadDivide, XtKey.NumpadDivide),
            new KeyValuePair<KeyCode, XtKey>(KeyCode.KeypadPeriod, XtKey.NumpadDecimal), new KeyValuePair<KeyCode, XtKey>(KeyCode.KeypadEnter, XtKey.NumpadEnter),
        };

        private static Dictionary<KeyCode, XtKey> s_legacyMap;

        /// <summary>The legacy KeyCode -> XtKey pairs (polled by <see cref="KeyboardPoller"/>).</summary>
        internal static KeyValuePair<KeyCode, XtKey>[] LegacyTable => s_legacyTable;

        /// <summary>Maps a legacy <see cref="KeyCode"/>; unknown keys give <see cref="XtKey.Unknown"/>.</summary>
        public static XtKey FromUnity(KeyCode key)
        {
            if (s_legacyMap == null)
            {
                s_legacyMap = new Dictionary<KeyCode, XtKey>(s_legacyTable.Length);
                foreach (KeyValuePair<KeyCode, XtKey> kv in s_legacyTable)
                    s_legacyMap[kv.Key] = kv.Value;
            }
            return s_legacyMap.TryGetValue(key, out XtKey k) ? k : XtKey.Unknown;
        }

#if XT_INPUT_SYSTEM
        private static readonly KeyValuePair<Key, XtKey>[] s_inputSystemTable =
        {
            new KeyValuePair<Key, XtKey>(Key.A, XtKey.KeyA), new KeyValuePair<Key, XtKey>(Key.B, XtKey.KeyB),
            new KeyValuePair<Key, XtKey>(Key.C, XtKey.KeyC), new KeyValuePair<Key, XtKey>(Key.D, XtKey.KeyD),
            new KeyValuePair<Key, XtKey>(Key.E, XtKey.KeyE), new KeyValuePair<Key, XtKey>(Key.F, XtKey.KeyF),
            new KeyValuePair<Key, XtKey>(Key.G, XtKey.KeyG), new KeyValuePair<Key, XtKey>(Key.H, XtKey.KeyH),
            new KeyValuePair<Key, XtKey>(Key.I, XtKey.KeyI), new KeyValuePair<Key, XtKey>(Key.J, XtKey.KeyJ),
            new KeyValuePair<Key, XtKey>(Key.K, XtKey.KeyK), new KeyValuePair<Key, XtKey>(Key.L, XtKey.KeyL),
            new KeyValuePair<Key, XtKey>(Key.M, XtKey.KeyM), new KeyValuePair<Key, XtKey>(Key.N, XtKey.KeyN),
            new KeyValuePair<Key, XtKey>(Key.O, XtKey.KeyO), new KeyValuePair<Key, XtKey>(Key.P, XtKey.KeyP),
            new KeyValuePair<Key, XtKey>(Key.Q, XtKey.KeyQ), new KeyValuePair<Key, XtKey>(Key.R, XtKey.KeyR),
            new KeyValuePair<Key, XtKey>(Key.S, XtKey.KeyS), new KeyValuePair<Key, XtKey>(Key.T, XtKey.KeyT),
            new KeyValuePair<Key, XtKey>(Key.U, XtKey.KeyU), new KeyValuePair<Key, XtKey>(Key.V, XtKey.KeyV),
            new KeyValuePair<Key, XtKey>(Key.W, XtKey.KeyW), new KeyValuePair<Key, XtKey>(Key.X, XtKey.KeyX),
            new KeyValuePair<Key, XtKey>(Key.Y, XtKey.KeyY), new KeyValuePair<Key, XtKey>(Key.Z, XtKey.KeyZ),
            new KeyValuePair<Key, XtKey>(Key.Digit1, XtKey.Key1), new KeyValuePair<Key, XtKey>(Key.Digit2, XtKey.Key2),
            new KeyValuePair<Key, XtKey>(Key.Digit3, XtKey.Key3), new KeyValuePair<Key, XtKey>(Key.Digit4, XtKey.Key4),
            new KeyValuePair<Key, XtKey>(Key.Digit5, XtKey.Key5), new KeyValuePair<Key, XtKey>(Key.Digit6, XtKey.Key6),
            new KeyValuePair<Key, XtKey>(Key.Digit7, XtKey.Key7), new KeyValuePair<Key, XtKey>(Key.Digit8, XtKey.Key8),
            new KeyValuePair<Key, XtKey>(Key.Digit9, XtKey.Key9), new KeyValuePair<Key, XtKey>(Key.Digit0, XtKey.Key0),
            new KeyValuePair<Key, XtKey>(Key.Enter, XtKey.Enter), new KeyValuePair<Key, XtKey>(Key.Escape, XtKey.Escape),
            new KeyValuePair<Key, XtKey>(Key.Backspace, XtKey.Backspace), new KeyValuePair<Key, XtKey>(Key.Tab, XtKey.Tab),
            new KeyValuePair<Key, XtKey>(Key.Space, XtKey.Space), new KeyValuePair<Key, XtKey>(Key.Delete, XtKey.Delete),
            new KeyValuePair<Key, XtKey>(Key.Home, XtKey.Home), new KeyValuePair<Key, XtKey>(Key.End, XtKey.End),
            new KeyValuePair<Key, XtKey>(Key.RightArrow, XtKey.Right), new KeyValuePair<Key, XtKey>(Key.LeftArrow, XtKey.Left),
            new KeyValuePair<Key, XtKey>(Key.DownArrow, XtKey.Down), new KeyValuePair<Key, XtKey>(Key.UpArrow, XtKey.Up),
            new KeyValuePair<Key, XtKey>(Key.F1, XtKey.F1), new KeyValuePair<Key, XtKey>(Key.F2, XtKey.F2),
            new KeyValuePair<Key, XtKey>(Key.F3, XtKey.F3), new KeyValuePair<Key, XtKey>(Key.F4, XtKey.F4),
            new KeyValuePair<Key, XtKey>(Key.F5, XtKey.F5), new KeyValuePair<Key, XtKey>(Key.F6, XtKey.F6),
            new KeyValuePair<Key, XtKey>(Key.F7, XtKey.F7), new KeyValuePair<Key, XtKey>(Key.F8, XtKey.F8),
            new KeyValuePair<Key, XtKey>(Key.F9, XtKey.F9), new KeyValuePair<Key, XtKey>(Key.F10, XtKey.F10),
            new KeyValuePair<Key, XtKey>(Key.F11, XtKey.F11), new KeyValuePair<Key, XtKey>(Key.F12, XtKey.F12),
            new KeyValuePair<Key, XtKey>(Key.PageUp, XtKey.PageUp), new KeyValuePair<Key, XtKey>(Key.PageDown, XtKey.PageDown),
            new KeyValuePair<Key, XtKey>(Key.Insert, XtKey.Insert),
            new KeyValuePair<Key, XtKey>(Key.LeftShift, XtKey.ShiftLeft), new KeyValuePair<Key, XtKey>(Key.RightShift, XtKey.ShiftRight),
            new KeyValuePair<Key, XtKey>(Key.LeftCtrl, XtKey.ControlLeft), new KeyValuePair<Key, XtKey>(Key.RightCtrl, XtKey.ControlRight),
            new KeyValuePair<Key, XtKey>(Key.LeftAlt, XtKey.AltLeft), new KeyValuePair<Key, XtKey>(Key.RightAlt, XtKey.AltRight),
            // LeftWindows / LeftCommand / LeftApple are aliases of LeftMeta (same enum value).
            new KeyValuePair<Key, XtKey>(Key.LeftMeta, XtKey.MetaLeft), new KeyValuePair<Key, XtKey>(Key.RightMeta, XtKey.MetaRight),
            new KeyValuePair<Key, XtKey>(Key.CapsLock, XtKey.CapsLock),
            new KeyValuePair<Key, XtKey>(Key.Minus, XtKey.Minus), new KeyValuePair<Key, XtKey>(Key.Equals, XtKey.Equal),
            new KeyValuePair<Key, XtKey>(Key.LeftBracket, XtKey.BracketLeft), new KeyValuePair<Key, XtKey>(Key.RightBracket, XtKey.BracketRight),
            new KeyValuePair<Key, XtKey>(Key.Backslash, XtKey.Backslash), new KeyValuePair<Key, XtKey>(Key.Semicolon, XtKey.Semicolon),
            new KeyValuePair<Key, XtKey>(Key.Quote, XtKey.Quote), new KeyValuePair<Key, XtKey>(Key.Backquote, XtKey.Backquote),
            new KeyValuePair<Key, XtKey>(Key.Comma, XtKey.Comma), new KeyValuePair<Key, XtKey>(Key.Period, XtKey.Period),
            new KeyValuePair<Key, XtKey>(Key.Slash, XtKey.Slash),
            new KeyValuePair<Key, XtKey>(Key.Numpad0, XtKey.Numpad0), new KeyValuePair<Key, XtKey>(Key.Numpad1, XtKey.Numpad1),
            new KeyValuePair<Key, XtKey>(Key.Numpad2, XtKey.Numpad2), new KeyValuePair<Key, XtKey>(Key.Numpad3, XtKey.Numpad3),
            new KeyValuePair<Key, XtKey>(Key.Numpad4, XtKey.Numpad4), new KeyValuePair<Key, XtKey>(Key.Numpad5, XtKey.Numpad5),
            new KeyValuePair<Key, XtKey>(Key.Numpad6, XtKey.Numpad6), new KeyValuePair<Key, XtKey>(Key.Numpad7, XtKey.Numpad7),
            new KeyValuePair<Key, XtKey>(Key.Numpad8, XtKey.Numpad8), new KeyValuePair<Key, XtKey>(Key.Numpad9, XtKey.Numpad9),
            new KeyValuePair<Key, XtKey>(Key.NumpadPlus, XtKey.NumpadAdd), new KeyValuePair<Key, XtKey>(Key.NumpadMinus, XtKey.NumpadSubtract),
            new KeyValuePair<Key, XtKey>(Key.NumpadMultiply, XtKey.NumpadMultiply), new KeyValuePair<Key, XtKey>(Key.NumpadDivide, XtKey.NumpadDivide),
            new KeyValuePair<Key, XtKey>(Key.NumpadPeriod, XtKey.NumpadDecimal), new KeyValuePair<Key, XtKey>(Key.NumpadEnter, XtKey.NumpadEnter),
        };

        private static Dictionary<Key, XtKey> s_inputSystemMap;

        internal static KeyValuePair<Key, XtKey>[] InputSystemTable => s_inputSystemTable;

        /// <summary>Maps an Input System <see cref="Key"/>; unknown keys give <see cref="XtKey.Unknown"/>.</summary>
        public static XtKey FromKey(Key key)
        {
            if (s_inputSystemMap == null)
            {
                s_inputSystemMap = new Dictionary<Key, XtKey>(s_inputSystemTable.Length);
                foreach (KeyValuePair<Key, XtKey> kv in s_inputSystemTable)
                    s_inputSystemMap[kv.Key] = kv.Value;
            }
            return s_inputSystemMap.TryGetValue(key, out XtKey k) ? k : XtKey.Unknown;
        }
#endif

        /// <summary>The held modifiers as an <see cref="XtModifiers"/> bit mask (0 without a keyboard backend).</summary>
        public static byte Modifiers()
        {
            XtModifiers m = XtModifiers.None;
#if XT_USE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb == null)
                return 0;
            if (kb.shiftKey.isPressed)
                m |= XtModifiers.Shift;
            if (kb.ctrlKey.isPressed)
                m |= XtModifiers.Ctrl;
            if (kb.altKey.isPressed)
                m |= XtModifiers.Alt;
            if (kb.leftMetaKey.isPressed || kb.rightMetaKey.isPressed)
                m |= XtModifiers.Super;
#elif XT_USE_LEGACY_INPUT
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                m |= XtModifiers.Shift;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                m |= XtModifiers.Ctrl;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                m |= XtModifiers.Alt;
            if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) ||
                Input.GetKey(KeyCode.LeftWindows) || Input.GetKey(KeyCode.RightWindows))
                m |= XtModifiers.Super;
#endif
            return (byte)m;
        }

        /// <summary>
        /// Vertical wheel notches from uGUI's PointerEventData.scrollDelta (already in lines).
        /// Unity scrolls up with positive y, XTFrame scrolls down with positive y, so the sign flips.
        /// </summary>
        public static float WheelNotches(Vector2 scrollDelta)
        {
            return -scrollDelta.y;
        }

        /// <summary>True when a keyboard backend was compiled in.</summary>
        public static bool HasKeyboardBackend
        {
            get
            {
#if XT_USE_INPUT_SYSTEM || XT_USE_LEGACY_INPUT
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>Removes control characters (backspace, enter, ...; they arrive as key events).</summary>
        internal static string StripControl(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            int i = 0;
            while (i < s.Length && !char.IsControl(s[i]))
                ++i;
            if (i == s.Length)
                return s;
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
                if (!char.IsControl(c))
                    sb.Append(c);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Polls the keyboard once per frame and forwards key presses, releases and committed text to a
    /// surface. Call <see cref="Enable"/> / <see cref="Disable"/> with the owning component.
    /// </summary>
    public sealed class KeyboardPoller
    {
#if !XT_USE_INPUT_SYSTEM && !XT_USE_LEGACY_INPUT
        private static bool s_warned;
#endif
        private readonly StringBuilder _text = new StringBuilder();
#if XT_USE_INPUT_SYSTEM
        private Keyboard _subscribed;
#endif

        /// <summary>Starts collecting text (Input System: subscribes to onTextInput).</summary>
        public void Enable()
        {
#if XT_USE_INPUT_SYSTEM
            Resubscribe();
#elif !XT_USE_LEGACY_INPUT
            if (!s_warned)
            {
                s_warned = true;
                Debug.LogWarning("[XTFrame] No keyboard backend (neither the Input System nor the legacy Input Manager " +
                                 "is enabled): keyboard and text input to XTFrame surfaces is off.");
            }
#endif
        }

        /// <summary>Stops collecting text.</summary>
        public void Disable()
        {
#if XT_USE_INPUT_SYSTEM
            if (_subscribed != null)
                _subscribed.onTextInput -= OnTextInput;
            _subscribed = null;
#endif
            _text.Clear();
        }

        /// <summary>Text committed since the last call, control characters removed ("" if none).</summary>
        public string TextThisFrame()
        {
#if XT_USE_INPUT_SYSTEM
            Resubscribe();
            if (_text.Length == 0)
                return string.Empty;
            string s = XtInput.StripControl(_text.ToString());
            _text.Clear();
            return s;
#elif XT_USE_LEGACY_INPUT
            string raw = Input.inputString;
            return string.IsNullOrEmpty(raw) ? string.Empty : XtInput.StripControl(raw);
#else
            return string.Empty;
#endif
        }

        /// <summary>Sends this frame's key downs / ups and text to the surface.</summary>
        public void Poll(XtSurface surface)
        {
            if (surface == null || surface.IsDisposed)
                return;
#if XT_USE_INPUT_SYSTEM
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                byte mods = XtInput.Modifiers();
                KeyValuePair<Key, XtKey>[] table = XtInput.InputSystemTable;
                for (int i = 0; i < table.Length; ++i)
                {
                    var control = kb[table[i].Key];
                    if (control.wasPressedThisFrame)
                        surface.PushKey(table[i].Value, true, mods, false);
                    if (surface.IsDisposed)
                        return;
                    if (control.wasReleasedThisFrame)
                        surface.PushKey(table[i].Value, false, mods, false);
                    if (surface.IsDisposed)
                        return;
                }
            }
#elif XT_USE_LEGACY_INPUT
            byte mods = XtInput.Modifiers();
            KeyValuePair<KeyCode, XtKey>[] table = XtInput.LegacyTable;
            for (int i = 0; i < table.Length; ++i)
            {
                if (Input.GetKeyDown(table[i].Key))
                    surface.PushKey(table[i].Value, true, mods, false);
                if (surface.IsDisposed)
                    return;
                if (Input.GetKeyUp(table[i].Key))
                    surface.PushKey(table[i].Value, false, mods, false);
                if (surface.IsDisposed)
                    return;
            }
#endif
            string text = TextThisFrame();
            if (text.Length > 0)
                surface.PushText(text);
        }

#if XT_USE_INPUT_SYSTEM
        private void Resubscribe()
        {
            Keyboard kb = Keyboard.current;
            if (kb == _subscribed)
                return;
            if (_subscribed != null)
                _subscribed.onTextInput -= OnTextInput;
            _subscribed = kb;
            if (kb != null)
                kb.onTextInput += OnTextInput;
        }

        private void OnTextInput(char c)
        {
            _text.Append(c);
        }
#endif
    }
}
