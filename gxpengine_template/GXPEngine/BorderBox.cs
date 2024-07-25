using GXPEngine.Core;

public readonly struct BorderBox
{
    public readonly float x;
    public readonly float y;
    public readonly float w;
    public readonly float h;

    public BorderBox(float x, float y, float w, float h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }

    public bool Contains(Vector2 p)
    {
        return !(p.x < x || p.y < y || p.x >= (x + w) || p.y >= (y + h));
    }

    public bool Contains(BorderBox other)
    {
        return other.x >= x && other.x + other.w < x + w &&
            other.y >= y && other.y + other.h < y + h;
    }

    public bool Overlaps(BorderBox other)
    {
        return x < other.x + other.w && x + w >= other.x &&
            y < other.y + other.h && y + h >= other.y;
    }
}
