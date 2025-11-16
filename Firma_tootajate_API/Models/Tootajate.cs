using System.Text.Json.Serialization;

namespace Firma_tootajate_API.Models
{
    public class Tootajate
    {
        [JsonIgnore]
        public int Id { get; set; }           
        public string Nimi { get; set; }           
        public string Isikukood { get; set; }      
        public string Amet { get; set; }          
        public decimal Tunnitasu { get; set; }  
        public string Email { get; set; }          
        public string Parool { get; set; }        
        public bool IsAdmin { get; set; } = false;  // false - töötaja, true - administraator
    }
}
