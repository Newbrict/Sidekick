using System;
using Serilog;
using Sidekick.Business.Apis.Poe.Models;
using Sidekick.Business.Languages;
using Sidekick.Core.Natives;

namespace Sidekick.Business.Apis.PoeDb
{
    /// <summary>
    /// https://poedb.tw/us/
    /// Only in English.
    /// </summary>
    public class PoeDbClient : IPoeDbClient
    {
        private const string PoeDbBaseUri = "https://poedb.tw/";
        private const string SubUrlUnique = "unique.php?n=";
        private const string SubUrlGem = "gem.php?n=";
        private const string SubUrlItem = "item.php?n=";
        private readonly ILogger logger;
        private readonly ILanguageProvider languageProvider;
        private readonly INativeBrowser nativeBrowser;

        public PoeDbClient(ILogger logger,
            ILanguageProvider languageProvider,
            INativeBrowser nativeBrowser)
        {
            this.logger = logger.ForContext(GetType());
            this.languageProvider = languageProvider;
            this.nativeBrowser = nativeBrowser;
        }

        public void Open(Item item)
        {
            if (item == null)
            {
                return;
            }

            if (languageProvider.Current.Name != languageProvider.DefaultLanguage)
            {
                return;
            }

            if (string.IsNullOrEmpty(item.Name))
            {
                logger.Warning("Unable to open PoeDB for specified item as it has no name! {@item}", item);
                return;
            }

            nativeBrowser.Open(CreateUri(item));
        }

        private Uri CreateUri(Item item)
        {
            var subUrl = item.Rarity switch
            {
                Rarity.Unique => SubUrlUnique,
                Rarity.Gem => SubUrlGem,
                _ => SubUrlItem
            };

            var searchLink = item.Name ?? item.Type;
            var wikiLink = subUrl + searchLink.Replace(" ", "+");
            return new Uri(PoeDbBaseUri + wikiLink);
        }
    }
}
