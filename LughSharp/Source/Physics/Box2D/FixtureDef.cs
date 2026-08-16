namespace LughSharp.Source.Physics.Box2D;

/// <summary>
/// A fixture definition is used to create a fixture. This class defines an abstract
/// fixture definition. You can reuse fixture definitions safely. Faithful port of
/// libgdx <c>FixtureDef</c>.
/// </summary>
[PublicAPI]
public class FixtureDef
{
    /// <summary>The shape, this must be set. The shape will be cloned, so you can
    /// create the shape on the stack.</summary>
    public Shape? Shape { get; set; }

    /// <summary>The friction coefficient, usually in the range [0, 1].</summary>
    public float Friction { get; set; } = 0.2f;

    /// <summary>The restitution (elasticity) usually in the range [0, 1].</summary>
    public float Restitution { get; set; }

    /// <summary>The density, usually in kg/m^2.</summary>
    public float Density { get; set; }

    /// <summary>A sensor shape collects contact information but never generates a
    /// collision response.</summary>
    public bool IsSensor { get; set; }

    /// <summary>Contact filtering data.</summary>
    public Filter Filter { get; set; } = new();
}

// ============================================================================
// ============================================================================
