using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;

namespace WebSpark.Portal.Areas.PromptSpark.Controllers;

[Area("PromptSpark")]
public class ResponsesController(IResponseService responseService) : PromptSparkBaseController
{

    private bool GPTResponseExists(int id)
    {
        return responseService.ResponseExists(id);
    }

    // GET: Responses/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Responses/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ServiceDefinitionId,TestPrompt")] DefinitionResponseDto gPTResponse)
    {
        if (ModelState.IsValid)
        {
            await responseService.AddResponseAsync(gPTResponse);
            return RedirectToAction(nameof(Index));
        }
        return View(gPTResponse);
    }

    // GET: Responses/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var gPTResponse = await responseService.GetResponseByIdAsync(id);
        if (gPTResponse == null)
        {
            return NotFound();
        }

        return View(gPTResponse);
    }

    // POST: Responses/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await responseService.DeleteResponseAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // GET: Responses/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var gPTResponse = await responseService.GetResponseByIdAsync(id);
        if (gPTResponse == null)
        {
            return NotFound();
        }

        return View(gPTResponse);
    }

    // GET: Responses/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var gPTResponse = await responseService.GetResponseByIdAsync(id);
        if (gPTResponse == null)
        {
            return NotFound();
        }
        return View(gPTResponse);
    }

    // POST: Responses/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        DefinitionResponseDto gPTResponse)
    {
        if (id != gPTResponse.ResponseId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await responseService.UpdateResponseAsync(gPTResponse);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!responseService.ResponseExists(gPTResponse.ResponseId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(gPTResponse);
    }

    // GET: Responses
    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await responseService.GetAllResponsesAsync();
            return View(list);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return View(new List<DefinitionResponseDto>());
        }
    }
}
