using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlSpark.MineralManager.Entities;

namespace ControlSpark.WebMvc.Areas.MineralCollection.Controllers
{
    [Area("MineralCollection")]
    public class CollectionItemsController(MineralDbContext context) : Controller
    {
        private readonly MineralDbContext _context = context;

        // GET: MineralCollection/CollectionItems
        public async Task<IActionResult> Index()
        {
            var mineralDbContext = _context.CollectionItems.Include(c => c.Collection).Include(c => c.LocationCity).Include(c => c.LocationCountry).Include(c => c.LocationState).Include(c => c.PrimaryMineral).Include(c => c.PurchasedFromCompany);
            return View(await mineralDbContext.ToListAsync());
        }

        // GET: MineralCollection/CollectionItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collectionItem = await _context.CollectionItems
                .Include(c => c.Collection)
                .Include(c => c.LocationCity)
                .Include(c => c.LocationCountry)
                .Include(c => c.LocationState)
                .Include(c => c.PrimaryMineral)
                .Include(c => c.PurchasedFromCompany)
                .FirstOrDefaultAsync(m => m.CollectionItemId == id);
            if (collectionItem == null)
            {
                return NotFound();
            }

            return View(collectionItem);
        }

        // GET: MineralCollection/CollectionItems/Create
        public IActionResult Create()
        {
            ViewData["CollectionId"] = new SelectList(_context.Collections, "CollectionId", "CollectionNm");
            ViewData["LocationCityId"] = new SelectList(_context.LocationCities, "LocationCityId", "City");
            ViewData["LocationCountryId"] = new SelectList(_context.LocationCountries, "LocationCountryId", "CountryNm");
            ViewData["LocationStateId"] = new SelectList(_context.LocationStates, "LocationStateId", "StateNm");
            ViewData["PrimaryMineralId"] = new SelectList(_context.Minerals, "MineralId", "MineralNm");
            ViewData["PurchasedFromCompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyNm");
            return View();
        }

        // POST: MineralCollection/CollectionItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CollectionItemId,CollectionId,SpecimenNumber,Nickname,PrimaryMineralId,MineralVariety,MineNm,PurchaseDate,PurchasePrice,Value,ShowWherePurchased,PurchasedFromCompanyId,StorageLocation,SpecimenNotes,Description,ExCollection,HeightCm,WidthCm,ThicknessCm,HeightIn,WidthIn,ThicknessIn,WeightGr,WeightKg,SaleDt,SalePrice,LocationCityId,LocationStateId,LocationCountryId,ModifiedId,ModifiedDt,IsFeatured,IsSold")] CollectionItem collectionItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(collectionItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollectionId"] = new SelectList(_context.Collections, "CollectionId", "CollectionNm", collectionItem.CollectionId);
            ViewData["LocationCityId"] = new SelectList(_context.LocationCities, "LocationCityId", "City", collectionItem.LocationCityId);
            ViewData["LocationCountryId"] = new SelectList(_context.LocationCountries, "LocationCountryId", "CountryNm", collectionItem.LocationCountryId);
            ViewData["LocationStateId"] = new SelectList(_context.LocationStates, "LocationStateId", "StateNm", collectionItem.LocationStateId);
            ViewData["PrimaryMineralId"] = new SelectList(_context.Minerals, "MineralId", "MineralNm", collectionItem.PrimaryMineralId);
            ViewData["PurchasedFromCompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyNm", collectionItem.PurchasedFromCompanyId);
            return View(collectionItem);
        }

        // GET: MineralCollection/CollectionItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collectionItem = await _context.CollectionItems.FindAsync(id);
            if (collectionItem == null)
            {
                return NotFound();
            }
            ViewData["CollectionId"] = new SelectList(_context.Collections, "CollectionId", "CollectionNm", collectionItem.CollectionId);
            ViewData["LocationCityId"] = new SelectList(_context.LocationCities, "LocationCityId", "City", collectionItem.LocationCityId);
            ViewData["LocationCountryId"] = new SelectList(_context.LocationCountries, "LocationCountryId", "CountryNm", collectionItem.LocationCountryId);
            ViewData["LocationStateId"] = new SelectList(_context.LocationStates, "LocationStateId", "StateNm", collectionItem.LocationStateId);
            ViewData["PrimaryMineralId"] = new SelectList(_context.Minerals, "MineralId", "MineralNm", collectionItem.PrimaryMineralId);
            ViewData["PurchasedFromCompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyNm", collectionItem.PurchasedFromCompanyId);
            return View(collectionItem);
        }

        // POST: MineralCollection/CollectionItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CollectionItemId,CollectionId,SpecimenNumber,Nickname,PrimaryMineralId,MineralVariety,MineNm,PurchaseDate,PurchasePrice,Value,ShowWherePurchased,PurchasedFromCompanyId,StorageLocation,SpecimenNotes,Description,ExCollection,HeightCm,WidthCm,ThicknessCm,HeightIn,WidthIn,ThicknessIn,WeightGr,WeightKg,SaleDt,SalePrice,LocationCityId,LocationStateId,LocationCountryId,ModifiedId,ModifiedDt,IsFeatured,IsSold")] CollectionItem collectionItem)
        {
            if (id != collectionItem.CollectionItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collectionItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollectionItemExists(collectionItem.CollectionItemId))
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
            ViewData["CollectionId"] = new SelectList(_context.Collections, "CollectionId", "CollectionNm", collectionItem.CollectionId);
            ViewData["LocationCityId"] = new SelectList(_context.LocationCities, "LocationCityId", "City", collectionItem.LocationCityId);
            ViewData["LocationCountryId"] = new SelectList(_context.LocationCountries, "LocationCountryId", "CountryNm", collectionItem.LocationCountryId);
            ViewData["LocationStateId"] = new SelectList(_context.LocationStates, "LocationStateId", "StateNm", collectionItem.LocationStateId);
            ViewData["PrimaryMineralId"] = new SelectList(_context.Minerals, "MineralId", "MineralNm", collectionItem.PrimaryMineralId);
            ViewData["PurchasedFromCompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyNm", collectionItem.PurchasedFromCompanyId);
            return View(collectionItem);
        }

        // GET: MineralCollection/CollectionItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collectionItem = await _context.CollectionItems
                .Include(c => c.Collection)
                .Include(c => c.LocationCity)
                .Include(c => c.LocationCountry)
                .Include(c => c.LocationState)
                .Include(c => c.PrimaryMineral)
                .Include(c => c.PurchasedFromCompany)
                .FirstOrDefaultAsync(m => m.CollectionItemId == id);
            if (collectionItem == null)
            {
                return NotFound();
            }

            return View(collectionItem);
        }

        // POST: MineralCollection/CollectionItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var collectionItem = await _context.CollectionItems.FindAsync(id);
            if (collectionItem != null)
            {
                _context.CollectionItems.Remove(collectionItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CollectionItemExists(int id)
        {
            return _context.CollectionItems.Any(e => e.CollectionItemId == id);
        }
    }
}
