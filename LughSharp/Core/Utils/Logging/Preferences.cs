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

using System.Globalization;
using System.Xml.Linq;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.Logging;

[PublicAPI]
public class Preferences : IPreferences
{
    private readonly string                       _filePath;
    private readonly Dictionary< string, object > _properties = [ ];
    private readonly string                       _propertiesFile;
    private          XDocument?                   _xDocument;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="Preferences" /> class.
    /// </summary>
    /// <param name="filename"> The name of the preferences file. </param>
    public Preferences( string filename )
    {
        _filePath       = AppContext.BaseDirectory + "logs" + Path.DirectorySeparatorChar;
        _propertiesFile = filename;

        if ( !Directory.Exists( _filePath ) )
        {
            Directory.CreateDirectory( _filePath );
        }

        var fullPath = _filePath + _propertiesFile;

        if ( !File.Exists( fullPath ) )
        {
            // Create the file and write the initial XML structure immediately.
            var initialDoc = new XDocument( new XDeclaration( "1.0", "utf-8", "yes" ),
                                            new XElement( "properties" ) );
            initialDoc.Save( fullPath );
        }

        try
        {
            _xDocument = XDocument.Load( fullPath );

            if ( _xDocument.Root?.Name == "properties" )
            {
                foreach ( var entryElement in _xDocument.Root.Elements( "entry" ) )
                {
                    var key   = entryElement.Attribute( "key" )?.Value;
                    var value = entryElement.Value;

                    if ( key != null )
                    {
                        _properties[ key ] = value;
                    }
                }
            }
            else
            {
                Logger.Error( "Invalid root element in preferences file." );
            }
        }
        catch ( Exception e )
        {
            Logger.Error( e.Message );
        }

        Flush();
    }

    // ========================================================================

    /// <inheritdoc />
    public IPreferences PutBool( string key, bool val )
    {
        return Put( key, val.ToString() );
    }

    /// <inheritdoc />
    public IPreferences PutInteger( string key, int val )
    {
        return Put( key, val.ToString() );
    }

    /// <inheritdoc />
    public IPreferences PutLong( string key, long val )
    {
        return Put( key, val.ToString() );
    }

    /// <inheritdoc />
    public IPreferences PutFloat( string key, float val )
    {
        return Put( key, val.ToString( CultureInfo.InvariantCulture ) );
    }

    /// <inheritdoc />
    public IPreferences PutString( string key, string val )
    {
        return Put( key, val );
    }

    /// <summary>
    /// Adds or updates multiple entries in the preferences.
    /// </summary>
    /// <param name="vals"> The dictionary of key-value pairs to add or update. </param>
    /// <returns> The current <see cref="IPreferences" /> instance. </returns>
    public IPreferences PutAll( Dictionary< string, object > vals )
    {
        foreach ( var entry in vals )
        {
            switch ( entry.Value )
            {
                case bool b:
                    PutBool( entry.Key, b );

                    break;

                case int i:
                    PutInteger( entry.Key, i );

                    break;

                case long l:
                    PutLong( entry.Key, l );

                    break;

                case float f:
                    PutFloat( entry.Key, f );

                    break;

                case string s:
                    PutString( entry.Key, s );

                    break;
            }
        }

        return this;
    }

    /// <summary>
    /// Gets a boolean value from the preferences.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <returns> The boolean value. </returns>
    public bool GetBool( string key )
    {
        return GetBool( key, false );
    }

    /// <summary>
    /// Gets an integer value from the preferences.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <returns> The integer value. </returns>
    public int GetInteger( string key )
    {
        return GetInteger( key, 0 );
    }

    /// <summary>
    /// Gets a long value from the preferences.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <returns> The long value. </returns>
    public long GetLong( string key )
    {
        return GetLong( key, 0 );
    }

    /// <summary>
    /// Gets a float value from the preferences.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <returns> The float value. </returns>
    public float GetFloat( string key )
    {
        return GetFloat( key, 0 );
    }

    /// <summary>
    /// Gets a boolean value from the preferences with a default value.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <param name="defValue"> The default value if the key does not exist. </param>
    /// <returns> The boolean value. </returns>
    public bool GetBool( string key, bool defValue )
    {
        if ( !_properties.TryGetValue( key, out var value )
             || ( ( value.ToString() != "true" ) && ( value.ToString() != "false" ) ) )
        {
            return defValue;
        }

        return Convert.ToBoolean( value );
    }

    /// <summary>
    /// Gets an integer value from the preferences with a default value.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <param name="defValue"> The default value if the key does not exist. </param>
    /// <returns> The integer value. </returns>
    public int GetInteger( string key, int defValue )
    {
        return !_properties.TryGetValue( key, out var value )
            ? defValue
            : Convert.ToInt16( value );
    }

    /// <summary>
    /// Gets a long value from the preferences with a default value.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <param name="defValue"> The default value if the key does not exist. </param>
    /// <returns> The long value. </returns>
    public long GetLong( string key, long defValue )
    {
        return !_properties.TryGetValue( key, out var value )
            ? defValue
            : Convert.ToInt32( value );
    }

    /// <summary>
    /// Gets a float value from the preferences with a default value.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <param name="defValue"> The default value if the key does not exist. </param>
    /// <returns> The float value. </returns>
    public float GetFloat( string key, float defValue )
    {
        return !_properties.TryGetValue( key, out var value )
            ? defValue
            : Convert.ToSingle( value );
    }

    /// <summary>
    /// Gets a string value from the preferences with an optional default value.
    /// </summary>
    /// <param name="key"> The key of the preference entry. </param>
    /// <param name="defValue">
    /// The default value if the key does not exist. Default is an empty string.
    /// </param>
    /// <returns> The string value. </returns>
    public string GetString( string key, string defValue = "" )
    {
        if ( !_properties.TryGetValue( key, out var value ) )
        {
            return defValue;
        }

        return ( string )value;
    }

    /// <summary>
    /// Gets all properties in the preferences.
    /// </summary>
    /// <returns> A dictionary of all properties. </returns>
    public Dictionary< string, object > ToDictionary()
    {
        return _properties;
    }

    /// <summary>
    /// Checks if a key exists in the preferences.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns> True if the key exists; otherwise, false. </returns>
    public bool Contains( string key )
    {
        return _properties.ContainsKey( key );
    }

    /// <summary>
    /// Clears all entries in the preferences.
    /// </summary>
    public void Clear()
    {
        foreach ( var property in _properties )
        {
            Put( property.Key, "" );
        }
    }

    /// <summary>
    /// Removes a specific entry from the preferences.
    /// </summary>
    /// <param name="key"> The key of the entry to remove. </param>
    public void Remove( string key )
    {
        _properties.Remove( key );
    }

    /// <summary>
    /// Saves the preferences to the file.
    /// </summary>
    /// <exception cref="GdxRuntimeException">
    /// Thrown when there is an error writing preferences.
    /// </exception>
    public void Flush()
    {
        if ( ( _xDocument?.Root == null ) || ( _xDocument.Root.Name != "properties" ) )
        {
            _xDocument = new XDocument( new XDeclaration( "1.0", "UTF-8", "no" ),
                                        new XElement( "properties" ) );
        }
        else
        {
            _xDocument.Root.RemoveNodes();
        }

        try
        {
            foreach ( var entry in _properties )
            {
                _xDocument.Root?.Add( new XElement( "entry", new XAttribute( "key", entry.Key ), entry.Value ) );
            }

            _xDocument.Save( _filePath + _propertiesFile );
        }
        catch ( Exception )
        {
            throw new GdxRuntimeException( "Error writing preferences!" );
        }
    }

    /// <summary>
    /// Returns the number of entries in the preferences.
    /// </summary>
    public int Count => _properties.Count;

    // ========================================================================

    private Preferences Put( string key, string value )
    {
        _properties[ key ] = value.ToLower();

        return this;
    }
}

// ========================================================================
// ========================================================================