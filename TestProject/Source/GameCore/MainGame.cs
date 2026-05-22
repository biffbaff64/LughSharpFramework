using JetBrains.Annotations;

using LughSharp.Source;

namespace TestProject.Source.GameCore;

[PublicAPI]
public class MainGame : LughGame
{
    private bool _disposed;

    public MainGame()
    {
        GameConfig.IsShowingSplachScreen = true;
    }

    /// <inheritdoc />
    public override void Create()
    {
    }

    /// <inheritdoc />
    public override void Update( float delta )
    {
    }

    /// <inheritdoc />
    public override void Render( float delta )
    {
    }

    /// <inheritdoc />
    public override void Resize( int width, int height )
    {
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected override void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
            }

            _disposed = true;
        }
    }
}

// ============================================================================
// ============================================================================