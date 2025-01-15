using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class AnnapurnaRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        private string Url => $"http://www.indicka-restaurace-annapurna.cz/mc/php/handlers/getDocument.php?documentName=weekly-menu&locationName=Brno";

        public AnnapurnaRestaurant() : base(RestaurantType.Annapurna)
        {
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Domain.Entities.Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);
            
            /*
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
                */
            
            // The menu consists of soups and main dishes, the menu items are in weekly-menu-main-dish class div and first MenuItemTitle class span in the div
            // so get all weekly-menu-main-dish divs and take first two, which will be soups, rest will be main courses
            
            // get current day in week (monday = 0, tuesday = 1, ...)
            int dayOfWeek = (int)DateTime.Now.DayOfWeek - 1;
            
            var curerntDay = htmlDocument.DocumentNode.Descendants("div")
                .Where(s => s.Attributes.Contains("data-dayid") && s.Attributes["data-dayid"].Value == dayOfWeek.ToString())
                .First(s => s.InnerText.Contains(GetToday(), StringComparison.InvariantCultureIgnoreCase));
            
            var menuItems = curerntDay.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "weekly-menu-main-dish")
                .Select(s => s
                    .Descendants("span")
                    .First(span => span.Attributes.Contains("class") && span.Attributes["class"].Value == "MenuItemTitle").InnerText.Trim())
                .ToList();
            
            var soups = menuItems.Take(2).Select(s => Soap.Create(s)).ToList();
            var meals = menuItems.Skip(2).Select(s => Meal.Create(s)).ToList();

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
