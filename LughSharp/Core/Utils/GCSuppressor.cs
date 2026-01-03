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

using System.Runtime;

namespace LughSharp.Core.Utils;

[PublicAPI]
public static class GCSuppressor
{
    public static IDisposable Suppress()
    {
        // Forces a garbage collection before suppressing GC activity. This ensures
        // that any pending garbage collections are completed before entering the
        // low-latency mode.
        GC.Collect();

        // Sets the garbage collector to low-latency mode.1 In this mode, the garbage
        // collector minimizes pauses by performing collections less frequently and
        // more aggressively.
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;

        return new Suppressor();
    }

    private class Suppressor : IDisposable
    {
        public void Dispose()
        {
            // Set GCSettings.LatencyMode back to GCLatencyMode.Batch, restoring
            // the normal garbage collection behavior.
            GCSettings.LatencyMode = GCLatencyMode.Batch;
        }
    }
}