using System;

namespace AG.WebHelpers
{
    public class AvailabilityEventArgs : EventArgs
    {
        public readonly bool IsAvailable;
        public readonly string Message;

        public AvailabilityEventArgs(bool isAvailable, string message)
        {
            IsAvailable = isAvailable;
            Message = message;
        }
    }
}
