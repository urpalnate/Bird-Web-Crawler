using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseURL
{
    public class Color
    {
        public Color(Bird bird, string color)
        {
            Bird = bird;
            ColorName = color;
        }
        public int Id { get; set; }
        public int BirdId { get; set; }
        public virtual Bird Bird { get; set; }
        public string ColorName { get; set; }
    }
}
