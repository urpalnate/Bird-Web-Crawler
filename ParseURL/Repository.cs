using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseURL
{
    public class Repository
    {
        public static void AddBird(Bird bird, int orderId, int familyId)
        {
            //This is to avoid duplicate entries of the navigation properties Family and Order
            //The if conditions tests whether the Order/Family object already exists, because
            //only EF will modify the Id property for Family/Order
            //So by associating the PK of Family/Order with the FK of bird EF knows 
            //to associate an existing Order/Family to this bird instance and doesn't create
            //a duplicate Order/Family record
            if (orderId > 0)
            {
                bird.Order = null;
                bird.OrderId = orderId;
            }
            if (familyId > 0)
            {
                bird.Family = null;
                bird.FamilyId = familyId;
            }
            using (var context = new Context())
            {
                context.Birds.Add(bird);
                context.SaveChanges();
            }
        }
    }
}
