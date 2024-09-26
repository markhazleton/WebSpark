using TriviaSpark.Domain.Models;
using TriviaSpark.Domain.Services;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers;
[Authorize]
public class TriviaMatchController : TriviaSparkBaseController
{
    private readonly ITriviaMatchService _matchService;
    public TriviaMatchController(ITriviaMatchService matchService)
    {
        _matchService = matchService;
    }

    // GET: /TriviaMatch/Index
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var matches = await _matchService.GetMatchesAsync(ct);
        return View(matches);
    }

    // GET: /TriviaMatch/Details/5
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var match = await _matchService.GetMatchAsync(id, ct);
        if (match == null)
        {
            return NotFound();
        }
        return View(match);
    }

    // GET: /TriviaMatch/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /TriviaMatch/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MatchModel match, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            await _matchService.CreateMatchAsync(match, null, ct);
            return RedirectToAction(nameof(Index));
        }
        return View(match);
    }

    // GET: /TriviaMatch/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var match = await _matchService.GetMatchAsync(id, ct);
        if (match == null)
        {
            return NotFound();
        }
        return View(match);
    }

    // POST: /TriviaMatch/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MatchModel match, CancellationToken ct)
    {
        if (id != match.MatchId)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            await _matchService.UpdateMatchAsync(match, ct);
            return RedirectToAction(nameof(Index));
        }
        return View(match);
    }

    // GET: /TriviaMatch/Delete/5
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var match = await _matchService.GetMatchAsync(id, ct);
        if (match == null)
        {
            return NotFound();
        }
        return View(match);
    }

    // POST: /TriviaMatch/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        await _matchService.DeleteUserMatchAsync(null, id, ct);
        return RedirectToAction(nameof(Index));
    }
}
