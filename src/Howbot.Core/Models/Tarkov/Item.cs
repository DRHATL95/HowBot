using Howbot.Infrastructure.Data.Models.Tarkov;
using Newtonsoft.Json;

namespace Howbot.Core.Models.Tarkov;

public class Item
{
  [JsonProperty("id")] public string Id { get; set; } = string.Empty;

  [JsonProperty("name")] public string? Name { get; set; }

  [JsonProperty("normalizedName")] public string? NormalizedName { get; set; }

  [JsonProperty("shortName")] public string? ShortName { get; set; }

  [JsonProperty("description")] public string? Description { get; set; }

  [JsonProperty("basePrice")] public int BasePrice { get; set; }

  [JsonProperty("updated")] public string? Updated { get; set; }

  [JsonProperty("width")] public int Width { get; set; }

  [JsonProperty("height")] public int Height { get; set; }

  [JsonProperty("backgroundColor")] public string BackgroundColor { get; set; } = string.Empty;

  [JsonProperty("iconLink")] public string? IconUrl { get; set; }

  [JsonProperty("gridImageLink")] public string? GridImageUrl { get; set; }

  [JsonProperty("baseImageLink")] public string? BaseImageUrl { get; set; }

  [JsonProperty("inspectImageLink")] public string? InspectImageUrl { get; set; }

  [JsonProperty("image512pxLink")] public string? Image512PxUrl { get; set; }

  [JsonProperty("image8xLink")] public string? Image8XUrl { get; set; }

  [JsonProperty("wikiLink")] public string? WikiUrl { get; set; }

  [JsonProperty("types")] public IEnumerable<ItemType> Types { get; set; } = [];

  [JsonProperty("avg24hPrice")] public int Avg24HPrice { get; set; }

  [JsonProperty("properties")] public ItemProperties? Properties { get; set; }

  [JsonProperty("conflictingItems")] public IEnumerable<Item> ConflictingItems { get; set; } = [];

  [JsonProperty("conflictingSlotIds")] public IEnumerable<string> ConflictingSlotIds { get; set; } = [];

  [JsonProperty("accuracyModifier")] public float? AccuracyModifier { get; set; }

  [JsonProperty("recoilModifier")] public float? RecoilModifier { get; set; }

  [JsonProperty("ergonomicsModifier")] public float? ErgonomicsModifier { get; set; }

  [JsonProperty("hasGrid")] public bool HasGrid { get; set; }

  [JsonProperty("blocksHeadphones")] public bool IsBlockHeadphones { get; set; }

  [JsonProperty("link")] public string? Link { get; set; }

  [JsonProperty("lastLowPrice")] public int LastLowPrice { get; set; }

  [JsonProperty("changeLast48h")] public float? ChangeLast48H { get; set; }

  [JsonProperty("changeLast48hPercent")] public float? ChangeLast48HPercent { get; set; }

  [JsonProperty("low24hPrice")] public int Low24HPrice { get; set; }

  [JsonProperty("high24hPrice")] public int High24HPrice { get; set; }

  [JsonProperty("lastOfferCount")] public int LastOfferCount { get; set; }

  [JsonProperty("sellFor")] public IEnumerable<ItemPrice> SellFor { get; set; } = [];

  [JsonProperty("buyFor")] public IEnumerable<ItemPrice> BuyFor { get; set; } = [];

  [JsonProperty("containsItems")] public IEnumerable<ContainedItem> ContainsItems { get; set; } = [];

  [JsonProperty("category")] public ItemCategory? Category { get; set; }

  [JsonProperty("categories")] public IEnumerable<ItemCategory> Categories { get; set; } = [];

  [JsonProperty("bsgCategoryId")] public string? BsgCategoryId { get; set; }

  [JsonProperty("handbookCategories")] public IEnumerable<ItemCategory> HandbookCategories { get; set; } = [];

  [JsonProperty("weight")] public float? Weight { get; set; }

  [JsonProperty("velocity")] public float? Velocity { get; set; }

  [JsonProperty("loudness")] public int Loudness { get; set; }

  [JsonProperty("usedInTasks")] public IEnumerable<Task> UsedInTasks { get; set; } = [];

  [JsonProperty("receivedFromTasks")] public IEnumerable<Task> ReceivedFromTasks { get; set; } = [];

  [JsonProperty("bartersFor")] public IEnumerable<Barter> BartersFor { get; set; } = [];

  [JsonProperty("bartersUsing")] public IEnumerable<Barter> BartersUsing { get; set; } = [];

  [JsonProperty("craftsFor")] public IEnumerable<Craft> CraftsFor { get; set; } = [];

  [JsonProperty("craftsUsing")] public IEnumerable<Craft> CraftsUsing { get; set; } = [];

  [JsonProperty("historicalPrices")] public IEnumerable<HistoricalPricePoint> HistoricalPrices { get; set; } = [];

  public int GetFleaMarketFee(int price, int intelCenterLevel, int hideoutManagementLevel, int count, bool requireAll)
  {
    return 0;
  }
}
