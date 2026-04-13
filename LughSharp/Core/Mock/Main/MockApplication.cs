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

using JetBrains.Annotations;

using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Mock.Main;

[PublicAPI]
public class MockApplication : IApplication
{
    /// <summary>
    /// What <see cref="Platform.ApplicationType"/> the application has.
    /// </summary>
    public Platform.ApplicationType AppType { get; set; }

    /// <summary>
    /// </summary>
    public IClipboard? Clipboard { get; set; }

    /// <summary>
    /// Returns the Android API level on Android, the major OS version on iOS (5, 6, 7, ..),
    /// or 0 on the desktop.
    /// </summary>
    public int GetVersion()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Returns the <see cref="IPreferences"/> instance of this Application. It can be
    /// used to store application settings across runs.
    /// </summary>
    /// <param name="name"> the name of the preferences, must be useable as a file name. </param>
    /// <returns> The preferences. </returns>
    public IPreferences GetPreferences( string name )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adds a new <see cref="ILifecycleListener"/> to the application. This can be
    /// used by extensions to hook into the lifecycle more easily.
    /// The <see cref="IApplicationListener"/> methods are sufficient for application
    /// level development.
    /// </summary>
    public void AddLifecycleListener( ILifecycleListener listener )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Removes the specified <see cref="ILifecycleListener"/>
    /// </summary>
    public void RemoveLifecycleListener( ILifecycleListener listener )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Posts a <see cref="IRunnable"/> to the event queue.
    /// </summary>
    public void PostRunnable( Action runnable )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Schedule an exit from the application. On android, this will cause a call to
    /// <see cref="IApplicationListener.Pause()"/> and <see cref="IDisposable.Dispose()"/>
    /// some time in the future.
    /// <para>
    /// It will not immediately finish your application.
    /// </para>
    /// <para>
    /// On iOS this should be avoided in production as it breaks Apples guidelines
    /// </para>
    /// </summary>
    public void Exit()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cleanup everything before shutdown.
    /// </summary>
    public void Cleanup()
    {
        throw new NotImplementedException();
    }
}

// ============================================================================
// ============================================================================