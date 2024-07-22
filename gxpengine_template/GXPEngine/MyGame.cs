using GXPEngine;
using GXPEngine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace gxpengine_template
{
    internal class MyGame : Game
    {
        public MyGame() : base(800,800,false)
        {
            AddChild(new Flocking(100,200));
        }

        static void Main()
        {
            new MyGame().Start();
        }
    }

    //use this in tandem with a quad tree space partitioning algorithm
    class Boid
    {
        public const float MAX_SPEED = 2;
        
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;

        public Vector2 OldPosition { get; private set; }
        public Vector2 OldVelocity { get; private set; }

        public Boid(Vector2 position, Vector2 velocity) 
        {
            Position = position;
            Velocity = velocity;
            OldPosition = position;
            OldVelocity = velocity;
        }

        public void Update()
        {
            if (float.IsNaN(Velocity.x))
            {

            }
            OldPosition = Position;
            OldVelocity = Velocity;
            
            Velocity += Acceleration;
            if (Velocity.Length() > MAX_SPEED)
                Velocity = Velocity.Normalized() * MAX_SPEED;
            Position += Velocity;
            Acceleration = Vector2.zero;

        }
    }

    class Flocking : EasyDraw
    {
        readonly float _persceptionDistance;
        readonly Boid[] _boids;

        public Flocking(float persceptionDistance, int boidCoint) : base(Game.main.width, Game.main.height)
        {
            _persceptionDistance = persceptionDistance;
            _boids = new Boid[boidCoint];

            ShapeAlign(CenterMode.Center, CenterMode.Center);
            Random random = new Random(9523);
            for (int i = 0; i < boidCoint; i++)
            {
                Vector2 randomDir = new Vector2(random.Next(-1, 2), random.Next(-1,2)).Normalized();
                randomDir *= .3f;
                _boids[i] = new Boid(new Vector2(random.Next(0, game.width), random.Next(0, game.width)), randomDir);
            }

        }

        void Update()
        {
            ClearTransparent();
            Fill(Color.Wheat);
            Text("FPS: " + game.currentFps);
            //instead of these loops use a quad tree space partitioning algorithm
            for (int i = 0; i < _boids.Length; ++i)
            {
                Boid currBoid = _boids[i];
                Vector2 alignmentAverage = new Vector2(0,0);
                Vector2 positionAverage = new Vector2(0,0);

                int closeBoidsCount = 0;
                for (int j = 0; j < _boids.Length; ++j)
                {
                    if (j == i) continue;

                    Boid checkedBoid = _boids[j];
                    float distance = Vector2.Distance(currBoid.OldPosition, checkedBoid.OldPosition);
                    if (distance > _persceptionDistance) continue;

                    closeBoidsCount++;
                    //allignment\
                    alignmentAverage += checkedBoid.OldVelocity;
                    positionAverage += checkedBoid.OldPosition;

                    //Line(currBoid.OldPosition.x, currBoid.OldPosition.y, checkedBoid.OldPosition.x, checkedBoid.OldPosition.y);
                }


                if(closeBoidsCount > 0) //allignment
                {
                    alignmentAverage /= closeBoidsCount;
                    alignmentAverage -= currBoid.Velocity;
                    

                    positionAverage /= closeBoidsCount;
                    positionAverage -= currBoid.Velocity;
                    positionAverage -= currBoid.Position;
                }

                currBoid.Acceleration += alignmentAverage ;
                currBoid.Acceleration += positionAverage;

                //draw according to new values
                currBoid.Update();

                TeleportBetweenEdges(currBoid);

                Fill(Color.Wheat);

                Ellipse(currBoid.Position.x, currBoid.Position.y, 5, 5);
                //NoFill();
                //Ellipse(currBoid.Position.x, currBoid.Position.y, _persceptionDistance * 2, _persceptionDistance * 2);

            }
        }

        void TeleportBetweenEdges(Boid boid)
        {
            if (boid.Position.x < 0)
                boid.Position.x = game.width;

            if (boid.Position.x > game.width)
                boid.Position.x = 0;

            if (boid.Position.y < 0)
                boid.Position.y = game.height;

            if (boid.Position.y > game.height)
                boid.Position.y = 0;
        }

        //Vector2 Separation()
        //{

        //}

        //Vector2 Alignment()
        //{

        //}

        //Vector2 Cohesion()
        //{

        //}

    }
}
