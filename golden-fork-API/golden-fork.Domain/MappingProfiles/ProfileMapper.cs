using AutoMapper;
using golden_fork.core.DTOs;
using golden_fork.core.Entities.AppUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.core.MappingProfiles
{
    public class ProfileMapper : Profile
    {
        public ProfileMapper()
        {
            CreateMap<RegistrationRequest, User>();   
        }
    }
}
