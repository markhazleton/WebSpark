using Serilog;
using System.Text.Json;
using WebSpark.Core.Extensions;

namespace WebSpark.Core.Providers;

public interface IStorageProvider
{
    bool FileExists(string path);
    Task<IList<string>> GetThemes();
    Task<Models.ThemeSettings> GetThemeSettings(string theme);
    Task<bool> SaveThemeSettings(string theme, Models.ThemeSettings settings);
    Task<string> UploadBase64Image(string baseImg, string root, string path = "");
    Task<bool> UploadFormFile(IFormFile file, string path = "");
    Task<string> UploadFromWeb(Uri requestUri, string root, string path = "");
}

public class StorageProvider : IStorageProvider
{
    private readonly string _slash = Path.DirectorySeparatorChar.ToString();
    private string _storageRoot;

    public StorageProvider()
    {
        _storageRoot = $"{ContentRoot}{_slash}wwwroot{_slash}data{_slash}";
    }

    string GetFileName(string fileName)
    {
        // some browsers pass uploaded file name as short file name 
        // and others include the path; remove path part if needed
        if (fileName.Contains(_slash))
        {
            fileName = fileName.Substring(fileName.LastIndexOf(_slash));
            fileName = fileName.Replace(_slash, string.Empty);
        }
        // when drag-and-drop or copy image to TinyMce editor
        // it uses "mceclip0" as file name; randomize it for multiple uploads
        if (fileName.StartsWith("mceclip0"))
        {
            Random rnd = new();
            fileName = fileName.Replace("mceclip0", rnd.Next(100000, 999999).ToString());
        }
        return fileName.SanitizePath();
    }

    static string GetImgSrcValue(string imgTag)
    {
        if (!(imgTag.Contains("data:image") && imgTag.Contains("src=")))
            return imgTag;

        int start = imgTag.IndexOf("src=");
        int srcStart = imgTag.IndexOf("\"", start) + 1;

        if (srcStart < 2)
            return imgTag;

        int srcEnd = imgTag.IndexOf("\"", srcStart);

        if (srcEnd < 1 || srcEnd <= srcStart)
            return imgTag;

        return imgTag.Substring(srcStart, srcEnd - srcStart);
    }

    string PathToUrl(string path)
    {
        string url = path.ReplaceIgnoreCase(_storageRoot, string.Empty).Replace(_slash, "/");
        return $"data/{url}";
    }

    static string TitleFromUri(Uri uri)
    {
        var title = uri.ToString().ToLower();
        title = title.Replace("%2f", "/");

        if (title.EndsWith(".axdx"))
        {
            title = title.Replace(".axdx", string.Empty);
        }
        if (title.Contains("image.axd?picture="))
        {
            title = title.Substring(title.IndexOf("image.axd?picture=") + 18);
        }
        if (title.Contains("file.axd?file="))
        {
            title = title.Substring(title.IndexOf("file.axd?file=") + 14);
        }
        if (title.Contains("encrypted-tbn") || title.Contains("base64,"))
        {
            Random rnd = new();
            title = $"{(rnd.Next(1000, 9999))}.png";
        }

        if (title.Contains("/"))
        {
            title = title.Substring(title.LastIndexOf("/"));
        }

        title = title.Replace(" ", "-");

        return title.Replace("/", string.Empty).SanitizeFileName();
    }

    void VerifyPath(string path)
    {
        path = path.SanitizePath();

        if (!string.IsNullOrEmpty(path))
        {
            var dir = Path.Combine(_storageRoot, path);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

    private string ContentRoot
    {
        get
        {
            string path = Directory.GetCurrentDirectory();
            string testsDirectory = $"tests{_slash}WebSpark.Tests";
            string appDirectory = $"src{_slash}ControlSpark";

            Log.Information($"Current directory path: {path}");

            // development unit test run
            if (path.LastIndexOf(testsDirectory) > 0)
            {
                path = path.Substring(0, path.LastIndexOf(testsDirectory));
                Log.Information($"Unit test path: {path}src{_slash}ControlSpark");
                return $"{path}src{_slash}ControlSpark";
            }

            // this needed to make sure we have correct data directory
            // when running in debug mode in Visual Studio
            // so instead of debug (src/ControlSpark/bin/Debug..)
            // will be used src/ControlSpark/wwwroot/data
            // !! this can mess up installs that have "src/ControlSpark" in the path !!
            if (path.LastIndexOf(appDirectory) > 0)
            {
                path = path.Substring(0, path.LastIndexOf(appDirectory));
                Log.Information($"Development debug path: {path}src{_slash}ControlSpark");
                return $"{path}src{_slash}ControlSpark";
            }
            Log.Information($"Final path: {path}");
            return path;
        }
    }

    public bool FileExists(string path)
    {
        Log.Information($"File exists: {Path.Combine(ContentRoot, path)}");
        return File.Exists(Path.Combine(ContentRoot, path));
    }

    public async Task<IList<string>> GetThemes()
    {
        var themes = new List<string>();
        var themesDirectory = Path.Combine(ContentRoot, $"Views{_slash}Themes");
        try
        {
            foreach (string dir in Directory.GetDirectories(themesDirectory))
            {
                themes.Add(Path.GetFileName(dir));
            }
        }
        catch { }
        return await Task.FromResult(themes);
    }

    public async Task<Models.ThemeSettings> GetThemeSettings(string theme)
    {
        var settings = new Models.ThemeSettings();
        var fileName = Path.Combine(ContentRoot, $"wwwroot{_slash}themes{_slash}{theme.ToLower()}{_slash}settings.json");
        if (File.Exists(fileName))
        {
            try
            {
                string jsonString = File.ReadAllText(fileName);
                settings = JsonSerializer.Deserialize<Models.ThemeSettings>(jsonString);
            }
            catch (Exception ex)
            {
                Log.Error($"Error reading theme settings: {ex.Message}");
                return null;
            }
        }

        return await Task.FromResult(settings);
    }

    public async Task<bool> SaveThemeSettings(string theme, Models.ThemeSettings settings)
    {
        var fileName = Path.Combine(ContentRoot, $"wwwroot{_slash}themes{_slash}{theme.ToLower()}{_slash}settings.json");
        try
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNameCaseInsensitive = true };

            string jsonString = JsonSerializer.Serialize(settings, options);

            using FileStream createStream = File.Create(fileName);
            await JsonSerializer.SerializeAsync(createStream, settings, options);
        }
        catch (Exception ex)
        {
            Log.Error($"Error writing theme settings: {ex.Message}");
            return false;
        }
        return true;
    }

    public async Task<string> UploadBase64Image(string baseImg, string root, string path = "")
    {
        path = path.Replace("/", _slash);
        var fileName = string.Empty;

        VerifyPath(path);
        string imgSrc = GetImgSrcValue(baseImg);

        Random rnd = new();

        if (imgSrc.StartsWith("data:image/png;base64,"))
        {
            fileName = $"{(rnd.Next(1000, 9999))}.png";
            imgSrc = imgSrc.Replace("data:image/png;base64,", string.Empty);
        }
        if (imgSrc.StartsWith("data:image/jpeg;base64,"))
        {
            fileName = $"{(rnd.Next(1000, 9999))}.jpeg";
            imgSrc = imgSrc.Replace("data:image/jpeg;base64,", string.Empty);
        }
        if (imgSrc.StartsWith("data:image/gif;base64,"))
        {
            fileName = $"{(rnd.Next(1000, 9999))}.gif";
            imgSrc = imgSrc.Replace("data:image/gif;base64,", string.Empty);
        }

        var filePath = string.IsNullOrEmpty(path) ?
             Path.Combine(_storageRoot, fileName) :
             Path.Combine(_storageRoot, $"{path}{_slash}{fileName}");

        await File.WriteAllBytesAsync(filePath, Convert.FromBase64String(imgSrc));

        return $"![{fileName}]({root}{PathToUrl(filePath)})";
    }

    public async Task<bool> UploadFormFile(IFormFile file, string path = "")
    {
        path = path.Replace("/", _slash);
        VerifyPath(path);

        var fileName = GetFileName(file.FileName);
        var filePath = string.IsNullOrEmpty(path) ?
             Path.Combine(_storageRoot, fileName) :
             Path.Combine(_storageRoot, $"{path}{_slash}{fileName}");

        Log.Information($"Storage root: {_storageRoot}");
        Log.Information($"Uploading file: {filePath}");
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                Log.Information($"Uploaded file: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error uploading file: {ex.Message}");
        }

        return true;
    }

    public async Task<string> UploadFromWeb(Uri requestUri, string root, string path = "")
    {
        path = path.Replace("/", _slash);
        VerifyPath(path);

        var fileName = TitleFromUri(requestUri);
        var filePath = string.IsNullOrEmpty(path) ?
             Path.Combine(_storageRoot, fileName) :
             Path.Combine(_storageRoot, $"{path}{_slash}{fileName}");

        HttpClient client = new();
        var response = await client.GetAsync(requestUri);
        using (var fs = new FileStream(filePath, FileMode.CreateNew))
        {
            await response.Content.CopyToAsync(fs);
            return await Task.FromResult($"![{fileName}]({root}{PathToUrl(filePath)})");
        }
    }
}
