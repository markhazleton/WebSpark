using PromptSpark.Domain.Models;
using PromptSpark.Domain.Service;

namespace PromptSpark.Areas.OpenAI.Controllers;
[Authorize]
public class DefinitionTypesController(IGPTDefinitionTypeService service) : OpenAIBaseController
{

    private bool GPTDefinitionTypeExists(string id)
    {
        return service.GetGPTDefinitionTypeByKey(id) != null;
    }

    // GET: DefinitionTypes/Create
    public IActionResult Create()
    {
        return View("Edit", new DefinitionTypeDto());
    }
    // GET: DefinitionTypes/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var definitionType = await service.GetGPTDefinitionTypeByKey(id);
        if (definitionType == null)
        {
            return NotFound();
        }
        return View(definitionType);
    }

    // POST: DefinitionTypes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await service.DeleteGPTDefinitionType(id);
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Details(string id)
    {
        var definitionType = await service.GetGPTDefinitionTypeByKey(id);
        if (definitionType == null)
        {
            return NotFound();
        }
        return View(definitionType);
    }

    // GET: DefinitionTypes/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        var definitionType = await service.GetGPTDefinitionTypeByKey(id);
        if (definitionType == null)
        {
            return NotFound();
        }
        return View(definitionType);
    }

    // POST: DefinitionTypes/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, DefinitionTypeDto gPTDefinitionType)
    {
        try
        {
            await service.UpdateGPTDefinitionType(gPTDefinitionType);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GPTDefinitionTypeExists(gPTDefinitionType.DefinitionType))
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
    public async Task<IActionResult> Index()
    {
        var list = await service.GetAllGPTDefinitionTypes();
        return View(list);
    }
}
