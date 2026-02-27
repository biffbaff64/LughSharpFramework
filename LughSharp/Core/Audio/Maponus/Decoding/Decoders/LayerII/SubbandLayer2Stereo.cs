// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Core.Audio.Maponus.Decoding.Decoders.LayerII;

/// <summary>
/// public class for layer II subbands in stereo mode.
/// </summary>
[PublicAPI]
public class SubbandLayer2Stereo : SubbandLayer2
{
    protected readonly float[] Channel2C          = [ 0 ];
    protected readonly int[]   Channel2Codelength = [ 0 ];
    protected readonly float[] Channel2D          = [ 0 ];
    protected readonly float[] Channel2Factor     = [ 0 ];
    protected readonly float[] Channel2Samples;
    protected          int     Channel2Allocation;
    protected          float   Channel2Scalefactor1;
    protected          float   Channel2Scalefactor2;
    protected          float   Channel2Scalefactor3;
    protected          int     Channel2Scfsi;

    // ========================================================================

    public SubbandLayer2Stereo( int subbandnumber )
        : base( subbandnumber )
    {
        Channel2Samples = new float[ 3 ];
    }

    /// <summary>
    /// </summary>
    public override void ReadAllocation( Bitstream stream, Header? header, Crc16? crc )
    {
        int length = GetAllocationLength( header );
        Allocation         = stream.GetBitsFromBuffer( length );
        Channel2Allocation = stream.GetBitsFromBuffer( length );

        if ( crc != null )
        {
            crc.AddBits( Allocation, length );
            crc.AddBits( Channel2Allocation, length );
        }
    }

    /// <summary>
    /// </summary>
    public override void ReadScaleFactorSelection( Bitstream stream, Crc16? crc )
    {
        if ( Allocation != 0 )
        {
            Scfsi = stream.GetBitsFromBuffer( 2 );
            crc?.AddBits( Scfsi, 2 );
        }

        if ( Channel2Allocation != 0 )
        {
            Channel2Scfsi = stream.GetBitsFromBuffer( 2 );
            crc?.AddBits( Channel2Scfsi, 2 );
        }
    }

    /// <summary>
    /// </summary>
    public override void ReadScaleFactor( Bitstream stream, Header? header )
    {
        base.ReadScaleFactor( stream, header );

        if ( Channel2Allocation != 0 )
        {
            switch ( Channel2Scfsi )
            {
                case 0:
                    Channel2Scalefactor1 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
                    Channel2Scalefactor2 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
                    Channel2Scalefactor3 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];

                    break;

                case 1:
                    Channel2Scalefactor1 =
                        Channel2Scalefactor2 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];

                    Channel2Scalefactor3 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];

                    break;

                case 2:
                    Channel2Scalefactor1 =
                        Channel2Scalefactor2 =
                            Channel2Scalefactor3 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];

                    break;

                case 3:
                    Channel2Scalefactor1 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];

                    Channel2Scalefactor2 =
                        Channel2Scalefactor3 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];

                    break;
            }

            PrepareForSampleRead( header,
                                  Channel2Allocation,
                                  1,
                                  Channel2Factor,
                                  Channel2Codelength,
                                  Channel2C,
                                  Channel2D );
        }
    }

    /// <summary>
    /// </summary>
    public override bool ReadSampleData( Bitstream stream )
    {
        bool returnvalue = base.ReadSampleData( stream );

        if ( Channel2Allocation != 0 )
        {
            if ( Groupingtable[ 1 ] != null )
            {
                int samplecode = stream.GetBitsFromBuffer( Channel2Codelength[ 0 ] );

                // create requantized samples:
                samplecode += samplecode << 1;
                /*
                float[] target = channel2_samples;
                float[] source = channel2_groupingtable[0];
                int tmp = 0;
                int temp = 0;
                target[tmp++] = source[samplecode + temp];
                temp++;
                target[tmp++] = source[samplecode + temp];
                temp++;
                target[tmp] = source[samplecode + temp];
                // memcpy (channel2_samples, channel2_groupingtable + samplecode, 3 * sizeof (real));
                */
                float[]  target = Channel2Samples;
                float[]? source = Groupingtable[ 1 ];
                var      tmp    = 0;
                int      temp   = samplecode;

                target[ tmp ] = source![ temp ];
                temp++;
                tmp++;
                target[ tmp ] = source[ temp ];
                temp++;
                tmp++;
                target[ tmp ] = source[ temp ];
            }
            else
            {
                Channel2Samples[ 0 ] =
                    ( float )( ( stream.GetBitsFromBuffer( Channel2Codelength[ 0 ] ) * Channel2Factor[ 0 ] ) - 1.0 );

                Channel2Samples[ 1 ] =
                    ( float )( ( stream.GetBitsFromBuffer( Channel2Codelength[ 0 ] ) * Channel2Factor[ 0 ] ) - 1.0 );

                Channel2Samples[ 2 ] =
                    ( float )( ( stream.GetBitsFromBuffer( Channel2Codelength[ 0 ] ) * Channel2Factor[ 0 ] ) - 1.0 );
            }
        }

        return returnvalue;
    }

    /// <summary>
    /// </summary>
    public override bool PutNextSample( int channels, SynthesisFilter? filter1, SynthesisFilter? filter2 )
    {
        bool returnvalue = base.PutNextSample( channels, filter1, filter2 );

        if ( ( Channel2Allocation != 0 ) && ( channels != OutputChannels.LEFT_CHANNEL ) )
        {
            float sample = Channel2Samples[ Samplenumber - 1 ];

            if ( Groupingtable[ 1 ] == null )
            {
                sample = ( sample + Channel2D[ 0 ] ) * Channel2C[ 0 ];
            }

            sample *= Groupnumber switch
                      {
                          <= 4  => Channel2Scalefactor1,
                          <= 8  => Channel2Scalefactor2,
                          var _ => Channel2Scalefactor3
                      };

            if ( channels == OutputChannels.BOTH_CHANNELS )
            {
                filter2?.AddSample( sample, Subbandnumber );
            }
            else
            {
                filter1?.AddSample( sample, Subbandnumber );
            }
        }

        return returnvalue;
    }
}

// ============================================================================
// ============================================================================