using System.Text.Json.Serialization;

namespace EduTrack.WebApi.Models.Auth
{
    public class LoginResponseDto
    {
        /// <summary>
        /// Token JWT
        /// </summary>
        public string? access_token { get; set; }

        public string? token_type { get; set; }

        [JsonIgnore]
        public DateTime expires { get; set; }

        /// <summary>
        /// Tiempo de expiracion en formato UNIX
        /// </summary>
        public int exp
        {
            get
            {
                return (int)expires.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            }
        }
    }
}
