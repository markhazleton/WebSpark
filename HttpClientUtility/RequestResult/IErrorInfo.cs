namespace HttpClientUtility.RequestResult;

/// <summary>
/// Interface representing error information for HTTP requests.
/// Contains a collection of error messages related to a request.
/// </summary>
public interface IErrorInfo
{
    /// <summary>
    /// Gets or sets the list of error messages.
    /// </summary>
    List<string> ErrorList { get; set; }
}
