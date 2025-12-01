using AutoMapper;
using EduTrack.Core.Entities.Core;
using EduTrack.WebApi.Models.Usuario;

namespace EduTrack.WebApi.Mapping
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<CrearUsuarioRequestDto, Usuario>();

            CreateMap<Usuario, UsuarioDto>();

            CreateMap<Rol, RolDto>();
        }
    }
}
