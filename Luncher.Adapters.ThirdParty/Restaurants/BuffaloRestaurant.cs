using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;
using System.Text;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class BuffaloRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        private string Url => "https://restauracebuffalo.cz/denni-menu/";

        public BuffaloRestaurant() : base(RestaurantType.Buffalo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);
            int dayOfWeek = (int)DateTime.Now.DayOfWeek - 1;

            var meals = htmlDocument.DocumentNode.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "q_elements_item_inner")
                .FirstOrDefault()
                .Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "qode-advanced-pricing-list")
                .Skip(dayOfWeek)
                .FirstOrDefault()
                .Descendants("h5")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "qode-apl-item-title")
                .Skip(1)
                .Select(tr => Meal.Create(tr.InnerText.Trim()))
                .ToList();
            

            var soap = htmlDocument.DocumentNode.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "q_elements_item_inner")
                .FirstOrDefault()
                ?.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "qode-advanced-pricing-list")
                .Skip(dayOfWeek)
                .FirstOrDefault()
                ?.Descendants("h5")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "qode-apl-item-title")
                .FirstOrDefault();
            
            var soaps = new List<Soap>();

            if (soap != null)
            {
                soaps.Add(Soap.Create(soap.InnerText.Trim()));
            }
            
            return Restaurant.Create(Type, Menu.Create(meals, soaps));
        }
    }
}
