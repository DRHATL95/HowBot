using Howbot.Core.Models.Tarkov;
using Newtonsoft.Json;
using Task = Howbot.Core.Models.Tarkov.Task;

namespace Howbot.Infrastructure.Data.Models.Tarkov;

public class TradeCashOffer
{
  [JsonProperty("id")] public ulong Id { get; set; }

  [JsonProperty("minTraderLevel")] public int MinTraderLevel { get; set; }

  [JsonProperty("price")] public int Price { get; set; }

  [JsonProperty("currency")] public string Currency { get; set; } = string.Empty;

  [JsonProperty("currencyItem")] public Item CurrencyItem { get; set; } = new();

  [JsonProperty("priceRUB")] public int PriceRubles { get; set; }

  [JsonProperty("taskUnlock")] public Task TaskUnlock { get; set; } = new();

  [JsonProperty("buyLimit")] public int BuyLimit { get; set; }
}
