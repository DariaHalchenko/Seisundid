using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seisundid.Data;
using Seisundid.Models;

namespace Seisundid.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaevaGraafikController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaevaGraafikController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/paevagraafik
        [HttpGet]
        public ActionResult<List<PaevaGraafik>> GetPaevaGraafiks()
        {
            // Tagastame kõik päevagraafikud
            return _context.PaevaGraafiks.ToList();
        }

        // POST: api/paevagraafik/save-many-graafikud
        [HttpPost("save-many-graafikud")]
        public ActionResult<List<PaevaGraafik>> SaveManyGraafikud([FromBody] List<PaevaGraafik> graafikud)
        {
            foreach (var g in graafikud)
            {
                // Kontrollime kohustuslikke välju
                if (g.PoodId == 0)
                    return BadRequest("PoodId on vajalik iga PaevaGraafik jaoks");

                var pood = _context.Poed.Find(g.PoodId);
                if (pood == null)
                    return BadRequest($"Pood ID={g.PoodId} ei leitud.");

                if (string.IsNullOrWhiteSpace(g.AvatudAlates) || string.IsNullOrWhiteSpace(g.AvatudKuni))
                    return BadRequest($"AvatudAlates ja AvatudKuni tuleb märkida päev {g.Paev}");

                // Kontrollime, et aeg oleks korrektne
                if (!TimeSpan.TryParse(g.AvatudAlates, out _) || !TimeSpan.TryParse(g.AvatudKuni, out _))
                    return BadRequest($"AvatudAlates või AvatudKuni ei ole korrektne TimeSpan: päev {g.Paev}");
            }

            _context.PaevaGraafiks.AddRange(graafikud);
            _context.SaveChanges();

            // Tagastame kõik salvestatud graafikud
            var result = _context.PaevaGraafiks
                .Where(g => graafikud.Select(x => x.PoodId).Contains(g.PoodId))
                .ToList();

            return Ok(result);
        }

        // PUT: api/paevagraafik/5
        [HttpPut("{id}")]
        public ActionResult<List<PaevaGraafik>> PutPaevaGraafik(int id, [FromBody] PaevaGraafik updates)
        {
            var graafik = _context.PaevaGraafiks.Find(id);
            if (graafik == null)
                return NotFound($"Graafikut ID={id} ei leitud.");

            // Uuendame graafiku väljad
            graafik.Paev = updates.Paev;
            graafik.AvatudAlates = updates.AvatudAlates;
            graafik.AvatudKuni = updates.AvatudKuni;

            _context.PaevaGraafiks.Update(graafik);
            _context.SaveChanges();

            return Ok(_context.PaevaGraafiks.ToList());
        }

        // DELETE: api/paevagraafik/5
        [HttpDelete("{id}")]
        public ActionResult<List<PaevaGraafik>> DeletePaevaGraafik(int id)
        {
            var graafik = _context.PaevaGraafiks.Find(id);
            if (graafik != null)
            {
                _context.PaevaGraafiks.Remove(graafik);
                _context.SaveChanges();
            }

            return _context.PaevaGraafiks.ToList();
        }
    }
}
