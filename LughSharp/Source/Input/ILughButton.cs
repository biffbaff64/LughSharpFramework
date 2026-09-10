// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

namespace LughSharp.Source.Input;

/// <summary>
/// Describes the base properties and methods for non-Scene2D button implementations.
/// </summary>
[PublicAPI]
public interface ILughButton
{
    /// <summary>
    /// The type of Button assigned to this button.
    /// <li>Switch - Basic on/off switch, no visual component, no virtual hitbox. </li>
    /// <li>GameButton - Standard Button, with drawable component and associated hitbox. </li>
    /// <li>AnimatedButton - As GameButton, but with an animation component. </li>
    /// <li>ButtonRegion - Essentially an invisible GameButton. </li>
    /// <br/>
    /// </summary>
    [PublicAPI]
    enum LughButtonType
    {
        Switch,
        GameButton,
        AnimatedButton,
        ButtonRegion,
    }

    // ========================================================================

    bool IsPressed  { get; set; }
    bool IsDisabled { get; set; }
    bool IsDrawable { get; set; }

    // ========================================================================

    /// <summary>
    /// Returns <c>true</c> if a press has been detected at the given co-ordinates.
    /// For a <see cref="LughSwitch"/> this will always return <c>false</c> as that
    /// object has no visual component. Any non-Scene2D visual buttons that implement
    /// this interface should return <c>true</c> or <c>false</c> as required.
    /// </summary>
    /// <param name="touchX"> The X-Coordinate. </param>
    /// <param name="touchY"> The Y-Coordinate. </param>
    /// <returns> <c>true</c> If pressed, otherwise <c>false</c>. </returns>
    bool CheckPress( int touchX, int touchY );

    /// <summary>
    /// Returns <c>true</c> if a release has been detected at the given co-ordinates.
    /// For a <see cref="LughSwitch"/> this will always return <c>false</c> as that
    /// object has no visual component. Any non-Scene2D visual buttons that implement
    /// this interface should return <c>true</c> or <c>false</c> as required.
    /// </summary>
    /// <param name="touchX"> The X-Coordinate. </param>
    /// <param name="touchY"> The Y-Coordinate. </param>
    /// <returns> <c>true</c> If released, otherwise <c>false</c>. </returns>
    bool CheckRelease( int touchX, int touchY );

    /// <summary>
    /// Sets the switch IsPressed state to <c>true</c>. If the switch is disabled this
    /// method does nothing.
    /// </summary>
    void Press();

    /// <summary>
    /// Sets the switch IsPressed state to <c>true</c> if the result of the provided
    /// condition is also <c>true</c>. If the switch is disabled this method does nothing.
    /// </summary>
    void PressConditional( bool condition );

    /// <summary>
    /// Sets the switch IsPressed state to <c>false</c>. If the switch is disabled this
    /// method does nothing.
    /// </summary>
    void Release();

    /// <summary>
    /// Toggles the state of the <c>IsDisabled</c> property.
    /// </summary>
    void ToggleDisabled();

    /// <summary>
    /// Toggles the state of the <c>IsPressed</c> property.
    /// </summary>
    void TogglePressed();

    /// <summary>
    /// Returns the type of Button assigned to this button. Valid values are
    /// obtained from the <see cref="LughButtonType"/> enum.
    /// <li>Switch - Basic on/off switch, no visual component, no virtual hitbox. </li>
    /// <li>GameButton - Standard Button, with drawable component and associated hitbox. </li>
    /// <li>AnimatedButton - As GameButton, but with an animation component. </li>
    /// <li>ButtonRegion - Essentially an invisible GameButton. </li>
    /// <br/>
    /// </summary>
    LughButtonType GetSwitchType();
}

// ============================================================================
// ============================================================================
