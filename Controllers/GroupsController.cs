using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RBPv1._1;
using RBPv1._1.Model;

namespace RBPWEB.Controllers
{
    public class GroupsController : Controller
    {
        private RBPdbContext db;
        public GroupsController(RBPdbContext context)
        {
            db = context;
        }
        public async Task<IActionResult> Index(string searchQuery)
        {

            ViewData["SearchQuery"] = searchQuery;
            var groups = string.IsNullOrEmpty(searchQuery)
                ? db.Groups
                : db.Groups.Where(g => EF.Functions.Like(g.GroupName, $"%{searchQuery}%"));
            return View(await groups.ToListAsync());
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Groups group)
        {
            db.Groups.Add(group);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if(id != null)
            {
                Groups? group = await db.Groups.FindAsync(id);
                if(group != null) return View(group);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Groups group)
        {
            db.Groups.Update(group);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await db.Groups.FindAsync(id);
            if(id != null)
            {
                db.Groups.Remove(group);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        
        
        public async Task<IActionResult> Delete()
        {
            return View(await db.Groups.ToListAsync());
        }
    }
}
