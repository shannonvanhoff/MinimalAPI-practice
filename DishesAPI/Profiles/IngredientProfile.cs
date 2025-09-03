using AutoMapper;
using DishesAPI.Entities;
using DishesAPI.Models;

namespace DishesAPI.Profiles
{
    public class IngredientProfile : Profile
    {
        public IngredientProfile()
        {
            CreateMap<Ingredient, IngrediantDto>()
              .ForMember(
                  d => d.DishId,
                  o => o.MapFrom(s => s.Dishes.First().Id));
        }
    }
}
