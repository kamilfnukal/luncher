using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;
using System.Text;
using System.Text.RegularExpressions;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class CharliesRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        // TODO

        private string Url => $"https://www.charliessquare.cz/menu/";

        public CharliesRestaurant() : base(RestaurantType.Charlies)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);
            

            var meals = htmlDocument.DocumentNode.Descendants("table")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "menu-one-day menu-to-week")
                .FirstOrDefault()
                .Descendants("tbody")
                .FirstOrDefault()
                .Descendants("tr")
                .Skip(1)
                .Select(tr => Meal.Create(tr.InnerText.Trim()))
                .ToList();
            

            var soap = new List<Soap>
            {
                Soap.Create(htmlDocument.DocumentNode.Descendants("table")
                    .Where(s => s.Attributes.Contains("class") &&
                                s.Attributes["class"].Value == "menu-one-day")
                    .FirstOrDefault()
                    .Descendants("tbody")
                    .FirstOrDefault()
                    .Descendants("tr")
                    .Last().InnerText.Trim())
            };

            return Restaurant.Create(Type, Menu.Create(meals, soap));

        }
    }
}
