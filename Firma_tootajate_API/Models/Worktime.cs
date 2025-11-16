using System.Text.Json.Serialization;

namespace Firma_tootajate_API.Models
{
    public class Worktime
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public int TootajateId { get; set; }
        [JsonIgnore]
        public Tootajate? Tootajate { get; set; }    
        public DateOnly Kuupaev { get; set; }
        public TimeOnly Sissepaas { get; set; }
        public TimeOnly Valjapaas { get; set; }       
    }
}
