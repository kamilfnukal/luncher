using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;
using System.Text;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class PoupeRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        private string Url => "https://www.pivovar-poupe.cz";

        public PoupeRestaurant() : base(RestaurantType.Poupe)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);
            int dayOfWeek = (int)DateTime.Now.DayOfWeek;
            int adjustedDayOfWeek = dayOfWeek == 0 ? 6 : dayOfWeek - 1;

            var accordionItem = htmlDocument.DocumentNode.Descendants("section")
                .Where(s => s.Attributes.Contains("id") && s.Attributes["id"].Value == "obedy")
                .FirstOrDefault()?
                .Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value.Contains("accordion__item"))
                .Skip(adjustedDayOfWeek)
                .FirstOrDefault();

            if (accordionItem == null)
            {
                return Restaurant.Create(Type, Menu.Create(new List<Meal>(), new List<Soap>()));
            }

            var meals = accordionItem.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "jidlo_name")
                .Skip(1)
                .Select(tr => Meal.Create(tr.InnerText.Trim()))
                .ToList();

            var soup = accordionItem.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "jidlo_name")
                .FirstOrDefault();

            var soaps = new List<Soap>();

            if (soup != null)
            {
                soaps.Add(Soap.Create(soup.InnerText.Trim()));
            }

            return Restaurant.Create(Type, Menu.Create(meals, soaps));
        }
    }
}
