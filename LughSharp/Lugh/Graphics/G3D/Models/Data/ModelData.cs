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

using LughSharp.Lugh.Assets.Loaders;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.G3D.Models.Data;

/// <summary>
/// Returned by a <see cref="ModelLoader" />, contains meshes, materials, nodes and animations.
/// OpenGL resources like textures or vertex buffer objects are not stored. Instead, a ModelData
/// instance needs to be converted to a Model first.
/// </summary>
public class ModelData
{
    public string?                ID         { get; set; }
    public short[]                Version    { get; set; } = new short[ 2 ];
    public List< ModelMesh >      Meshes     { get; set; } = [ ];
    public List< ModelMaterial >? Materials  { get; set; } = [ ];
    public List< ModelNode >      Nodes      { get; set; } = [ ];
    public List< ModelAnimation > Animations { get; set; } = [ ];

    public virtual void AddMesh( ModelMesh mesh )
    {
        foreach ( var other in Meshes )
        {
            if ( ( other.ID != null ) && other.ID.Equals( mesh.ID ) )
            {
                throw new GdxRuntimeException( "Mesh with id '" + other.ID + "' already in model" );
            }
        }

        Meshes.Add( mesh );
    }
}