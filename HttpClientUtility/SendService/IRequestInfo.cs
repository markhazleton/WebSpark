using System.Net;

namespace HttpClientUtility.SendService;

public interface IRequestInfo
{
    int Iteration { get; set; }
    string RequestPath { get; set; }
    HttpMethod RequestMethod { get; set; }
    StringContent? RequestBody { get; set; }
    Dictionary<string, string>? RequestHeaders { get; set; }
}
