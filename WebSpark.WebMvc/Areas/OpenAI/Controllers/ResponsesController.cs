using Microsoft.EntityFrameworkCore;
using PromptSpark.Areas.OpenAI.Data;

namespace PromptSpark.Areas.OpenAI.Controllers
{
    public class ResponsesController : OpenAIBaseController
    {
        private readonly GPTDbContext _context;

        public ResponsesController(GPTDbContext context)
        {
            _context = context;
        }

        // GET: Responses
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await _context.Chats.ToListAsync();
                return View(list);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View(new GPTUserPrompt());
            }
        }

        // GET: Responses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gPTResponse = await _context.Chats
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gPTResponse == null)
            {
                return NotFound();
            }

            return View(gPTResponse);
        }

        // GET: Responses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Responses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceDefintionId,TestPrompt")] GPTUserPrompt gPTResponse)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gPTResponse);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            var gPTResponse = await _context.Chats.FindAsync(id);
            if (gPTResponse == null)
            {
                return NotFound();
            }
            return View(gPTResponse);
        }

        // POST: Responses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceDefintionId,TestPrompt")] GPTUserPrompt gPTResponse)
        {
            if (id != gPTResponse.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gPTResponse);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GPTResponseExists(gPTResponse.Id))
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

        // GET: Responses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gPTResponse = await _context.Chats
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var gPTResponse = await _context.Chats.FindAsync(id);
            if (gPTResponse != null)
            {
                _context.Chats.Remove(gPTResponse);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GPTResponseExists(int id)
        {
            return _context.Chats.Any(e => e.Id == id);
        }
    }
}
