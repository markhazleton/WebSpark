using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;

namespace WebSpark.Admin.Areas.Prompt.Controllers;


public class UserPromptsController(IUserPromptService userPromptService) : PromptBaseController
{
    /// <summary>
    /// Creates a new prompt.
    /// </summary>
    /// <param name="request">The GPT request.</param>
    /// <returns>The details view for the created prompt.</returns>
    [HttpGet]
    public async Task<ActionResult> Refresh(int id)
    {
        try
        {
            var response = await userPromptService.RefreshDefinitionResponses(id);
            return View("Details", response);
        }
        catch
        {
            return NotFound();
        }
    }


    // GET: TestPrompts
    public async Task<IActionResult> Index()
    {
        var prompts = await userPromptService.GetAllAsync();
        return View(prompts);
    }

    // GET: TestPrompts/Details/5
    public async Task<IActionResult> Details(int id)
    {
        UserPromptDto promptDto = await userPromptService.ReadAsync(id);
        if (promptDto != null)
        {
            return View(promptDto);
        }
        promptDto = new();
        return View(promptDto);
    }

    // GET: TestPrompts/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        UserPromptDto promptDto = await userPromptService.ReadAsync(id);
        if (promptDto == null)
        {
            promptDto = new();
        }
        return View(promptDto);
    }

    // POST: TestPrompts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserPromptDto promptDto)
    {
        if (id != promptDto.UserPromptId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await userPromptService.CreateOrUpdateAsync(promptDto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserPromptExists(promptDto.UserPromptId))
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
        return View(promptDto);
    }

    // GET: TestPrompts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var promptDto = await userPromptService.ReadAsync(id.Value);
        if (promptDto == null)
        {
            return NotFound();
        }

        return View(promptDto);
    }

    // POST: TestPrompts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await userPromptService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> UserPromptExists(int id)
    {
        var promptDto = await userPromptService.ReadAsync(id);
        return promptDto != null;
    }
}
