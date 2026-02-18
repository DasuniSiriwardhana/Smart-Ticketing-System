using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;

namespace SmartTicketingSystem.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class PROMO_CODEController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PROMO_CODEController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                if (minDiscount.HasValue) query = query.Where(p => p.DiscountValue >= minDiscount.Value);
                if (maxDiscount.HasValue) query = query.Where(p => p.DiscountValue <= maxDiscount.Value);
            }

            else if (mode == "Status" && isActive.HasValue)
                query = query.Where(p => p.isActive == isActive.Value);

            else if (mode == "ValidityRange")
            {
                if (fromStartDate.HasValue) query = query.Where(p => p.startDate >= fromStartDate.Value);
                if (toEndDate.HasValue) query = query.Where(p => p.endDate <= toEndDate.Value);
            }

            else if (mode == "Advanced")
            {
                if (promoCodeId.HasValue) query = query.Where(p => p.PromoCodeID == promoCodeId.Value);

                if (!string.IsNullOrWhiteSpace(code))
                    query = query.Where(p => (p.code ?? "").Contains(code));

                if (!string.IsNullOrWhiteSpace(discountType))
                    query = query.Where(p => (p.DiscountType ?? "").Contains(discountType));

                if (minDiscount.HasValue) query = query.Where(p => p.DiscountValue >= minDiscount.Value);
                if (maxDiscount.HasValue) query = query.Where(p => p.DiscountValue <= maxDiscount.Value);

                if (isActive.HasValue) query = query.Where(p => p.isActive == isActive.Value);

                if (fromStartDate.HasValue) query = query.Where(p => p.startDate >= fromStartDate.Value);
                if (toEndDate.HasValue) query = query.Where(p => p.endDate <= toEndDate.Value);

                if (fromCreatedDate.HasValue) query = query.Where(p => p.createdAt >= fromCreatedDate.Value);
                if (toCreatedDate.HasValue) query = query.Where(p => p.createdAt < toCreatedDate.Value.AddDays(1));
            }

            return View("Index", await query.ToListAsync());
        }

        public async Task<IActionResult> Index()
            => View(await _context.PROMO_CODE.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PROMO_CODE.FirstOrDefaultAsync(m => m.PromoCodeID == id);
            if (item == null) return NotFound();

            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PromoCodeID,code,DiscountType,DiscountValue,startDate,endDate,isActive,createdAt")] PROMO_CODE item)
        {
            if (ModelState.IsValid)
            {
                item.createdAt = DateTime.Now;
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PROMO_CODE.FindAsync(id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PromoCodeID,code,DiscountType,DiscountValue,startDate,endDate,isActive,createdAt")] PROMO_CODE item)
        {
            if (id != item.PromoCodeID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PROMO_CODE.FirstOrDefaultAsync(m => m.PromoCodeID == id);
            if (item == null) return NotFound();

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.PROMO_CODE.FindAsync(id);
            if (item != null) _context.PROMO_CODE.Remove(item);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
