using ControlSpark.RecipeManager.Interfaces;
using ControlSpark.RecipeManager.Models;

namespace ControlSpark.WebMvc.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ILogger<RecipeController> _logger;
        private readonly IScopeInformation _scopeInfo;
        private readonly IRecipeService _RecipeService;

        public RecipeController(ILogger<RecipeController> logger, IScopeInformation scopeInfo, IRecipeService RecipeService)
        {
            _RecipeService = RecipeService;
            _logger = logger;
            _scopeInfo = scopeInfo;
        }

        // GET: RecipeController
        public ActionResult Index()
        {
            return View(_RecipeService.Get());
        }

        // GET: RecipeController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View(_RecipeService.Get(id));
        }

        // GET: RecipeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RecipeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RecipeController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var rec = _RecipeService.Get(id);
            return View(rec);
        }

        // POST: RecipeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, RecipeModel item)
        {
            try
            {
                var RecipeToUpdate = _RecipeService.Get().Where(w => w.Id == id).FirstOrDefault();
                if (RecipeToUpdate != null)
                {
                    RecipeToUpdate.RecipeCategoryID = item.RecipeCategoryID;
                    RecipeToUpdate.AuthorNM = item.AuthorNM;
                    RecipeToUpdate.Description = item.Description;
                    RecipeToUpdate.Name = item.Name;
                    RecipeToUpdate.Ingredients = item.Ingredients;
                    RecipeToUpdate.Instructions = item.Instructions;
                    var saveResult = _RecipeService.Save(RecipeToUpdate);
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: RecipeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RecipeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
