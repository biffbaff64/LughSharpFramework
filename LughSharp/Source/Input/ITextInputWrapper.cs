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

[PublicAPI]
public interface ITextInputWrapper
{
    /// <summary>
    /// This method will be queried for the initial text. No guarantee of the
    /// calling thread is made. 
    /// </summary>
    string GetText();

    /// <summary>
    /// This method will be queried for the initial text selection start. No guarantee
    /// of the calling thread is made.
    /// Should be consistent with the text returned by <see cref="GetText()"/>. 
    /// </summary>
    int GetSelectionStart();

    /// <summary>
    /// This method will be queried for the initial text selection end. No guarantee
    /// of the calling thread is made.
    /// Should be consistent with the text returned by <see cref="GetText()"/>. 
    /// </summary>
    int GetSelectionEnd();

    /// <summary>
    /// This is called, when text was retrieved from the native input. Only use this
    /// to write back results. This will always be called on the main thread. For close
    /// logic set <see cref="NativeInputConfiguration.CloseCallback"/> appropriately. 
    /// </summary>
    void WriteResults( string text, int selectionStart, int selectionEnd );
}

// ============================================================================
// ============================================================================