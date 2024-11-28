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
                .Descendants("div")
                .Where(div => div.Attributes.Contains("class") && div.Attributes["class"].Value == "qode-apl-item")
                .Skip(1)
                .Select(item => 
                {
                    var title = item.Descendants("h5")
                        .Where(h5 => h5.Attributes.Contains("class") && h5.Attributes["class"].Value == "qode-apl-item-title")
                        .FirstOrDefault()?.InnerText.Trim();
        
                    var description = item.Descendants("div")
                        .Where(div => div.Attributes.Contains("class") && div.Attributes["class"].Value == "qode-apl-item-description")
                        .FirstOrDefault()?.InnerText.Trim();

                    return Meal.Create($"{title} - {description}");
                })
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
