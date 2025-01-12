namespace DocSpark;

public class MethodInfo
{
    public string Name { get; set; }
    public string ReturnType { get; set; }
    public List<string> Parameters { get; set; }
    public string Comment { get; set; }
    public bool IsPublic { get; set; }
    public string Route { get; set; }
    public bool IsEndPoint { get; set; }
}
