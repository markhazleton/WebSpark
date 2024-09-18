namespace InquirySpark.Domain.Models;

public class BaseResponseCollection<T>
{
    public BaseResponseCollection()
    {
        IsSuccessful = false;
    }
    public BaseResponseCollection(List<T> data)
    {
        IsSuccessful = true;
        Data = data;
    }

    public BaseResponseCollection(T[] data)
    {
        IsSuccessful = true;
        Data = [.. data];
    }

    protected BaseResponseCollection(string message)
    {
        IsSuccessful = false;
        Errors.Add(message);
        Data = default;
    }

    public BaseResponseCollection(string[] errors)
    {
        IsSuccessful = false;
        Errors.AddRange(errors);
        Data = default;
    }

    public List<T> Data { get; set; } = [];

    public string Error
    {
        get
        {
            return string.Join(Environment.NewLine, Errors);
        }
    }
    public List<string> Errors { get; set; } = [];
    public bool IsSuccessful { get; set; } = true;
}
