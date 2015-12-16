using System.Threading.Tasks;
using HomeWorld.Tracker.Web.Models;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;

namespace HomeWorld.Tracker.Web.Controllers
{
    public class PeopleController : Controller
    {
        private readonly TrackerDbContext _context;

        public PeopleController(TrackerDbContext context)
        {
            _context = context;    
        }

        // GET: People
        public async Task<IActionResult> Index()
        {
            return View(await _context.Person.ToListAsync());
        }

        // GET: People/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Person person = await _context.Person.SingleAsync(m => m.Id == id);
            if (person == null)
            {
                return HttpNotFound();
            }

            return View(person);
        }

        // GET: People/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: People/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            if (ModelState.IsValid)
            {
                _context.Person.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(person);
        }

        // GET: People/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Person person = await _context.Person.SingleAsync(m => m.Id == id);
            if (person == null)
            {
                return HttpNotFound();
            }
            return View(person);
        }

        // POST: People/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Person person)
        {
            if (ModelState.IsValid)
            {
                _context.Update(person);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(person);
        }

        // GET: People/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Person person = await _context.Person.SingleAsync(m => m.Id == id);
            if (person == null)
            {
                return HttpNotFound();
            }

            return View(person);
        }

        // POST: People/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Person person = await _context.Person.SingleAsync(m => m.Id == id);
            _context.Person.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
