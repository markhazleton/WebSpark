﻿using System.Diagnostics;
using TriviaSpark.JShow.Models;
using TriviaSpark.JShow.Service;
using WebSpark.HttpClientUtility.MemoryCache;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;

/// <summary>
/// TriviaSparkBaseController 
/// </summary>
[Area("TriviaSpark")]
public class TriviaSparkBaseController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService) : Controller
{
    protected readonly IMemoryCacheManager _memoryCacheManager = memoryCacheManager;
    protected readonly IJShowService _jShowService = jShowService;
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    protected async Task<List<JShowVM>> GetJShowList(JShowVM newShow = null)
    {
        var shows = await _memoryCacheManager.Get("JShowList", async () =>
        {
            var list = await _jShowService.GetJShowsAsync() ?? [];
            _memoryCacheManager.Set("JShowList", list, 30);
            return list;
        });
        if (newShow == null)
        {
            return shows;
        }
        // look for the show in the list by theme
        var existingShow = shows.FirstOrDefault(s => s.Theme == newShow.Theme);
        if (existingShow == null)
        {
            newShow = await _jShowService.CreateJShowAsync(newShow);
            shows = await _jShowService.GetJShowsAsync() ?? [];
            _memoryCacheManager.Set("JShowList", shows, 30);
            return shows;
        }
        return shows;
    }





}