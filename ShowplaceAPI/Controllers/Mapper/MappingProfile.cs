using AutoMapper;
using System.Runtime;
using ShowplaceAPI.Models;
using ShowplaceAPI.Models.DTOModles;

namespace ShowplaceAPI.Controllers.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Landmark map
            CreateMap<CreateLandmarkDto, Landmark>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Reviews,
                    opt => opt.Ignore());

            CreateMap<UpdateLandmarkDTO, Landmark>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Reviews,
                    opt => opt.Ignore());

            CreateMap<Landmark, LandmarkDTO>()
                .ForMember(dest => dest.ReviewsCount,
                    opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageRating,
                    opt => opt.MapFrom(src => src.Reviews.Any() ?
                        src.Reviews.Average(r => r.Rating) : (double?)null));

            // Review map
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Landmark,
                    opt => opt.Ignore());

            CreateMap<UpdateReviewDTO, Review>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.Ignore())
                .ForMember(dest => dest.LandmarkId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Landmark,
                    opt => opt.Ignore());

            CreateMap<Review, ReviewDTO>()
                .ForMember(dest => dest.LandmarkName,
                    opt => opt.MapFrom(src => src.Landmark != null ? src.Landmark.Name : "Unknown"));

            // User map
            CreateMap<CreateUserDTO, User>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Reviews,
                    opt => opt.Ignore());

            CreateMap<UpdateUserDTO, User>()
                .ForMember(dest => dest.CreatedDate,
                    opt => opt.Ignore())
                .ForMember(dest => dest.Reviews,
                    opt => opt.Ignore());

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.ReviewsCount,
                    opt => opt.MapFrom(src => src.Reviews.Count));
        }
    }
}
