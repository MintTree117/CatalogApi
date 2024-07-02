namespace CatalogApplication.Seeding.SeedData;

internal static class CategorySeedData
{
    internal static readonly string[] PrimaryCategories = [
        "Computers & Laptops",
        "Smartphones & Tablets",
        "TVs & Home Entertainment",
        "Cameras & Photography",
        "Audio & Headphones",
        "Wearable Technology",
        "Cables & Adapters",
        "Storage Devices",
        "Home Appliances",
        "Office Electronics"
    ];

    internal static readonly Dictionary<string, string>[] SecondaryCategories = {
        // Computers & Laptops
        new() {
            { "Laptops", "Laptop" },
            { "Desktops", "Desktop" },
            { "Tablets", "Tablet" },
            { "Monitors", "Monitor" },
            { "Computer Accessories", "Computer Accessory" },
            { "Gaming Laptops & PCs", "Gaming Laptop/PC" },
            { "Gaming Consoles", "Gaming Console" },
            { "Video Games", "Video Game" },
            { "Gaming Accessories", "Gaming Accessory" },
            { "Networking Equipment", "Networking Equipment" },
            { "Software", "Software" }
        },
        // Smartphones & Tablets
        new() {
            { "Smartphones", "Smartphone" },
            { "Tablets", "Tablet" },
            { "Smartwatches", "Smartwatch" },
            { "Mobile Accessories", "Mobile Accessory" },
            { "Chargers & Cables", "Charger & Cable" },
            { "Phone Cases", "Phone Case" },
            { "Screen Protectors", "Screen Protector" },
            { "Power Banks", "Power Bank" },
            { "Bluetooth Headsets", "Bluetooth Headset" },
            { "Car Accessories", "Car Accessory" }
        },
        // TVs & Home Entertainment
        new() {
            { "Televisions", "Television" },
            { "Home Theater Systems", "Home Theater System" },
            { "Media Players", "Media Player" },
            { "Projectors", "Projector" },
            { "TV Accessories", "TV Accessory" },
            { "Sound Bars", "Sound Bar" },
            { "Streaming Devices", "Streaming Device" },
            { "Blu-ray Players", "Blu-ray Player" },
            { "TV Mounts", "TV Mount" },
            { "Remote Controls", "Remote Control" }
        },
        // Cameras & Photography
        new() {
            { "Digital Cameras", "Digital Camera" },
            { "DSLR Cameras", "DSLR Camera" },
            { "Action Cameras", "Action Camera" },
            { "Lenses & Accessories", "Lens & Accessory" },
            { "Camera Drones", "Camera Drone" },
            { "Camera Bags & Cases", "Camera Bag & Case" },
            { "Tripods & Supports", "Tripod & Support" },
            { "Memory Cards", "Memory Card" },
            { "Lighting & Studio Equipment", "Lighting & Studio Equipment" },
            { "Camera Batteries & Chargers", "Camera Battery & Charger" }
        },
        // Audio & Headphones
        new() {
            { "Headphones", "Headphone" },
            { "Speakers", "Speaker" },
            { "Home Audio Systems", "Home Audio System" },
            { "MP3 Players", "MP3 Player" },
            { "Audio Accessories", "Audio Accessory" },
            { "Bluetooth Speakers", "Bluetooth Speaker" },
            { "Sound Bars", "Sound Bar" },
            { "Voice Assistants", "Voice Assistant" },
            { "Home Theater Systems", "Home Theater System" },
            { "Audio Cables", "Audio Cable" }
        },
        // Wearable Technology
        new() {
            { "Smartwatches", "Smartwatch" },
            { "Fitness Trackers", "Fitness Tracker" },
            { "VR Headsets", "VR Headset" },
            { "Wearable Accessories", "Wearable Accessory" },
            { "Smart Glasses", "Smart Glasses" },
            { "Wearable Cameras", "Wearable Camera" },
            { "Health Monitors", "Health Monitor" },
            { "Smart Rings", "Smart Ring" },
            { "Smart Clothing", "Smart Clothing" },
            { "Wearable Chargers", "Wearable Charger" }
        },
        // Cables & Adapters
        new() {
            { "USB Cables", "USB Cable" },
            { "Lightning Cables", "Lightning Cable" },
            { "HDMI Cables", "HDMI Cable" },
            { "DVI Cables", "DVI Cable" },
            { "Chargers", "Charger" },
            { "Power Banks", "Power Bank" },
            { "Batteries", "Battery" },
            { "Adapters", "Adapter" },
            { "Extension Cords", "Extension Cord" },
            { "Ethernet Cables", "Ethernet Cable" }
        },
        // Storage Devices
        new() {
            { "USB Drives", "USB Drive" },
            { "Hard Drives", "Hard Drive" },
            { "Solid State Drives", "Solid State Drive" },
            { "Memory Cards", "Memory Card" },
            { "External Hard Drives", "External Hard Drive" },
            { "Network Attached Storage", "Network Attached Storage" },
            { "Cloud Storage", "Cloud Storage" },
            { "Backup Drives", "Backup Drive" },
            { "Flash Drives", "Flash Drive" },
            { "Optical Drives", "Optical Drive" }
        },
        // Home Appliances
        new() {
            { "Washing Machines", "Washing Machine" },
            { "Dryers", "Dryer" },
            { "Vacuum Cleaners", "Vacuum Cleaner" },
            { "Carpet Cleaners", "Carpet Cleaner" },
            { "Microwaves", "Microwave" },
            { "Ovens", "Oven" },
            { "Refrigerators", "Refrigerator" },
            { "Dishwashers", "Dishwasher" },
            { "Coffee Makers", "Coffee Maker" },
            { "Air Conditioners", "Air Conditioner" }
        },
        // Office Electronics
        new() {
            { "Printers & Scanners", "Printer/Scanner" },
            { "Projectors", "Projector" },
            { "Office Accessories", "Office Accessory" },
            { "Calculators", "Calculator" },
            { "Shredders", "Shredder" },
            { "Fax Machines", "Fax Machine" },
            { "Office Phones", "Office Phone" },
            { "Laminators", "Laminator" },
            { "Binding Machines", "Binding Machine" },
            { "Label Makers", "Label Maker" }
        }
    };
}