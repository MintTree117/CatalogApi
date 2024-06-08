namespace CatalogApplication.Seed;

internal static class SeedData
{
    internal static readonly Dictionary<string, string> PrimaryCategories = new() {
        { "Computers & Laptops", "Computer" },
        { "Smartphones & Tablets", "Smartphone" },
        { "TVs & Home Entertainment", "Television" },
        { "Cameras & Photography", "Camera" },
        { "Audio & Headphones", "Headphone" },
        { "Wearable Technology", "Wearable" },
        { "Cables & Adapters", "Cable" },
        { "Storage Devices", "Storage Device" },
        { "Home Appliances", "Appliance" },
        { "Office Electronics", "Office Electronic" }
    };
    internal static readonly Dictionary<string, string>[] SubCategories = [
        // Computers & Laptops
        new Dictionary<string, string> {
            { "Laptops", "Laptop" },
            { "Desktops", "Desktop" },
            { "Tablets", "Tablet" },
            { "Monitors", "Monitor" },
            { "Computer Accessories", "Computer Accessory" },
            { "Gaming Laptops & PCs", "Gaming Laptop/PC" },
            { "Gaming Consoles", "Gaming Console" },
            { "Video Games", "Video Game" },
            { "Gaming Accessories", "Gaming Accessory" }
        },
        // Smartphones & Tablets
        new Dictionary<string, string> {
            { "Smartphones", "Smartphone" },
            { "Tablets", "Tablet" },
            { "Smartwatch", "Smartwatch" },
            { "Mobile Accessories", "Mobile Accessory" }
        },
        // TVs & Home Entertainment
        new Dictionary<string, string> {
            { "Televisions", "Television" },
            { "Home Theater Systems", "Home Theater System" },
            { "Media Players", "Media Player" },
            { "Projectors", "Projector" },
            { "TV Accessories", "TV Accessory" }
        },
        // Cameras & Photography
        new Dictionary<string, string> {
            { "Digital Cameras", "Digital Camera" },
            { "DSLR Cameras", "DSLR Camera" },
            { "Action Cameras", "Action Camera" },
            { "Lenses & Accessories", "Lens & Accessory" },
            { "Camera Drones", "Camera Drone" }
        },
        // Audio & Headphones
        new Dictionary<string, string> {
            { "Headphones", "Headphone" },
            { "Speakers", "Speaker" },
            { "Home Audio Systems", "Home Audio System" },
            { "MP3 Players", "MP3 Player" },
            { "Audio Accessories", "Audio Accessory" }
        },
        // Wearable Technology
        new Dictionary<string, string> {
            { "Smartwatches", "Smartwatch" },
            { "Fitness Trackers", "Fitness Tracker" },
            { "VR Headsets", "VR Headset" },
            { "Wearable Accessories", "Wearable Accessory" }
        },
        // Cables & Adapters
        new Dictionary<string, string> {
            { "Usb Cables", "Usb Cable" },
            { "Lightning Cables", "Lightning Cable" },
            { "Hdmi Cables", "Hdmi Cable" },
            { "DVI Cables", "DVI Cable" },
            { "Chargers", "Charger" },
            { "Power Banks", "Power Bank" },
            { "Batteries", "Battery" }
        },
        // Storage Devices
        new Dictionary<string, string> {
            { "Usb Drives", "Usb Drive" },
            { "Hard Drives", "Hard Drive" },
            { "Solid Drives", "Solid Drive" }
        },
        // Home Appliances
        new Dictionary<string, string> {
            { "Washing Machines", "Washing Machine" },
            { "Dryers", "Dryer" },
            { "Vacuum Cleaners", "Vacuum Cleaner" },
            { "Carpet Cleaners", "Carpet Cleaner" },
            { "Microwaves", "Microwave" },
            { "Ovens", "Oven" },
            { "Refrigerators", "Refrigerator" },
            { "Dishwashers", "Dishwasher" }
        },
        // Office Electronics
        new Dictionary<string, string> {
            { "Printers & Scanners", "Printer/Scanner" },
            { "Projectors", "Projector" },
            { "Office Accessories", "Office Accessory" },
            { "Calculators", "Calculator" },
            { "Shredders", "Shredder" }
        }
    ];

    internal static Dictionary<string, List<string>> ProductImagesByPrimaryCategory = [];

    internal const double MaxPrice = 10000;
}