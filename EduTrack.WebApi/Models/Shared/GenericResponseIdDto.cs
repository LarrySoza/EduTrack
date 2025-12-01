namespace EduTrack.WebApi.Models.Shared
{
    /// <summary>
    /// DTO genérico que contiene un identificador como respuesta.
    /// </summary>
    public class GenericResponseIdDto<T>
    {
        public T id { get; private set; }

        public GenericResponseIdDto(T id)
        {
            this.id = id;
        }
    }
}
