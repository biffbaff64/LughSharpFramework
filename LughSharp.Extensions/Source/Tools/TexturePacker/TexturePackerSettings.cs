﻿using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Utils;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
public class TexturePackerSettings
{
    /// <summary>
    /// 
    /// </summary>
    public bool MultipleOfFour { get; set; }

    /// <summary>
    /// If true, TexturePacker will attempt more efficient packing by rotating
    /// images 90 degrees. Applications must take special care to draw these
    /// regions properly.
    /// </summary>
    public bool Rotation { get; set; }

    /// <summary>
    /// If true, output pages will have power of two dimensions.
    /// </summary>
    public bool PowerOfTwo { get; set; }

    /// <summary>
    /// The number of pixels between packed images on the x-axis
    /// </summary>
    public int PaddingX { get; set; }

    /// <summary>
    /// The number of pixels between packed images on the y-axis
    /// </summary>
    public int PaddingY { get; set; }

    /// <summary>
    /// If true, half of the paddingX and paddingY will be used around the edges
    /// of the packed texture.
    /// </summary>
    public bool EdgePadding { get; set; }

    /// <summary>
    /// If true, edge pixels are copied into the padding. paddingX/Y should be >= 2.
    /// </summary>
    public bool DuplicatePadding { get; set; }

    /// <summary>
    /// The minimum width of output pages.
    /// </summary>
    public int MinWidth { get; set; }

    /// <summary>
    /// The minimum height of output pages.
    /// </summary>
    public int MinHeight { get; set; }

    /// <summary>
    /// The maximum width of output pages.
    /// </summary>
    public int MaxWidth { get; set; }

    /// <summary>
    /// The maximum height of output pages.
    /// </summary>
    public int MaxHeight { get; set; }

    /// <summary>
    /// If true, output pages are forced to have the same width and height.
    /// </summary>
    public bool Square { get; set; }

    /// <summary>
    /// If true, blank pixels on the left and right edges of input images will
    /// be removed. Applications must take special care to draw these regions
    /// properly.
    /// </summary>
    public bool StripWhitespaceX { get; set; }

    /// <summary>
    /// If true, blank pixels on the top and bottom edges of input images will
    /// be removed. Applications must take special care to draw these regions
    /// properly.
    /// </summary>
    public bool StripWhitespaceY { get; set; }

    /// <summary>
    /// From 0 to 255. Alpha values below this are treated as zero when whitespace
    /// is stripped.
    /// </summary>
    public int AlphaThreshold { get; set; }

    /// <summary>
    /// The minification filter for the texture.
    /// </summary>
    public TextureFilterMode FilterMin { get; set; }

    /// <summary>
    /// The magnification filter for the texture.
    /// </summary>
    public TextureFilterMode FilterMag { get; set; }

    /// <summary>
    /// The wrap setting in the x direction for the texture.
    /// </summary>
    public TextureWrapMode WrapX { get; set; }

    /// <summary>
    /// The wrap setting in the y direction for the texture.
    /// </summary>
    public TextureWrapMode WrapY { get; set; }

    /// <summary>
    /// The <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/> the texture will use in-memory.
    /// </summary>
    public Gdx2DPixmap.Gdx2DPixmapFormat Format { get; set; }

    /// <summary>
    /// If true, two images that are pixel for pixel the same will only be packed once.
    /// </summary>
    public bool IsAlias { get; set; }

    /// <summary>
    /// The image type for output pages, “png” or “jpg”.
    /// ".bmp" is planned.
    /// </summary>
    public string OutputFormat { get; set; }

    /// <summary>
    /// From 0 to 1. The quality setting if outputFormat is “jpg”
    /// </summary>
    public float JpegQuality { get; set; }

    /// <summary>
    /// If true, texture packer won’t add regions for completely blank images
    /// </summary>
    public bool IgnoreBlankImages { get; set; }

    /// <summary>
    /// If true, the texture packer will not pack as efficiently but will execute
    /// much faster.
    /// </summary>
    public bool Fast { get; set; }

    /// <summary>
    /// If true, lines are drawn on the output pages to show the packed image bounds.
    /// </summary>
    public bool Debug { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Silent { get; set; }

    /// <summary>
    /// If true, the directory containing the settings file and all subdirectories are
    /// packed as if they were in the same directory.
    /// Any settings files in the subdirectories are ignored.
    /// </summary>
    public bool CombineSubdirectories { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// If true, subdirectory prefixes are stripped from image file names.
    /// Image file names should be unique.
    /// </summary>
    public bool FlattenPaths { get; set; }

    /// <summary>
    /// If true, the RGB will be multiplied by the alpha.
    /// </summary>
    public bool PremultiplyAlpha { get; set; }

    /// <summary>
    /// If false, image names are used without stripping any image index suffix.
    /// </summary>
    public bool UseIndexes { get; set; }

    /// <summary>
    /// If true, RGB values for transparent pixels are set based on the RGB values of the
    /// nearest non-transparent pixels. This prevents filtering artifacts when RGB values
    /// are sampled for transparent pixels.
    /// </summary>
    public bool Bleed { get; set; }

    /// <summary>
    /// The amount of bleed iterations that should be performed. Use greater values such
    /// as 4 or 8 if you’re having artifacts when downscaling your textures.
    /// </summary>
    public int BleedIterations { get; set; }

    /// <summary>
    /// If true, only one image is in memory at any given time, but each image will be read
    /// twice. If false, all images are kept in memory during packing but are only read once.
    /// </summary>
    public bool LimitMemory { get; set; }

    /// <summary>
    /// If true, images are packed in a uniform grid, in order.
    /// </summary>
    public bool Grid { get; set; }

    /// <summary>
    /// For each scale, the images are scaled and an entire atlas is output.
    /// </summary>
    public float[] Scale { get; set; }

    /// <summary>
    /// For each scale, the suffix to use for the output files. If omitted, files for multiple
    /// scales will be output with the same name to a subdirectory for each scale.
    /// </summary>
    public string[] ScaleSuffix { get; set; }

    /// <summary>
    /// For each scale, the type of interpolation used for resampling the source to the scaled
    /// size. One of nearest, bilinear or bicubic.
    /// </summary>
    public List< Resampling > ScaleResampling { get; set; }

    /// <summary>
    /// The file extension to be appended to the atlas filename.
    /// </summary>
    public string AtlasExtension { get; set; }

    /// <summary>
    /// If true, removes all whitespace except newlines.
    /// </summary>
    public bool PrettyPrint { get; set; }

    /// <summary>
    /// If true, the atlas uses a less efficient output format. Exists for backwards-compatibility
    /// reasons.
    /// </summary>
    public bool LegacyOutput { get; set; }

    /// <summary>
    /// If true, texture packer will clear out the output folder before processing.
    /// This is a DEBUG setting only.
    /// </summary>
    #if DEBUG
    public bool CleanAtStart { get; set; }
    #endif
    
    // ====================================================================

    private JsonSerializerOptions _defaultJsonSerializerOptions = new()
    {
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented       = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        },
    };

    // ====================================================================

    /// <summary>
    /// Default constructor
    /// </summary>
    public TexturePackerSettings()
    {
        MinWidth              = 16;
        MinHeight             = 16;
        MaxWidth              = 1024;
        MaxHeight             = 1024;
        Fast                  = false;
        Rotation              = false;
        PowerOfTwo            = true;
        MultipleOfFour        = false;
        PaddingX              = 2;
        PaddingY              = 2;
        EdgePadding           = true;
        DuplicatePadding      = false;
        AlphaThreshold        = 0;
        IgnoreBlankImages     = true;
        StripWhitespaceX      = false;
        StripWhitespaceY      = false;
        IsAlias               = true;
        Format                = Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888;
        JpegQuality           = 1.0f;
        OutputFormat          = "png";
        FilterMin             = TextureFilterMode.Nearest;
        FilterMag             = TextureFilterMode.Nearest;
        WrapX                 = TextureWrapMode.ClampToEdge;
        WrapY                 = TextureWrapMode.ClampToEdge;
        Debug                 = false;
        Silent                = true;
        CombineSubdirectories = false;
        Ignore                = false;
        FlattenPaths          = false;
        PremultiplyAlpha      = false;
        Square                = false;
        UseIndexes            = true;
        Bleed                 = true;
        BleedIterations       = 2;
        LimitMemory           = true;
        Grid                  = false;
        AtlasExtension        = ".atlas";
        PrettyPrint           = true;
        LegacyOutput          = true;
        Scale                 = [ 1.0f ];
        ScaleSuffix           = [ "" ];
        ScaleResampling       = [ Resampling.Bicubic ];
        
        #if DEBUG
        CleanAtStart = false;
        #endif
    }

    public TexturePackerSettings( TexturePackerSettings settings )
    {
        MinWidth              = settings.MinWidth;
        MinHeight             = settings.MinHeight;
        MaxWidth              = settings.MaxWidth;
        MaxHeight             = settings.MaxHeight;
        Fast                  = settings.Fast;
        Rotation              = settings.Rotation;
        PowerOfTwo            = settings.PowerOfTwo;
        MultipleOfFour        = settings.MultipleOfFour;
        PaddingX              = settings.PaddingX;
        PaddingY              = settings.PaddingY;
        EdgePadding           = settings.EdgePadding;
        DuplicatePadding      = settings.DuplicatePadding;
        AlphaThreshold        = settings.AlphaThreshold;
        IgnoreBlankImages     = settings.IgnoreBlankImages;
        StripWhitespaceX      = settings.StripWhitespaceX;
        StripWhitespaceY      = settings.StripWhitespaceY;
        IsAlias               = settings.IsAlias;
        Format                = settings.Format;
        JpegQuality           = settings.JpegQuality;
        OutputFormat          = settings.OutputFormat;
        FilterMin             = TextureFilterMode.Nearest;
        FilterMag             = TextureFilterMode.Nearest;
        WrapX                 = TextureWrapMode.ClampToEdge;
        WrapY                 = TextureWrapMode.ClampToEdge;
        Debug                 = settings.Debug;
        Silent                = settings.Silent;
        CombineSubdirectories = settings.CombineSubdirectories;
        Ignore                = settings.Ignore;
        FlattenPaths          = settings.FlattenPaths;
        PremultiplyAlpha      = settings.PremultiplyAlpha;
        Square                = settings.Square;
        UseIndexes            = settings.UseIndexes;
        Bleed                 = settings.Bleed;
        BleedIterations       = settings.BleedIterations;
        LimitMemory           = settings.LimitMemory;
        Grid                  = settings.Grid;
        AtlasExtension        = settings.AtlasExtension;
        PrettyPrint           = settings.PrettyPrint;
        LegacyOutput          = settings.LegacyOutput;
        Scale                 = [ 1.0f ];
        ScaleSuffix           = [ "" ];
        ScaleResampling       = [ Resampling.Bicubic ];
        
        #if DEBUG
        CleanAtStart = false;
        #endif
    }

    // ====================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    public void Set( TexturePackerSettings settings )
    {
        MinWidth              = settings.MinWidth;
        MinHeight             = settings.MinHeight;
        MaxWidth              = settings.MaxWidth;
        MaxHeight             = settings.MaxHeight;
        Fast                  = settings.Fast;
        Rotation              = settings.Rotation;
        PowerOfTwo            = settings.PowerOfTwo;
        MultipleOfFour        = settings.MultipleOfFour;
        PaddingX              = settings.PaddingX;
        PaddingY              = settings.PaddingY;
        EdgePadding           = settings.EdgePadding;
        DuplicatePadding      = settings.DuplicatePadding;
        AlphaThreshold        = settings.AlphaThreshold;
        IgnoreBlankImages     = settings.IgnoreBlankImages;
        StripWhitespaceX      = settings.StripWhitespaceX;
        StripWhitespaceY      = settings.StripWhitespaceY;
        IsAlias               = settings.IsAlias;
        Format                = settings.Format;
        JpegQuality           = settings.JpegQuality;
        OutputFormat          = settings.OutputFormat;
        FilterMin             = settings.FilterMin;
        FilterMag             = settings.FilterMag;
        WrapX                 = settings.WrapX;
        WrapY                 = settings.WrapY;
        Debug                 = settings.Debug;
        Silent                = settings.Silent;
        CombineSubdirectories = settings.CombineSubdirectories;
        Ignore                = settings.Ignore;
        FlattenPaths          = settings.FlattenPaths;
        PremultiplyAlpha      = settings.PremultiplyAlpha;
        Square                = settings.Square;
        UseIndexes            = settings.UseIndexes;
        Bleed                 = settings.Bleed;
        BleedIterations       = settings.BleedIterations;
        LimitMemory           = settings.LimitMemory;
        Grid                  = settings.Grid;
        AtlasExtension        = settings.AtlasExtension;
        PrettyPrint           = settings.PrettyPrint;
        LegacyOutput          = settings.LegacyOutput;

        settings.Scale.CopyTo( Scale, 0 );
        settings.ScaleSuffix.CopyTo( ScaleSuffix, 0 );
        ScaleResampling = settings.ScaleResampling.ToList();
        
        #if DEBUG
        CleanAtStart = false;
        #endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packFileName"></param>
    /// <param name="scaleIndex"></param>
    /// <returns></returns>
    public string GetScaledPackFileName( string packFileName, int scaleIndex )
    {
        // Use suffix if not empty string.
        if ( ScaleSuffix[ scaleIndex ].Length > 0 )
        {
            packFileName += ScaleSuffix[ scaleIndex ];
        }
        else
        {
            // Otherwise if scale != 1 or multiple scales, use subdirectory.
            var scaleValue = Scale[ scaleIndex ];

            if ( Scale.Length != 1 )
            {
                packFileName = ( ( scaleValue % 1 ) == 0f ? $"{( int )scaleValue}" : $"{scaleValue}" )
                               + "/"
                               + packFileName;
            }
        }

        return packFileName;
    }

    /// <summary>
    /// Merges the supplied Settings instance with this one.
    /// </summary>
    public void Merge( TexturePackerSettings? source )
    {
        if ( source == null )
        {
            return;
        }

        var properties = typeof( TexturePackerSettings ).GetProperties( BindingFlags.Public
                                                                        | BindingFlags.Instance );

        foreach ( var property in properties )
        {
            switch ( property )
            {
                case { CanRead: true, CanWrite: true } when property.PropertyType == typeof( bool ):
                {
                    var sourceValue = ( bool? )property.GetValue( source ); // Treat as nullable to check if set
                    var targetValue = ( bool? )property.GetValue( this );

                    // Update if the source value has any value (true or false)
                    if ( sourceValue.HasValue && ( sourceValue.Value != targetValue ) )
                    {
                        property.SetValue( this, sourceValue.Value );
                    }

                    break;
                }

                // Handle other property types as needed
                case { CanRead: true, CanWrite: true } when property.PropertyType != typeof( bool ):
                {
                    var sourceValue = property.GetValue( source );

                    if ( sourceValue != null )
                    {
                        property.SetValue( this, sourceValue );
                    }

                    break;
                }
            }
        }
    }

    /// <summary>
    /// Read Json from the supplied file and return as a Settings object.
    /// The value return may be NULL.
    /// </summary>
    public TexturePackerSettings? ReadFromJsonFile( FileInfo settingsFile )
    {
        var jsonString = File.ReadAllText( settingsFile.FullName );

        return JsonSerializer.Deserialize< TexturePackerSettings >( jsonString, _defaultJsonSerializerOptions );
    }

    /// <summary>
    /// Writes this Settings object to a Json string and saves that string
    /// to a .json file with the specified name.
    /// </summary>
    /// <param name="fileName"></param>
    public void WriteToJsonFile( string fileName )
    {
        var jsonString      = WriteToJsonString();
        var writeableString = new UTF8Encoding( true ).GetBytes( jsonString );

        if ( writeableString.Length > 0 )
        {
            // Ensure the directory exists before creating the file
            var directoryPath = Path.GetDirectoryName( fileName );

            if ( !string.IsNullOrEmpty( directoryPath ) )
            {
                Directory.CreateDirectory( directoryPath );
            }

            using ( var fs = File.Create( fileName ) )
            {
                fs.Write( writeableString );
            }
        }
    }

    /// <summary>
    /// Returns this Settings object as a Json string.
    /// </summary>
    /// <returns></returns>
    public string WriteToJsonString()
    {
        return JsonSerializer.Serialize( this, _defaultJsonSerializerOptions );
    }
}

// ============================================================================
// ============================================================================