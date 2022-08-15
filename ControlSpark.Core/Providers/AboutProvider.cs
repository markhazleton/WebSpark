using ControlSpark.Core.Data;
using System.Runtime.InteropServices;

namespace ControlSpark.Core.Providers;

public interface IAboutProvider
{
    Task<AboutModel> GetAboutModel();
}

public class AboutProvider : IAboutProvider
{
    private readonly AppDbContext _db;

    public AboutProvider(AppDbContext db)
    {
        _db = db;
    }
    public async Task<AboutModel> GetAboutModel()
    {
        var model = new AboutModel();
        model.DatabaseProvider = _db.Database.ProviderName;
        model.OperatingSystem = RuntimeInformation.OSDescription;
        try
        {
            model.Build = new BuildVersion(Assembly.GetExecutingAssembly());
            model.Version = model.Build.ToString();
        }
        catch
        {
            model.Version = typeof(AboutProvider)
                   .GetTypeInfo()
                   .Assembly
                   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                   .InformationalVersion;

        }


        return await Task.FromResult(model);
    }
}
