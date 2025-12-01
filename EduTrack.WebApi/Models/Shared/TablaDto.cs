namespace EduTrack.WebApi.Models.Shared
{
    /// <summary>
    /// DTO genérico para catálogos simples (id, nombre).
    /// </summary>
    public class TablaDto
    {
        public string id { get; set; } = default!;
        public string? nombre { get; set; }
    }
}
