using HttpClientUtility.MemoryCache;
using Microsoft.AspNetCore.Mvc.Rendering;
using TriviaSpark.JShow.Data;
using TriviaSpark.JShow.Models;
using TriviaSpark.JShow.Service;

namespace WebSpark.Portal.Areas.TriviaSpark.Controllers
{
    [Area("TriviaSpark")]
    public class JShowQuestionController(
    IMemoryCacheManager memoryCacheManager,
    IJShowService jShowService,
    JShowDbContext context) : TriviaSparkBaseController(memoryCacheManager, jShowService)
    {
        private readonly JShowDbContext _context = context;


        // GET: TriviaSpark/JShowQuestion
        public async Task<IActionResult> Index()
        {
            var questions = await _jShowService.GetQuestionVMsAsync();
            return View(questions);
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
            var questionVM = await _jShowService.GetQuestionByIdAsync(id);
            return View(questionVM);
        }

        // POST: TriviaSpark/JShowQuestion/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, QuestionVM questionVM)
        {
            if (id != questionVM.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                questionVM = await _jShowService.UpdateQuestionAsync(questionVM);
                return RedirectToAction(nameof(Index));
            }
            return View(questionVM);
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
