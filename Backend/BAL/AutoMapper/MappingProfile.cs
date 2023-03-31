using AutoMapper;
using DAL;
using Microsoft.AspNetCore.Identity;

namespace BAL
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<ApplicationUser, AuthModel>().ReverseMap();
			CreateMap<ApplicationUserDto, ApplicationUser>().ReverseMap();
			CreateMap<ApplicationUser, ReturnUsersDto>();
			CreateMap<ThreeDModel, ThreeDModelDto>().ReverseMap();
			CreateMap<CreateThreeDModelDto, ThreeDModel>().ReverseMap();
		}
	}
}