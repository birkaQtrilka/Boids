using GXPEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace gxpengine_template
{
    internal class MyGame : Game
    {
        public MyGame() : base(900,750,false)
        {
            AddChild(new Flocking(25,700));
            //AddChild(new Test());
        }

        static void Main()
        {
            new MyGame().Start();
        }
    }
}
