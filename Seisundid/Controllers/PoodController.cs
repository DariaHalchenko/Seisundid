using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seisundid.Data;
using Seisundid.Models;

namespace Seisundid.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoodController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PoodController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/pood
        [HttpGet]
        public ActionResult<List<object>> GetPoodd()
        {
            var poed = _context.Poed.Include(p => p.Graafik).ToList();

            // Получаем текущее время и день недели 
            var now = DateTime.Now;
            var currentTime = now.TimeOfDay;
            var currentDay = now.DayOfWeek;

            // Формируем результат для каждого магазина
            var result = poed.Select(p =>
            {
                // Определяем, открыт ли магазин в текущий момент
                var isOpen = IsOpen(p, currentTime, currentDay);

                return new
                {
                    p.Id,
                    p.Nimi,
                    TananePaev = currentDay.ToString(), // Текущий день недели 
                    PraeguneAeg = new TimeSpan(currentTime.Hours, currentTime.Minutes, currentTime.Seconds)
                                  .ToString(@"hh\:mm\:ss"), // Текущее время
                    OnAvatud = isOpen, // Статус магазина: открыт или закрыт

                    Graafik = p.Graafik.Select(g => new
                    {
                        g.Paev,
                        g.AvatudAlates,
                        g.AvatudKuni
                    })
                };
            });

            return Ok(result);
        }

        // POST: api/pood/save-many-poed
        [HttpPost("save-many-poed")]
        public ActionResult<List<Pood>> CreateMultiple([FromBody] List<Pood> newPoed)
        {
            foreach (var pood in newPoed)
            {
                if (string.IsNullOrWhiteSpace(pood.Nimi))
                    return BadRequest("Igal poel peab olema väli Nimi");

                // määrame automaatselt algväärtused
                pood.TananePaev = DayOfWeek.Monday;
                pood.PraeguneAeg = TimeSpan.Zero;
                pood.OnAvatud = false;
                pood.Graafik = new List<PaevaGraafik>();
            }

            _context.Poed.AddRange(newPoed);
            _context.SaveChanges();

            return Ok(newPoed);
        }

        // Kontrollige, kas kõik poed on kindlal ajal avatud
        [HttpGet("check-time")]
        public ActionResult<List<string>> CheckByTime([FromQuery] TimeSpan time)
        {
            var poodList = _context.Poed.Include(p => p.Graafik).ToList();

            var results = poodList.Select(p =>
            {
                bool open = p.Graafik.Any(g =>
                {
                    if (!TimeSpan.TryParse(g.AvatudAlates, out var alates)) return false;
                    if (!TimeSpan.TryParse(g.AvatudKuni, out var kuni)) return false;
                    return time >= alates && time < kuni;
                });
                return $"{p.Nimi}: {(open ? "avatud" : "suletud")}";
            }).ToList();

            return results;
        }

        // Kontrolli, kas kõik poed on avatud vastavalt nädalapäevale ja kellaajale
        [HttpGet("check")]
        public ActionResult<List<string>> CheckByDayAndTime([FromQuery] DayOfWeek day, [FromQuery] TimeSpan time)
        {
            var poodList = _context.Poed.Include(p => p.Graafik).ToList();

            var results = poodList.Select(p =>
            {
                var g = p.Graafik.FirstOrDefault(g => g.Paev == day);
                if (g == null) return $"{p.Nimi}: suletud";

                if (!TimeSpan.TryParse(g.AvatudAlates, out var alates)) return $"{p.Nimi}: suletud";
                if (!TimeSpan.TryParse(g.AvatudKuni, out var kuni)) return $"{p.Nimi}: suletud";

                bool open = time >= alates && time < kuni;
                return $"{p.Nimi}: {(open ? "avatud" : "suletud")}";
            }).ToList();

            return results;
        }

        // +1 tund konkreetse poe jaoks
        [HttpPost("{id}/add-hour")]
        public ActionResult<string> AddHour(int id)
        {
            // Leidke pood ID järgi koos graafikuga
            var pood = _context.Poed.Include(p => p.Graafik).FirstOrDefault(p => p.Id == id);
            if (pood == null)
            {
                return NotFound($"Pood ID={id} ei leitud.");
            }

            bool wasOpen = IsOpen(pood, DateTime.Now.TimeOfDay, pood.TananePaev);

            // Lisame poe lahtiolekuajale 1 tunni
            var g = pood.Graafik.FirstOrDefault(g => g.Paev == pood.TananePaev);
            if (g != null && TimeSpan.TryParse(g.AvatudKuni, out var kuni))
            {
                g.AvatudKuni = (kuni + TimeSpan.FromHours(1)).ToString();
                _context.PaevaGraafiks.Update(g);
            }

            // praegune aeg = текущее время
            pood.PraeguneAeg = DateTime.Now.TimeOfDay;
            pood.OnAvatud = IsOpen(pood, pood.PraeguneAeg, pood.TananePaev);

            _context.Poed.Update(pood);
            _context.SaveChanges();

            return GetStateChangeMessage(wasOpen, pood.OnAvatud);
        }

        // +1 päev konkreetse poe jaoks
        [HttpPost("{id}/add-day")]
        public ActionResult<string> AddDay(int id)
        {
            var pood = _context.Poed.Include(p => p.Graafik).FirstOrDefault(p => p.Id == id);
            if (pood == null)
            {
                return NotFound($"Pood ID={id} ei leitud.");
            }

            bool wasOpen = IsOpen(pood, DateTime.Now.TimeOfDay, pood.TananePaev);

            // Suurendame nädalapäeva 1 võrra
            pood.TananePaev = (DayOfWeek)(((int)pood.TananePaev + 1) % 7);

            // praegune aeg = текущее время
            pood.PraeguneAeg = DateTime.Now.TimeOfDay;
            pood.OnAvatud = IsOpen(pood, pood.PraeguneAeg, pood.TananePaev);

            _context.Poed.Update(pood);
            _context.SaveChanges();

            return GetStateChangeMessage(wasOpen, pood.OnAvatud);
        }

        // meetod poe avatuse kontrollimiseks
        private bool IsOpen(Pood pood, TimeSpan currentTime, DayOfWeek day)
        {
            var g = pood.Graafik.FirstOrDefault(g => g.Paev == day);
            if (g == null) return false;

            // konverteerime stringid TimeSpan-iks
            if (!TimeSpan.TryParse(g.AvatudAlates, out var alates)) return false;
            if (!TimeSpan.TryParse(g.AvatudKuni, out var kuni)) return false;

            return currentTime >= alates && currentTime < kuni;
        }

        // meetod poe seisundi muutuse teate koostamiseks
        private string GetStateChangeMessage(bool wasOpen, bool isOpen)
        {
            if (wasOpen && !isOpen) return "suletakse";
            if (!wasOpen && isOpen) return "avatakse";
            if (wasOpen && isOpen) return "endiselt lahti";
            return "endiselt kinni";
        }
    }
}
