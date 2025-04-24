using System.Text.Json;

internal class Product
{
    public string Slug { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ArtikelId { get; set; }
    public string? Barcode { get; set; }
    public List<string?> Links { get; set; }

    public Product(JsonElement item)
    {
        this.Id = item.GetProperty("id").GetInt32();
        this.Name = item.GetProperty("name").GetString() ?? "";
        this.Slug = item.GetProperty("slug").GetString() ?? "";

        var metaData = item.GetProperty("meta_data").EnumerateArray();

        var artikelIdMetaData = metaData.FirstOrDefault(entry => entry.GetProperty("key").GetString() == "artikel-id");
        if (artikelIdMetaData.ValueKind == JsonValueKind.Object)
        {
            this.ArtikelId = artikelIdMetaData.GetProperty("value").GetString();
        }
        else
        {
            this.ArtikelId = "";
        }

        var barcodeMetaData = metaData.FirstOrDefault(entry => entry.GetProperty("key").GetString() == "barcode");
        if (barcodeMetaData.ValueKind == JsonValueKind.Object)
        {
            this.Barcode = barcodeMetaData.GetProperty("value").GetString();
        }
        else
        {
            this.Barcode = "";
        }

        var selfLinks = item.GetProperty("_links").GetProperty("self").EnumerateArray();
        this.Links = selfLinks.Select(selfLink => selfLink.GetProperty("href").GetString()).ToList();
    }
}