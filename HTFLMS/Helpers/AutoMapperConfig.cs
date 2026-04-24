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
