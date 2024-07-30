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
    public const int MAX_DEPTH = 4;
    public const int CAPACITY = 7;

    readonly int _depth;

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

        for (int i = 0; i < 4; i++)
        {
            _children[i] = null;
        }
    }

    public int Size()
    {
        int size = _units.Count;
        for (int i = 0; i < 4; i++)
        {
            if (_children[i] != null) size += _children[i].Size();
        }
        return size;
    }

    public void AddItem(T item)
    {
        item.Iterator.Clear();
        Add(item);
    }

    public void Remove(T unit)//use the binary trick from the third episode of LOD video instead of iterator?
    {
        QuadTree<T> housing = GetQuadrantOfUnit(unit);
        Remove(housing, unit);
    }

    void Remove(QuadTree<T> node, T unit)
    {
        node._units.Remove(unit);
        unit.Iterator.Clear();
    }

    public void Relocate(T unit)
    {
        QuadTree<T> housing = GetQuadrantOfUnit(unit);

        if (housing.Rect.Contains(unit.GetPosition())) return;

        Remove(housing, unit);
        Add(unit);

    }

    QuadTree<T> GetQuadrantOfUnit(T unit)
    {
        int index = 0;
        QuadTree<T> housing = this;

        while (index < unit.Iterator.Count)
        {
            housing = housing._children[unit.Iterator[index]];
            index++;
        }
        return housing;
    }

    public List<T> Search(Vector2 circlePos, float radius)
    {
        var results = new List<T>();
        Search(circlePos, radius, results);
        return results;
    }

    void Add(T unit)
    {
        if (!IsLeaf())
        {
            int quadrant = GetQuadrant(unit.GetPosition());
            unit.Iterator.Add(quadrant);
            _children[quadrant].Add(unit);
            return;
        }

        _units.Add(unit);

        if (_units.Count <= CAPACITY || _depth == MAX_DEPTH) return;
        
        Subdivide();

        foreach (T currentUnit in _units)
        {
            int quadrant = GetQuadrant(currentUnit.GetPosition());
            currentUnit.Iterator.Add(quadrant);
            _children[quadrant].Add(currentUnit);
        }
        _units.Clear();
    }

    void Subdivide()
    {
        for (int i = 0; i < 4; i++)
            _children[i] = new QuadTree<T>(_borderBoxes[i], _depth + 1);
        
    }

    bool IsLeaf()
    {
        return _children[0] == null;
    }

    int GetQuadrant(Vector2 unitPos)
    {
        Vector2 centerPos = new Vector2(Rect.x + Rect.w * .5f, Rect.y + Rect.h * .5f);
        if(unitPos.x < centerPos.x)
        {
            if (unitPos.y < centerPos.y)
                return 0; //up left
            return 2; //down left
        }

        if (unitPos.y < centerPos.y)
            return 1; //up right
        return 3; //down right
    }

    public void Search(Vector2 circlePos, float radius, List<T> results)
    {
        if (IsLeaf())
        {
            foreach (var unit in _units)
                if (CircleContainsPoint(circlePos, radius, unit.GetPosition()))
                    results.Add(unit);
            return;
        }
        

        for (int i = 0; i < 4; i++)
        {
            QuadTree<T> child = _children[i];

            if (CircleContainsBox(circlePos, radius, _borderBoxes[i]))
                child.AppendItems(results);
            else if (CircleOverlapsBox(circlePos, radius,_borderBoxes[i]))
                child.Search(circlePos, radius, results);
        }
    }

    bool CircleContainsBox(Vector2 pos, float radius, BorderBox box)
    {
        float dx = Mathf.Max(Mathf.Abs(pos.x - box.x), Mathf.Abs(box.x + box.w - pos.x));
        float dy = Mathf.Max(Mathf.Abs(pos.y - box.y), Mathf.Abs(box.y + box.h - pos.y));
        return (radius * radius) >= (dx * dx) + (dy * dy);
    }

    bool CircleOverlapsBox(Vector2 pos, float radius, BorderBox box)
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

    bool CircleContainsPoint(Vector2 pos, float radius, Vector2 point)
    {
        return Vector2.Distance(pos,point) <= radius;
    }

    void AppendItems(List<T> otherContainer)
    {
        if(!IsLeaf())
        {
            foreach (QuadTree<T> child in _children)
                child.AppendItems(otherContainer);
            return;
        }

        foreach (var unit in _units)
            otherContainer.Add(unit);
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
