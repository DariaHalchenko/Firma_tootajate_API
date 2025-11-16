using System.Text.Json.Serialization;

namespace Firma_tootajate_API.Models
{
    public class Login
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Parool { get; set; }
        [JsonIgnore]
        public bool IsAdmin { get; set; } = false; // false - töötaja, true - administraator
    }
}
