using System;

namespace XTFrame
{
    /// <summary>A failed XTFrame native call: the status plus the library's last error text.</summary>
    public sealed class XtException : Exception
    {
        /// <summary>The status the native call returned.</summary>
        public XtStatus Status { get; }

        /// <summary>The native last-error message at the time of failure ("" if none).</summary>
        public string LastError { get; }

        /// <summary>Creates the exception from a status and the native last error.</summary>
        public XtException(XtStatus status, string lastError)
            : base(Format(status, lastError))
        {
            Status = status;
            LastError = lastError ?? string.Empty;
        }

        private static string Format(XtStatus status, string lastError)
        {
            return string.IsNullOrEmpty(lastError) ? "XTFrame: " + status : "XTFrame: " + status + ": " + lastError;
        }
    }
}
