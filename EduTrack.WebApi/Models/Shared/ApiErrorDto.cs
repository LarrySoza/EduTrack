namespace EduTrack.WebApi.Models.Shared
{
    /// <summary>
    /// DTO que representa un error de API con código y mensaje.
    /// </summary>
    public class ApiErrorDto
    {
        public string code { get; private set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string message { get; private set; }

        public ApiErrorDto(string code, string message = "")
        {
            this.code = code;
            this.message = message;
        }
    }
}
