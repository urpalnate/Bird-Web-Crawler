using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ParseURL
{
    public class Bird
    {
        public int Id { get; set; }
        public List<BirdImage> Images { get; set; } = new List<BirdImage>();
        [Required]
        public List<string> IdentificationTips { get; set; } = new List<string>();
        public List<string> MorphOne { get; set; } = new List<string>();
        public List<string> MorphTwo { get; set; } = new List<string>();
        [Required]
        public string Name { get; set; }
        [Required]
        public string ScientificName { get; set; }
        [Required]
        public Family Family { get; set; }
        public int FamilyId { get; set; }
        public int OrderId { get; set; }
        [Required]
        public Order Order { get; set; }
        public HashSet<string> Colors { get; set; } = new HashSet<string>();
        public int Length { get; set; }
        public int WingSpan { get; set; }
        public Size Size { get; set; }
        //The site seems to limit itself to these colors when describing the birds, but I'm not sure this is exhaustive
        public static string[] PossibleColors = new string[] { "pink", "pinkish", "flesh", "pale", "orange", "purple",
            "dark", "darker", "darkish", "light", "lighter", "white", "black", "blackish", "red", "reddish", "gold", "silver", "tan",
            "blue", "bluish", "yellow", "yellowish", "green", "greenish", "gray", "grayish", "brown", "brownish"};
    }
    //Not yet implemented
    public enum Size { Small, Medium, Large }
}


