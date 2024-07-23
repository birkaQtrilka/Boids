using GXPEngine;
using GXPEngine.Core;
using System;
using System.Drawing;
using System.Net;


namespace gxpengine_template
{
    class Flocking : EasyDraw
    {
        readonly float _persceptionDistance;
        readonly Boid[] _boids;
        public const float MAX_FORCE = .1f;

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
        }

        void Update()
        {
            ClearTransparent();
            //Fill(Color.Wheat);
            //Text("FPS: " + game.currentFps);

            //instead of these loops use a quad tree space partitioning algorithm
            for (int i = 0; i < _boids.Length; ++i)
            {
                Boid currBoid = _boids[i];
                Vector2 alignmentAverage = new Vector2(0,0);
                Vector2 positionAverage = new Vector2(0,0);
                Vector2 separationAverage = new Vector2(0,0);

                int closeBoidsCount = 0;
                for (int j = 0; j < _boids.Length; ++j)
                {
                    if (j == i) continue;

                    Boid checkedBoid = _boids[j];
                    float distance = Vector2.Distance(currBoid.OldPosition, checkedBoid.OldPosition);
                    if (distance > _persceptionDistance) continue;
                    if (distance == 0) distance = 0.0001f;

                    closeBoidsCount++;
                    //allignment
                    alignmentAverage += checkedBoid.OldVelocity;
                    //cohesion
                    positionAverage += checkedBoid.OldPosition;
                    //separation
                    Vector2 desired = currBoid.Position - checkedBoid.OldPosition;

                    desired /= distance;//length of vector is inversly proportional to the distance between the current and checked boid
                    separationAverage += desired;
                }

                if (closeBoidsCount > 0) 
                {
                    //alignment
                    alignmentAverage /= closeBoidsCount; 
                    alignmentAverage.SetLength(Boid.MAX_SPEED);
                    alignmentAverage -= currBoid.Velocity;
                    alignmentAverage.Limit(MAX_FORCE);
                    //separation
                    separationAverage /= closeBoidsCount;
                    separationAverage.SetLength(1.8f);
                    separationAverage -= currBoid.Velocity;
                    separationAverage.Limit(MAX_FORCE);
                    //cohesion
                    positionAverage /= closeBoidsCount;
                    positionAverage -= currBoid.Position;
                    positionAverage.SetLength(Boid.MAX_SPEED);
                    positionAverage -= currBoid.Velocity;
                    positionAverage.Limit(MAX_FORCE);

                }

                currBoid.Acceleration += positionAverage;
                currBoid.Acceleration += alignmentAverage ;
                currBoid.Acceleration += separationAverage;
                currBoid.Acceleration.Limit(MAX_FORCE);

                currBoid.Update();

                TeleportBetweenEdges(currBoid);

                ColorBoidBasedOnDensity(closeBoidsCount);

                currBoid.Draw(this);
            }
        }

        void ColorBoidBasedOnDensity(float density)
        {
            float maxCount = 20;
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
