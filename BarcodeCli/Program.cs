using Microsoft.Extensions.Configuration;
using NetCoreAudio;
using System.Media;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Settings? settings = config.GetRequiredSection("WooCommerce").Get<Settings>();

if (settings == null) {
    throw new Exception("Missing WooCommerce settings in appsettings.config.");
}

if (settings.Uri == null) {
    throw new Exception("Missing Uri in appsettings.config file.");
}

if (String.IsNullOrEmpty(settings.ConsumerKey)) {
    throw new Exception("Missing ConsumerKey in appsettings.config file.");
}

if (String.IsNullOrEmpty(settings.ConsumerSecret)) {
    throw new Exception("Missing ConsumerSecret in appsettings.config file.");
}



Console.WriteLine("");
Console.WriteLine(@"__________                                .___       _________ .____    .___ ");
Console.WriteLine(@"\______   \_____ _______   ____  ____   __| _/____   \_   ___ \|    |   |   |");
Console.WriteLine(@" |    |  _/\__  \\_  __ \_/ ___\/  _ \ / __ |/ __ \  /    \  \/|    |   |   |");
Console.WriteLine(@" |    |   \ / __ \|  | \/\  \__(  <_> ) /_/ \  ___/  \     \___|    |___|   |");
Console.WriteLine(@" |______  /(____  /__|    \___  >____/\____ |\___  >  \______  /_______ \___|");
Console.WriteLine(@"        \/      \/            \/           \/    \/          \/        \/    ");
Console.WriteLine("");
Console.WriteLine("URL: "+settings.Uri);
Console.WriteLine("ConsumerKey: "+settings.ConsumerKey);
Console.WriteLine("ConsumerSecret: "+new string('*', settings.ConsumerSecret.Length));
Console.WriteLine(new string('-', 100));
Console.WriteLine("");

Console.Write("Initializing ...");
var service = new ProductService(settings);
var allProducts = await service.ReloadProductsAsync();
Console.Write(Environment.NewLine);
Console.WriteLine("Initialized "+allProducts.Count+" products.");
Console.WriteLine("Press Ctrl-C to exit");
Console.WriteLine("");
var player = new Player();
var alert = "sound/alert.mp3";
var confirm = "sound/confirm.mp3";

do {
    Console.Write("Product: ");
    var productInput = Console.ReadLine();
    var products = service.Search(productInput ?? "");
    switch (products.Count) {
        case 0:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No product found with search term.");

            await player.Play(alert);
            break;
            
        case 1:
            var product = products[0];

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Found: "+product.ArtikelId +" - "+product.Name);

            await player.Play(confirm);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Barcode: ");

            var barcodeInput = Console.ReadLine();
            await service.SetBarcodeAsync(product, barcodeInput);
            break;

        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Found multiple products.");
            await player.Play(alert);
            break;
    }
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine();
    Console.WriteLine(new string('-', 100));
    Console.WriteLine();
} while (true);