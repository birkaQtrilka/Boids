using System;
using System.Collections.Generic;

namespace GXPEngine.Core
{
    public struct Vector2
    {
        public float x;
        public float y;
        public static Vector2 zero = new Vector2(0f, 0f);
        public static Vector2 left = new Vector2(-1f, 0f);
        public static Vector2 right = new Vector2(1f, 0f);
        public static Vector2 up = new Vector2(0f, 1f);


        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        #region Operator overloads
        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2
            {
                x = v1.x + v2.x,
                y = v1.y + v2.y
            };

        }
        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2
            {
                x = v1.x - v2.x,
                y = v1.y - v2.y
            };

        }
        public static Vector2 operator *(Vector2 v1, float s)
        {
            return new Vector2
            {
                x = v1.x * s,
                y = v1.y * s
            };

        }

        public static Vector2 operator /(Vector2 v1, float s)
        {
            return new Vector2
            {
                x = v1.x / s,
                y = v1.y / s
            };

        }
        #endregion

        public float Length()
        {
            return Mathf.Sqrt(x * x + y * y);
        }
        public Vector2 Normalized()
        {
            float l = Length();
            if (l == 0)
                return Vector2.zero;
            return new Vector2(x / l, y / l);
        }

        public void Normalize()
        {
            float l = Length();
            if (l == 0)
                this = zero;
            else
                this = new Vector2(x / l, y / l);
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return a + (b - a) * t;

        }
        override public string ToString() {
			return "[Vector2 " + x + ", " + y + "]";
		}
		public static float Distance(Vector2 a, Vector2 b)
		{ 
			return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
		}

        public void Limit(float limit) 
        {
            float l = Length();
            if (l > limit && l != 0)
                this = (this / l) * limit;
        }

        public void SetLength(float length)
        {
            float l = Length();
            if (l == 0) return;
            this = (this / l) * length;
        }

        public float GetAngleRadians()
        {
            return Mathf.Atan2(y, x);
        }

        public void RotateRadians(float rads)
        {
            float cos = Mathf.Cos(rads);
            float sin = Mathf.Sin(rads);
            Vector2 copy = this;
            x = copy.x * cos - copy.y * sin;
            y = copy.x * sin + copy.y * cos;

        }
        public void RotateAroundRadians(float rads, Vector2 point)
        {
            x -= point.x;
            y -= point.y;

            RotateRadians(rads);

            x += point.x;
            y += point.y;
        }

    }
}

