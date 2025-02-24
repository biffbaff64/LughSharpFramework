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

using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLbitfield = uint;
using GLuint = uint;
using GLboolean = bool;

// ============================================================================

namespace LughSharp.Lugh.Graphics.OpenGL;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public void GetMultisamplefv( GLenum pname, GLuint index, GLfloat* val )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETMULTISAMPLEFVPROC >( "glGetMultisamplefv", out _glGetMultisamplefv );

        _glGetMultisamplefv( pname, index, val );
    }

    /// <inheritdoc />
    public void GetMultisamplefvSafe( GLenum pname, GLuint index, ref GLfloat[] val )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETMULTISAMPLEFVPROC >( "glGetMultisamplefv", out _glGetMultisamplefv );

        {
            fixed ( GLfloat* dp = &val[ 0 ] )
            {
                _glGetMultisamplefv( pname, index, dp );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SampleMaski( GLuint maskNumber, GLbitfield mask )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLEMASKIPROC >( "glSampleMaski", out _glSampleMaski );

        _glSampleMaski( maskNumber, mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SampleCoverage( GLfloat value, GLboolean invert )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLECOVERAGEPROC >( "glSampleCoverage", out _glSampleCoverage );

        _glSampleCoverage( value, invert );
    }

    // ========================================================================

    public void GenSamplers( GLsizei count, GLuint* samplers )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGENSAMPLERSPROC >( "glGenSamplers", out _glGenSamplers );

        _glGenSamplers( count, samplers );
    }

    /// <inheritdoc />
    public GLuint[] GenSamplers( GLsizei count )
    {
        var result = new GLuint[ count ];

        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGENSAMPLERSPROC >( "glGenSamplers", out _glGenSamplers );

        fixed ( GLuint* dp = &result[ 0 ] )
        {
            _glGenSamplers( count, dp );
        }

        return result;
    }

    /// <inheritdoc />
    public GLuint GenSampler()
    {
        return GenSamplers( 1 )[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteSamplers( GLsizei count, GLuint* samplers )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLDELETESAMPLERSPROC >( "glDeleteSamplers", out _glDeleteSamplers );

        _glDeleteSamplers( count, samplers );
    }

    /// <inheritdoc />
    public void DeleteSamplers( params GLuint[] samplers )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLDELETESAMPLERSPROC >( "glDeleteSamplers", out _glDeleteSamplers );

        fixed ( GLuint* dp = &samplers[ 0 ] )
        {
            _glDeleteSamplers( samplers.Length, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsSampler( GLuint sampler )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLISSAMPLERPROC >( "glIsSampler", out _glIsSampler );

        return _glIsSampler( sampler );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindSampler( GLuint unit, GLuint sampler )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLBINDSAMPLERPROC >( "glBindSampler", out _glBindSampler );

        _glBindSampler( unit, sampler );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameteri( GLuint sampler, GLenum pname, GLint param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIPROC >( "glSamplerParameteri", out _glSamplerParameteri );

        _glSamplerParameteri( sampler, pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameteriv( GLuint sampler, GLenum pname, GLint* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIVPROC >( "glSamplerParameteriv", out _glSamplerParameteriv );

        _glSamplerParameteriv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameteriv( GLuint sampler, GLenum pname, GLint[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIVPROC >( "glSamplerParameteriv", out _glSamplerParameteriv );

        {
            fixed ( GLint* dp = &param[ 0 ] )
            {
                _glSamplerParameteriv( sampler, pname, dp );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterf( GLuint sampler, GLenum pname, GLfloat param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERFPROC >( "glSamplerParameterf", out _glSamplerParameterf );

        _glSamplerParameterf( sampler, pname, param );
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterfv( GLuint sampler, GLenum pname, GLfloat* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERFVPROC >( "glSamplerParameterfv", out _glSamplerParameterfv );

        _glSamplerParameterfv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameterfv( GLuint sampler, GLenum pname, GLfloat[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERFVPROC >( "glSamplerParameterfv", out _glSamplerParameterfv );

        fixed ( GLfloat* dp = &param[ 0 ] )
        {
            _glSamplerParameterfv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterIiv( GLuint sampler, GLenum pname, GLint* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIIVPROC >( "glSamplerParameterIiv", out _glSamplerParameterIiv );

        _glSamplerParameterIiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameterIiv( GLuint sampler, GLenum pname, GLint[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIIVPROC >( "glSamplerParameterIiv", out _glSamplerParameterIiv );

        fixed ( GLint* dp = &param[ 0 ] )
        {
            _glSamplerParameterIiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void SamplerParameterIuiv( GLuint sampler, GLenum pname, GLuint* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIUIVPROC >( "glSamplerParameterIuiv", out _glSamplerParameterIuiv );

        _glSamplerParameterIuiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void SamplerParameterIuiv( GLuint sampler, GLenum pname, GLuint[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLSAMPLERPARAMETERIUIVPROC >( "glSamplerParameterIuiv", out _glSamplerParameterIuiv );

        fixed ( GLuint* dp = &param[ 0 ] )
        {
            _glSamplerParameterIuiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameteriv( GLuint sampler, GLenum pname, GLint* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERIVPROC >( "glGetSamplerParameteriv", out _glGetSamplerParameteriv );

        _glGetSamplerParameteriv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameteriv( GLuint sampler, GLenum pname, ref GLint[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERIVPROC >( "glGetSamplerParameteriv", out _glGetSamplerParameteriv );

        fixed ( GLint* dp = &param[ 0 ] )
        {
            _glGetSamplerParameteriv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameterIiv( GLuint sampler, GLenum pname, GLint* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERIIVPROC >( "glGetSamplerParameterIiv", out _glGetSamplerParameterIiv );

        _glGetSamplerParameterIiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameterIiv( GLuint sampler, GLenum pname, ref GLint[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERIIVPROC >( "glGetSamplerParameterIiv", out _glGetSamplerParameterIiv );

        fixed ( GLint* dp = &param[ 0 ] )
        {
            _glGetSamplerParameterIiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameterfv( GLuint sampler, GLenum pname, GLfloat* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERFVPROC >( "glGetSamplerParameterfv", out _glGetSamplerParameterfv );

        _glGetSamplerParameterfv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameterfv( GLuint sampler, GLenum pname, ref GLfloat[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERFVPROC >( "glGetSamplerParameterfv", out _glGetSamplerParameterfv );

        fixed ( GLfloat* dp = &param[ 0 ] )
        {
            _glGetSamplerParameterfv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetSamplerParameterIuiv( GLuint sampler, GLenum pname, GLuint* param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERIUIVPROC >( "glGetSamplerParameterIuiv", out _glGetSamplerParameterIuiv );

        _glGetSamplerParameterIuiv( sampler, pname, param );
    }

    /// <inheritdoc />
    public void GetSamplerParameterIuiv( GLuint sampler, GLenum pname, ref GLuint[] param )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETSAMPLERPARAMETERIUIVPROC >( "glGetSamplerParameterIuiv", out _glGetSamplerParameterIuiv );

        fixed ( GLuint* dp = &param[ 0 ] )
        {
            _glGetSamplerParameterIuiv( sampler, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void MinSampleShading( GLfloat value )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLMINSAMPLESHADINGPROC >( "glMinSampleShading", out _glMinSampleShading );

        _glMinSampleShading( value );
    }

    // ========================================================================

    public void BindSamplers( GLuint first, GLsizei count, GLuint* samplers )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLBINDSAMPLERSPROC >( "glBindSamplers", out _glBindSamplers );

        _glBindSamplers( first, count, samplers );
    }

    public void BindSamplers( GLuint first, GLuint[] samplers )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLBINDSAMPLERSPROC >( "glBindSamplers", out _glBindSamplers );

        fixed ( GLuint* pSamplers = &samplers[ 0 ] )
        {
            _glBindSamplers( first, samplers.Length, pSamplers );
        }
    }

    // ========================================================================

    public void CreateSamplers( GLsizei n, GLuint* samplers )
    {
        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATESAMPLERSPROC >( "glCreateSamplers", out _glCreateSamplers );

        _glCreateSamplers( n, samplers );
    }

    public GLuint[] CreateSamplers( GLsizei n )
    {
        var samplers = new GLuint[ n ];

        OpenGL.GLBindings.GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATESAMPLERSPROC >( "glCreateSamplers", out _glCreateSamplers );

        fixed ( GLuint* ptrSamplers = &samplers[ 0 ] )
        {
            _glCreateSamplers( n, ptrSamplers );
        }

        return samplers;
    }

    public GLuint CreateSamplers()
    {
        return CreateSamplers( 1 )[ 0 ];
    }

    // ========================================================================

}