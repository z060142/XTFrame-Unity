using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace XTFrame
{
    // Native callbacks of XtSurface. They are static (IL2CPP requirement) and recover the surface
    // from `user`, the GCHandle the constructor allocated. Strings are copied into the managed
    // event structs first; no exception escapes into native code.
    public sealed partial class XtSurface
    {
        private static XtSurface FromUser(IntPtr user)
        {
            if (user == IntPtr.Zero)
                return null;
            return GCHandle.FromIntPtr(user).Target as XtSurface;
        }

        // ---------------------------------------------------------------- native callbacks
        // Static (IL2CPP requirement); `user` is the GCHandle of the surface. Strings are copied
        // into the managed event structs before anything else; no exception escapes into native.

        [MonoPInvokeCallback(typeof(XtNative.PortEventFn))]
        private static unsafe void OnPortEvent(IntPtr s, IntPtr e, IntPtr user)
        {
            try
            {
                XtSurface self = FromUser(user);
                if (self != null && e != IntPtr.Zero)
                    self.Raise(self.PortEvent, new XtPortEvent(in *(XtPortEventNative*)e));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(XtNative.HostEventFn))]
        private static unsafe void OnHostEvent(IntPtr s, IntPtr e, IntPtr user)
        {
            try
            {
                XtSurface self = FromUser(user);
                if (self != null && e != IntPtr.Zero)
                    self.Raise(self.HostEvent, new XtHostEvent(in *(XtHostEventNative*)e));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(XtNative.TimelineEventFn))]
        private static unsafe void OnTimelineEvent(IntPtr s, IntPtr e, IntPtr user)
        {
            try
            {
                XtSurface self = FromUser(user);
                if (self != null && e != IntPtr.Zero)
                    self.Raise(self.TimelineEvent, new XtTimelineEvent(in *(XtTimelineEventNative*)e));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [MonoPInvokeCallback(typeof(XtNative.VisibilityFn))]
        private static void OnVisibility(IntPtr s, int visible, IntPtr user)
        {
            try
            {
                XtSurface self = FromUser(user);
                if (self != null)
                    self.Raise(self.VisibilityChanged, visible != 0);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        // Runs a handler with the callback depth raised, so a Dispose from it is deferred.
        private void Raise<T>(Action<T> handler, T value)
        {
            if (handler == null)
                return;
            _callbackDepth++;
            try
            {
                handler(value);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _callbackDepth--;
            }
        }
    }
}
