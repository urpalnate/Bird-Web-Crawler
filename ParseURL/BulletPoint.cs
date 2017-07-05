using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseURL
{
    //Each BulletPoint represents the text from a bulletpoint on the source website
    //Dependent Entity in relation to Description
    public class BulletPoint
    {
        public BulletPoint(string text, Description description)
        {
            Text = text;
            Description = description;
        }
        //Primary and Foreign Keys
        public int Id { get; set; }
        public int DescriptionId { get; set; }

        //Navigation Property
        public virtual Description Description { get; set; }

        //Scalar Property
        public string Text { get; set; }
    }
}
