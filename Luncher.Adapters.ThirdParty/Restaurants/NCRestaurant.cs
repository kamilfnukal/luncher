using Luncher.Core.Entities;
using Luncher.Domain.Entities;

namespace Luncher.Adapters.ThirdParty.Restaurants;

internal class NCRestaurant : RestaurantBase
{
    public NCRestaurant() : base(RestaurantType.NC)
    {
    }

    protected override async Task<Domain.Entities.Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
    {
        var meals = new List<Meal>();
        meals.Add(Meal.Create("Gyros, pikantní omáčka"));

        return Restaurant.Create(Type, Menu.Create(meals));
    }
}