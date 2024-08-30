namespace HttpClientUtility.MockService
{
    public interface ICommonLogger
    {
        void TrackEvent(string message);
        void TrackException(Exception exception, string message);
    }
}

