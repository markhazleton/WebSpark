using Microsoft.AspNetCore.Mvc;
using PromptSpark.Chat.Services;
using System.Text.Json;

namespace PromptSpark.Chat.Controllers;

public class HomeController(IHttpClientFactory factory) : Controller
{
    private readonly HttpClient _httpClient = factory.CreateClient("workflow");
    public IActionResult Index()
    {
        return View();
    }
    public IActionResult Chat()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Start()
    {
        // Call the API to start the workflow using only the path
        var response = await _httpClient.GetAsync("workflow/init");
        response.EnsureSuccessStatusCode();

        // Deserialize the response content
        var jsonContent = await response.Content.ReadAsStringAsync();
        var node = JsonSerializer.Deserialize<WorkflowNodeResponse>(jsonContent);

        return View("Node", node);
    }

    [HttpPost]
    public async Task<IActionResult> Step(string nextLink)
    {
        // Follow the provided next link for the user's selected answer
        var response = await _httpClient.GetAsync(nextLink);
        response.EnsureSuccessStatusCode();

        // Deserialize the response content
        var jsonContent = await response.Content.ReadAsStringAsync();
        var node = JsonSerializer.Deserialize<WorkflowNodeResponse>(jsonContent);

        // If node is null, we're at the end of the workflow
        if (node == null) return View("End");

        // Render the next node view
        return View("Node", node);
    }


}
