using System.Collections.Generic;

namespace ParseURL
{
    public class Family
    {
        public Family(string name)
        {
            FamilyName = name;
            Birds = new List<Bird>();
        }
        public int Id { get; set; }
        public string FamilyName { get; set; }
        public ICollection<Bird> Birds { get; set; }
    }
}
