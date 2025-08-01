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

using LughSharp.Lugh.Audio;

namespace DesktopGLBackend.Audio.Mock;

/// <summary>
/// Audio stubs for use when Audio is disabled, or when Audio failed to initialise.
/// </summary>
[PublicAPI]
public class MockAudio : IAudio
{
    /// <inheritdoc />
    public IAudioDevice NewAudioDevice( int samplingRate, bool isMono )
    {
        return new MockAudioDevice();
    }

    /// <inheritdoc />
    public IAudioRecorder NewAudioRecorder( int samplingRate, bool isMono )
    {
        return new MockAudioRecorder();
    }

    /// <inheritdoc />
    public ISound NewSound( FileInfo? fileHandle )
    {
        return new MockSound();
    }

    /// <inheritdoc />
    public IMusic NewMusic( FileInfo? file )
    {
        return new MockMusic();
    }

    public void Update()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}