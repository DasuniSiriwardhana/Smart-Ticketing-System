using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    public class PROMO_CODEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PROMO_CODEController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Search Bar

        public async Task<IActionResult> Search(
            string mode,
            int? promoCodeId,
            string code,
            string discountType,
            decimal? minDiscount,
            decimal? maxDiscount,
            char? isActive,
            DateTime? fromStartDate,
            DateTime? toEndDate,
            DateTime? fromCreatedDate,
            DateTime? toCreatedDate)
    {
        var query = _context.Set<PROMO_CODE>().AsQueryable();

        if (mode == "PromoCodeID" && promoCodeId.HasValue)
            query = query.Where(p => p.PromoCodeID == promoCodeId.Value);

        else if (mode == "Code" && !string.IsNullOrWhiteSpace(code))
            query = query.Where(p => (p.code ?? "").Contains(code));

        else if (mode == "DiscountType" && !string.IsNullOrWhiteSpace(discountType))
            query = query.Where(p => (p.DiscountType ?? "").Contains(discountType));

        else if (mode == "DiscountRange")
        {
            if (minDiscount.HasValue)
                query = query.Where(p => p.DiscountValue >= minDiscount.Value);

            if (maxDiscount.HasValue)
                query = query.Where(p => p.DiscountValue <= maxDiscount.Value);
        }

        else if (mode == "Status" && isActive.HasValue)
            query = query.Where(p => p.isActive == isActive.Value);

        else if (mode == "ValidityRange")
        {
            if (fromStartDate.HasValue)
                query = query.Where(p => p.startDate >= fromStartDate.Value);

            if (toEndDate.HasValue)
                query = query.Where(p => p.endDate <= toEndDate.Value);
        }

        else if (mode == "Advanced")
        {
            if (promoCodeId.HasValue)
                query = query.Where(p => p.PromoCodeID == promoCodeId.Value);

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(p => (p.code ?? "").Contains(code));

            if (!string.IsNullOrWhiteSpace(discountType))
                query = query.Where(p => (p.DiscountType ?? "").Contains(discountType));

            if (minDiscount.HasValue)
                query = query.Where(p => p.DiscountValue >= minDiscount.Value);

            if (maxDiscount.HasValue)
                query = query.Where(p => p.DiscountValue <= maxDiscount.Value);

            if (isActive.HasValue)
                query = query.Where(p => p.isActive == isActive.Value);

            if (fromStartDate.HasValue)
                query = query.Where(p => p.startDate >= fromStartDate.Value);

            if (toEndDate.HasValue)
                query = query.Where(p => p.endDate <= toEndDate.Value);

            if (fromCreatedDate.HasValue)
                query = query.Where(p => p.createdAt >= fromCreatedDate.Value);

            if (toCreatedDate.HasValue)
                query = query.Where(p => p.createdAt < toCreatedDate.Value.AddDays(1));
        }

        return View("Index", await query.ToListAsync());
    }


    // GET: PROMO_CODE
    public async Task<IActionResult> Index()
        {
            return View(await _context.PROMO_CODE.ToListAsync());
        }

        // GET: PROMO_CODE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pROMO_CODE = await _context.PROMO_CODE
                .FirstOrDefaultAsync(m => m.PromoCodeID == id);
            if (pROMO_CODE == null)
            {
                return NotFound();
            }

            return View(pROMO_CODE);
        }

        // GET: PROMO_CODE/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PROMO_CODE/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PromoCodeID,code,DiscountType,DiscountValue,startDate,endDate,isActive,createdAt")] PROMO_CODE pROMO_CODE)
        {
            if (ModelState.IsValid)
            {
                _context.Add(pROMO_CODE);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pROMO_CODE);
        }

        // GET: PROMO_CODE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pROMO_CODE = await _context.PROMO_CODE.FindAsync(id);
            if (pROMO_CODE == null)
            {
                return NotFound();
            }
            return View(pROMO_CODE);
        }

        // POST: PROMO_CODE/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PromoCodeID,code,DiscountType,DiscountValue,startDate,endDate,isActive,createdAt")] PROMO_CODE pROMO_CODE)
        {
            if (id != pROMO_CODE.PromoCodeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pROMO_CODE);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PROMO_CODEExists(pROMO_CODE.PromoCodeID))
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
            return View(pROMO_CODE);
        }

        // GET: PROMO_CODE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pROMO_CODE = await _context.PROMO_CODE
                .FirstOrDefaultAsync(m => m.PromoCodeID == id);
            if (pROMO_CODE == null)
            {
                return NotFound();
            }

            return View(pROMO_CODE);
        }

        // POST: PROMO_CODE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pROMO_CODE = await _context.PROMO_CODE.FindAsync(id);
            if (pROMO_CODE != null)
            {
                _context.PROMO_CODE.Remove(pROMO_CODE);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PROMO_CODEExists(int id)
        {
            return _context.PROMO_CODE.Any(e => e.PromoCodeID == id);
        }
    }
}
