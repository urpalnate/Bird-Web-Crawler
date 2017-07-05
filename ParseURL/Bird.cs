using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParseURL
{
    public class Bird
    {
        public Bird()
        {
            Images = new List<BirdImage>();
            Colors = new List<Color>();
            Descriptions = new List<Description>();
        }
        //Primary Key and Foreign Keys
        public int Id { get; set; }
        public int FamilyId { get; set; }

        //Navigation Properties
        public virtual Family Family { get; set; }
        public virtual ICollection<BirdImage> Images { get; set; }
        public virtual ICollection<Color> Colors { get; set; }
        //Each description represents a <ul> section of the source website. It will always have a "Primary" Description...
        //and may contain several Secondary descriptions of variations (morphs) of the same specie
        public virtual List<Description> Descriptions { get; set; }

        //Scalar Properties
        [Required]
        public string Name { get; set; }
        [Required]
        public string ScientificName { get; set; }
        public int Length { get; set; }
        public int WingSpan { get; set; }
        public Size Size { get; set; }
        public bool Pelagic { get; set; }
        public string SimilarSpecies { get; set; }

        //Helper Array Not Part of Database
        public static string[] PossibleColors = new string[] { "pink", "pinkish", "flesh", "pale", "orange", "purple",
            "dark", "darker", "darkish", "light", "lighter", "white", "black", "blackish", "red", "reddish", "gold", "silver", "tan",
            "blue", "bluish", "yellow", "yellowish", "green", "greenish", "gray", "grayish", "brown", "brownish"};
    }
    //Not yet implemented
    public enum Size { Smallest, Small, Medium, Large, Largest }
}


