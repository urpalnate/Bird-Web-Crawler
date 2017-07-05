using System.Collections.Generic;

namespace ParseURL
{
    //Principal Entity in relation to Bird, Dependent in relation to Order
    public class Family
    {
        public Family(string name, Order order)
        {
            FamilyName = name;
            Order = order;
        }
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string FamilyName { get; set; }
        public string CommonName { get; set; }
        public virtual Order Order { get; set; }
    }
}
