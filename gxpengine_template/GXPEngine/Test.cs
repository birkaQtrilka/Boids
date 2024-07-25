using GXPEngine;
using GXPEngine.Core;
using gxpengine_template;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Test : EasyDraw
{
    QuadTree <Boid> tree;

    public Test() : base(Game.main.width, Game.main.height,false)
    {
    }

    void Update()
    {
        ClearTransparent();
        NoFill();
        StrokeWeight(5);

        tree = new QuadTree<Boid>(new BorderBox(0,0, width, height));

        if(Input.GetMouseButtonDown(0))
        {
            
        }

    }

    public bool CircleCointainsBox(Vector2 pos, float radius, BorderBox box)
    {
        float dx = Mathf.Max(Mathf.Abs(pos.x - box.x), Mathf.Abs(box.x + box.w - pos.x));
        float dy = Mathf.Max(Mathf.Abs(pos.y - box.y), Mathf.Abs(box.y + box.h - pos.y));
        return (radius * radius) >= (dx * dx) + (dy * dy);
    }

    public bool CircleOverlapsBox(Vector2 pos, float radius, BorderBox box)
    {
        Vector2 circleDistance = new Vector2(
            Mathf.Abs(pos.x - (box.x + box.w * .5f)),
            Mathf.Abs(pos.y - (box.y + box.h * .5f))
            );

        if (circleDistance.x > (box.w * .5f + radius)) return false;
        if (circleDistance.y > (box.h * .5f + radius)) return false;

        if (circleDistance.x <= (box.w * .5f)) return true;
        if (circleDistance.y <= (box.h * .5f)) return true;

        float cornerDistance_sq = Mathf.Pow(circleDistance.x - box.w * .5f, 2) +
                             Mathf.Pow(circleDistance.y - box.h * .5f, 2);

        return cornerDistance_sq <= (radius * radius);

    }
}
