using System.Runtime.InteropServices;
using WebSpark.Core.Data;

namespace WebSpark.Core.Providers;

public interface IAboutProvider
{
    Task<Models.AboutModel> GetAboutModel();
}

public class AboutProvider : IAboutProvider
{
    private readonly WebSparkDbContext _db;

    public AboutProvider(WebSparkDbContext db)
    {
        _db = db;
    }
    public async Task<Models.AboutModel> GetAboutModel()
    {
        var model = new Models.AboutModel();
        model.DatabaseProvider = _db.Database.ProviderName;
        model.OperatingSystem = RuntimeInformation.OSDescription;
        try
        {
            model.Build = new Models.BuildVersion(Assembly.GetExecutingAssembly());
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
