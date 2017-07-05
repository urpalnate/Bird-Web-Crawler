using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseURL
{
    //This class represents all the text from a particular Morph of a particular specie
    //Each BulletPoint represents the text from a bulletpoint on the source website
    public class Description
    {
        public Description(Bird bird, string title)
        {
            BulletPoints = new List<BulletPoint>();
            Bird = bird;
            Title = title;
        }
        //Primary and Foreign Keys
        public int Id { get; set; }
        public int BirdId { get; set; }
        
        //Navigation Properties
        public virtual List<BulletPoint> BulletPoints { get; set; }
        public virtual Bird Bird { get; set; }

        //Scalar Property
        public string Title { get; set; }
    }
}
