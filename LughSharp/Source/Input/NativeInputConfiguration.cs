// ///////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Source.Input;

/// <summary>
/// This will be called on the main thread, when the closing of a native input is
/// processed. This does not mean, that the keyboard is already hidden.
/// You can schedule a new `openTextInputField` call here.
/// </summary>
/// <param name="confirmativeAction">
/// Whether the way the keyboard was closed can be considered a confirmative action
/// e.g. to advance the UI
/// </param>
/// <returns>
/// Whether the keyboard should be kept open to be opened again soon. e.g. when
/// advancing through multiple text fields
/// </returns>
public delegate bool NativeInputCloseCallback( bool confirmativeAction );

/// <summary>
/// Configuration for the native input field.
/// </summary>
[PublicAPI]
public class NativeInputConfiguration
{
    public IInput.OnscreenKeyboardType?  KeyboardType     { get; set; } = IInput.OnscreenKeyboardType.Default;
    public IInput.IInputStringValidator? Validator        { get; set; }
    public ITextInputWrapper?            TextInputWrapper { get; set; }

    /// <summary>
    /// Enables / Disables the auto-complete / auto-correction of the input.
    /// </summary>
    public bool PreventCorrection { get; set; }

    /// <summary>
    /// whether the keyboard should accept multiple lines.
    /// </summary>
    public bool IsMultiLine { get; set; }

    /// <summary>
    /// What the text length limit should be. -1 for no max length.
    /// </summary>
    public int MaxLength { get; set; } = -1;

    /// <summary>
    /// A defaut string to show, if nothing is inputted yet.
    /// </summary>
    public string? Placeholder { get; set; } = "";

    /// <summary>
    /// Whether to hide the text input while typing (usually for passwords).
    /// </summary>
    public bool MaskInput { get; set; }

    /// <summary>
    /// Whether to show a button to show unhidden password.
    /// </summary>
    public bool ShowUnmaskButton { get; set; }

    /// <summary>
    /// A list of autocompletable strings to present to the user while typing.
    /// </summary>
    public string[]? AutoComplete { get; set; }

    /// <inheritdoc cref="NativeInputCloseCallback" />
    public NativeInputCloseCallback? CloseCallback { get; set; } = _ => false;

    // ========================================================================

    public void Validate()
    {
        string message = null;

        if ( KeyboardType == null ) message           = "OnscreenKeyboardType needs to be non null";
        if ( TextInputWrapper == null ) message       = "TextInputWrapper needs to be non null";
        if ( ShowUnmaskButton && !MaskInput ) message = "ShowUnmaskButton only works with MaskInput";
        if ( Placeholder == null ) message            = "Placeholder needs to be non null";

        if ( AutoComplete != null && KeyboardType != IInput.OnscreenKeyboardType.Default )
        {
            message = "AutoComplete should only be used with OnscreenKeyboardType.Default";
        }

        if ( AutoComplete != null && IsMultiLine ) message = "AutoComplete shouldn't be used with multiline";
        if ( CloseCallback == null ) message               = "CloseCallback needs to be non null";

        if ( message != null )
        {
            throw new ArgumentException( "NativeInputConfiguration validation failed: " + message );
        }
    }
}

// ============================================================================
// ============================================================================