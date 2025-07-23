// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.OpenGL.Enums;
using LughSharp.Lugh.Maths;



namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class NewShapeRenderer : IDisposable
{
    public enum ShapeTypes
    {
        Points = IGL.GL_POINTS,
        Lines  = IGL.GL_LINES,
        Filled = IGL.GL_TRIANGLES,
    }

    // ========================================================================

    private readonly Color   _color            = new( 1, 1, 1, 1 );
    private readonly Matrix4 _combinedMatrix   = new();
    private readonly Vector2 _tmp              = new();
    private          Matrix4 _projectionMatrix = new();
    private          Matrix4 _transformMatrix  = new();

//TODO:    private readonly float _defaultRectLineWidth = 0.75f;
//TODO:    private          bool  _matrixDirty          = false;

    private int _programId;
    private int _vaoId;
    private int _vboId;

    // ========================================================================

    public NewShapeRenderer()
    {
        _programId = CreateShapeShaderProgram();

        _vboId = ( int )GL.GenBuffer();
        _vaoId = ( int )GL.GenVertexArray();

        GL.BindVertexArray( ( uint )_vaoId );
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, ( uint )_vboId );

        // 3. Setup vertex attributes
        GL.EnableVertexAttribArray( 0 ); // Position

//        GL.VertexAttribPointer( 0u, 2, IGL.GL_FLOAT, false, 4 * sizeof( float ), IntPtr.Zero );

        GL.EnableVertexAttribArray( 1 ); // Color

//        GL.VertexAttribPointer( 1u, 4, IGL.GL_FLOAT, false, 4 * sizeof( float ), new IntPtr( 2 * sizeof( float ) ) );

        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
        GL.BindVertexArray( 0 );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    public void Begin()
    {
        GL.UseProgram( _programId );
        GL.BindVertexArray( ( uint )_vaoId );
    }

    public void End()
    {
        GL.BindVertexArray( 0 );
        GL.UseProgram( 0 );
    }

    private int CreateShapeShaderProgram()
    {
        throw new NotImplementedException();
    }

    // ========================================================================

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    protected virtual void Dispose( bool disposing )
    {
        ReleaseUnmanagedResources();

        if ( disposing )
        {
            // TODO release managed resources here
        }
    }

    /// <inheritdoc />
    ~NewShapeRenderer()
    {
        Dispose( false );
    }
}