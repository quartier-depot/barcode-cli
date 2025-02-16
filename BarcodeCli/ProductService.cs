using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

class ProductService
{
    private const double MaximumItemsPerPage = 100;
    private readonly WooCommerceApi api;
    private readonly List<Product> products = [];

    public ProductService(Settings settings)
    {
        api = new WooCommerceApi(settings);
    }

    public async Task<List<Product>> ReloadProductsAsync()
    {
        this.products.Clear();

        var response = await api.GetAsync("products", new Dictionary<string, string>
        {
            { "status", "publish" }
        });
        Console.Write(".");

        response.EnsureSuccessStatusCode();
        int total = int.Parse(response.Headers.GetValues("x-wp-total").First());

        int numberOfRequests = (int)Math.Ceiling(total / MaximumItemsPerPage);
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < numberOfRequests; i++)
        {
            tasks.Add(api.GetAsync("products", new Dictionary<string, string>
            {
                { "status", "publish" },
                { "per_page", MaximumItemsPerPage.ToString() },
                { "page", (i + 1).ToString() }
            }));
        }

        var responses = await Task.WhenAll(tasks);
        foreach (var resp in responses)
        {
            Console.Write(".");
            resp.EnsureSuccessStatusCode();
            var respJson = await resp.Content.ReadAsStringAsync();
            using var respDoc = JsonDocument.Parse(respJson);
            foreach (var item in respDoc.RootElement.EnumerateArray())
            {
                products.Add(new Product(item));
            }
        }

        return products;
    }

    internal List<Product> Search(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            return [];
        }

        return [.. products.FindAll(product =>
            (searchTerm.StartsWith("https") && searchTerm.EndsWith(product.Id.ToString())) ||
            product.ArtikelId == searchTerm)];
    }

    internal void SetBarcode(Product product, string? barcodeInput)
    {
        throw new NotImplementedException();
    }

    internal async Task SetBarcodeAsync(Product product, string? barcode)
    {
        if (String.IsNullOrWhiteSpace(barcode))
        {
            Console.WriteLine("ERROR Barcode not saved (no barcode given).");
            return;
        }

        if ("skip".Equals(barcode, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("SKIP Barcode not saved.");
            return;
        }

        string json = $$"""
        {
            "id": "{{product.Id}}",
            "meta_data": [
                {
                    "key": "barcode",
                    "value": "{{barcode}}"
                }
            ]
        }
        """;

        try
        {
            var response = await api.PostAsync("products/" + product.Id, [], json);
            var updatedJson = await response.Content.ReadAsStringAsync();
            var updatedProduct = new Product(JsonDocument.Parse(updatedJson).RootElement);
            response.EnsureSuccessStatusCode();
            if (updatedProduct == null) {
                throw new Exception("Updated product is null.");
            }
            if (updatedProduct.Barcode != barcode) {
                throw new Exception("Updated product does not contain barcode.");
            }
            Console.WriteLine("Barcode saved.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR Barcode not saved ({e.Message})");
        }
    }
}
