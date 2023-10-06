using AutoMapper;
using nxLINEadmin.Entity;
using nxLINEadmin.Model;

namespace nxLINEadmin.Utilities
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<User, RegisterUserViewModel>().ReverseMap();

        }
    }
}
