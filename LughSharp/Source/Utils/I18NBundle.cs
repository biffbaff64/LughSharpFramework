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

namespace LughSharp.Source.Utils;

/// <summary>
/// A bundle of localized strings, loaded from Java-style <c>.properties</c> files.
/// <para>
/// Given a root file such as <c>i18n/strings.properties</c>, culture specific variants
/// are found by appending the culture name to the file name, using underscores in place
/// of hyphens. For the culture <c>fr-CA</c> the files searched, from most to least
/// specific, are:
/// </para>
/// <code>
///     i18n/strings_fr_CA.properties
///     i18n/strings_fr.properties
///     i18n/strings.properties
/// </code>
/// <para>
/// Each file found becomes a bundle whose <see cref="Parent"/> is the next, less specific,
/// bundle in the chain. Keys missing from a bundle are looked up in its parent, so a
/// variant file only needs to contain the strings that differ from its parent. Missing
/// files are skipped, but at least one file in the chain must exist.
/// </para>
/// <para>
/// Messages are formatted with <see cref="string.Format(IFormatProvider, string, object[])"/>,
/// so placeholders use .NET syntax, e.g. <c>{0}</c>, <c>{1:N2}</c>, and literal braces are
/// written as <c>{{</c> and <c>}}</c>. Note that this differs from libGDX, which uses Java's
/// <c>MessageFormat</c> and its single-quote escaping.
/// </para>
/// </summary>
[PublicAPI]
public class I18NBundle
{
    /// <summary>
    /// The file extension used for bundle files.
    /// </summary>
    public const string Extension = ".properties";

    /// <summary>
    /// If true, <see cref="Get"/> and <see cref="Format"/> throw a
    /// <see cref="KeyNotFoundException"/> for missing keys. If false, they
    /// return the key wrapped in <c>???</c> instead, e.g. <c>???menu.title???</c>,
    /// which makes untranslated strings easy to spot on screen.
    /// Default is true.
    /// </summary>
    public static bool ExceptionOnMissingKey { get; set; } = true;

    /// <summary>
    /// The culture of the file this bundle was loaded from. This is
    /// <see cref="CultureInfo.InvariantCulture"/> for the root file.
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// The less specific bundle that is searched when a key is not found in
    /// this one, or null if this is the last bundle in the chain.
    /// </summary>
    public I18NBundle? Parent { get; }

    /// <summary>
    /// The keys defined directly in this bundle, not including those of its parents.
    /// </summary>
    public IEnumerable< string > Keys => _properties.Keys;

    // ========================================================================

    private readonly Dictionary< string, string > _properties;

    // The culture used when formatting messages. This is the culture originally
    // requested, which may be more specific than the culture of any file found.
    private readonly CultureInfo _formatCulture;

    // ========================================================================
    // ========================================================================

    private I18NBundle( Dictionary< string, string > properties,
                        CultureInfo culture,
                        CultureInfo formatCulture,
                        I18NBundle? parent )
    {
        _properties    = properties;
        _formatCulture = formatCulture;
        Culture        = culture;
        Parent         = parent;
    }

    /// <summary>
    /// Creates a bundle for <see cref="CultureInfo.CurrentUICulture"/>, using UTF-8 encoding.
    /// </summary>
    /// <param name="rootFile"> The root bundle file, e.g. <c>i18n/strings.properties</c>. </param>
    public static I18NBundle CreateBundle( FileInfo rootFile )
    {
        return CreateBundle( rootFile, CultureInfo.CurrentUICulture );
    }

    /// <summary>
    /// Creates a bundle for the specified culture.
    /// </summary>
    /// <param name="rootFile"> The root bundle file, e.g. <c>i18n/strings.properties</c>. </param>
    /// <param name="culture"> The culture to load strings for. </param>
    /// <param name="encoding"> The file encoding. Defaults to UTF-8. </param>
    /// <exception cref="FileNotFoundException">
    /// Thrown if neither the root file nor any of its culture variants exist.
    /// </exception>
    public static I18NBundle CreateBundle( FileInfo rootFile, CultureInfo culture, Encoding? encoding = null )
    {
        Guard.Against.Null( rootFile );
        Guard.Against.Null( culture );

        encoding ??= Encoding.UTF8;

        I18NBundle? bundle = null;

        // Build the chain from the least specific culture to the most specific,
        // so that each bundle can be given its parent as it is created.
        foreach ( CultureInfo candidate in GetCandidateCultures( culture ) )
        {
            FileInfo file = GetVariantFile( rootFile, candidate );

            if ( !file.Exists )
            {
                continue;
            }

            using var reader = new StreamReader( file.FullName, encoding );

            bundle = new I18NBundle( ParseProperties( reader ), candidate, culture, bundle );
        }

        return bundle ?? throw new FileNotFoundException( $"No bundle found for '{rootFile.FullName}' " +
                                                          $"with culture '{culture.Name}'.", rootFile.FullName );
    }

    /// <summary>
    /// Creates a bundle from the given reader, with no parent. Mostly useful
    /// for testing, or for strings that do not come from a file.
    /// </summary>
    /// <param name="reader"> The reader supplying the properties text. </param>
    /// <param name="culture"> The culture of the strings, also used for formatting. </param>
    public static I18NBundle CreateBundle( TextReader reader, CultureInfo culture )
    {
        Guard.Against.Null( reader );
        Guard.Against.Null( culture );

        return new I18NBundle( ParseProperties( reader ), culture, culture, null );
    }

    // ========================================================================

    /// <summary>
    /// Returns the string for the given key, searching parent bundles if
    /// it is not defined in this one.
    /// </summary>
    /// <param name="key"> The key to look up. </param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the key is not found and <see cref="ExceptionOnMissingKey"/> is true.
    /// </exception>
    public string Get( string key )
    {
        Guard.Against.Null( key );

        if ( TryGet( key, out string? value ) )
        {
            return value;
        }

        return ExceptionOnMissingKey
            ? throw new KeyNotFoundException( $"Can't find bundle key '{key}'" )
            : $"???{key}???";
    }

    /// <summary>
    /// Looks up the string for the given key, searching parent bundles if
    /// it is not defined in this one.
    /// </summary>
    /// <param name="key"> The key to look up. </param>
    /// <param name="value"> The string found, or null. </param>
    /// <returns> True if the key was found. </returns>
    public bool TryGet( string key, [NotNullWhen( true )] out string? value )
    {
        for ( I18NBundle? bundle = this; bundle != null; bundle = bundle.Parent )
        {
            if ( bundle._properties.TryGetValue( key, out value ) )
            {
                return true;
            }
        }

        value = null;

        return false;
    }

    /// <summary>
    /// Returns true if the key is defined in this bundle or any of its parents.
    /// </summary>
    public bool ContainsKey( string key )
    {
        return TryGet( key, out _ );
    }

    /// <summary>
    /// Returns the string for the given key, with its placeholders replaced by the
    /// supplied arguments. Formatting uses the culture the bundle was requested for.
    /// </summary>
    /// <param name="key"> The key to look up. </param>
    /// <param name="args"> The arguments to substitute into the string. </param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the key is not found and <see cref="ExceptionOnMissingKey"/> is true.
    /// </exception>
    public string Format( string key, params object?[] args )
    {
        Guard.Against.Null( key );

        return TryGet( key, out string? pattern )
            ? string.Format( _formatCulture, pattern, args )
            : Get( key );
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"I18NBundle[{( Culture.Name.Length == 0 ? "root" : Culture.Name )}]";
    }

    // ========================================================================

    /// <summary>
    /// Returns the cultures to search, from least specific (invariant) to most
    /// specific, e.g. for <c>fr-CA</c>: invariant, <c>fr</c>, <c>fr-CA</c>.
    /// </summary>
    private static List< CultureInfo > GetCandidateCultures( CultureInfo culture )
    {
        var cultures = new List< CultureInfo >();

        for ( CultureInfo c = culture; !Equals( c, CultureInfo.InvariantCulture ); c = c.Parent )
        {
            cultures.Add( c );
        }

        cultures.Add( CultureInfo.InvariantCulture );
        cultures.Reverse();

        return cultures;
    }

    /// <summary>
    /// Returns the file for the given culture, e.g. <c>strings.properties</c> and
    /// <c>fr-CA</c> gives <c>strings_fr_CA.properties</c>. The invariant culture
    /// gives the root file itself.
    /// </summary>
    private static FileInfo GetVariantFile( FileInfo rootFile, CultureInfo culture )
    {
        if ( culture.Name.Length == 0 )
        {
            return rootFile;
        }

        string directory = rootFile.DirectoryName ?? "";
        string baseName  = Path.GetFileNameWithoutExtension( rootFile.Name );
        string extension = rootFile.Extension.Length > 0 ? rootFile.Extension : Extension;

        return new FileInfo( Path.Combine( directory, $"{baseName}_{culture.Name.Replace( '-', '_' )}{extension}" ) );
    }

    // ========================================================================

    /// <summary>
    /// Parses text in the Java <c>.properties</c> format:
    /// <list type="bullet">
    /// <item>Lines starting with <c>#</c> or <c>!</c> are comments.</item>
    /// <item>Keys are separated from values by <c>=</c>, <c>:</c> or whitespace.</item>
    /// <item>A line ending in an unescaped <c>\</c> continues on the next line.</item>
    /// <item>Supports the escapes <c>\t \n \r \f \uXXXX</c>; any other escaped
    /// character, such as <c>\=</c> or <c>\\</c>, stands for itself.</item>
    /// </list>
    /// If a key appears more than once, the last value wins.
    /// </summary>
    private static Dictionary< string, string > ParseProperties( TextReader reader )
    {
        var properties = new Dictionary< string, string >();

        while ( ReadLogicalLine( reader ) is { } line )
        {
            int pos = 0;

            string key = ReadToken( line, ref pos, isKey: true );

            // Skip whitespace, at most one separator, then whitespace again.
            SkipWhitespace( line, ref pos );

            if ( ( pos < line.Length ) && line[ pos ] is '=' or ':' )
            {
                pos++;
                SkipWhitespace( line, ref pos );
            }

            properties[ key ] = ReadToken( line, ref pos, isKey: false );
        }

        return properties;
    }

    /// <summary>
    /// Reads the next non-blank, non-comment line, joining continuation lines.
    /// Leading whitespace is removed from each physical line. Escape sequences
    /// are left in place, to be decoded by <see cref="ReadToken"/>.
    /// </summary>
    private static string? ReadLogicalLine( TextReader reader )
    {
        while ( reader.ReadLine() is { } physical )
        {
            string line = physical.TrimStart( ' ', '\t', '\f' );

            if ( ( line.Length == 0 ) || line[ 0 ] is '#' or '!' )
            {
                continue;
            }

            var builder = new StringBuilder();

            while ( EndsWithContinuation( line ) )
            {
                builder.Append( line, 0, line.Length - 1 );

                if ( reader.ReadLine() is not { } next )
                {
                    return builder.ToString();
                }

                line = next.TrimStart( ' ', '\t', '\f' );
            }

            return builder.Append( line ).ToString();
        }

        return null;
    }

    /// <summary>
    /// A line continues if it ends with an odd number of backslashes.
    /// </summary>
    private static bool EndsWithContinuation( string line )
    {
        int count = 0;

        for ( int i = line.Length - 1; ( i >= 0 ) && ( line[ i ] == '\\' ); i-- )
        {
            count++;
        }

        return ( count % 2 ) == 1;
    }

    private static void SkipWhitespace( string line, ref int pos )
    {
        while ( ( pos < line.Length ) && line[ pos ] is ' ' or '\t' or '\f' )
        {
            pos++;
        }
    }

    /// <summary>
    /// Reads a key or value starting at <paramref name="pos"/>, decoding escape sequences.
    /// A key ends at the first unescaped separator or whitespace; a value runs to the
    /// end of the line.
    /// </summary>
    private static string ReadToken( string line, ref int pos, bool isKey )
    {
        var builder = new StringBuilder();

        while ( pos < line.Length )
        {
            char c = line[ pos ];

            if ( isKey && c is '=' or ':' or ' ' or '\t' or '\f' )
            {
                break;
            }

            pos++;

            if ( ( c != '\\' ) || ( pos >= line.Length ) )
            {
                builder.Append( c );

                continue;
            }

            c = line[ pos++ ];

            switch ( c )
            {
                case 't':
                    builder.Append( '\t' );

                    break;

                case 'n':
                    builder.Append( '\n' );

                    break;

                case 'r':
                    builder.Append( '\r' );

                    break;

                case 'f':
                    builder.Append( '\f' );

                    break;

                case 'u':
                    if ( ( ( pos + 4 ) > line.Length )
                      || !int.TryParse( line.AsSpan( pos, 4 ), NumberStyles.HexNumber, null, out int code ) )
                    {
                        throw new FormatException( $"Malformed \\uXXXX escape in properties line: {line}" );
                    }

                    builder.Append( ( char )code );
                    pos += 4;

                    break;

                default:
                    builder.Append( c );

                    break;
            }
        }

        return builder.ToString();
    }
}

// ============================================================================
// ============================================================================
