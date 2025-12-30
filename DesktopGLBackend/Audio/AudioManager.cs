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

using DesktopGLBackend.Audio.Mock;
using LughSharp.Core.Audio;
using LughSharp.Utils.source.Logging;

namespace DesktopGLBackend.Audio;

[PublicAPI]
public static class AudioManager
{
    /// <summary>
    /// Creates the Audio device.
    /// </summary>
    public static IAudio CreateAudio( DesktopGLApplicationConfiguration config )
    {
        IAudio audio;

        if ( !config.DisableAudio )
        {
            try
            {
                audio = new OpenALAudio( config.AudioDeviceSimultaneousSources,
                                         config.AudioDeviceBufferCount,
                                         config.AudioDeviceBufferSize );
            }
            catch ( Exception e )
            {
                Logger.Debug( $"Couldn't initialize audio, disabling audio: {e}" );

                audio = new MockAudio();
            }
        }
        else
        {
            Logger.Debug( "Audio is disabled in Config, using MockAudio instead." );

            audio = new MockAudio();
        }

        return audio;
    }
}

// ========================================================================
// ========================================================================