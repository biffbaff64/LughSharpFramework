namespace LughSharp.Source.Physics.Box2D;

/// <summary>
/// This holds contact filtering data. Faithful port of libgdx <c>Filter</c>.
/// </summary>
[PublicAPI]
public class Filter
{
    /// <summary>
    /// The collision category bits. Normally you would just set one bit.
    /// </summary>
    public short CategoryBits { get; set; } = 0x0001;

    /// <summary>
    /// The collision mask bits. This states the categories that this shape would
    /// accept for collision.
    /// </summary>
    public short MaskBits { get; set; } = -1;

    /// <summary>
    /// Collision groups allow a certain group of objects to never collide (negative)
    /// or always collide (positive). Zero means no collision group. Non-zero group
    /// filtering always wins against the mask bits.
    /// </summary>
    public short GroupIndex { get; set; } = 0;

    // ========================================================================
    
    /// <summary>
    /// Copies the values from the supplied filter into this one.
    /// </summary>
    public void Set(Filter filter)
    {
        CategoryBits = filter.CategoryBits;
        MaskBits     = filter.MaskBits;
        GroupIndex   = filter.GroupIndex;
    }
}

// ============================================================================
// ============================================================================
