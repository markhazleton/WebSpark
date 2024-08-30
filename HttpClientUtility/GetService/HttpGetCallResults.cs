using System.ComponentModel.DataAnnotations;
namespace HttpClientUtility.GetService;
/// <summary>
/// 
/// </summary>
public class HttpGetCallResults
{
    public HttpGetCallResults()
    {
        Iteration = 0;
        StatusPath = string.Empty;
    }

    public HttpGetCallResults(HttpGetCallResults statusCall)
    {
        Iteration = statusCall.Iteration;
        StatusPath = statusCall.StatusPath;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="it"></param>
    /// <param name="path"></param>
    public HttpGetCallResults(int it, string path)
    {
        Iteration = it;
        StatusPath = path;
    }
    [DisplayFormat(DataFormatString = "{0:yyyy.MM.dd hh:mm:ss.ffff}")]
    public DateTime? CompletionDate { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public int Iteration { get; set; }
    public string StatusPath { get; set; }
    public dynamic? StatusResults { get; set; }
}
