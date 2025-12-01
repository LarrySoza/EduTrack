using AutoMapper;
using EduTrack.Core.Entities.Core;
using EduTrack.WebApi.Models.Alumno;

namespace EduTrack.WebApi.Profiles
{
    public class AlumnoProfile : Profile
    {
        public AlumnoProfile()
        {
            CreateMap<CrearAlumnoRequest, Alumno>();
            CreateMap<ActualizarAlumnoRequest, Alumno>();
        }
    }
}
