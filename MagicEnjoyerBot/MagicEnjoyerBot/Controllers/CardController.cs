using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MagicEnjoyerBot.Controllers
{
    /*
     * Controller for all random single card related commands
     */
    public static class CardController
    {
        public static string RandomCardFlavor()
        {
            string data = ScryfallQuery("https://api.scryfall.com/cards/random?q=has%3Aflavor");

            if (string.IsNullOrEmpty(data)) return "";

            JsonElement scryfallResult, flavorResult;
            scryfallResult = JsonSerializer.Deserialize<JsonElement>(data);
            scryfallResult.TryGetProperty("flavor_text", out flavorResult);
            string flavorText = flavorResult.GetString();

            return flavorText;
        }

        public static Tuple<string, string> RandomCommander()
        {
            Tuple<string, string> randoMander = new Tuple<string, string>("", "");

            string data = ScryfallQuery("https://api.scryfall.com/cards/random?q=%28t%3Acreature+or+t%3Aplaneswalker%29+and+t%3ALegendary+and+f%3Acommander&unique=cards");

            if (string.IsNullOrEmpty(data)) return randoMander;

            JsonElement scryfallResult, scryfallURI, cardname;
            scryfallResult = JsonSerializer.Deserialize<JsonElement>(data);
            scryfallResult.TryGetProperty("scryfall_uri", out scryfallURI);
            scryfallResult.TryGetProperty("name", out cardname);

            randoMander = new Tuple<string, string>(cardname.GetString() ?? "", scryfallURI.GetString() ?? "");

            return randoMander;
        }

        static string ScryfallQuery(string url)
        {
            try 
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = client.GetAsync(url).Result)
                    {
                        using (HttpContent content = res.Content)
                        {
                            var data = content.ReadAsStringAsync();
                            if (content != null)
                            {
                                return data.Result;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying scryfall - URL: " + url + "  ERROR: " + ex);
            }
            return "";
        }
    }
}
