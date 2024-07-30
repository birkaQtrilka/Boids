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
        readonly QuadTree<Boid> _quadTree;
        readonly List<Boid> _flock = new List<Boid>();

        struct FlockData
        {
            public Vector2 SeparationForce;
            public Vector2 CohesionForce;
            public Vector2 AlignmentForce;
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

            _quadTree = new QuadTree<Boid>(new BorderBox(0,0,game.width, game.height));
            foreach (Boid boid in _boids)
                _quadTree.AddItem(boid);
        }

        void Update()
        {
            ClearTransparent();
            Fill(Color.Wheat);
            Text("FPS: " + game.currentFps);
            
            for (int i = 0; i < _boids.Length; ++i)
            {
                Boid currBoid = _boids[i];

                //to avoid checking against every other boid
                _flock.Clear();
                _quadTree.Search(currBoid.Position, _persceptionDistance, _flock);

                FlockData flock = GetFlockData(_flock, currBoid);

                currBoid.Acceleration += flock.CohesionForce;
                currBoid.Acceleration += flock.AlignmentForce;
                currBoid.Acceleration += flock.SeparationForce;
                currBoid.Acceleration.Limit(MAX_FORCE);

                currBoid.Update();
                TeleportBetweenEdges(currBoid);
                _quadTree.Relocate(currBoid);
                ColorBoidBasedOnDensity(_flock.Count - 1);

                currBoid.Draw(this);
            }
            //_spacePartitioning.Update(this);//drawing the tree
        }

        FlockData GetFlockData(List<Boid> flock, Boid currBoid)
        {
            FlockData flockData = new FlockData();
            foreach (Boid checkedBoid in flock)
            {
                if (checkedBoid == currBoid) continue;

                float distance = Vector2.Distance(currBoid.OldPosition, checkedBoid.OldPosition);

                flockData.AlignmentForce += checkedBoid.OldVelocity;
                //cohesion
                flockData.CohesionForce += checkedBoid.OldPosition;
                //separation
                Vector2 desired = currBoid.Position - checkedBoid.OldPosition;
                desired /= distance * distance;//length of vector is inversly proportional to the distance between the current and checked boid
                flockData.SeparationForce += desired;
            }
            int closeBoidsCount = flock.Count - 1; //subtract 1 cuz the current boid is included in the list
            
            if (closeBoidsCount == 0)
                return flockData;

            //alignment
            flockData.AlignmentForce /= closeBoidsCount;
            flockData.AlignmentForce.SetLength(Boid.MAX_SPEED);
            flockData.AlignmentForce -= currBoid.Velocity;
            flockData.AlignmentForce.Limit(MAX_FORCE);
            //separation
            flockData.SeparationForce /= closeBoidsCount;
            flockData.SeparationForce.SetLength(Boid.MAX_SPEED);
            flockData.SeparationForce -= currBoid.Velocity;
            flockData.SeparationForce.Limit(MAX_FORCE);
            //cohesion
            flockData.CohesionForce /= closeBoidsCount;
            flockData.CohesionForce -= currBoid.Position;
            flockData.CohesionForce.SetLength(Boid.MAX_SPEED);
            flockData.CohesionForce -= currBoid.Velocity;
            flockData.CohesionForce.Limit(MAX_FORCE);

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
