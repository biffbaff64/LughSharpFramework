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

namespace LughSharp.Source.Network;

/// <summary>
/// Provides utility methods to work with the <see cref="INet.HttpRequest"/> content and parameters.
/// </summary>
[PublicAPI]
public class HttpParametersUtils
{
    private HttpParametersUtils()
    {
    }

    public static String defaultEncoding    = "UTF-8";
    public static String nameValueSeparator = "=";
    public static String parameterSeparator = "&";

    /// <summary>
    /// Useful method to convert a map of key,value pairs to a <c>string</c> to be used as part
    /// of a GET or POST content operation.
    /// </summary>
    /// <param name="parameters"> A Dictionary with the parameters to encode. </param>
    /// <returns> The String with the parameters encoded. </returns>
    public static string ConvertHttpParameters( Dictionary< string, string > parameters )
    {
//        Set<String>   keySet              = parameters.keySet();
//        StringBuilder convertedParameters = new StringBuilder();
//        for (String name : keySet) {
//            convertedParameters.append(encode(name, defaultEncoding));
//            convertedParameters.append(nameValueSeparator);
//            convertedParameters.append(encode(parameters.get(name), defaultEncoding));
//            convertedParameters.append(parameterSeparator);
//        }
//        if (convertedParameters.length() > 0) convertedParameters.deleteCharAt(convertedParameters.length() - 1);
//        return convertedParameters.toString();

        return string.Empty;
    }

    private static String encode( String content, String encoding )
    {
//        try
//        {
//            return URLEncoder.encode( content, encoding );
//        }
//        catch ( UnsupportedEncodingException e )
//        {
//            throw new IllegalArgumentException( e );
//        }

        return string.Empty;
    }
}

// ============================================================================
// ============================================================================