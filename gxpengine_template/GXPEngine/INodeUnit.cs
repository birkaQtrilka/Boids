using System.Collections.Generic;

public interface INodeUnit
{
    BorderBox BoundingBox { get; }
    List<int> Iterator { get; }

    void Handle(List<INodeUnit> others);
}
