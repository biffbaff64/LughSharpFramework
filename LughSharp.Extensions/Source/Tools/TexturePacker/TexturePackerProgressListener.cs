// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;

using LughSharp.Core.Utils.Collections;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
public class TexturePackerProgressListener
{
    public int  Count    { get; set; }
    public int  Total    { get; set; }
    public bool Canceled { get; set; }

    // ====================================================================

    private readonly List< float > _portions = new( 8 );

    private float _scale = 1;
    private float _lastUpdate;

    // ====================================================================

    public void Start( float portion )
    {
        if ( portion == 0 )
        {
            throw new ArgumentException( "portion cannot be 0." );
        }

        _portions.Add( _lastUpdate );
        _portions.Add( _scale * portion );
        _portions.Add( _scale );

        _scale *= portion;
    }

    public bool Update( int count, int total )
    {
        Update( total == 0 ? 0 : count / ( float )total );

        return Canceled;
    }

    public void Update( float percent )
    {
        _lastUpdate = _portions[ _portions.Count - 3 ]
                    + ( _portions[ _portions.Count - 2 ] * percent );

        Progress( _lastUpdate );
    }

    public void End()
    {
        _scale = _portions.Pop();

        float portion = _portions.Pop();

        _lastUpdate = _portions.Pop() + portion;

        Progress( _lastUpdate );
    }

    public void Reset()
    {
        _scale  = 1;
        Message = "";
        Count   = 0;
        Total   = 0;

        Progress( 0 );
    }

    public string Message
    {
        get;
        set
        {
            field = value;

            Progress( _lastUpdate );
        }
    } = "";

    protected virtual void Progress( float progress )
    {
    }
}

// ============================================================================
// ============================================================================