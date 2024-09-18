namespace InquirySpark.Domain.Models;

/// <summary>
/// Build Version
/// </summary>
public sealed record BuildVersion(int Build, int Major, int Minor, int Revision)
{
    public BuildVersion(Version oVer) : this(Math.Abs(oVer.Build), Math.Abs(oVer.Major), Math.Abs(oVer.Minor), Math.Abs(oVer.Revision))
    {
    }
    public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";

}