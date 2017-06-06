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

        public static string[] PossibleColors = new string[] { "Pink", "pink", "Pinkish", "pinkish", "Flesh", "flesh", "Flesh-colored", "flesh-colored", "pale", "Pale", "orange", "Orange", "purple", "Purple",
            "Dark", "dark", "Darker", "darker", "Darkish", "darkish", "Light", "light", "Lighter", "lighter", "White", "white", "Black", "black", "Blackish", "blackish", "Red", "red", "Reddish", "reddish",
            "Blue", "blue", "Bluish", "bluish", "Yellow", "yellow", "Yellowish", "yellowish", "Green", "green", "Greenish", "greenish", "Gray", "gray", "grayish", "Grayish", "brown", "Brown", "brownish", "Brownish"};
    }

    public enum Size { Small, Medium, Large }
}


