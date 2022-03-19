using System;

namespace AG.WebHelpers
{
    public class AvailabilityEventArgs : EventArgs
    {
        public readonly bool IsAvailable;
        public readonly string Message;
        public DateTime LastAvailabilityTime { get; }
        public DateTime LastUnavailabilityTime { get; }

        public AvailabilityEventArgs(bool isAvailable, string message, DateTime lastAvailabilityTime, DateTime lastUnavailabilityTime)
        {
            IsAvailable = isAvailable;
            Message = message;
            LastAvailabilityTime = lastAvailabilityTime;
            LastUnavailabilityTime = lastUnavailabilityTime;
        }

        public override string ToString() => $"{(IsAvailable ? "Available" : "Unavailable")}, availability time: {LastAvailabilityTime}, unavailability time: {LastUnavailabilityTime}, message: {Message}";
    }
}
