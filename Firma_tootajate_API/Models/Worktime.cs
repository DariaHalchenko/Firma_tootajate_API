namespace Firma_tootajate_API.Models
{
    public class Worktime
    {
        public int Id { get; set; }              
        public int TootajateId { get; set; }       
        public Tootajate? Tootajate { get; set; }    
        public DateOnly Kuupaev { get; set; }
        public TimeOnly Sissepaas { get; set; }
        public TimeOnly Valjapaas { get; set; }       
    }
}
