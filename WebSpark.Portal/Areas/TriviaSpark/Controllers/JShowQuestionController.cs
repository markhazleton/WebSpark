using Microsoft.AspNetCore.Mvc.Rendering;
using TriviaSpark.JShow.Data;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers
{
    [Area("TriviaSpark")]
    public class JShowQuestionController : Controller
    {
        private readonly JShowDbContext _context;

        public JShowQuestionController(JShowDbContext context)
        {
            _context = context;
        }

        // GET: TriviaSpark/JShowQuestion
        public async Task<IActionResult> Index()
        {
            var jShowDbContext = _context.Questions.Include(q => q.Category);
            return View(await jShowDbContext.ToListAsync());
        }

        // GET: TriviaSpark/JShowQuestion/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionEntity = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionEntity == null)
            {
                return NotFound();
            }

            return View(questionEntity);
        }

        // GET: TriviaSpark/JShowQuestion/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        // POST: TriviaSpark/JShowQuestion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Value,QuestionText,Answer,Theme,Id,CreatedDate,ModifiedDate,CreatedBy,ModifiedBy")] QuestionEntity questionEntity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(questionEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", questionEntity.CategoryId);
            return View(questionEntity);
        }

        // GET: TriviaSpark/JShowQuestion/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionEntity = await _context.Questions.FindAsync(id);
            if (questionEntity == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", questionEntity.CategoryId);
            return View(questionEntity);
        }

        // POST: TriviaSpark/JShowQuestion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CategoryId,Value,QuestionText,Answer,Theme,Id,CreatedDate,ModifiedDate,CreatedBy,ModifiedBy")] QuestionEntity questionEntity)
        {
            if (id != questionEntity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionEntity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionEntityExists(questionEntity.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", questionEntity.CategoryId);
            return View(questionEntity);
        }

        // GET: TriviaSpark/JShowQuestion/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var questionEntity = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionEntity == null)
            {
                return NotFound();
            }

            return View(questionEntity);
        }

        // POST: TriviaSpark/JShowQuestion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var questionEntity = await _context.Questions.FindAsync(id);
            if (questionEntity != null)
            {
                _context.Questions.Remove(questionEntity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionEntityExists(string id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
