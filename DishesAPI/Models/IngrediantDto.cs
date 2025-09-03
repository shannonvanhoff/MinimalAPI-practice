namespace DishesAPI.Models
{
    public class IngrediantDto
    {

        public Guid Id { get; set; }

        public required string Name { get; set; }

        public Guid DishId { get; set; }
    }
}
