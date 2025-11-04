using System.Text.Json.Serialization;

namespace Seisundid.Models
{
    public class PaevaGraafik
    {
        public int Id { get; set; }
        public DayOfWeek Paev { get; set; }
        public string? AvatudAlates { get; set; } = "00:00:00";
        public string? AvatudKuni { get; set; } = "00:00:00";
        public int PoodId { get; set; }
        [JsonIgnore]
        public Pood? Pood { get; set; }
    }
}
