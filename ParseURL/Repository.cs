using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseURL
{
    public class Repository
    {
        public static void AddBird(Bird bird)
        {
            using (var context = new Context())
            {
                context.Birds.Add(bird);
                context.SaveChanges();
            }
        }

        public static void AddOrder(Order order)
        {
            using (var context = new Context())
            {
                context.Orders.Add(order);
                context.SaveChanges();
            }
        }

        public static void AddFamily(Family family)
        {
            using (var context = new Context())
            {
                context.Families.Add(family);
                context.SaveChanges();
            }
        }
    }
}
