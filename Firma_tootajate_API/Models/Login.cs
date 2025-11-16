using System.Text.Json.Serialization;

namespace Firma_tootajate_API.Models
{
    public class Login
    {
        public string Email { get; set; }
        public string Parool { get; set; }
    }
}
