namespace HttpClientUtility.MockService;
/// <summary>
/// Mock Results
/// </summary>
public class MockResults
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockResults"/> class with specified parameters.
    /// </summary>
    /// <param name="loopCount">The number of iterations to perform</param>
    /// <param name="maxTimeMS">The maximum time in milliseconds allowed for execution</param>
    public MockResults(int loopCount, int maxTimeMS)
    {
        LoopCount = loopCount;
        MaxTimeMS = maxTimeMS;

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockResults"/> class with default values.
    /// </summary>
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
    public string? Message { get; set; } = "initialization";

    /// <summary>
    /// Return Value from calling for results
    /// </summary>
    public string? ResultValue { get; set; } = "initialization";
}
