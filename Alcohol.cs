using System;

namespace Alcohol
{
    public delegate void AddAlcoholObject(object obj, int target_index = -1);

    public class NameAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public enum hop_types
    {
        perle = 1,
        saphir = 2,
        comet = 3,
        mosaic = 4,
        citra = 5,
        galaxy = 6,
    }

    public enum extra_flavours
    {
        fruit = 1,
        flower = 2,
        honey = 3,
        herbs = 4,
        wheat = 5,
    }

    public enum strong_additions
    {
        wheat = 1,
        birch_buds = 2,
        gold = 3,
        silver = 4,
    }

    public class AlcoholDrink
    {
        public string title;
        public string manufacturer;
        public double degree;
        [Name(Name = "container volume")] public double container_volume;
    }

    public class Beer : AlcoholDrink
    {
        public string sort;
        public bool filtration;
        public bool lightness;
        public string color;
        public int consistency;
        public BeerContainer container;
    }

    public class CraftBeer : Beer
    {
        [Name(Name = "addition type")] public string addition_type;
        [Name(Name = "hop type")] public hop_types hop_type;
        [Name(Name = "extra flavour")] public extra_flavours extra_flavour;
    }

    public class Wine : AlcoholDrink
    {
        [Name(Name = "sugar type")] public string sugar_type;
        public string color;
        public WineOrigin origin;
        public string aroma;
        [Name(Name = "harmonious dish")] public string harmonious_dish;
    }

    public class StrongDrink : AlcoholDrink
    {
        [Name(Name = "main component")] public string main_component;
        public strong_additions addition;
        [Name(Name = "distillation count")] public int distillation_count;
        [Name(Name = "alcohol type")] public string alcohol_type;
    }

    public class WineOrigin
    {
        public int year;
        public string country;
        public string region;
    }

    public class BeerContainer
    {
        [Name(Name = "container material")] public string container_material;
        [Name(Name = "decor quality")] public int decor_quality;
        [Name(Name = "open mechanism")] public bool open_mechanism;
    }
}