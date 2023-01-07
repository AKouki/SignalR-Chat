using AutoMapper;
using Chat.Web.Models;
using Chat.Web.ViewModels;

namespace Chat.Web.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserViewModel>()
                .ForMember(dst => dst.UserName, opt => opt.MapFrom(x => x.UserName));

            CreateMap<UserViewModel, ApplicationUser>();
        }
    }
}
