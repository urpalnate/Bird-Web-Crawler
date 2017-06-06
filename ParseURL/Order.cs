using System.Collections.Generic;

namespace ParseURL
{
    public class Order
    {
        public Order(string name)
        {
            OrderName = name;
            Birds = new List<Bird>();
        }
        public int Id { get; set; }
        public string OrderName { get; set; }
        public ICollection<Bird> Birds { get; set; }
    }
}
