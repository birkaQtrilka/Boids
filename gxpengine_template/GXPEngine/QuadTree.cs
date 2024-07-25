using GXPEngine;
using GXPEngine.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class QuadTree<T> where T : INodeUnit
{
    public const int MAX_DEPTH = 6;

    readonly int _depth;
    //private bool _cleared; 

    public BorderBox Rect { get; private set; }

    BorderBox[] _borderBoxes;
    readonly QuadTree<T>[] _children = new QuadTree<T>[4];
    readonly List<T> _units = new List<T>();

    public QuadTree(BorderBox rect)
    {
        Resize(rect);
    }

    QuadTree(BorderBox rect, int depth)
    {
        this._depth = depth;
        Resize(rect);
    }

    public void Resize(BorderBox newArea)
    {
        Rect = newArea;
        Clear();

        float childW = Rect.w / 2;
        float childH = Rect.h / 2;

        _borderBoxes = new BorderBox[]
        {
            new BorderBox(Rect.x, Rect.y, childW, childH),
            new BorderBox(Rect.x + childW, Rect.y, childW, childH),
            new BorderBox(Rect.x, Rect.y + childH, childW, childH),
            new BorderBox(Rect.x + childW, Rect.y + childH, childW, childH),
        };
    }

    public void Clear()
    {
        _units.Clear();
        //_cleared = true;

        for (int i = 0; i < 4; i++)
        {
            _children[i]?.Clear();
        }
    }

    //public int Size()
    //{
    //    int size = _units.Count;
    //    for (int i = 0; i < 4; i++)
    //    {
    //        if (_children[i] != null) size += _children[i].Size();
    //    }
    //    return size;
    //}
    //2 0 0 3
    public void AddItem(T item)
    {
        item.Iterator.Clear();
        Add(item);
    }

    public void Remove(T unit)//use the binary trick from the third episode of LOD video instead of iterator
    {
        Remove(unit, 0);
    }

    public void Relocate(T unit)
    {
        Remove(unit);
        //calc direction of quad to relocate into and add item more efficiently
        AddItem(unit);
    }

    public List<T> Search(BorderBox box)
    {
        var results = new List<T>();
        Search(box, results);
        return results;
    }

    public List<T> Search(Vector2 circlePos, float radius)
    {
        var results = new List<T>();
        Search(circlePos, radius, results);
        return results;
    }

    void Add(T unit)
    {
        if (_depth + 1 >= MAX_DEPTH)//adding them untill it reaches max depth or is not contained
        {
            _units.Add(unit);
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            if (!_borderBoxes[i].Contains(unit.BoundingBox))
                continue;
            //I would call subdivide here
            if (_children[i] == null)
                _children[i] = new QuadTree<T>(_borderBoxes[i], _depth + 1);
            unit.Iterator.Add(i);
            //_cleared = false;
            _children[i].Add(unit);
            return;
        }

        _units.Add(unit);

    }

    void Remove(T unit, int index)//don't need ref
    {
        if (index < unit.Iterator.Count)
        {
            _children[unit.Iterator[index]].Remove(unit, ++index);
            return;
        }

        _units.Remove(unit);
        unit.Iterator.Clear();
    }

    public void Search(BorderBox box, List<T> results)
    {
        foreach (var unit in _units)
            if (box.Overlaps(unit.BoundingBox))
                results.Add(unit);

        for (int i = 0; i < 4; i++)
        {
            QuadTree<T> child = _children[i];
            if (child == null /*|| child._cleared*/) continue;

            if (box.Contains(_borderBoxes[i]))
                child.AppendItems(results);
            else if (_borderBoxes[i].Overlaps(box))
                child.Search(box, results);
        }
    }

    public void Search(Vector2 circlePos, float radius, List<T> results)
    {
        foreach (var unit in _units)
            if (CircleOverlapsBox(circlePos, radius, unit.BoundingBox))
                results.Add(unit);

        for (int i = 0; i < 4; i++)
        {
            QuadTree<T> child = _children[i];
            if (child == null /*|| child._cleared*/) continue;

            if (CircleCointainsBox(circlePos, radius, _borderBoxes[i]))
                child.AppendItems(results);
            else if (CircleOverlapsBox(circlePos, radius,_borderBoxes[i]))
                child.Search(circlePos, radius, results);
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

    void AppendItems(List<T> otherContainer)
    {
        foreach (var unit in _units)
            otherContainer.Add(unit);

        foreach (QuadTree<T> child in _children)
        {
            if (child == null /*|| child._cleared*/) continue;
            child.AppendItems(otherContainer);
        }
    }

    void DrawSelf(EasyDraw canvas)
    {
        canvas.StrokeWeight(2);
        if (_units.Count > 0)
            canvas.Fill(Color.Red, 100);
        else
            canvas.Fill(Color.Aqua, 50);

        canvas.Stroke(Color.White);
        canvas.ShapeAlign(CenterMode.Min, CenterMode.Min);
        canvas.Rect(Rect.x, Rect.y, Rect.w, Rect.h);
        canvas.Stroke(Color.Black);
        canvas.TextAlign(CenterMode.Center, CenterMode.Center);
        canvas.Text(_units.Count.ToString(), Rect.x + Rect.w / 2, Rect.y + Rect.h / 2);

    }

    public void Update(EasyDraw canvas)
    {

        DrawSelf(canvas);
        foreach (QuadTree<T> child in _children)
            child?.Update(canvas);

    }
}
