using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;
using System.Text.RegularExpressions;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class AnnapurnaRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        private string Url => $"http://www.indicka-restaurace-annapurna.cz/";

        public AnnapurnaRestaurant() : base(RestaurantType.Annapurna)
        {
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Domain.Entities.Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);

            var todayMenuNode = htmlDocument.DocumentNode.Descendants("p")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "TJden")
                .First(s => s.InnerText.Contains(GetToday(), StringComparison.InvariantCultureIgnoreCase))
                .NextSibling;

            var soupTextMatch = Regex.Match(todayMenuNode.InnerHtml, @"Polévky:\s*(.*?)<br><b>", RegexOptions.Singleline);

            var soups = soupTextMatch.Success
                ? soupTextMatch.Groups[1].Value
                    .Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Soap.Create(Regex.Replace(s, @"<.*?>", "").Trim()))
                    .ToList()
                : new List<Soap>();

            var meals = todayMenuNode
                .Descendants("b")
                .Select(s => Meal.Create(Regex.Replace(s.InnerText, @"^[0-9]\.", "").Trim()))
                .ToList();

            if (soups.Count == 0)
            {
                return Restaurant.Create(Type, Menu.Create(meals));
            }
            return Restaurant.Create(Type, Menu.Create(meals, soups));
        }

        private string GetToday()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            var culture = new System.Globalization.CultureInfo("cs-CZ");
            return culture.DateTimeFormat.GetDayName(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).DayOfWeek);
        }
    }
}
