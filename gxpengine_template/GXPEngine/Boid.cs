using GXPEngine;
using GXPEngine.Core;
using System.Collections.Generic;
using System.Drawing;


namespace gxpengine_template
{
    //use this in tandem with a quad tree space partitioning algorithm
    class Boid : INodeUnit
    {
        public const float MAX_SPEED = 2f;
        
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;

        public Vector2 OldPosition { get; private set; }
        public Vector2 OldVelocity { get; private set; }

        public List<int> Iterator { get; } = new List<int>();

        public Boid(Vector2 position, Vector2 velocity) 
        {
            Position = position;
            Velocity = velocity;
            OldPosition = position;
            OldVelocity = velocity;

        }

        public void Update()
        {
            OldPosition = Position;
            OldVelocity = Velocity;
            
            Velocity += Acceleration;
            Velocity.Limit(MAX_SPEED);
            //Velocity.SetLength(MAX_SPEED);

            Position += Velocity;
            Acceleration = Vector2.zero;

        }

        public void Draw(EasyDraw canvas)
        {
            //canvas.Fill(Color.Wheat);


            float rotation = Velocity.GetAngleRadians() - Mathf.PI * .5f;
            Vector2 offset = Velocity.Normalized() * 15f / 3f;

            Vector2 backP1 = Position + Vector2.right * 4f - offset;
            Vector2 backP2 = Position + Vector2.right * -4f - offset;
            Vector2 frontP = Position + Vector2.up * 15f - offset;

            //backP1.RotateRadians(rotation);
            //backP2.RotateRadians(rotation);
            //frontP.RotateRadians(rotation);


            //backP1 += Position - offset;
            //backP2 += Position - offset;
            //frontP += Position - offset;

            backP1.RotateAroundRadians(rotation, Position);
            backP2.RotateAroundRadians(rotation, Position);
            frontP.RotateAroundRadians(rotation, Position);

            canvas.Line(backP1.x, backP1.y, backP2.x, backP2.y);
            canvas.Line(backP1.x, backP1.y, frontP.x, frontP.y);
            canvas.Line(backP2.x, backP2.y, frontP.x, frontP.y);
        }

        public void Handle(List<INodeUnit> others)
        {
            throw new System.NotImplementedException();
        }

        public Vector2 GetPosition()
        {
            return OldPosition;
        }
    }
}
