using System.Net;
using HtmlAgilityPack;
using Luncher.Adapters.ThirdParty.Restaurants;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;
using System.Text;
using System.Text.RegularExpressions;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class GrandKitchenRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        private string Url => "https://www.menubot.cz/app/users/pcgkv21524598494946161846445/export/dailymenu_a.js";

        public GrandKitchenRestaurant() : base(RestaurantType.GrandKitchen)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            try
            {
                var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);

                var todayMenuNode = htmlDocument.DocumentNode.Descendants("section")
                    .Where(s => s.Attributes.Contains("class") &&
                                s.Attributes["class"].Value == "fly-dish-menu container-min jidel").ToList().First();

                var soaps = todayMenuNode.Descendants("h5")
                    .Select(s => WebUtility.HtmlDecode(s.InnerText))
                    .Select(Soap.Create)
                    .Take(1)
                    .ToList();

                var meals = todayMenuNode.Descendants("h5")
                    .Select(s => WebUtility.HtmlDecode(s.InnerText))
                    .Select(Meal.Create)
                    .Skip(1)
                    .Take(3)
                    .ToList();

                return Restaurant.Create(Type, Menu.Create(meals, soaps));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}