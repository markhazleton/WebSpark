
namespace AsyncSpark.Models;
/// <summary>
/// Mock Results
/// </summary>
public class MockResults
{
    public MockResults(int loopCount, int maxTimeMS)
    {
        LoopCount = loopCount;
        MaxTimeMS = maxTimeMS;

    }
    public MockResults()
    {
        LoopCount = 0;
        MaxTimeMS = 0;

    }
    /// <summary>
    /// Loop Count (number of iterations of work to perform)
    /// </summary>
    public int LoopCount { get; set; }

    /// <summary>
    /// Max Time for completing all iterations
    /// </summary>
    public int MaxTimeMS { get; set; }

    /// <summary>
    /// Actual Runtime to complete the requested loops (iterations)
    /// </summary>
    public long? RunTimeMS { get; set; } = 0;

    /// <summary>
    /// Return Message from calling for results
    /// </summary>
    public string? Message { get; set; } = "init";

    /// <summary>
    /// Return Value from calling for results
    /// </summary>
    public string? ResultValue { get; set; } = "empty";
}
