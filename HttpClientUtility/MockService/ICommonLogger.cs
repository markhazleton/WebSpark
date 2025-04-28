namespace HttpClientUtility.MockService
{
    /// <summary>
    /// Interface for a common logging service that can track events and exceptions.
    /// </summary>
    public interface ICommonLogger
    {
        /// <summary>
        /// Tracks an event with the specified message.
        /// </summary>
        /// <param name="message">The message describing the event</param>
        void TrackEvent(string message);

        /// <summary>
        /// Tracks an exception with the specified message.
        /// </summary>
        /// <param name="exception">The exception to track</param>
        /// <param name="message">Additional message for context</param>
        void TrackException(Exception exception, string message);
    }
}

