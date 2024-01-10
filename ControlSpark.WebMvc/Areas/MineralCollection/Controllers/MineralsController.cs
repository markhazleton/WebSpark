using ControlSpark.MineralManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace ControlSpark.WebMvc.Areas.MineralCollection.Controllers;

[Area("MineralCollection")]
public class MineralsController(MineralDbContext context) : Controller
{
    private readonly MineralDbContext _context = context;

    // GET: MineralCollection/Minerals
    public async Task<IActionResult> Index()
    {
        return View(await _context.Minerals.Include(i=>i.CollectionItems).ToListAsync());
    }

    // GET: MineralCollection/Minerals/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mineral = await _context.Minerals.Include(m => m.CollectionItems)
            .FirstOrDefaultAsync(m => m.MineralId == id);
        if (mineral == null)
        {
            return NotFound();
        }

        return View(mineral);
    }

    // GET: MineralCollection/Minerals/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: MineralCollection/Minerals/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MineralId,MineralNm,MineralDs,WikipediaUrl,ModifiedId,ModifiedDt")] Mineral mineral)
    {
        if (ModelState.IsValid)
        {
            _context.Add(mineral);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(mineral);
    }

    // GET: MineralCollection/Minerals/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mineral = await _context.Minerals.FindAsync(id);
        if (mineral == null)
        {
            return NotFound();
        }
        return View(mineral);
    }

    // POST: MineralCollection/Minerals/Edit/5
    // To protect from over posting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="mineral"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MineralId,MineralNm,MineralDs,WikipediaUrl,ModifiedId,ModifiedDt")] Mineral mineral)
    {
        if (id != mineral.MineralId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                mineral.ModifiedDt = DateTime.Now;
                mineral.ModifiedId = 1;
                _context.Update(mineral);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MineralExists(mineral.MineralId))
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
        return View(mineral);
    }

    // GET: MineralCollection/Minerals/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var mineral = await _context.Minerals
            .FirstOrDefaultAsync(m => m.MineralId == id);
        if (mineral == null)
        {
            return NotFound();
        }

        return View(mineral);
    }

    // POST: MineralCollection/Minerals/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var mineral = await _context.Minerals.FindAsync(id);
        if (mineral != null)
        {
            _context.Minerals.Remove(mineral);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool MineralExists(int id)
    {
        return _context.Minerals.Any(e => e.MineralId == id);
    }
}
