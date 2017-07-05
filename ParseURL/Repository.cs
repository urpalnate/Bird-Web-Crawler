using System.Data.Entity;

namespace ParseURL
{
    public class Repository
    {
        public static void AddBird(Bird bird)
        {
            //This is to avoid duplicate entries of the navigation properties Family and Order
            //The if conditions tests whether the Order/Family object already exists, because
            //only EF will modify the FK property when it adds it to the Database Context.
            //So by setting EntityState.Unchanged EF should stop change tracking and not add duplicate entries
            using (var context = new Context())
            {
                if (bird.Family.OrderId > 0 && bird.Family.Order != null)
                {
                    context.Entry(bird.Family.Order).State = EntityState.Unchanged;
                }
                if (bird.FamilyId > 0 && bird.Family != null)
                {
                    context.Entry(bird.Family).State = EntityState.Unchanged; 
                }

                context.Birds.Add(bird);
                context.SaveChanges();
            }
        }
    }
}
