using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBPv1._1.Model;
using RBPv1._1;

namespace RBPWEB.Controllers
{
    public class WorkloadController : Controller
    {
        RBPdbContext db;
        public WorkloadController(RBPdbContext context)
        {
            db = context;
        }
        private async Task LoadViewDataAsync()
        {
            ViewBag.Groups = await db.Groups.ToListAsync();
            ViewBag.Teachers = await db.Teachers.ToListAsync();
        }
        public async Task<IActionResult> Index(string searchQuery)
        {
            await LoadViewDataAsync();
            ViewData["SearchQuery"] = searchQuery;
            var worload = string.IsNullOrEmpty(searchQuery)
                ? db.Workloads
                : db.Workloads.Where(w => EF.Functions.Like(w.Subject, $"%{searchQuery}%"));
            return View(await worload.ToListAsync());
        }
        public async Task<IActionResult> Create()
        {
            await LoadViewDataAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Workload workload)
        {
            db.Workloads.Add(workload);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Edit(int? id)
        {
            await LoadViewDataAsync();
            if (id != null)
            {
                Workload? workload = await db.Workloads.FindAsync(id);
                if (workload != null) return View(workload);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Workload workload)
        {
            db.Workloads.Update(workload);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var w = await db.Workloads.FindAsync(id);
            if (id != null)
            {
                db.Workloads.Remove(w);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }



        public async Task<IActionResult> Delete()
        {
            await LoadViewDataAsync();
            return View(await db.Workloads.ToListAsync());
        }
    }
}
