namespace DocSpark;
public class ClassInfo
{
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Comment { get; set; }
    public string InheritedClass { get; set; }
    public List<string> ImplementedInterfaces { get; set; }
    public string FilePath { get; set; }
    public string ProjectRoot { get; set; }
    public string RoutePrefix { get; set; }
    public List<PropertyInfo> Properties { get; set; }
    public List<MethodInfo> Methods { get; set; }
    public bool IsApiClass { get; set; }

    public string RelativePath()
    {
        return Path.GetRelativePath(ProjectRoot, FilePath).Replace('\\', '/');
    }
}
