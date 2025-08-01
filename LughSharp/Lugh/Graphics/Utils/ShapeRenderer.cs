﻿// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class ShapeRenderer : IDisposable
{
    public enum ShapeTypes
    {
        Points = IGL.GL_POINTS,
        Lines  = IGL.GL_LINES,
        Filled = IGL.GL_TRIANGLES,
    }

    // ========================================================================
    
    public IImmediateModeRenderer Renderer      { get; set; }
    public ShapeTypes?            ShapeType     { get; set; }
    public bool                   AutoShapeType { get; set; }

    // ========================================================================
    
    private readonly Color     _color                = new( 1, 1, 1, 1 );
    private readonly Matrix4 _combinedMatrix       = new();
    private readonly float     _defaultRectLineWidth = 0.75f;
    private readonly Vector2   _tmp                  = new();

    private bool      _matrixDirty      = false;
    private Matrix4 _projectionMatrix = new();
    private Matrix4 _transformMatrix  = new();

    // ========================================================================
    
    public ShapeRenderer( int maxVertices = 5000, ShaderProgram? defaultShader = null )
    {
        Renderer = defaultShader == null
            ? new ImmediateModeRenderer20( maxVertices, false, true, 0 )
            : new ImmediateModeRenderer20( maxVertices, false, true, 0, defaultShader );

        _projectionMatrix.SetToOrtho2D( 0, 0, Api.Graphics.Width, Api.Graphics.Height );
        _matrixDirty = true;
    }

    public Color Color
    {
        get => _color;
        set => _color.Set( value );
    }

    public Matrix4 ProjectionMatrix
    {
        get => _projectionMatrix;
        set
        {
            _projectionMatrix = value;
            _matrixDirty      = true;
        }
    }

    public Matrix4 TransformMatrix
    {
        get => _transformMatrix;
        set
        {
            _transformMatrix = value;
            _matrixDirty     = true;
        }
    }

    public void Dispose()
    {
    }

    // ========================================================================

    public void SetColor( float r, float g, float b, float a )
    {
        _color.Set( r, g, b, a );
    }

    public void UpdateMatrices()
    {
        _matrixDirty = true;
    }

    public void Identity()
    {
        TransformMatrix.ToIdentity();
        _matrixDirty = true;
    }

    public bool IsDrawing()
    {
        return ShapeType != null;
    }

    public void Translate( float x, float y, float z )
    {
        TransformMatrix.Translate( x, y, z );
        _matrixDirty = true;
    }

    public void Rotate( float axisX, float axisY, float axisZ, float degrees )
    {
        TransformMatrix.Rotate( axisX, axisY, axisZ, degrees );
        _matrixDirty = true;
    }

    public void Scale( float scaleX, float scaleY, float scaleZ )
    {
        TransformMatrix.Scale( scaleX, scaleY, scaleZ );
        _matrixDirty = true;
    }

    public void Begin()
    {
        if ( !AutoShapeType )
        {
            throw new InvalidOperationException( "autoShapeType must be true to use this method." );
        }

        Begin( ShapeTypes.Lines );
    }

    public void Begin( ShapeTypes? type )
    {
        if ( ShapeType != null )
        {
            throw new InvalidOperationException( "Call end() before beginning a new shape batch." );
        }

        if ( type == null )
        {
            throw new GdxRuntimeException( "Cannot BEGIN with NULL shape!" );
        }

        ShapeType = type;

        if ( _matrixDirty )
        {
            _combinedMatrix.Set( _projectionMatrix );
            Matrix4.Mul( _combinedMatrix.Val, _transformMatrix.Val );
            _matrixDirty = false;
        }

        Renderer.Begin( _combinedMatrix, ( int )ShapeType );
    }

    public void End()
    {
        Renderer.End();
        ShapeType = null;
    }

    public void Flush()
    {
        Flush( ShapeType );
    }

    public void Flush( ShapeTypes? shapeType )
    {
        if ( shapeType == null )
        {
            return;
        }

        End();
        Begin( shapeType );
    }

    public void Set( ShapeTypes type )
    {
        if ( ShapeType == type )
        {
            return;
        }

        if ( ShapeType == null )
        {
            throw new InvalidOperationException( "begin must be called first." );
        }

        if ( !AutoShapeType )
        {
            throw new InvalidOperationException( "autoShapeType must be enabled." );
        }

        Flush( type );
    }

    public void Point( float x, float y, float z )
    {
        if ( ShapeType == ShapeTypes.Lines )
        {
            var size = _defaultRectLineWidth * 0.5f;
            Line( x - size, y - size, z, x + size, y + size, z );

            return;
        }

        if ( ShapeType == ShapeTypes.Filled )
        {
            var size = _defaultRectLineWidth * 0.5f;
            Box( x - size, y - size, z - size, _defaultRectLineWidth, _defaultRectLineWidth, _defaultRectLineWidth );

            return;
        }

        Check( ShapeTypes.Points, null, 1 );
        Renderer.SetColor( _color );
        Renderer.Vertex( x, y, z );
    }

    public void Line( float x, float y, float z, float x2, float y2, float z2 )
    {
        Line( x, y, z, x2, y2, z2, _color, _color );
    }

    public void Line( Vector3 v0, Vector3 v1 )
    {
        Line( v0.X, v0.Y, v0.Z, v1.X, v1.Y, v1.Z, _color, _color );
    }

    public void Line( float x, float y, float x2, float y2 )
    {
        Line( x, y, 0.0f, x2, y2, 0.0f, _color, _color );
    }

    public void Line( Vector2 v0, Vector2 v1 )
    {
        Line( v0.X, v0.Y, 0.0f, v1.X, v1.Y, 0.0f, _color, _color );
    }

    public void Line( float x, float y, float x2, float y2, Color c1, Color c2 )
    {
        Line( x, y, 0.0f, x2, y2, 0.0f, c1, c2 );
    }

    public void Line( float x, float y, float z, float x2, float y2, float z2, Color c1, Color c2 )
    {
        if ( ShapeType == ShapeTypes.Filled )
        {
            RectLine( x, y, x2, y2, _defaultRectLineWidth, c1, c2 );

            return;
        }

        Check( ShapeTypes.Lines, null, 2 );

        Renderer.SetColor( c1.R, c1.G, c1.B, c1.A );
        Renderer.Vertex( x, y, z );
        Renderer.SetColor( c2.R, c2.G, c2.B, c2.A );
        Renderer.Vertex( x2, y2, z2 );
    }

    public void Curve( float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2, int segments )
    {
        Check( ShapeTypes.Lines, null, ( segments * 2 ) + 2 );

        var colorBits   = _color.ToFloatBitsABGR();
        var subdivStep  = 1f / segments;
        var subdivStep2 = subdivStep * subdivStep;
        var subdivStep3 = subdivStep * subdivStep * subdivStep;
        var pre1        = 3 * subdivStep;
        var pre2        = 3 * subdivStep2;
        var pre4        = 6 * subdivStep2;
        var pre5        = 6 * subdivStep3;
        var tmp1X       = ( x1 - ( cx1 * 2 ) ) + cx2;
        var tmp1Y       = ( y1 - ( cy1 * 2 ) ) + cy2;
        var tmp2X       = ( ( ( cx1 - cx2 ) * 3 ) - x1 ) + x2;
        var tmp2Y       = ( ( ( cy1 - cy2 ) * 3 ) - y1 ) + y2;
        var fx          = x1;
        var fy          = y1;
        var dfx         = ( ( cx1 - x1 ) * pre1 ) + ( tmp1X * pre2 ) + ( tmp2X * subdivStep3 );
        var dfy         = ( ( cy1 - y1 ) * pre1 ) + ( tmp1Y * pre2 ) + ( tmp2Y * subdivStep3 );
        var ddfx        = ( tmp1X * pre4 ) + ( tmp2X * pre5 );
        var ddfy        = ( tmp1Y * pre4 ) + ( tmp2Y * pre5 );
        var dddfx       = tmp2X * pre5;
        var dddfy       = tmp2Y * pre5;

        while ( segments-- > 0 )
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( fx, fy, 0 );

            fx   += dfx;
            fy   += dfy;
            dfx  += ddfx;
            dfy  += ddfy;
            ddfx += dddfx;
            ddfy += dddfy;

            Renderer.SetColor( colorBits );
            Renderer.Vertex( fx, fy, 0 );
        }

        Renderer.SetColor( colorBits );
        Renderer.Vertex( fx, fy, 0 );
        Renderer.SetColor( colorBits );
        Renderer.Vertex( x2, y2, 0 );
    }

    public void Triangle( float x1, float y1, float x2, float y2, float x3, float y3 )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 6 );

        var colorBits = _color.ToFloatBitsABGR();

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1, y1, 0 );
        }
        else
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x3, y3, 0 );
        }
    }

    public void Triangle( float x1, float y1, float x2, float y2, float x3, float y3, Color col1, Color col2, Color col3 )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 6 );

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x1, y1, 0 );
        }
        else
        {
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
        }
    }

    public void Rect( float x, float y, float width, float height )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 8 );
        var colorBits = _color.ToFloatBitsABGR();

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, 0 );
        }
        else
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, 0 );
        }

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x, y + height, 0 );
        Renderer.SetColor( colorBits );
        Renderer.Vertex( x, y, 0 );
    }

    public void Rect( float x, float y, float width, float height, Color col1, Color col2, Color col3, Color col4 )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 8 );

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x, y, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x + width, y, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x + width, y, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x + width, y + height, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x + width, y + height, 0 );
            Renderer.SetColor( col4.R, col4.G, col4.B, col4.A );
            Renderer.Vertex( x, y + height, 0 );
        }
        else
        {
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x, y, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x + width, y, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x + width, y + height, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x + width, y + height, 0 );
        }

        Renderer.SetColor( col4.R, col4.G, col4.B, col4.A );
        Renderer.Vertex( x, y + height, 0 );
        Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
        Renderer.Vertex( x, y, 0 );
    }

    public void Rect( float x, float y, float originX, float originY, float width, float height, float scaleX, float scaleY, float degrees )
    {
        Rect( x, y, originX, originY, width, height, scaleX, scaleY, degrees, _color, _color, _color, _color );
    }

    public void Rect( float x, float y, float originX, float originY,
                      float width, float height, float scaleX, float scaleY, float degrees,
                      Color col1, Color col2, Color col3, Color col4 )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 8 );

        var cos = MathUtils.CosDeg( degrees );
        var sin = MathUtils.SinDeg( degrees );
        var fx  = -originX;
        var fy  = -originY;
        var fx2 = width - originX;
        var fy2 = height - originY;

        if ( !scaleX.Equals( 1 ) || !scaleY.Equals( 1 ) )
        {
            fx  *= scaleX;
            fy  *= scaleY;
            fx2 *= scaleX;
            fy2 *= scaleY;
        }

        var worldOriginX = x + originX;
        var worldOriginY = y + originY;
        var x1           = ( ( cos * fx ) - ( sin * fy ) ) + worldOriginX;
        var y1           = ( sin * fx ) + ( cos * fy ) + worldOriginY;
        var x2           = ( ( cos * fx2 ) - ( sin * fy ) ) + worldOriginX;
        var y2           = ( sin * fx2 ) + ( cos * fy ) + worldOriginY;
        var x3           = ( ( cos * fx2 ) - ( sin * fy2 ) ) + worldOriginX;
        var y3           = ( sin * fx2 ) + ( cos * fy2 ) + worldOriginY;
        var x4           = x1 + ( x3 - x2 );
        var y4           = y3 - ( y2 - y1 );

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( col4.R, col4.G, col4.B, col4.A );
            Renderer.Vertex( x4, y4, 0 );
        }
        else
        {
            Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( col2.R, col2.G, col2.B, col2.A );
            Renderer.Vertex( x2, y2, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
            Renderer.SetColor( col3.R, col3.G, col3.B, col3.A );
            Renderer.Vertex( x3, y3, 0 );
        }

        Renderer.SetColor( col4.R, col4.G, col4.B, col4.A );
        Renderer.Vertex( x4, y4, 0 );
        Renderer.SetColor( col1.R, col1.G, col1.B, col1.A );
        Renderer.Vertex( x1, y1, 0 );
    }

    public void RectLine( float x1, float y1, float x2, float y2, float width )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 8 );

        var colorBits = _color.ToFloatBitsABGR();
        var t         = _tmp.Set( y2 - y1, x1 - x2 ).Nor();

        width *= 0.5f;

        var tx = t.X * width;
        var ty = t.Y * width;

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1 + tx, y1 + ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1 - tx, y1 - ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 - tx, y2 - ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1 + tx, y1 + ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 - tx, y2 - ty, 0 );
        }
        else
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1 + tx, y1 + ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1 - tx, y1 - ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 - tx, y2 - ty, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
        }

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x1 - tx, y1 - ty, 0 );
    }

    public void RectLine( float x1, float y1, float x2, float y2, float width, Color c1, Color c2 )
    {
        Check( ShapeTypes.Lines, ShapeTypes.Filled, 8 );

        var col1Bits = c1.ToFloatBitsABGR();
        var col2Bits = c2.ToFloatBitsABGR();
        var t        = _tmp.Set( y2 - y1, x1 - x2 ).Nor();

        width *= 0.5f;

        var tx = t.X * width;
        var ty = t.Y * width;

        if ( ShapeType == ShapeTypes.Lines )
        {
            Renderer.SetColor( col1Bits );
            Renderer.Vertex( x1 + tx, y1 + ty, 0 );
            Renderer.SetColor( col1Bits );
            Renderer.Vertex( x1 - tx, y1 - ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 - tx, y2 - ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
            Renderer.SetColor( col1Bits );
            Renderer.Vertex( x1 + tx, y1 + ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 - tx, y2 - ty, 0 );
        }
        else
        {
            Renderer.SetColor( col1Bits );
            Renderer.Vertex( x1 + tx, y1 + ty, 0 );
            Renderer.SetColor( col1Bits );
            Renderer.Vertex( x1 - tx, y1 - ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 - tx, y2 - ty, 0 );
            Renderer.SetColor( col2Bits );
            Renderer.Vertex( x2 + tx, y2 + ty, 0 );
        }

        Renderer.SetColor( col1Bits );
        Renderer.Vertex( x1 - tx, y1 - ty, 0 );
    }

    public void RectLine( Vector2 p1, Vector2 p2, float width )
    {
        RectLine( p1.X, p1.Y, p2.X, p2.Y, width );
    }

    public void Box( float x, float y, float z, float width, float height, float depth )
    {
        depth = -depth;
        var colorBits = _color.ToFloatBitsABGR();

        if ( ShapeType == ShapeTypes.Lines )
        {
            Check( ShapeTypes.Lines, ShapeTypes.Filled, 24 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z + depth );
        }
        else
        {
            Check( ShapeTypes.Lines, ShapeTypes.Filled, 36 );

            // Front
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );

            // Back
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z + depth );

            // Left
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z + depth );

            // Right
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );

            // Top
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y + height, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y + height, z + depth );

            // Bottom
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + depth );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + width, y, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
        }
    }

    public void XShape( float x, float y, float size )
    {
        Line( x - size, y - size, x + size, y + size );
        Line( x - size, y + size, x + size, y - size );
    }

    public void XShape( Vector2 p, float size )
    {
        XShape( p.X, p.Y, size );
    }

    public void Arc( float x, float y, float radius, float start, float degrees )
    {
        Arc( x, y, radius, start, degrees, Math.Max( 1, ( int )( 6 * ( float )Math.Cbrt( radius ) * ( degrees / 360.0f ) ) ) );
    }

    public void Arc( float x, float y, float radius, float start, float degrees, int segments )
    {
        if ( segments <= 0 )
        {
            throw new ArgumentException( "segments must be > 0." );
        }

        var colorBits = _color.ToFloatBitsABGR();
        var theta     = ( 2 * MathUtils.PI * ( degrees / 360.0f ) ) / segments;
        var cos       = MathUtils.Cos( theta );
        var sin       = MathUtils.Sin( theta );
        var cx        = radius * MathUtils.Cos( start * MathUtils.DEGREES_TO_RADIANS );
        var cy        = radius * MathUtils.Sin( start * MathUtils.DEGREES_TO_RADIANS );

        float temp;

        if ( ShapeType == ShapeTypes.Lines )
        {
            Check( ShapeTypes.Lines, ShapeTypes.Filled, ( segments * 2 ) + 2 );

            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + cx, y + cy, 0 );

            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );

                temp = cx;
                cx   = ( cos * cx ) - ( sin * cy );
                cy   = ( sin * temp ) + ( cos * cy );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );
            }
        }
        else
        {
            Check( ShapeTypes.Lines, ShapeTypes.Filled, ( segments * 3 ) + 3 );

            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x, y, 0 );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );

                temp = cx;
                cx   = ( cos * cx ) - ( sin * cy );
                cy   = ( sin * temp ) + ( cos * cy );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );
            }

            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, 0 );
        }

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x + cx, y + cy, 0 );

        cx = 0;
        cy = 0;

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x + cx, y + cy, 0 );
    }

    public void Circle( float x, float y, float radius )
    {
        Circle( x, y, radius, Math.Max( 1, ( int )( 6 * ( float )Math.Cbrt( radius ) ) ) );
    }

    public void Circle( float x, float y, float radius, int segments )
    {
        if ( segments <= 0 )
        {
            throw new ArgumentException( "segments must be > 0." );
        }

        var   colorBits = _color.ToFloatBitsABGR();
        var   angle     = ( 2 * MathUtils.PI ) / segments;
        var   cos       = MathUtils.Cos( angle );
        var   sin       = MathUtils.Sin( angle );
        var   cx        = radius;
        var   cy        = 0f;
        float temp;

        if ( ShapeType == ShapeTypes.Lines )
        {
            Check( ShapeTypes.Lines, ShapeTypes.Filled, ( segments * 2 ) + 2 );

            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );

                temp = cx;

                cx = ( cos * cx ) - ( sin * cy );
                cy = ( sin * temp ) + ( cos * cy );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );
            }
        }
        else
        {
            Check( ShapeTypes.Lines, ShapeTypes.Filled, ( segments * 3 ) + 3 );
            segments--;

            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x, y, 0 );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );

                temp = cx;
                cx   = ( cos * cx ) - ( sin * cy );
                cy   = ( sin * temp ) + ( cos * cy );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, 0 );
            }

            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, 0 );
        }

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x + cx, y + cy, 0 );

        cx = radius;
        cy = 0;

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x + cx, y + cy, 0 );
    }

    public void Ellipse( float x, float y, float width, float height )
    {
        Ellipse( x, y, width, height, Math.Max( 1, ( int )( 12 * ( float )Math.Cbrt( Math.Max( width * 0.5f, height * 0.5f ) ) ) ) );
    }

    public void Ellipse( float x, float y, float width, float height, int segments )
    {
        if ( segments <= 0 )
        {
            throw new ArgumentException( "segments must be > 0." );
        }

        Check( ShapeTypes.Lines, ShapeTypes.Filled, segments * 3 );

        var colorBits = _color.ToFloatBitsABGR();
        var angle     = ( 2 * MathUtils.PI ) / segments;

        float cx = x + ( width / 2 ), cy = y + ( height / 2 );

        if ( ShapeType == ShapeTypes.Lines )
        {
            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( cx + ( width * 0.5f * MathUtils.Cos( i * angle ) ),
                                 cy + ( height * 0.5f * MathUtils.Sin( i * angle ) ),
                                 0 );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( cx + ( width * 0.5f * MathUtils.Cos( ( i + 1 ) * angle ) ),
                                 cy + ( height * 0.5f * MathUtils.Sin( ( i + 1 ) * angle ) ),
                                 0 );
            }
        }
        else
        {
            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( cx + ( width * 0.5f * MathUtils.Cos( i * angle ) ),
                                 cy + ( height * 0.5f * MathUtils.Sin( i * angle ) ),
                                 0 );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( cx, cy, 0 );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( cx + ( width * 0.5f * MathUtils.Cos( ( i + 1 ) * angle ) ),
                                 cy + ( height * 0.5f * MathUtils.Sin( ( i + 1 ) * angle ) ),
                                 0 );
            }
        }
    }

    public void Ellipse( float x, float y, float width, float height, float rotation )
    {
        Ellipse( x, y, width, height, rotation, Math.Max( 1, ( int )( 12 * ( float )Math.Cbrt(
                                                               Math.Max( width * 0.5f, height * 0.5f ) ) ) ) );
    }

    public void Ellipse( float x, float y, float width, float height, float rotation, int segments )
    {
        if ( segments <= 0 )
        {
            throw new ArgumentException( "segments must be > 0." );
        }

        Check( ShapeTypes.Lines, ShapeTypes.Filled, segments * 3 );

        var colorBits = _color.ToFloatBitsABGR();
        var angle     = ( 2 * MathUtils.PI ) / segments;

        rotation = ( MathUtils.PI * rotation ) / 180f;

        var sin = MathUtils.Sin( rotation );
        var cos = MathUtils.Cos( rotation );

        float cx = x + ( width / 2 ), cy = y + ( height / 2 );
        var   x1 = width * 0.5f;
        float y1 = 0;

        if ( ShapeType == ShapeTypes.Lines )
        {
            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( ( cx + ( cos * x1 ) ) - ( sin * y1 ), cy + ( sin * x1 ) + ( cos * y1 ), 0 );

                x1 = width * 0.5f * MathUtils.Cos( ( i + 1 ) * angle );
                y1 = height * 0.5f * MathUtils.Sin( ( i + 1 ) * angle );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( ( cx + ( cos * x1 ) ) - ( sin * y1 ), cy + ( sin * x1 ) + ( cos * y1 ), 0 );
            }
        }
        else
        {
            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( ( cx + ( cos * x1 ) ) - ( sin * y1 ), cy + ( sin * x1 ) + ( cos * y1 ), 0 );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( cx, cy, 0 );

                x1 = width * 0.5f * MathUtils.Cos( ( i + 1 ) * angle );
                y1 = height * 0.5f * MathUtils.Sin( ( i + 1 ) * angle );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( ( cx + ( cos * x1 ) ) - ( sin * y1 ), cy + ( sin * x1 ) + ( cos * y1 ), 0 );
            }
        }
    }

    public void Cone( float x, float y, float z, float radius, float height )
    {
        Cone( x, y, z, radius, height, Math.Max( 1, ( int )( 4 * ( float )Math.Sqrt( radius ) ) ) );
    }

    public void Cone( float x, float y, float z, float radius, float height, int segments )
    {
        if ( segments <= 0 )
        {
            throw new ArgumentException( "segments must be > 0." );
        }

        Check( ShapeTypes.Lines, ShapeTypes.Filled, ( segments * 4 ) + 2 );

        var   colorBits = _color.ToFloatBitsABGR();
        var   angle     = ( 2 * MathUtils.PI ) / segments;
        var   cos       = MathUtils.Cos( angle );
        var   sin       = MathUtils.Sin( angle );
        float cx        = radius, cy = 0;
        float temp;
        float temp2;

        if ( ShapeType == ShapeTypes.Lines )
        {
            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, z );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x, y, z + height );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, z );

                temp = cx;
                cx   = ( cos * cx ) - ( sin * cy );
                cy   = ( sin * temp ) + ( cos * cy );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, z );
            }
        }
        else
        {
            segments--;

            for ( var i = 0; i < segments; i++ )
            {
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x, y, z );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, z );

                temp  = cx;
                temp2 = cy;

                cx = ( cos * cx ) - ( sin * cy );
                cy = ( sin * temp ) + ( cos * cy );

                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, z );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + temp, y + temp2, z );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x + cx, y + cy, z );
                Renderer.SetColor( colorBits );
                Renderer.Vertex( x, y, z + height );
            }

            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z );
        }

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x + cx, y + cy, z );

        temp  = cx;
        temp2 = cy;
        cx    = radius;
        cy    = 0;

        Renderer.SetColor( colorBits );
        Renderer.Vertex( x + cx, y + cy, z );

        if ( ShapeType != ShapeTypes.Lines )
        {
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + temp, y + temp2, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x + cx, y + cy, z );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x, y, z + height );
        }
    }

    public void Polygon( float[] vertices, int offset, int count )
    {
        if ( count < 6 )
        {
            throw new ArgumentException( "Polygons must contain at least 3 points." );
        }

        if ( ( count % 2 ) != 0 )
        {
            throw new ArgumentException( "Polygons must have an even number of vertices." );
        }

        Check( ShapeTypes.Lines, null, count );

        var colorBits = _color.ToFloatBitsABGR();
        var firstX    = vertices[ 0 ];
        var firstY    = vertices[ 1 ];

        for ( int i = offset, n = offset + count; i < n; i += 2 )
        {
            var   x1 = vertices[ i ];
            var   y1 = vertices[ i + 1 ];
            float x2;
            float y2;

            if ( ( i + 2 ) >= count )
            {
                x2 = firstX;
                y2 = firstY;
            }
            else
            {
                x2 = vertices[ i + 2 ];
                y2 = vertices[ i + 3 ];
            }

            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2, y2, 0 );
        }
    }

    public void Polyline( float[] vertices, int offset, int count )
    {
        if ( count < 4 )
        {
            throw new ArgumentException( "Polylines must contain at least 2 points." );
        }

        if ( ( count % 2 ) != 0 )
        {
            throw new ArgumentException( "Polylines must have an even number of vertices." );
        }

        Check( ShapeTypes.Lines, null, count );

        var colorBits = _color.ToFloatBitsABGR();

        for ( int i = offset, n = ( offset + count ) - 2; i < n; i += 2 )
        {
            var x1 = vertices[ i ];
            var y1 = vertices[ i + 1 ];
            var x2 = vertices[ i + 2 ];
            var y2 = vertices[ i + 3 ];

            Renderer.SetColor( colorBits );
            Renderer.Vertex( x1, y1, 0 );
            Renderer.SetColor( colorBits );
            Renderer.Vertex( x2, y2, 0 );
        }
    }

    public void Polygon( float[] vertices )
    {
        Polygon( vertices, 0, vertices.Length );
    }

    public void Polyline( float[] vertices )
    {
        Polyline( vertices, 0, vertices.Length );
    }

    private void Check( ShapeTypes preferred, ShapeTypes? other, int newVertices )
    {
        if ( ShapeType == null )
        {
            throw new GdxRuntimeException( "Begin() must be called first." );
        }

        if ( ( ShapeType != preferred ) && ( ShapeType != other ) )
        {
            if ( !AutoShapeType ) // Shape type is not valid.
            {
                if ( other == null )
                {
                    throw new GdxRuntimeException( $"Must call Begin(ShapeType.{preferred})." );
                }

                throw new GdxRuntimeException( $"Must call Begin(ShapeType.{preferred}) or Begin(ShapeType.{other})." );
            }

            Flush( preferred );
        }
        else if ( _matrixDirty || ( ( Renderer.MaxVertices - Renderer.NumVertices ) < newVertices ) )
        {
            // Either matrix has been changed or there is not enough space
            Flush( ShapeType );
        }
    }
}