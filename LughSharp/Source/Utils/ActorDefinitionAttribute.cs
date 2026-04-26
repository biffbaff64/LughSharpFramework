// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

using System;

using JetBrains.Annotations;

namespace LughSharp.Source.Utils;

/// <summary>
/// To check if a single class has this attribute:
/// <code>
/// var myPlayer = new Player();
/// bool isActor = Attribute.IsDefined(myPlayer.GetType(), typeof(ActorDefinitionAttribute));
/// if (isActor) 
/// {
///     Console.WriteLine("This object is an Actor!");
/// }
/// </code>
/// <br/>
/// If you want to find all classes in your project marked as Actors (common for
/// Dependency Injection or Plugin systems):
/// <code>
/// using System.Linq;
/// using System.Reflection;
/// 
/// var actorTypes = Assembly.GetExecutingAssembly()
///                          .GetTypes()
///                          .Where(t => t.GetCustomAttribute&lt;ActorDefinitionAttribute&gt;() != null);
/// foreach (var type in actorTypes)
/// {
///     Console.WriteLine($"Found Actor Type: {type.Name}");
/// }
/// </code>
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class)]
public class ActorDefinitionAttribute : Attribute
{
    /// <summary>
    /// The role of the actor. This is used for identifying the actor in the,
    /// and is a user-defined string.
    /// <br/>
    /// For instance:-
    /// <code>
    /// [ActorDefinition(Role = "UI")]
    /// </code>
    /// </summary>
    public string Role { get; set; } = "Generic";
}

// ============================================================================
// ============================================================================
