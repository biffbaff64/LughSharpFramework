// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using Environment = System.Environment;

namespace LughSharp.Source.Config;

/// <summary>
/// Platform specific flags and methods.
/// </summary>
/// <seealso cref="GraphicsDevice"/>
/// <seealso cref="LibraryVersion"/>
/// <seealso cref="ApplicationConfiguration"/>
[PublicAPI]
public static class Platform
{
    public static bool IsWindows { get; private set; } = RuntimeInformation.IsOSPlatform( OSPlatform.Windows );
    public static bool Is64Bit   { get; private set; } = Environment.Is64BitOperatingSystem;
    public static bool IsLinux   { get; private set; } = RuntimeInformation.IsOSPlatform( OSPlatform.Linux );
    public static bool IsMac     { get; private set; } = RuntimeInformation.IsOSPlatform( OSPlatform.OSX );
    public static bool IsArm     { get; private set; } = IsArmArchitecture();

    // ========================================================================

    public static bool IsIos     { get; private set; } //TODO: For the future, concentrating on desktop for now. 
    public static bool IsAndroid { get; private set; } //TODO: For the future, concentrating on desktop for now.
    public static bool IsXBox    { get; private set; } //TODO: For the future, concentrating on desktop for now.

    // ========================================================================

    /// <summary>
    /// The target platform for the app.
    /// Must be a member of the enum <see cref="ApplicationType"/>
    /// </summary>
    /// <exception cref="LughRuntimeException">
    /// Thrown if the target platform is undefined ( get operation ), or
    /// if an invalid target is provided ( set operation ).
    /// </exception>
    public static ApplicationType TargetPlatform
    {
        get
        {
            if ( ValidApplicationType( field ) )
            {
                return field;
            }

            throw new LughRuntimeException( $"Undefined Target Platform: {field.ToString()}" );
        }
        set
        {
            if ( ValidApplicationType( value ) )
            {
                field = value;
            }
            else
            {
                throw new LughRuntimeException( $"Unsupported Target Platform: {value.ToString()}" );
            }
        }
    }

    /// <summary>
    /// Returns <c>true</c> if the provided type is a member of the
    /// <see cref="ApplicationType"/> enum.
    /// </summary>
    private static bool ValidApplicationType( ApplicationType type )
    {
        return type switch
               {
                   ApplicationType.Android
                    or ApplicationType.IOS
                    or ApplicationType.MacOs
                    or ApplicationType.NintendoSwitch
                    or ApplicationType.PlayStation4
                    or ApplicationType.PlayStation5
                    or ApplicationType.WebGL
                    or ApplicationType.Windows
                    or ApplicationType.WindowsGdk
                    or ApplicationType.WindowsGL
                    or ApplicationType.WindowsGles
                    or ApplicationType.WindowsVk
                    or ApplicationType.XBoxOne
                    or ApplicationType.XBoxSeries => true,

                   // ---------------------------

                   var _ => false,
               };

//        return Enum.IsDefined( typeof( ApplicationType ), type );
    }

    /// <summary>
    /// The target family group (mobile, console, desktop etc).
    /// Must be one of the enum <see cref="PlatformFamily"/>.
    /// </summary>
    /// <exception cref="LughRuntimeException"></exception>
    public static PlatformFamily PlatformFamilyGroup
    {
        get;
        set
        {
            if ( ( value != PlatformFamily.Unknown )
              && Enum.IsDefined( typeof( PlatformFamily ), value ) )
            {
                field = value;
            }
            else
            {
                throw new LughRuntimeException( $"Unsupported Platform Family Group: {value.ToString()}" );
            }
        }
    }

    /// <summary>
    /// Returns TRUE if the OS architecture is ARM based.
    /// </summary>
    public static bool IsArmArchitecture()
    {
        return RuntimeInformation.OSArchitecture switch
               {
                   Architecture.Arm   => true,
                   Architecture.Arm64 => true,

                   // ----------------------------------

                   var _ => false
               };
    }

    /// <summary>
    /// Returns a string representation of a new GUID structure.
    /// </summary>
    public static string RandomUUID()
    {
        return Guid.NewGuid().ToString();
    }
}

// ============================================================================
// ============================================================================
