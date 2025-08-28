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

using System.Diagnostics;
using System.Globalization;
using System.Text;

using Environment = System.Environment;

namespace LughSharp.Lugh.Utils;

/// <summary>
/// A class to write debug messages to console and a text file. Primarily intended for flow
/// tracing, messages will display calling file/class/methods and any provided debug message.
/// <li>Debug messages will not work if (TraceLevel &amp; LOG_DEBUG) is false.</li>
/// <li>Error Debug messages will not work if (TraceLevel &amp; LOG_ERROR) is false.</li>
/// <para>
/// To enable writing to file, <see cref="EnableWriteToFile" /> must be TRUE
/// and <see cref="OpenDebugFile" /> must be called.
/// </para>
/// </summary>
[PublicAPI]
public static class Logger
{
    #region constants

    // Enable / Disable flags.
    public const int LOG_NONE  = 0;
    public const int LOG_DEBUG = 1;
    public const int LOG_ERROR = 2;

    private const string DEBUG_TAG              = "[DEBUG.....]";
    private const string ERROR_TAG              = "[WARNING...]";
    private const string CHECKPOINT_TAG         = "[CHECKPOINT]";
    private const string PREFS_FOLDER           = "logs";
    private const string DEFAULT_TRACE_FILENAME = "trace.txt";

    private const string DIVIDER = "-----------------------------------------------------";

    #endregion constants

    // ========================================================================

    #region properties

    public static bool EnableWriteToFile { get; set; } = false;
    public static int  TraceLevel        { get; set; } = LOG_NONE;
    public static bool IsMinimal         { get; set; } = false;

    #endregion properties

    // ========================================================================

    private static string        _debugFilePath = "";
    private static string        _debugFileName = "";
    private static StreamWriter? _streamWriter;

    // ========================================================================
    // ========================================================================

    #region public methods

    /// <summary>
    /// Initialises the Logger.
    /// </summary>
    /// <param name="logLevel"> The initially enabled log level(s). </param>
    /// <param name="enableWriteToFile"> TRUE to enable outputting messages to a file. </param>
    /// <param name="filename"> The name of the file to write to. Default is trace.txt. </param>
    [Conditional( "DEBUG" )]
    public static void Initialise( int logLevel = LOG_DEBUG | LOG_ERROR,
                                   bool enableWriteToFile = true,
                                   string filename = DEFAULT_TRACE_FILENAME )
    {
        TraceLevel        = logLevel;
        EnableWriteToFile = enableWriteToFile;
        IsMinimal         = false;

        if ( EnableWriteToFile )
        {
            OpenDebugFile( filename, true );
        }
    }

    /// <summary>
    /// Send a DEBUG message to output window/console/File.
    /// </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="boxedDebug">
    /// If TRUE, a dividing line will be written before and after this debug message.
    /// </param>
    /// <param name="callerFilePath"> The File this message was sent from. </param>
    /// <param name="callerMethod"> The Method this message was sent from. </param>
    /// <param name="callerLine"> The Line this message was sent from. </param>
    [Conditional( "DEBUG" )]
    public static void Debug( string message,
                              bool boxedDebug = false,
                              [CallerFilePath] string callerFilePath = "",
                              [CallerMemberName] string callerMethod = "",
                              [CallerLineNumber] int callerLine = 0 )
    {
        if ( !IsEnabled( LOG_DEBUG ) )
        {
            return;
        }

        if ( boxedDebug )
        {
            Divider();
        }

        string str;
        
        if ( IsMinimal )
        {
            str = CreateMessage( null, message, null );
        }
        else
        {
            var callerID = MakeCallerID( callerFilePath, callerMethod, callerLine );
            str      = CreateMessage( DEBUG_TAG, message, callerID );
        }
        
        Console.WriteLine( str );
        WriteToFile( str );

        if ( boxedDebug )
        {
            Divider();
        }
    }

    /// <summary>
    /// Send a DEBUG message to output window/console/File.
    /// </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="callerFilePath"> The File this message was sent from. </param>
    /// <param name="callerMethod"> The Method this message was sent from. </param>
    /// <param name="callerLine"> The Line this message was sent from. </param>
    [Conditional( "DEBUG" )]
    public static void Warning( string message,
                                [CallerFilePath] string callerFilePath = "",
                                [CallerMemberName] string callerMethod = "",
                                [CallerLineNumber] int callerLine = 0 )
    {
        if ( !IsEnabled( LOG_ERROR ) )
        {
            return;
        }

        string str;
        
        if ( IsMinimal )
        {
            str = CreateMessage( ERROR_TAG, message, null );
        }
        else
        {
            var callerID = MakeCallerID( callerFilePath, callerMethod, callerLine );
            str      = CreateMessage( ERROR_TAG, message, callerID );
        }

        Console.WriteLine( str );
        WriteToFile( str );
    }

    /// <summary>
    /// Write a message to console if the supplied condition is TRUE.
    /// </summary>
    /// <param name="message"> The message to send. </param>
    /// <param name="condition"> The condition to evaluate. </param>
    /// <param name="callerFilePath"> The File this message was sent from. </param>
    /// <param name="callerMethod"> The Method this message was sent from. </param>
    /// <param name="callerLine"> The Line this message was sent from. </param>
    [Conditional( "DEBUG" )]
    public static void DebugCondition( string message,
                                       bool condition = false,
                                       [CallerFilePath] string callerFilePath = "",
                                       [CallerMemberName] string callerMethod = "",
                                       [CallerLineNumber] int callerLine = 0 )
    {
        if ( !IsEnabled( LOG_DEBUG ) || !condition )
        {
            return;
        }

        var callerID = MakeCallerID( callerFilePath, callerMethod, callerLine );
        var str      = CreateMessage( DEBUG_TAG, message, callerID );

        Console.WriteLine( str );
        WriteToFile( str );
    }

    /// <summary>
    /// Writes a debug message consisting solely of the following:-
    /// <li> Current time and date. </li>
    /// <li> Calling Class/method/line number information. </li>
    /// </summary>
    /// <param name="lineBefore"> Draws a divider line first, if set to true. </param>
    /// <param name="lineAfter"> Draws a divider line after checkpoint is logged, if set to true. </param>
    /// <param name="callerFilePath"> The File this message was sent from. </param>
    /// <param name="callerMethod"> The Method this message was sent from. </param>
    /// <param name="callerLine"> The Line this message was sent from. </param>
    [Conditional( "DEBUG" )]
    public static void Checkpoint( bool lineBefore = false,
                                   bool lineAfter = false,
                                   [CallerFilePath] string callerFilePath = "",
                                   [CallerMemberName] string callerMethod = "",
                                   [CallerLineNumber] int callerLine = 0 )
    {
        if ( !IsEnabled( LOG_DEBUG ) )
        {
            return;
        }

        if ( lineBefore )
        {
            Divider();
        }

        string str;
        
        if ( IsMinimal )
        {
            str = CreateMessage( CHECKPOINT_TAG, "< CHECKPOINT >", null );
        }
        else
        {
            var callerID = MakeCallerID( callerFilePath, callerMethod, callerLine );
            str      = CreateMessage( CHECKPOINT_TAG, "< CHECKPOINT >", callerID );
        }

        Console.WriteLine( str );
        WriteToFile( str );

        if ( lineAfter )
        {
            Divider();
        }
    }

    /// <summary>
    /// Adds a dividing line to text output.
    /// </summary>
    /// <param name="ch"> The character to use, default is '-' </param>
    /// <param name="length"> The line length, default is 80. </param>
    [Conditional( "DEBUG" )]
    public static void Divider( char ch = '-', int length = 80 )
    {
        var sb = new StringBuilder( DEBUG_TAG );

        sb.Append( " : " );

        for ( var i = 0; i < length; i++ )
        {
            sb.Append( ch );
        }

        Console.WriteLine( sb.ToString() );
        WriteToFile( sb.ToString() );
    }

    /// <summary>
    /// Adds a dividing line to text output, but only if then supplied condition is true.
    /// </summary>
    /// <param name="condition"> The condition. </param>
    /// <param name="ch"> The character to use, default is '-' </param>
    /// <param name="length"> The line length, default is 80. </param>
    [Conditional( "DEBUG" )]
    public static void DividerConditional( bool condition, char ch = '-', int length = 80 )
    {
        if ( condition )
        {
            Divider( ch, length );
        }
    }

    /// <summary>
    /// Writes an <see cref="Environment.NewLine"/> to console.
    /// Does NOT create a string holding caller data or timestamp.
    /// </summary>
    [Conditional( "DEBUG" )]
    public static void NewLine()
    {
        if ( IsEnabled( LOG_DEBUG ) )
        {
            Console.WriteLine( Environment.NewLine );
            WriteToFile( Environment.NewLine );
        }
    }

    /// <summary>
    /// Opens a physical file for writing copies of debug messages to.
    /// </summary>
    /// <param name="fileName">
    /// The filename. This should be filename only, without path, and the file will
    /// be created in a 'logs' folder in the working directory.
    /// </param>
    /// <param name="deleteExisting">
    /// True to delete existing copies of the file, False to append to existing file.
    /// </param>
    [Conditional( "DEBUG" )]
    public static void OpenDebugFile( string fileName, bool deleteExisting )
    {
        try
        {
            if ( fileName.Equals( string.Empty ) )
            {
                fileName = DEFAULT_TRACE_FILENAME;
            }

            // Get the base directory
            var baseDirectory = AppContext.BaseDirectory;

            // Construct the log directory path
            _debugFilePath = $"{baseDirectory}logs{Path.DirectorySeparatorChar}";
            _debugFileName = fileName;

            if ( File.Exists( _debugFilePath + _debugFileName ) && deleteExisting )
            {
                File.Delete( _debugFilePath + _debugFileName );
            }

            using ( _streamWriter = new StreamWriter( _debugFilePath + _debugFileName, true ) )
            {
                var dateTime = DateTime.Now;
                var time     = dateTime.ToShortTimeString();

                _streamWriter.Write( DIVIDER );
                _streamWriter.Write( time, 0, time.Length );
                _streamWriter.WriteLine( DIVIDER );
            }
        }
        catch ( Exception )
        {
            Console.WriteLine( $"Unable to open loge file: {_debugFilePath + _debugFileName}" );

            _debugFilePath    = null!;
            _debugFileName    = null!;
            EnableWriteToFile = false;
        }
    }

    /// <summary>
    /// Writes text to the logFile, if it exists.
    /// </summary>
    /// <param name="text">String holding the text to write.</param>
    [Conditional( "DEBUG" )]
    public static void WriteToFile( string text )
    {
        var filePath = _debugFilePath + _debugFileName;

        // Check if the file exists before attempting to write
        if ( !File.Exists( filePath ) )
        {
            OpenDebugFile( filePath, true );
        }

        try
        {
            // Determine whether to add a new line based on the content itself
            var contentToWrite = text;

            if ( !text.EndsWith( Environment.NewLine ) )
            {
                contentToWrite += Environment.NewLine;
            }

            using ( _streamWriter = new StreamWriter( filePath, true ) )
            {
                _streamWriter.Write( contentToWrite );
            }
        }
        catch ( UnauthorizedAccessException ex )
        {
            Console.WriteLine( $"Error: Access to the file '{filePath}' is denied. {ex.Message}" );
        }
        catch ( Exception ex )
        {
            Console.WriteLine( $"An unexpected error occurred while writing to file '{filePath}'. {ex.Message}" );
        }
    }

    /// <summary>
    /// Closes the debug file if it is open, releasing any associated resources.
    /// </summary>
    [Conditional( "DEBUG" )]
    public static void CloseDebugFile()
    {
        _streamWriter?.Close();
    }

    /// <summary>
    /// Disables DEBUG Log messages without affecting other types.
    /// </summary>
    [Conditional( "DEBUG" )]
    public static void DisableDebugLogging()
    {
        TraceLevel &= ~LOG_DEBUG;
    }

    /// <summary>
    /// Disables Error Log messages without affecting other types.
    /// </summary>
    [Conditional( "DEBUG" )]
    public static void DisableErrorLogging()
    {
        TraceLevel &= ~LOG_ERROR;
    }

    /// <summary>
    /// Enables DEBUG Log messages without affecting other types.
    /// </summary>
    [Conditional( "DEBUG" )]
    public static void EnableDebugLogging()
    {
        TraceLevel |= LOG_DEBUG;
    }

    /// <summary>
    /// Enables ERROR Log messages without affecting other types.
    /// </summary>
    [Conditional( "DEBUG" )]
    public static void EnableErrorLogging()
    {
        TraceLevel |= LOG_ERROR;
    }

    #endregion public methods

    // ========================================================================

    #region private methods

    /// <summary>
    /// Creates a debug/error/info message ready for dumping.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="formatString">The base message</param>
    /// <param name="cid">Stack trace info from the calling method/file.</param>
    /// <returns></returns>
    private static string CreateMessage( string? tag, string formatString, CallerID? cid )
    {
        var sb = new StringBuilder( tag ?? "" );

        if ( cid != null )
        {
            sb.Append( " : " );
            sb.Append( GetTimeStampInfo() );
            sb.Append( " : " );
            sb.Append( GetCallerInfo( ( CallerID )cid ) );
            sb.Append( " : " );
        }

        if ( !string.IsNullOrEmpty( formatString ) )
        {
            sb.Append( formatString );
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a <see cref="CallerID" /> object from the supplied file path, method and line number.
    /// </summary>
    /// <param name="callerFilePath"> The File this message was sent from. </param>
    /// <param name="callerMethod"> The Method this message was sent from. </param>
    /// <param name="callerLine"> The Line this message was sent from. </param>
    /// <returns></returns>
    private static CallerID MakeCallerID( string callerFilePath, string callerMethod, int callerLine )
    {
        return new CallerID
        {
            FileName   = Path.GetFileNameWithoutExtension( callerFilePath ),
            MethodName = callerMethod,
            LineNumber = callerLine,
        };
    }

    /// <summary>
    /// Returns a string holding the current time.
    /// </summary>
    private static string GetTimeStampInfo()
    {
        var c = new GregorianCalendar();

        return $"{c.GetHour( DateTime.Now )}"
               + $":{c.GetMinute( DateTime.Now )}"
               + $":{c.GetSecond( DateTime.Now )}"
               + $":{c.GetMilliseconds( DateTime.Now )}";
    }

    /// <summary>
    /// Returns a string holding the calling filename, method and line number.
    /// </summary>
    private static string GetCallerInfo( CallerID cid )
    {
        return $"{cid.FileName}::{cid.MethodName}::{cid.LineNumber}";
    }

    /// <summary>
    /// Returns whether the requested trace level is enabled or not.
    /// </summary>
    /// <param name="traceLevel">
    /// The trace level to check, either LOG_DEBUG, LOG_INFO, LOG_ERROR or LOG_ASSERT
    /// </param>
    /// <returns> True if the level is enabled. </returns>
    private static bool IsEnabled( int traceLevel )
    {
        return traceLevel switch
        {
            LOG_DEBUG
                or LOG_ERROR => ( TraceLevel & traceLevel ) != 0,
            var _ => false,
        };
    }

    #endregion private methods
}

// ====================================================================--------
// ====================================================================--------

/// <summary>
/// Object used for creating debug messages which include
/// the calling file and method.
/// </summary>
[StructLayout( LayoutKind.Sequential )]
internal struct CallerID
{
    public string FileName;
    public string MethodName;
    public int    LineNumber;
}