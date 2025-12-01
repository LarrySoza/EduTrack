using System.Text.Json;

namespace EduTrack.WebApi.Models.Shared
{
    public class ApiExceptionDto : Exception
    {
        public string message { get { return base.Message; } }

        public ApiErrorDto[] Errors { get; private set; }

        public ApiExceptionDto(string message, params ApiErrorDto[] errors) : base(message)
        {
            Errors = errors;
        }

        public override string ToString() => JsonSerializer.Serialize(new { message, errors = Errors });

    }
}
