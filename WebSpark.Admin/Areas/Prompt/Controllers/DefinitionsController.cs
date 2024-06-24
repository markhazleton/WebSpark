using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;

namespace WebSpark.Admin.Areas.Prompt.Controllers;

public class DefinitionsController(IGPTDefinitionService definitionService) : PromptBaseController
{
    /// <summary>
    /// Refresh a prompt.
    /// </summary>
    /// <param name="userPromptDto">The GPT userPromptDto.</param>
    /// <returns>The details view for the created prompt.</returns>
    [HttpGet]
    public async Task<ActionResult> Refresh(int id)
    {
        try
        {
            DefinitionDto response = await definitionService.RefreshDefinitionResponses(id);
            return View("Details", response);
        }
        catch
        {
            return View();
        }
    }


    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await definitionService.GetDefinitionsAsync();
            return View(list);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return View(new List<DefinitionDto>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var definition = await definitionService.GetDefinitionDtoAsync(id);
        return View(definition);
    }

    public IActionResult Create()
    {
        return View(new DefinitionDto());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DefinitionDto gPTDefinition)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var newDefinition = await definitionService.CreateAsync(gPTDefinition);
                return RedirectToAction(nameof(Index));

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(gPTDefinition);

            }
        }
        return View(gPTDefinition);
    }

    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        return View(await definitionService.GetDefinitionDtoAsync(id));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DefinitionDto gPTDefinition)
    {

        if (id == 0)
        {
            gPTDefinition.Created = DateTime.UtcNow;
        }
        gPTDefinition.Updated = DateTime.UtcNow;

        if (ModelState.IsValid)
        {
            try
            {
                await definitionService.UpdateDefinitionAsync(gPTDefinition);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await definitionService.GPTDefinitionExists(gPTDefinition.DefinitionId))
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
        return View(gPTDefinition);
    }

    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var gPTDefinition = await definitionService.GetDefinitionDtoAsync(id);
        return View(gPTDefinition);
    }

    // POST: Definitions/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var gPTDefinition = await definitionService.GetDefinitionDtoAsync(id);
        if (gPTDefinition.DefinitionId > 0)
        {
            await definitionService.DeleteDefinitionAsync(gPTDefinition.DefinitionId);
        }
        return RedirectToAction(nameof(Index));
    }
}
