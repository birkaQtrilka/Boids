using GXPEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace gxpengine_template
{
    internal class MyGame : Game
    {
        public MyGame() : base(550,550,false)
        {
            AddChild(new Flocking(20,100));
        }

        static void Main()
        {
            new MyGame().Start();
        }
    }
}
