using Seisundid.Models;
using System.Text.Json.Serialization;

namespace Seisundid.Models
{
    public class Pood
    {
        public int Id { get; set; }
        public string Nimi { get; set; }

        public DayOfWeek TananePaev { get; set; } = DayOfWeek.Monday;
        public TimeSpan PraeguneAeg { get; set; } = new TimeSpan(0, 0, 0);
        public bool OnAvatud { get; set; } = false;
        public List<PaevaGraafik> Graafik { get; set; } = new();
    }
}
