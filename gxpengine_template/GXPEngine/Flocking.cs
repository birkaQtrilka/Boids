using GXPEngine;
using GXPEngine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace gxpengine_template
{
    class Flocking : EasyDraw
    {
        public const float MAX_FORCE = .1f;

        readonly float _persceptionDistance;
        readonly Boid[] _boids;
        readonly QuadTree<Boid> _spacePartitioning;
        readonly List<Boid> _flock = new List<Boid>();

        struct FlockData
        {
            public Vector2 SeparationAverage;
            public Vector2 PositionAverage;
            public Vector2 AlignmentAverage;
        }

        public Flocking(float persceptionDistance, int boidCoint) : base(Game.main.width, Game.main.height)
        {
            _persceptionDistance = persceptionDistance;
            _boids = new Boid[boidCoint];

            ShapeAlign(CenterMode.Center, CenterMode.Center);
            Random random = new Random(943543223);
            for (int i = 0; i < boidCoint; i++)
            {
                int dir1 = random.Next(0,2) == 1 ? - 1: 1; 
                int dir2 = random.Next(0,2) == 1 ? - 1 : 1; 

                Vector2 randomDir = new Vector2(dir1, dir2);
                randomDir.SetLength(Boid.MAX_SPEED);
                Vector2 randomPos = new Vector2(random.Next(0, game.width), random.Next(0, game.width));
                _boids[i] = new Boid(randomPos, randomDir);
            }

            _spacePartitioning = new QuadTree<Boid>(new BorderBox(0,0,game.width, game.height));
            foreach (Boid boid in _boids)
                _spacePartitioning.AddItem(boid);
        }

        void Update()
        {
            ClearTransparent();
            Fill(Color.Wheat);
            Text("FPS: " + game.currentFps);

            //int maxCount = 0;
            //instead of these loops use a quad tree space partitioning algorithm
            for (int i = 0; i < _boids.Length; ++i)
            {
                Boid currBoid = _boids[i];

                _flock.Clear();
                _spacePartitioning.Search(currBoid.Position, _persceptionDistance, _flock);

                FlockData flock = GetFlockData(_flock, currBoid);

                currBoid.Acceleration += flock.PositionAverage;
                currBoid.Acceleration += flock.AlignmentAverage;
                currBoid.Acceleration += flock.SeparationAverage;
                currBoid.Acceleration.Limit(MAX_FORCE);

                currBoid.Update();
                //maxCount = Mathf.Max(maxCount, _flock.Count);
                TeleportBetweenEdges(currBoid);
                _spacePartitioning.Relocate(currBoid);
                ColorBoidBasedOnDensity(_flock.Count - 1);

                currBoid.Draw(this);
            }
            //_spacePartitioning.Update(this);//drawing the tree
            //Console.WriteLine(maxCount.ToString());
        }

        FlockData GetFlockData(List<Boid> flock, Boid currBoid)
        {
            FlockData flockData = new FlockData();
            foreach (Boid checkedBoid in flock)
            {
                if (checkedBoid == currBoid) continue;

                float distance = Vector2.Distance(currBoid.OldPosition, checkedBoid.OldPosition);

                flockData.AlignmentAverage += checkedBoid.OldVelocity;
                //cohesion
                flockData.PositionAverage += checkedBoid.OldPosition;
                //separation
                Vector2 desired = currBoid.Position - checkedBoid.OldPosition;
                //desired.SetLength(desired.Length() / distance);
                desired /= distance * distance;//length of vector is inversly proportional to the distance between the current and checked boid
                flockData.SeparationAverage += desired;
            }
            int closeBoidsCount = flock.Count - 1; //subtract 1 cuz the current boid is included in the list
            if (closeBoidsCount > 0)
            {
                //alignment
                flockData.AlignmentAverage /= closeBoidsCount;
                flockData.AlignmentAverage.SetLength(Boid.MAX_SPEED);
                flockData.AlignmentAverage -= currBoid.Velocity;
                flockData.AlignmentAverage.Limit(MAX_FORCE);
                //separation
                flockData.SeparationAverage /= closeBoidsCount;
                flockData.SeparationAverage.SetLength(Boid.MAX_SPEED);
                flockData.SeparationAverage -= currBoid.Velocity;
                flockData.SeparationAverage.Limit(MAX_FORCE);
                //cohesion
                flockData.PositionAverage /= closeBoidsCount;
                flockData.PositionAverage -= currBoid.Position;
                flockData.PositionAverage.SetLength(Boid.MAX_SPEED);
                flockData.PositionAverage -= currBoid.Velocity;
                flockData.PositionAverage.Limit(MAX_FORCE);

            }

            return flockData;
        }

        void ColorBoidBasedOnDensity(float density, float maxCount = 50)
        {
            
            float ratio = 1 - density / maxCount;
            ratio = Mathf.Clamp(ratio, 0, 1);
            Color red = Color.Red;
            Color green = Color.Green;

            int gradientR = (int)Utils.Lerp(red.R, green.R, ratio);
            int gradientG = (int)Utils.Lerp(red.G, green.G, ratio);
            int gradientB = (int)Utils.Lerp(red.B, green.B, ratio);

            Stroke(gradientR, gradientG, gradientB);
            StrokeWeight(2f);
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

    }
}
