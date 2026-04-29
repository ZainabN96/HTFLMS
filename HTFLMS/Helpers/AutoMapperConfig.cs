using AutoMapper;
using HTFLMS.Dtos;
using HTFLMS.Models;

namespace HTFLMS.Helper
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings(IServiceCollection services)
        {
            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<User, UserDto>().ReverseMap();

                cfg.CreateMap<Course, CourseDto>().ReverseMap()
                    .ForMember(dest => dest.CourseImagePath, opt => opt.Ignore())
                    .ForMember(dest => dest.HandbookFilePath, opt => opt.Ignore())
                    .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                    .ForMember(dest => dest.Modules, opt => opt.Ignore())
                    .ForMember(dest => dest.Materials, opt => opt.Ignore())
                    .ForMember(dest => dest.Assignments, opt => opt.Ignore())
                    .ForMember(dest => dest.Enrollments, opt => opt.Ignore())
                    .ForMember(dest => dest.CertificateRequests, opt => opt.Ignore());
            });
        }
    }
    //public class AutoMapperConfig
    //{
    //    public static IMapper RegisterMappings()
    //    {
    //        var config = new MapperConfiguration(cfg =>
    //        {
    //           cfg.CreateMap<User, UserDto>().ReverseMap();
    //        });

    //        return config.CreateMapper();
    //    }

    //}
}
