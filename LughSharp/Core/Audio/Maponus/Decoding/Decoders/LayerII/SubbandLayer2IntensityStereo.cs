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
/// public class for layer II subbands in joint stereo mode.
/// </summary>
[PublicAPI]
public class SubbandLayer2IntensityStereo : SubbandLayer2
{
    protected float Channel2Scalefactor1;
    protected float Channel2Scalefactor2;
    protected float Channel2Scalefactor3;
    protected int   Channel2Scfsi;

    public SubbandLayer2IntensityStereo( int subbandnumber )
        : base( subbandnumber )
    {
    }

    /// <summary>
    /// </summary>
    public override void ReadScaleFactorSelection( Bitstream stream, Crc16? crc )
    {
        if ( Allocation != 0 )
        {
            Scfsi         = stream.GetBitsFromBuffer( 2 );
            Channel2Scfsi = stream.GetBitsFromBuffer( 2 );

            if ( crc != null )
            {
                crc.AddBits( Scfsi, 2 );
                crc.AddBits( Channel2Scfsi, 2 );
            }
        }
    }

    /// <summary>
    /// </summary>
    public override void ReadScaleFactor( Bitstream stream, Header? header )
    {
        if ( Allocation != 0 )
        {
            base.ReadScaleFactor( stream, header );

            if ( Channel2Scfsi is >= 0 and <= 3 )
            {
                Channel2Scalefactor1 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
                Channel2Scalefactor2 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
                Channel2Scalefactor3 = ScaleFactors[ stream.GetBitsFromBuffer( 6 ) ];
            }
        }

        if ( Allocation != 0 )
        {
            base.ReadScaleFactor( stream, header );

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
        }
    }

    /// <summary>
    /// </summary>
    public override bool PutNextSample( int channels, SynthesisFilter? filter1, SynthesisFilter? filter2 )
    {
        if ( Allocation != 0 )
        {
            var sample = Samples[ Samplenumber ];

            if ( Groupingtable[ 0 ] == null )
            {
                sample = ( sample + D[ 0 ] ) * CFactor[ 0 ];
            }

            switch ( channels )
            {
                case OutputChannels.BOTH_CHANNELS:
                {
                    var sample2 = sample;

                    switch ( Groupnumber )
                    {
                        case <= 4:
                            sample  *= Scalefactor1;
                            sample2 *= Channel2Scalefactor1;

                            break;

                        case <= 8:
                            sample  *= Scalefactor2;
                            sample2 *= Channel2Scalefactor2;

                            break;

                        default:
                            sample  *= Scalefactor3;
                            sample2 *= Channel2Scalefactor3;

                            break;
                    }

                    filter1?.AddSample( sample, Subbandnumber );
                    filter2?.AddSample( sample2, Subbandnumber );

                    break;
                }

                case OutputChannels.LEFT_CHANNEL:
                    sample *= Groupnumber switch
                    {
                        <= 4  => Scalefactor1,
                        <= 8  => Scalefactor2,
                        var _ => Scalefactor3,
                    };

                    filter1?.AddSample( sample, Subbandnumber );

                    break;

                default:
                    sample *= Groupnumber switch
                    {
                        <= 4  => Channel2Scalefactor1,
                        <= 8  => Channel2Scalefactor2,
                        var _ => Channel2Scalefactor3,
                    };

                    filter1?.AddSample( sample, Subbandnumber );

                    break;
            }
        }

        return ++Samplenumber == 3;
    }
}

// ============================================================================
// ============================================================================

