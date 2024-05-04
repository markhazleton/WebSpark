namespace HttpClientUtility.Concurrent;

/// <summary>
/// Represents a model for concurrent processing tasks.
/// </summary>
public class ConcurrentProcessorModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrentProcessorModel"/> class with the specified task ID.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    public ConcurrentProcessorModel(int taskId)
    {
        TaskId = taskId;
    }

    /// <summary>
    /// Gets or sets the task ID.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Gets or sets the task count.
    /// </summary>
    public int TaskCount { get; set; }

    /// <summary>
    /// Gets or sets the duration in milliseconds.
    /// </summary>
    public long DurationMS { get; set; }

    /// <summary>
    /// Gets or sets the semaphore count.
    /// </summary>
    public int SemaphoreCount { get; set; }

    /// <summary>
    /// Gets or sets the semaphore wait ticks.
    /// </summary>
    public long SemaphoreWaitTicks { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string? ToString()
    {
        return $"Task:{TaskId:D4} Duration:{DurationMS:D5} TaskCount:{TaskCount:D2} SemaphoreCount:{SemaphoreCount:D2} SemaphoreWaitTicks:{SemaphoreWaitTicks:D4}";
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ConcurrentProcessorModel"/> class with the specified task ID.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <returns>A new instance of the <see cref="ConcurrentProcessorModel"/> class.</returns>
    public ConcurrentProcessorModel CreateInitialTaskData(int taskId)
    {
        return new ConcurrentProcessorModel(taskId);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ConcurrentProcessorModel"/> class with a new task ID.
    /// </summary>
    /// <param name="newTaskId">The new task ID.</param>
    /// <returns>A new instance of the <see cref="ConcurrentProcessorModel"/> class with the new task ID.</returns>
    public ConcurrentProcessorModel CloneWithNewTaskId(int newTaskId)
    {
        return new ConcurrentProcessorModel(newTaskId)
        {
            TaskId = newTaskId,
            TaskCount = TaskCount + 1,
            DurationMS = DurationMS,
            SemaphoreCount = SemaphoreCount,
            SemaphoreWaitTicks = SemaphoreWaitTicks
        };
    }
}
