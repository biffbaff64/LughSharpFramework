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

using LughUtils.source.Logging;

namespace Extensions.Source.Json;

public partial class JsonReader
{
    public void Debug()
    {
        Logger.Debug( $"_jsonActions          : {_jsonActions.Length}" );
        Logger.Debug( $"_jsonKeyOffsets       : {_jsonKeyOffsets.Length}" );
        Logger.Debug( $"_jsonTransitionKeys   : {_jsonTransitionKeys.Length}" );
        Logger.Debug( $"_jsonSingleLengths    : {_jsonSingleLengths.Length}" );
        Logger.Debug( $"_jsonRangeLengths     : {_jsonRangeLengths.Length}" );
        Logger.Debug( $"_jsonIndexOffsets     : {_jsonIndexOffsets.Length}" );
        Logger.Debug( $"_jsonIndicies         : {_jsonIndicies.Length}" );
        Logger.Debug( $"_jsonTransitionTargs  : {_jsonTransitionTargs.Length}" );
        Logger.Debug( $"_jsonTransitionActions: {_jsonTransitionActions.Length}" );
        Logger.Debug( $"_jsonEofActions       : {_jsonEofActions.Length}" );
    }

    // ========================================================================
    //@formatter:off
    // ========================================================================

    // 0 = Set '_stringIsName' to true
    // 1 = Handle String Value
    // 2 = Start Object
    // 3 = End Object
    // 4 = Start Array
    // 5 = End Array
    // 6 = Skip Comment
    // 7 = Handle Unquoted Chars
    // 8 = Handle Quoted Chars
    
    private static byte[] init__json_actions_0()
    {
        return
        [
            0, 1, 1, 1, 2, 1, 3, 1, 4, 1,
            5, 1, 6, 1, 7, 1, 8, 2, 0, 7,
            2, 0, 8, 2, 1, 3, 2, 1, 5,
        ];
    }

    private static readonly byte[] _jsonActions = init__json_actions_0();

    // ========================================================================

    private static short[] init__json_key_offsets_0()
    {
        return
        [
              0,   0,  11,  13,  14,  16,  25,  31,  37,  39,
             50,  57,  64,  73,  74,  83,  85,  87,  96,  98,
            100, 101, 103, 105, 116, 123, 130, 141, 142, 153,
            155, 157, 168, 170, 172, 174, 179, 184, 184,
        ];
    }

    private static readonly short[] _jsonKeyOffsets = init__json_key_offsets_0();

    // ========================================================================

    private static char[] init__json_transition_keys_0()
    {
        return
        [
            ( char )13,  ( char )32,  ( char )34,  ( char )44,  ( char )47,
            ( char )58,  ( char )91,  ( char )93,  ( char )123, ( char )9,
            ( char )10,  ( char )42,  ( char )47,  ( char )34,  ( char )42,
            ( char )47,  ( char )13,  ( char )32,  ( char )34,  ( char )44,
            ( char )47,  ( char )58,  ( char )125, ( char )9,   ( char )10,
            ( char )13,  ( char )32,  ( char )47,  ( char )58,  ( char )9,
            ( char )10,  ( char )13,  ( char )32,  ( char )47,  ( char )58,
            ( char )9,   ( char )10,  ( char )42,  ( char )47,  ( char )13,
            ( char )32,  ( char )34,  ( char )44,  ( char )47,  ( char )58,
            ( char )91,  ( char )93,  ( char )123, ( char )9,   ( char )10,
            ( char )9,   ( char )10,  ( char )13,  ( char )32,  ( char )44,
            ( char )47,  ( char )125, ( char )9,   ( char )10,  ( char )13,
            ( char )32,  ( char )44,  ( char )47,  ( char )125, ( char )13,
            ( char )32,  ( char )34,  ( char )44,  ( char )47,  ( char )58,
            ( char )125, ( char )9,   ( char )10,  ( char )34,  ( char )13,
            ( char )32,  ( char )34,  ( char )44,  ( char )47,  ( char )58,
            ( char )125, ( char )9,   ( char )10,  ( char )42,  ( char )47,
            ( char )42,  ( char )47,  ( char )13,  ( char )32,  ( char )34,
            ( char )44,  ( char )47,  ( char )58,  ( char )125, ( char )9,
            ( char )10,  ( char )42,  ( char )47,  ( char )42,  ( char )47,
            ( char )34,  ( char )42,  ( char )47,  ( char )42,  ( char )47,
            ( char )13,  ( char )32,  ( char )34,  ( char )44,  ( char )47,
            ( char )58,  ( char )91,  ( char )93,  ( char )123, ( char )9,
            ( char )10,  ( char )9,   ( char )10,  ( char )13,  ( char )32,
            ( char )44,  ( char )47,  ( char )93,  ( char )9,   ( char )10,
            ( char )13,  ( char )32,  ( char )44,  ( char )47,  ( char )93,
            ( char )13,  ( char )32,  ( char )34,  ( char )44,  ( char )47,
            ( char )58,  ( char )91,  ( char )93,  ( char )123, ( char )9,
            ( char )10,  ( char )34,  ( char )13,  ( char )32,  ( char )34,
            ( char )44,  ( char )47,  ( char )58,  ( char )91,  ( char )93,
            ( char )123, ( char )9,   ( char )10,  ( char )42,  ( char )47,
            ( char )42,  ( char )47,  ( char )13,  ( char )32,  ( char )34,
            ( char )44,  ( char )47,  ( char )58,  ( char )91,  ( char )93,
            ( char )123, ( char )9,   ( char )10,  ( char )42,  ( char )47,
            ( char )42,  ( char )47,  ( char )42,  ( char )47,  ( char )13,
            ( char )32,  ( char )47,  ( char )9,   ( char )10,  ( char )13,
            ( char )32,  ( char )47,  ( char )9,   ( char )10,  ( char )0,
        ];
    }

    private static readonly char[] _jsonTransitionKeys = init__json_transition_keys_0();

    // ========================================================================

    private static byte[] init__json_single_lengths_0()
    {
        return
        [
            0, 9, 2, 1, 2, 7, 4, 4, 2, 9, 7, 7, 7, 1, 7, 2, 2, 7, 2, 2,
            1, 2, 2, 9, 7, 7, 9, 1, 9, 2, 2, 9, 2, 2, 2, 3, 3, 0, 0,
        ];
    }

    private static readonly byte[] _jsonSingleLengths = init__json_single_lengths_0();

    // ========================================================================

    private static byte[] init__json_range_lengths_0()
    {
        return
        [
            0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0,
            0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0,
        ];
    }

    private static readonly byte[] _jsonRangeLengths = init__json_range_lengths_0();

    // ========================================================================

    private static short[] init__json_index_offsets_0()
    {
        return
        [
              0,   0,  11,  14,  16,  19,  28,  34,  40,  43,  54,  62,  70,  79,  81,
             90,  93,  96, 105, 108, 111, 113, 116, 119, 130, 138, 146, 157, 159, 170,
            173, 176, 187, 190, 193, 196, 201, 206, 207,
        ];
    }

    private static readonly short[] _jsonIndexOffsets = init__json_index_offsets_0();

    // ========================================================================

    private static byte[] init__json_indicies_0()
    {
        return
        [
             1,  1,  2,  3,  4,  3,  5,  3,  6,  1,  0,  7,  7,  3,  8,  3,  9,  9,  3, 11,
            11, 12, 13, 14,  3, 15, 11, 10, 16, 16, 17, 18, 16,  3, 19, 19, 20, 21, 19,  3,
            22, 22,  3, 21, 21, 24,  3, 25,  3, 26,  3, 27, 21, 23, 28, 29, 29, 28, 30, 31,
            32,  3, 33, 34, 34, 33, 13, 35, 15,  3, 34, 34, 12, 36, 37,  3, 15, 34, 10, 16,
             3, 36, 36, 12,  3, 38,  3,  3, 36, 10, 39, 39,  3, 40, 40,  3, 13, 13, 12,  3,
            41,  3, 15, 13, 10, 42, 42,  3, 43, 43,  3, 28,  3, 44, 44,  3, 45, 45,  3, 47,
            47, 48, 49, 50,  3, 51, 52, 53, 47, 46, 54, 55, 55, 54, 56, 57, 58,  3, 59, 60,
            60, 59, 49, 61, 52,  3, 60, 60, 48, 62, 63,  3, 51, 52, 53, 60, 46, 54,  3, 62,
            62, 48,  3, 64,  3, 51,  3, 53, 62, 46, 65, 65,  3, 66, 66,  3, 49, 49, 48,  3,
            67,  3, 51, 52, 53, 49, 46, 68, 68,  3, 69, 69,  3, 70, 70,  3,  8,  8, 71,  8,
             3, 72, 72, 73, 72,  3,  3,  3,  0,
        ];
    }

    private static readonly byte[] _jsonIndicies = init__json_indicies_0();

    // ========================================================================

    private static byte[] init__json_transition_targets_0()
    {
        return
        [
            35,  1,  3,  0,  4, 36, 36, 36, 36,  1,  6,  5, 13, 17, 22, 37,  7,  8,  9,  7,
             8,  9,  7, 10, 20, 21, 11, 11, 11, 12, 17, 19, 37, 11, 12, 19, 14, 16, 15, 14,
            12, 18, 17, 11,  9,  5, 24, 23, 27, 31, 34, 25, 38, 25, 25, 26, 31, 33, 38, 25,
            26, 33, 28, 30, 29, 28, 26, 32, 31, 25, 23,  2, 36,  2,
        ];
    }

    private static readonly byte[] _jsonTransitionTargs = init__json_transition_targets_0();

    // ========================================================================

    private static byte[] init__json_transition_actions_0()
    {
        return
        [
            13,  0, 15,  0,  0,  7,  3, 11,  1, 11, 17,  0, 20,  0,  0,  5,  1,  1,  1,  0,
             0,  0, 11, 13, 15,  0,  7,  3,  1,  1,  1,  1, 23,  0,  0,  0,  0,  0,  0, 11,
            11,  0, 11, 11, 11, 11, 13,  0, 15,  0,  0,  7,  9,  3,  1,  1,  1,  1, 26,  0,
             0,  0,  0,  0,  0, 11, 11,  0, 11, 11, 11,  1,  0,  0,
        ];
    }

    private static readonly byte[] _jsonTransitionActions = init__json_transition_actions_0();

    // ========================================================================

    private static byte[] init__json_eof_actions_0()
    {
        return
        [
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 1, 0, 0, 0,
        ];
    }

    private static readonly byte[] _jsonEofActions = init__json_eof_actions_0();

    // ========================================================================
    //@formatter:on
    // ========================================================================
}