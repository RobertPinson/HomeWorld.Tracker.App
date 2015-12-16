using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Tracker.Models;

namespace Tracker.Controllers
{
    [Produces("application/json")]
    [Route("api/Track")]
    public class TrackController : Controller
    {
        private TrackerDbContext _context;

        public TrackController(TrackerDbContext context)
        {
            _context = context;
        }

        // GET: api/Track
        [HttpGet]
        public IEnumerable<Person> GetPerson()
        {
            return _context.Person;
        }

        // GET: api/Track/5

        [ResponseCache(Duration = 10)]
        [HttpGet("{uid}", Name = "GetPerson")]
        public IActionResult GetPerson([FromRoute] string uid)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            var person = (from c in _context.Card
                join pc in _context.PersonCard on c.Id equals pc.CardId
                join p in _context.Person on pc.PersonId equals p.Id
                where c.Uid.Equals(uid)
                select p).FirstOrDefault();

            if (person == null)
            {
                return HttpNotFound();
            }

            return Ok(person);
        }

        // PUT: api/Track/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson([FromRoute] int id, [FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != person.Id)
            {
                return HttpBadRequest();
            }

            _context.Entry(person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/Track
        [HttpPost]
        public async Task<IActionResult> PostPerson([FromBody] Person person)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            _context.Person.Add(person);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PersonExists(person.Id))
                {
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetPerson", new { id = person.Id }, person);
        }

        // DELETE: api/Track/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            Person person = await _context.Person.SingleAsync(m => m.Id == id);
            if (person == null)
            {
                return HttpNotFound();
            }

            _context.Person.Remove(person);
            await _context.SaveChangesAsync();

            return Ok(person);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PersonExists(int id)
        {
            return _context.Person.Count(e => e.Id == id) > 0;
        }
    }
}