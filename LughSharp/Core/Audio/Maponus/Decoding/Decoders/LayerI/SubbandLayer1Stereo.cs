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

using JetBrains.Annotations;

namespace LughSharp.Core.Audio.Maponus.Decoding.Decoders.LayerI;

/// <summary>
/// public class for layer I subbands in stereo mode.
/// </summary>
[PublicAPI]
public class SubbandLayer1Stereo : SubbandLayer1
{
    protected int   Channel2Allocation;
    protected float Channel2Factor;
    protected float Channel2Offset;
    protected float Channel2Sample;
    protected int   Channel2Samplelength;
    protected float Channel2Scalefactor;
    
    // ========================================================================
    
    public SubbandLayer1Stereo( int subbandnumber )
        : base( subbandnumber )
    {
    }

    /// <summary>
    /// </summary>
    public override void ReadAllocation( Bitstream stream, Header? header, Crc16? crc )
    {
        Allocation = stream.GetBitsFromBuffer( 4 );

        if ( Allocation > 14 )
        {
            return;
        }

        Channel2Allocation = stream.GetBitsFromBuffer( 4 );

        if ( crc != null )
        {
            crc.AddBits( Allocation, 4 );
            crc.AddBits( Channel2Allocation, 4 );
        }

        if ( Allocation != 0 )
        {
            Samplelength = Allocation + 1;
            Factor       = TableFactor[ Allocation ];
            Offset       = TableOffset[ Allocation ];
        }

        if ( Channel2Allocation != 0 )
        {
            Channel2Samplelength = Channel2Allocation + 1;
            Channel2Factor       = TableFactor[ Channel2Allocation ];
            Channel2Offset       = TableOffset[ Channel2Allocation ];
        }
    }

    /// <summary>
    /// </summary>
    public override void ReadScaleFactor( Bitstream stream, Header? header )
    {
        if ( Allocation != 0 )
        {
            Scalefactor = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
        }

        if ( Channel2Allocation != 0 )
        {
            Channel2Scalefactor = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
        }
    }

    /// <summary>
    /// </summary>
    public override bool ReadSampleData( Bitstream stream )
    {
        var returnvalue = base.ReadSampleData( stream );

        if ( Channel2Allocation != 0 )
        {
            Channel2Sample = stream.GetBitsFromBuffer( Channel2Samplelength );
        }

        return returnvalue;
    }

    /// <summary>
    /// </summary>
    public override bool PutNextSample( int channels, SynthesisFilter? filter1, SynthesisFilter? filter2 )
    {
        base.PutNextSample( channels, filter1, filter2 );

        if ( ( Channel2Allocation != 0 ) && ( channels != OutputChannels.LEFT_CHANNEL ) )
        {
            var sample2 = ( ( Channel2Sample * Channel2Factor ) + Channel2Offset ) * Channel2Scalefactor;

            if ( channels == OutputChannels.BOTH_CHANNELS )
            {
                filter2?.AddSample( sample2, Subbandnumber );
            }
            else
            {
                filter1?.AddSample( sample2, Subbandnumber );
            }
        }

        return true;
    }
}

// ============================================================================
// ============================================================================

