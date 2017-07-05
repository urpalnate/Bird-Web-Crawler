using System.Collections.Generic;

namespace ParseURL
{
    //Principal Entity in relation to Bird and Family
    public class Order
    {
        public Order(string name)
        {
            OrderName = name;
        }
        public int Id { get; set; }
        public string OrderName { get; set; }
    }
}
