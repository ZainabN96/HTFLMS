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

                cfg.CreateMap<Module, ModuleDto>().ReverseMap()
                    .ForMember(dest => dest.Course, opt => opt.Ignore())
                    .ForMember(dest => dest.Lessons, opt => opt.Ignore())
                    .ForMember(dest => dest.Materials, opt => opt.Ignore())
                    .ForMember(dest => dest.Assignments, opt => opt.Ignore())
                    .ForMember(dest => dest.Quiz, opt => opt.Ignore())
                    .ForMember(dest => dest.ModuleProgresses, opt => opt.Ignore());

                cfg.CreateMap<Lesson, LessonDto>().ReverseMap()
                    .ForMember(dest => dest.Module, opt => opt.Ignore())
                    .ForMember(dest => dest.Materials, opt => opt.Ignore())
                    .ForMember(dest => dest.LessonProgresses, opt => opt.Ignore());

                cfg.CreateMap<Quiz, QuizDto>().ReverseMap()
                    .ForMember(dest => dest.Module, opt => opt.Ignore())
                    .ForMember(dest => dest.Questions, opt => opt.Ignore())
                    .ForMember(dest => dest.Attempts, opt => opt.Ignore());

                cfg.CreateMap<QuizQuestion, QuizQuestionDto>()
                    .ForMember(dest => dest.OptionA,
                        opt => opt.MapFrom(src =>
                            src.Options != null && src.Options.Count > 0
                                ? src.Options.ElementAt(0).OptionText
                                : ""))

                    .ForMember(dest => dest.OptionB,
                        opt => opt.MapFrom(src =>
                            src.Options != null && src.Options.Count > 1
                                ? src.Options.ElementAt(1).OptionText
                                : ""))

                    .ForMember(dest => dest.OptionC,
                        opt => opt.MapFrom(src =>
                            src.Options != null && src.Options.Count > 2
                                ? src.Options.ElementAt(2).OptionText
                                : ""))

                    .ForMember(dest => dest.OptionD,
                        opt => opt.MapFrom(src =>
                            src.Options != null && src.Options.Count > 3
                                ? src.Options.ElementAt(3).OptionText
                                : ""))

                    .ForMember(dest => dest.CorrectAnswer,
                        opt => opt.MapFrom(src =>
                            src.Options != null
                                ? src.Options
                                    .Select((x, index) => new { x, index })
                                    .Where(x => x.x.IsCorrect)
                                    .Select(x =>
                                        x.index == 0 ? "A" :
                                        x.index == 1 ? "B" :
                                        x.index == 2 ? "C" : "D")
                                    .FirstOrDefault()
                                : ""))

                    .ReverseMap()

                    .ForMember(dest => dest.Quiz, opt => opt.Ignore())
                    .ForMember(dest => dest.Options, opt => opt.Ignore())
                    .ForMember(dest => dest.AttemptAnswers, opt => opt.Ignore());

                cfg.CreateMap<Material, MaterialDto>().ReverseMap()
                    .ForMember(dest => dest.Course, opt => opt.Ignore())
                    .ForMember(dest => dest.Module, opt => opt.Ignore())
                    .ForMember(dest => dest.Lesson, opt => opt.Ignore());
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
