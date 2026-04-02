LUGHSHARP 2D GAME FRAMEWORK - ROUND 1
-------------------------------------

ALL CLASSES WILL BE UP FOR MODIFICATION FOLLOWING TESTING.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
<!-- TOC -->

* [LUGHSHARP 2D GAME FRAMEWORK - ROUND 1](#lughsharp-2d-game-framework---round-1)
* [ASSETS](#assets)
* [ASSETS/LOADERS](#assetsloaders)
* [ASSETS/LOADERS/RESOLVERS](#assetsloadersresolvers)
* [AUDIO](#audio)
* [AUDIO/MAPONUS ( MAPONUS is the God of Music )](#audiomaponus--maponus-is-the-god-of-music-)
* [AUDIO/MAPONUS/DECODING](#audiomaponusdecoding)
* [AUDIO/MAPONUS/DECODING/DECODERS](#audiomaponusdecodingdecoders)
* [AUDIO/MAPONUS/DECODING/DECODERS/LAYERI](#audiomaponusdecodingdecoderslayeri)
* [AUDIO/MAPONUS/DECODING/DECODERS/LAYERII](#audiomaponusdecodingdecoderslayerii)
* [AUDIO/MAPONUS/DECODING/DECODERS/LAYERIII](#audiomaponusdecodingdecoderslayeriii)
* [AUDIO/MAPONUS/IO](#audiomaponusio)
* [AUDIO/MAPONUS/SUPPORT](#audiomaponussupport)
* [AUDIO/OPENAL](#audioopenal)
* [FILES](#files)
* [GRAPHICS](#graphics)
* [GRAPHICS/ATLASES](#graphicsatlases)
* [GRAPHICS/CAMERAS](#graphicscameras)
* [GRAPHICS/FRAMEBUFFERS](#graphicsframebuffers)
* [GRAPHICS/G2D](#graphicsg2d)
* [GRAPHICS/G3D](#graphicsg3d)
* [GRAPHICS/IMAGEDECODERS](#graphicsimagedecoders)
* [GRAPHICS/OPENGL](#graphicsopengl)
* [GRAPHICS/OPENGL/BINDINGS](#graphicsopenglbindings)
* [GRAPHICS/OPENGL/ENUMS](#graphicsopenglenums)
* [GRAPHICS/SHADERS](#graphicsshaders)
* [GRAPHICS/TEXT](#graphicstext)
* [GRAPHICS/UTILS](#graphicsutils)
* [GRAPHICS/VIEWPORTS](#graphicsviewports)
* [INPUT](#input)
* [MAIN](#main)
* [MAPS](#maps)
* [MAPS/OBJECTS](#mapsobjects)
* [MAPS/TILED](#mapstiled)
* [MAPS/TILED/LOADERS](#mapstiledloaders)
* [MAPS/TILED/OBJECTS](#mapstiledobjects)
* [MAPS/TILED/RENDERERS](#mapstiledrenderers)
* [MAPS/TILED/TILES](#mapstiledtiles)
* [MATHS](#maths)
* [MATH/COLLISION](#mathcollision)
* [NETWORK](#network)
* [SCENEGRAPH2D](#scenesscene2d)
* [SCENEGRAPH2D/ACTIONS](#scenesscene2dactions)
* [SCENEGRAPH2D/LISTENERS](#scenesscene2dlisteners)
* [SCENEGRAPH2D/STYLES](#scenesscene2dstyles)
* [SCENEGRAPH2D/UI](#scenesscene2dui)
* [SCENEGRAPH2D/UTILS](#scenesscene2dutils)
* [UTILS](#utils)
* [UTILS/COLLECTIONS](#utilscollections)
* [UTILS/EXCEPTIONS](#utilsexceptions)
* [UTILS/JSON](#utilsjson)
* [UTILS/LOGGING](#utilslogging)
* [UTILS/POOLING](#utilspooling)
* [UTILS/XML](#utilsxml)

<!-- TOC -->- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- STEP 1: Complete all conversions so that the code will build.
- STEP 2: Refactor, where possible and necessary, to take advantage of C# language features.
    - Some classes can become structs / records instead.
    - Some/Most Get / Set method declarations in interfaces could become Properties.
    - switch expressions instead of switch statements where appropriate.
    - switch expressions instead of if...if/else...else where appropriate.
    - Check methods to see if they can be virtual.
    - Check and/or correct visibility of classes/methods/properties etc.
    - Use sealed classes only where strictly necessary.
    - Use of virtual for base classes/methods and classes that are likely to be extended is essential.
    - Constantly look for opportunities to improve this code.
- STEP 3: Resolve ALL remaining TODOs.
- STEP 4: Ensure code is fully documented.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Methods like **Dispose(), ToString(), Equals(), GetHashCode() ( Essentially overridden system methods )**
- should be positioned at the END of source files.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Make all thrown exceptions clearly explain what went wrong and what the user should do
  to fix the problem, where possible.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Rename class Graphics.OpenGL.OpenGL
- Sort out the mess that is GraphicsCapabilities and GraphicsDevice. Maybe merge
  them into one class? Maybe rename GraphicsDevice to something like GLContext or GLInfo?

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- NO MAGIC NUMBERS!!!
- SORT OUT VERSIONING!!!
- PRIORITY is 2D classes first

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- There seems to be different namings for width/height etc. properties and methods.
  Make it more uniform

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- IP = Conversion In Progress.
- DONE = Class finished but may not be fully 'CSHARP-ified'

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      -

ASSETS
------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AssetDescriptor
    - DONE - DONE - DONE - AssetHelper
    - DONE - DONE - DONE - AssetLoaderParameters
    - DONE - DONE - DONE - AssetLoadingTask
    - DONE - DONE - DONE - AssetManager
    - DONE - DONE - DONE - IAssetErrorListener
    - DONE - DONE - DONE - IAssetTask
    - DONE - DONE - DONE - RefCountedContainer

ASSETS/LOADERS
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AssetLoader
    - DONE - DONE - DONE - AsynchronousAssetLoader
    - DONE - DONE - DONE - BitmapFontLoader
    - DONE - DONE - DONE - CubemapLoader
    - DONE - DONE - DONE - ModelLoader
    - DONE - DONE - DONE - MusicLoader
    - DONE - DONE - DONE - ParticleEffectLoader
    - DONE - DONE - DONE - PixmapLoader
    - DONE - DONE - DONE - ShaderProgramLoader
    - DONE - DONE - DONE - SkinLoader
    - DONE - DONE - DONE - SoundLoader
    - DONE - DONE - DONE - SynchronousAssetLoader
    - DONE - DONE - DONE - TextureAtlasLoader
    - DONE - DONE - DONE - TextureLoader

ASSETS/LOADERS/RESOLVERS
------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AbsoluteFileHandleResolver
    - DONE - DONE - DONE - ClasspathFileHandleResolver
    - DONE - DONE - DONE - ExternalFileHandleResolver
    - DONE - DONE - DONE - IFileHandleResolver
    - DONE - DONE - DONE - InternalFileHandleResolver
    - DONE - DONE - DONE - LocalFileHandleResolver
    - DONE - DONE - DONE - PrefixFileHandleResolver
    - DONE - DONE - DONE - ResolutionFileResolver

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- **I'm currently considering ditching Lugh.Audio in favour of NAudio.**
- **Decision to be made asap.**

AUDIO
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - IAudio
    -      -      -      - IAudioDevice
    -      -      -      - IAudioDeviceAsync
    -      -      -      - IAudioRecorder
    -      -      -      - IMusic
    -      -      -      - ISound

AUDIO/MAPONUS ( MAPONUS is the God of Music )
---------------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Buffer16BitSterso
    -      -      -      - MP3SharpException
    -      -      -      - MP3Stream
    -      -      -      - SoundFormat

AUDIO/MAPONUS/DECODING
-----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AudioBase
    -      -      -      - BitReserve
    -      -      -      - Bitstream
    -      -      -      - BitstreamErrors
    -      -      -      - BitstreamException
    -      -      -      - CircularByteBuffer
    -      -      -      - Crc16
    -      -      -      - Decoder
    -      -      -      - DecoderParameters
    -      -      -      - DecoderErrors
    -      -      -      - DecoderException
    -      -      -      - Equalizer
    -      -      -      - Header
    -      -      -      - Huffman
    -      -      -      - OutputChannels
    -      -      -      - PushbackStream
    -      -      -      - SampleBuffer
    -      -      -      - SynthesisFilter

AUDIO/MAPONUS/DECODING/DECODERS
-----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ASubband
    -      -      -      - IFrameDecoder
    -      -      -      - LayerIDecoder
    -      -      -      - LayerIIDecoder
    -      -      -      - LayerIIIDecoder

AUDIO/MAPONUS/DECODING/DECODERS/LAYERI
---------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - SubbandLayer1
    -      -      -      - SubbandLayer1IntensityStereo
    -      -      -      - SubbandLayer1Stereo

AUDIO/MAPONUS/DECODING/DECODERS/LAYERII
----------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - SubbandLayer2
    -      -      -      - SubbandLayer2IntensityStereo
    -      -      -      - SubbandLayer2Stereo

AUDIO/MAPONUS/DECODING/DECODERS/LAYERIII
-----------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ChannelData
    -      -      -      - GranuleInfo
    -      -      -      - Layer3SideInfo
    -      -      -      - SBI
    -      -      -      - ScaleFactorData
    -      -      -      - ScaleFactorTable

AUDIO/MAPONUS/IO
-----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - RandomAccessFileStream
    -      -      -      - RiffFile
    -      -      -      - WaveFile
    -      -      -      - WaveFileBuffer

AUDIO/MAPONUS/SUPPORT
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - SupportClass

AUDIO/OPENAL
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AL
    -      -      -      - ALC

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

FILES
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - FileHandle
    - DONE - DONE - DONE - Files
    - DONE - DONE - DONE - FileService
    - DONE - DONE - DONE - IFilenameFilter
    - DONE - DONE - DONE - IFiles
    - DONE - DONE - DONE - IFileService
    - DONE - DONE - DONE - IOUtils
    - DONE - DONE - DONE - PathType
    - DONE - DONE - DONE - StreamUtils

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

GRAPHICS
--------

    Task: Streamline the image heirarchy and classes. Currently, we have:-
            - GLTexture
            - GLTextureArray
            - Pixmap
            - PixmapIO
            - PixmapData
            - Gdx2DPixmap
            - Texture
            - TextureDataFactory
            - TextureRegion
            - ETC1
            - Etc1TextureData
            - FacedCubemapData
            - FileTextureData
            - FileTextureArrayData
            - FloatTextureData
            - GLOnlyTextureData
            - ITextureData
            - ITextureArrayData
            - KTXTTextureData
            - MipMapTextureData
            - PixmapTextureData

    - Move from a "Format-First" approach to a "Role-First" approach using Abstraction and Factories.

    1. The Core Abstraction (The "Big Three")
       Instead of 20+ classes, the user should primarily interact with only three concepts:

        1. ImageSource (or Pixmap): Raw pixel data in CPU memory. This is for loading, editing, 
           and saving (e.g., Pixmap, PixmapIO, Gdx2DPixmap).
        2. Texture: Data that has been uploaded to the GPU. This is what is actually "rendered"
        3. TextureRegion: A "view" or a "crop" of a Texture. This is essential for texture
           packing and animations.

    2. Use the "Factory" Pattern for Loading
       Instead of forcing the user to know if they need a FileTextureData, KTXTTextureData, or
       ETC1, hide that logic inside a Factory.

        Before: new Texture(new FileTextureData(file)) or new Texture(new ETC1TextureData(file))
        
        After: Texture.load("image.png") or Texture.load("compressed.etc1")
        
        The factory identifies the file extension/header and internally handles the KTX, ETC1, or
        Pixmap logic without the user ever seeing those classes.

    3. Consolidate "Data" classes into an Internal Hierarchy
       There are many classes ending in TextureData. These should be internal implementation details,
       so use an Interface to collapse them:

        Interface: ITextureData

        Hidden Implementations: FileTextureData, FloatTextureData, MipMapTextureData, etc.
        Put these in an .internal or .loaders sub-folder so they don't clutter the primary API.

    4. Proposed Folder Structure
       A tidier framework would look like this:

        /graphics
            /images
                Pixmap.cs          (CPU-side pixels)
                PixmapIO.cs        (Loading/Saving Pixmaps)
                Texture.cs         (GPU-side handle)
                TextureArray.cs    (GPU-side array)
                TextureRegion.cs   (Sub-rectangles of textures)
            /loaders (Internal)
                ITextureData.cs    (The interface)
                FileLoader.cs      (Handles PNG, JPG)
                Etc1Loader.cs      (Handles ETC1)
                KtxLoader.cs       (Handles KTX)
            /gl (Low-level)
                GLTexture.cs       (Base OpenGL wrapper)
    
        Merge GLTexture and Texture: Texture can just hold the OpenGL logic directly.
        
        Hide ETC1 and KTX: These are specific compression formats. Unless you are manually building
        buffers byte-by-byte, these should be handled automatically by your Texture.load() function.
        
        Combine TextureArray logic: currently there is GLTextureArray, FileTextureArrayData, and
        ITextureArrayData. Try to unify these into a single TextureArray class that functions
        similarly to the standard Texture class.

        Summary of the "Tidy" API:
        
        - To load a sprite: Texture tex = Texture.load("player.png");
        - To get a frame: TextureRegion frame = new TextureRegion(tex, 0, 0, 32, 32);
        - To manipulate pixels: Pixmap p = new Pixmap("raw.png"); p.drawPixel(...);

        - Encapsulation: Keep the complex TextureData logic (like ETC1, KTX, or Float formats)
          behind a common interface.
        - Entry Points: Make Texture and Pixmap the primary entry points. The user shouldn't
          have to care how a file is loaded, just that they want a Texture object at the end.
        - Composition: Use TextureRegion to handle sub-images rather than creating new texture
          objects for every frame of an animation. This keeps GPU memory usage (and class list)
          much leaner.


    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - CIM
    -      -      -      - Color 
    -      -      -      - ColorWIP                 - WIP to replace Color when finished 
    - DONE - DONE - DONE - Colors
    - IP   - IP   - DONE - Cubemap
    - DONE - IP   - DONE - GLTextureArray
    - DONE - IP   - DONE - GraphicsDevice
    - DONE - IP   - DONE - GStructs
    - DONE - DONE - DONE - ICubemapData
    - DONE - DONE - DONE - ICursor
    - DONE - DONE - DONE - IGraphicsDevice
    - DONE - DONE - DONE - ITextureArrayData
    - DONE - IP   - DONE - ITextureData
    - DONE - IP   - DONE - LughFormat               ( Needs a better name )
    -      -      -      - Mesh
    - DONE - IP   - DONE - PixelFormat
    - DONE - IP   - DONE - PNG
    -      -      -      - VertexAttribute
    -      -      -      - VertexAttributes
    - DONE - IP   - DONE - VertexDataType

GRAPHICS/ATLASES
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AtlasRegion
    - DONE -      - DONE - AtlasSprite
    - DONE - DONE - DONE - TextureAtlas
    - DONE -      - DONE - TextureAtlasData

GRAPHICS/CAMERAS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Camera
    -      -      -      - CameraData
    -      -      -      - IGameCamera
    -      -      -      - OrthographicCamera
    -      -      -      - OrthographicGameCamera
    -      -      -      - PerspectiveCamera
    -      -      -      - Shake

GRAPHICS/FRAMEBUFFERS
---------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - FloatFrameBuffer
    -      -      -      - FloatFrameBufferBuilder
    -      -      -      - FrameBuffer
    -      -      -      - FrameBufferBuilder
    -      -      -      - FrameBufferConfig
    -      -      -      - FrameBufferCubemap
    -      -      -      - FrameBufferCubemapBuilder
    -      -      -      - FrameBufferRenderBufferAttachmentSpec
    -      -      -      - FrameBufferTextureAttachmentSpec
    -      -      -      - GLFrameBuffer
    -      -      -      - GLFrameBufferBuilder

GRAPHICS/G2D
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Animation
    -      -      -      - CpuSpriteBatch                  Some methods have too many parameters
    -      -      -      - IBatch
    -      -      -      - IPolygonBatch                   Some methods have too many parameters
    -      -      -      - ParticleEffect
    -      -      -      - ParticleEffectPool
    -      -      -      - ParticleEmitter
    -      -      -      - PixmapPacker
    -      -      -      - PixmapPackerIO
    -      -      -      - PolygonRegion
    -      -      -      - PolygonRegionLoader
    -      -      -      - PolygonSprite
    -      -      -      - PolygonSpriteBatch              Some methods have too many parameters
    -      -      -      - RepeatablePolygonSprite
    -      -      -      - Sprite2D
    -      -      -      - SpriteBatch
    -      -      -      - SpriteCache

GRAPHICS/G3D
------------

    See Documents/TODO_G3D.MD

GRAPHICS/IMAGEDECODERS
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BMPFormatStructs
    -      -      -      - BMPUtils
    -      -      -      - PNGDecoder
    -      -      -      - PNGFormatStructs

    ------------------------------------
    ( Possible future additions )
    -      -      -      - IImageDecoder
    -      -      -      - ImageDecoder
    -      -      -      - ImageFormat
    -      -      -      - ImageIO
    -      -      -      - ImageUtils

GRAPHICS/IMAGES
---------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Gdx2DPixmap
    -      -      -      - GLTexture
    -      -      -      - NinePatch
    - DONE - IP   - DONE - Pixmap                   y=down, x=right
    -      -      -      - PixmapData
    -      -      -      - PixmapDownloader
    -      -      -      - PixmapIO
    -      -      -      - Texture                  y=down, x=right
    -      -      -      - TextureArray
    -      -      -      - TextureDataFactory
    -      -      -      - TextureRegion            y=down, x=right

GRAPHICS/LOADERS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Etc1Loader
    -      -      -      - FileLoader
    -      -      -      - ITextureData
    -      -      -      - KtxLoader

GRAPHICS/OPENGL
---------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - DebugSeverity
    -      -      -      - GLData
    -      -      -      - GLDebugControl
    -      -      -      - GLFormatChooser
    -      -      -      - GLImage
    -      -      -      - GLUtils
    -      -      -      - IGL
    -      -      -      - IGL.GL20
    -      -      -      - IGL.GL30
    -      -      -      - IGL.GL31
    -      -      -      - IGL.GL32
    -      -      -      - OpenGLActions

GRAPHICS/OPENGL/BINDINGS
------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BufferObjectBindings
    -      -      -      - DebugBindings
    -      -      -      - DrawBindings
    -      -      -      - ErrorBindings
    -      -      -      - FrameBufferBindings
    -      -      -      - GLBindings
    -      -      -      - GLBindingsHelpers
    -      -      -      - GLBindingsStructs
    -      -      -      - GLFunctionDelegates
    -      -      -      - GLFunctionsLoader
    -      -      -      - IGLBindings
    -      -      -      - IGLBindings.GL20
    -      -      -      - IGLBindings.GL30
    -      -      -      - IGLBindings.GL31
    -      -      -      - IGLBindings.GL32
    -      -      -      - IGLBindingsExtra
    -      -      -      - ProgramBindings
    -      -      -      - QueryBindings
    -      -      -      - RenderBufferBindings
    -      -      -      - ShaderBindings
    -      -      -      - StateBindings
    -      -      -      - StencilBindings
    -      -      -      - TextureBindings
    -      -      -      - TextureSamplerBindings
    -      -      -      - TransformBindings
    -      -      -      - TransformFeedbackBindings
    -      -      -      - UniformBindings
    -      -      -      - VertexArrayBindings
    -      -      -      - VertexAttribBindings

GRAPHICS/OPENGL/ENUMS
---------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BlendMode
    -      -      -      - BufferEnums
    -      -      -      - ClearBufferMask
    -      -      -      - ClientApi
    -      -      -      - CompareFunction
    -      -      -      - ContextApi
    -      -      -      - CullFaceMode
    -      -      -      - DataType
    -      -      -      - DebugEnums
    -      -      -      - DrawElementsType
    -      -      -      - EnableCap
    -      -      -      - FrameBufferEnums
    -      -      -      - GLParameter
    -      -      -      - JoystickHats
    -      -      -      - LogicOp
    -      -      -      - MatrixMode
    -      -      -      - PixelEnums
    -      -      -      - PolygonMode
    -      -      -      - PrimitiveType
    -      -      -      - ProgramParameter
    -      -      -      - QueryTarget
    -      -      -      - ShaderEnums
    -      -      -      - StencilEnums
    -      -      -      - StringName
    -      -      -      - TextureFilterMode
    -      -      -      - TextureFormat
    -      -      -      - TextureLimit
    -      -      -      - TextureParameter
    -      -      -      - TextureTarget
    -      -      -      - TextureUnit
    -      -      -      - TextureUsage
    -      -      -      - TextureWrapMode
    -      -      -      - VertexAttribParameter
    -      -      -      - VertexAttribType

GRAPHICS/SHADERS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - ShaderLoader
    -      -      -      - ShaderProgram
    - DONE - DONE - DONE - ShaderStrings

GRAPHICS/TEXT
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BitmapFont
    -      -      -      - BitmapFontCache
    -      -      -      - BitmapFontData
    -      -      -      - CharacterUtils
    -      -      -      - DistanceFieldFont
    -      -      -      - Glyph
    -      -      -      - GlyphLayout
    -      -      -      - RegexUtils
    -      -      -      - Subset
    -      -      -      - UnicodeBlock

GRAPHICS/UTILS
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AppVersion
    -      -      -      - ETC1
    -      -      -      - ETC1TextureData
    -      -      -      - FacedCubemapData
    -      -      -      - FileTextureArrayData
    -      -      -      - FileTextureData
    -      -      -      - FloatTextureData
    -      -      -      - GLOnlyTextureData
    -      -      -      - HdpiUtils
    -      -      -      - IImmediateModeRenderer
    -      -      -      - IIndexData
    -      -      -      - IInstanceData
    -      -      -      - ImageUtils
    -      -      -      - ImmediateModeRenderer20
    -      -      -      - IndexArray
    -      -      -      - IndexBufferObject
    -      -      -      - IndexBufferObjectSubData
    -      -      -      - InstanceBufferObject
    -      -      -      - InstanceBufferObjectSubData
    -      -      -      - IVertexData
    -      -      -      - KTXTTextureData
    -      -      -      - MipMapGenerator
    -      -      -      - MipMapTextureData
    -      -      -      - PixmapTextureData
    -      -      -      - ShapeRenderer
    -      -      -      - TextureUtils
    -      -      -      - Vertex
    -      -      -      - VertexArray
    -      -      -      - VertexBufferObject
    -      -      -      - VertexBufferObjectSubData
    -      -      -      - VertexBufferObjectWithVAO
    -      -      -      - VertexConstants

GRAPHICS/VIEWPORTS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ExtendedViewport
    -      -      -      - FillViewport
    -      -      -      - FitViewport
    -      -      -      - ScalingViewport
    -      -      -      - ScreenViewport
    -      -      -      - StretchViewport
    -      -      -      - Viewport

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

INPUT
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AbstractInput
    -      -      -      - GestureDetector
    -      -      -      - IInput
    -      -      -      - IInputProcessor
    -      -      -      - InputAdapter
    -      -      -      - InputEventQueue
    -      -      -      - InputMultiplexer
    -      -      -      - InputUtils
    -      -      -      - RemoteInput
    -      -      -      - RemoteSender

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MAIN
----

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - ApplicationAdapter
    - DONE - DONE - DONE - ApplicationConfiguration
    - DONE - DONE - DONE - Engine
    - DONE - DONE - DONE - Game
    - DONE - DONE - DONE - IApplication
    - DONE - DONE - DONE - IApplicationListener
    - DONE - DONE - DONE - ILifecycleListener
    - DONE - DONE - DONE - IScreen
    - DONE - DONE - DONE - LibraryVersion
    - DONE - DONE - DONE - Platform
    - DONE - DONE - DONE - ScreenAdapter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MAPS
----

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - IImageResolver
    - DONE - DONE - DONE - IMapRenderer
    - DONE - DONE - DONE - Map
    - DONE - DONE - DONE - MapGroupLayer
    - DONE - DONE - DONE - MapLayer
    - DONE - DONE - DONE - MapLayers
    - DONE - IP   - DONE - MapObject
    - DONE - DONE - DONE - MapObjects
    - DONE - DONE - DONE - MapProperties

MAPS/OBJECTS
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - CircleMapObject
    -      -      -      - EllipseMapObject
    -      -      -      - PolygonMapObject
    -      -      -      - PolylineMapObject
    -      -      -      - RectangleMapObject
    -      -      -      - TextureMapObject

MAPS/TILED
----------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ITiledMapTile
    -      -      -      - TiledMap
    -      -      -      - TiledMapImageLayer
    -      -      -      - TiledMapTileLayer
    -      -      -      - TiledMapTileSet
    -      -      -      - TiledMapTileSets

MAPS/TILED/LOADERS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AtlasTmxMapLoader
    -      -      -      - BaseTmxMapLoader
    -      -      -      - TmxMapLoader

MAPS/TILED/OBJECTS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ImageDetails
    -      -      -      - TileContext
    -      -      -      - TiledMapTileMapObject
    -      -      -      - TileMetrics

MAPS/TILED/RENDERERS
--------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BatchTiledMapRenderer
    -      -      -      - HexagonalTiledMapRenderer
    -      -      -      - IsometricStaggeredTiledMapRenderer
    -      -      -      - IsometricTiledMapRenderer
    -      -      -      - ITiledMapRenderer
    -      -      -      - OrthoCachedTiledMapRenderer
    -      -      -      - OrthogonalTiledMapRenderer

MAPS/TILED/TILES
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AnimatedTileBuilder
    -      -      -      - AnimatedTileMapTile
    -      -      -      - StaticTileBuilder
    -      -      -      - StaticTiledMapTile

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MATHS
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Affine2
    -      -      -      - Bezier
    -      -      -      - Bresenham2
    -      -      -      - BSpline
    -      -      -      - CatmullRomSpline
    -      -      -      - Circle
    -      -      -      - ConvexHull
    -      -      -      - CumulativeDistribution
    -      -      -      - DelaunayTriangulator        Unsure about method ComputeTriangles()
    -      -      -      - EarClippingTriangulator     Needs some testing
    -      -      -      - Ellipse
    -      -      -      - FloatCounter
    -      -      -      - FloatMatrixStructs
    -      -      -      - Frustrum
    -      -      -      - GeometryUtils
    -      -      -      - GridPoint2
    -      -      -      - GridPoint3
    -      -      -      - Interpolation
    -      -      -      - Intersector
    -      -      -      - IPath
    -      -      -      - IShape2D
    -      -      -      - IVector
    -      -      -      - MathUtils
    -      -      -      - Matrix3
    -      -      -      - Matrix4
    -      -      -      - NumberUtils
    -      -      -      - Plane
    -      -      -      - Point2D
    -      -      -      - Polygon
    -      -      -      - Polyline
    -      -      -      - Quaternion
    -      -      -      - RandomXS128
    -      -      -      - Rectangle
    -      -      -      - SimpleVectors
    -      -      -      - Vector2
    -      -      -      - Vector3
    -      -      -      - Vector4
    -      -      -      - WindowedMean

MATH/COLLISION
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BoundingBox
    -      -      -      - Ray
    -      -      -      - Segment
    -      -      -      - Sphere

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MOCK/AUDIO
----------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   - IP   - DONE - MockAudio
    - IP   - IP   - DONE - MockAudioDevice
    - IP   - IP   - DONE - MockAudioRecorder
    - IP   - IP   - DONE - MockMusic
    - IP   - IP   - DONE - MockSound

MOCK/FILES
----------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   - IP   - DONE - MockFiles

MOCK/GRAPHICS
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   - IP   - DONE - MockGraphics

MOCK/INPUT
----------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   - IP   - DONE - MockInput

MOCK/MAIN
---------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   - IP   - DONE - MockApplication

MOCK/NET
--------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   - IP   - DONE - MockNet

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

NETWORK
-------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - HttpParameterUtils
    -      -      -      - HttpRequestBuilder
    -      -      -      - HttpStatus
    -      -      -      - IHttpRequestHeader
    -      -      -      - IHttpResponseHeader
    -      -      -      - INet
    -      -      -      - IServerSocket
    -      -      -      - ISocket
    -      -      -      - NetHandler
    -      -      -      - ServerSocketHints
    -      -      -      - SocketHints

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

SCENEGRAPH2D
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - Action
    - DONE - IP   - DONE - Actor
    - DONE - DONE - DONE - Event
    - DONE - IP   - DONE - Group
    - DONE - DONE - DONE - IAction
    -      -      -      - IActor       - Add members or remove
    -      -      -      - InputEvent
    -      -      -      - Stage
    -      -      -      - Touchable

SCENEGRAPH2D/ACTIONS
--------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Actions
    -      -      -      - AddAction
    -      -      -      - AddListenerAction
    -      -      -      - AfterAction
    -      -      -      - AlphaAction
    -      -      -      - ColorAction
    -      -      -      - CountdownEventAction
    -      -      -      - DelayAction
    -      -      -      - DelegateAction
    -      -      -      - EventAction
    -      -      -      - FloatAction
    -      -      -      - IntAction
    -      -      -      - LayoutAction
    -      -      -      - MoveByAction
    -      -      -      - MoveToAction
    -      -      -      - ParallelAction
    -      -      -      - RelativeTemporalAction
    -      -      -      - RemoveAction
    -      -      -      - RemoveActorAction
    -      -      -      - RemoveListenerAction
    -      -      -      - RepeatAction
    -      -      -      - RotateByAction
    -      -      -      - RotateToAction
    -      -      -      - RunnableAction
    -      -      -      - ScaleByAction
    -      -      -      - ScaleToAction
    -      -      -      - SequenceAction
    -      -      -      - SizeByAction
    -      -      -      - SizeToAction
    -      -      -      - TemporalAction
    -      -      -      - TimeScaleAction
    -      -      -      - TouchableAction
    -      -      -      - VisibleAction

SCENEGRAPH2D/LISTENERS
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ActorGestureListener
    -      -      -      - ChangeListener
    -      -      -      - ClickListener
    -      -      -      - DialogChangeListener
    -      -      -      - DialogFocusListener
    -      -      -      - DialogInputListener
    -      -      -      - DragListener
    -      -      -      - DragScrollListener
    -      -      -      - FocusListener
    -      -      -      - IEventListener
    -      -      -      - InputListener
    -      -      -      - ScrollPaneListeners

```
TODO: Use Lambdas for these, i.e.

AddListener( new ClickListener()
{
    // Clicked needs to be a Func<>
    Clicked = ( ev, x, y ) =>
    {
    }
} );
```

SCENEGRAPH2D/STYLES
-------------------

( Styles to use with StyleRegistry / StyleFactory )

    CODE   DOCU   FOOTER
    ----   ----   ------
    - IP   -      - DONE - ButtonStyleRecord
    -      -      - DONE - CheckBoxStyleRecord
    -      -      - DONE - ImageButtonStyleRecord
    -      -      - DONE - ImageTextButtonStyleRecord
    -      -      - DONE - LabelStyleRecord
    -      -      - DONE - ListBoxStyleRecord
    -      -      - DONE - ProgressBarStyleRecord
    -      -      - DONE - ScrollPaneStyleRecord
    -      -      - DONE - SelectBoxStyleRecord
    -      -      - DONE - SliderStyleRecord
    -      -      - DONE - SplitPaneStyleRecord
    - IP   -      - DONE - StyleFactory
    - IP   -      - DONE - StyleRegistry
    -      -      - DONE - TextButtonStyleRecord
    -      -      - DONE - TextFieldStyleRecord
    -      -      - DONE - TextTooltipStyleRecord
    -      -      - DONE - TouchpadStyleRecord
    -      -      - DONE - TreeStyleRecord
    -      -      - DONE - WindowStyleRecord

SCENEGRAPH2D/UI
---------------

    TODO: I don't like the way Cell and Value classes are implemented.
          They seem confusing and are candidates for a rewrite.

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - Button                   - Draws, but not clickable yet.
    -      -      -      - ButtonClickListener      - 
    -      -      -      - ButtonGroup              - 
    -      -      -      - Cell                     - 
    - IP   - IP   - DONE - CheckBox                 - 
    -      -      -      - Container                - 
    -      -      -      - Dialog                   - 
    -      -      -      - HorizontalGroup          - 
    - IP   - IP   - DONE - ImageButton              - 
    - IP   - IP   - DONE - ImageTextButton          - 
    -      -      -      - IOnScreenKeyboard        - 
    -      -      -      - Label                    - 
    -      -      -      - ListBox                  - 
    -      -      -      - ParticleEffectActor      - 
    -      -      -      - ProgressBar              - 
    -      -      -      - Scene2DImage             - 
    -      -      -      - ScrollPane               - 
    -      -      -      - SelectBox                - 
    -      -      -      - Skin                     - 
    -      -      -      - Slider                   - 
    -      -      -      - SplitPane                - 
    -      -      -      - Stack                    - 
    -      -      -      - Table                    - 
    -      -      -      - TextArea                 - 
    - IP   - IP   - DONE - TextButton               - Text drawing at wrong coords
    -      -      -      - TextField                - 
    -      -      -      - TextTooltip              - 
    -      -      -      - Tooltip                  - 
    -      -      -      - TooltipManager           - 
    -      -      -      - Touchpad                 - 
    -      -      -      - Tree                     - 
    -      -      -      - Value                    - 
    -      -      -      - VerticalGroup            - 
    -      -      -      - Widget                   - 
    -      -      -      - WidgetGroup              - 
    -      -      -      - Window                   - 

SCENEGRAPH2D/UI/STYLES
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      - DONE - ButtonStyle
    -      -      - DONE - CheckBoxStyle
    -      -      - DONE - ImageButtonStyle
    -      -      - DONE - ImageTextButtonStyle
    -      -      - DONE - ISceneStyle
    - DONE - DONE - DONE - LabelStyle
    -      -      - DONE - ListBoxStyle
    -      -      - DONE - ProgressBarStyle
    -      -      - DONE - ScrollPaneStyle
    -      -      - DONE - SelectBoxStyle
    -      -      - DONE - SliderStyle
    -      -      - DONE - SplitPaneStyle
    -      -      - DONE - TextButtonStyle
    -      -      - DONE - TextFieldStyle
    -      -      - DONE - TextTooltipStyle
    -      -      - DONE - TouchpadStyle
    -      -      - DONE - TreeStyle
    -      -      - DONE - WindowStyle

SCENEGRAPH2D/UTILS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ArraySelection
    -      -      -      - BaseDrawable
    -      -      -      - DragAndDrop
    -      -      -      - ICullable
    -      -      -      - IDisableable
    -      -      -      - ILayout
    -      -      -      - ISceneDrawable
    -      -      -      - ITransformDrawable
    -      -      -      - NinePatchDrawable
    -      -      -      - ScissorStack
    -      -      -      - Selection
    -      -      -      - SpriteDrawable
    -      -      -      - TextureRegionDrawable
    -      -      -      - TiledDrawable

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

UTILS
-----

    - Move Utils/Collections out of Utils and into somewhere more appropriate.

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - ActorDefinitionAttribute
    - DONE - DONE - DONE - Align
    - DONE - IP   - DONE - AsyncExecutor
    - DONE - IP   - DONE - AsyncResult
    - DONE - DONE - DONE - BaseClassFactory
    - DONE - DONE - DONE - BinaryHeap
    - DONE - DONE - DONE - Bits
    - DONE - IP   - DONE - Buffer<T>
    - DONE - IP   - DONE - BufferUtils
    - DONE - IP   - DONE - ByteOrder
    - DONE - IP   - DONE - BytePointerToString
    - DONE - IP   - DONE - CaseInsensitiveEnumArrayConverterFactory
    -      -      -      - ComparableTimSort
    - DONE - DONE - DONE - DataOutput
    - DONE - DONE - DONE - DataUtils
    -      -      -      - GCSuppressor
    - DONE - DONE - DONE - HashHelpers
    -      -      -      - IAsyncTask
    -      -      -      - IClipboard
    -      -      -      - ICloseable
    -      -      -      - IDrawable            Conflicts with Scene2D.Utils.IDrawable
    -      -      -      - IManaged
    -      -      -      - IReadable
    -      -      -      - IResetable
    -      -      -      - IRunnable
    -      -      -      - MinimalCrc32
    -      -      -      - PerformanceCounter
    -      -      -      - PerformanceCounters
    -      -      -      - PropertiesUtils
    -      -      -      - QuadTreeFloat
    -      -      -      - QuickSelect
    -      -      -      - Scaling
    -      -      -      - ScreenUtils
    -      -      -      - Selector
    -      -      -      - SortUtils
    -      -      -      - StreamUtils
    -      -      -      - StringUtils
    -      -      -      - SystemArrayUtils
    -      -      -      - Timer
    -      -      -      - TimeUtils
    -      -      -      - TimSort

UTILS/COLLECTIONS
-----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - IP   - DONE - ArrayList
    - DONE - IP   - DONE - ByteArray
    - DONE - DONE - DONE - Collections
    - DONE - DONE - DONE - DelayedRemovalList
    - DONE - DONE - DONE - DictionaryExtensions
    - DONE - DONE - DONE - DirectoryInfoComparer
    - DONE - DONE - DONE - IPredicate
    - DONE - IP   - DONE - LinkedHashMap
    - DONE - DONE - DONE - ListExtensions
    - IP   - IP   - DONE - ObjectMap                Look into removing this.
    - IP   - IP   - DONE - OrderedMap               Look into removing this.
    - IP   - IP   - DONE - PredicateIterable        Can be removed if ArrayList is removed.
    - IP   - IP   - DONE - PredicateIterator        Can be removed if ArrayList is removed.
    - DONE - IP   - DONE - ResettableStack
    - DONE - DONE - DONE - SnapshotArrayList

UTILS/EXCEPTIONS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AssetNotLoadedException
    - DONE - DONE - DONE - Guard
    - DONE - DONE - DONE - ListenerFailureException
    - DONE - DONE - DONE - RuntimeException
    - DONE - DONE - DONE - SerializationException

UTILS/JSON
----------

    ///////////////////////////////////////////////
    ALL JSON CLASSES NEED FULLY TESTING!!!
    ///////////////////////////////////////////////

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - IP   - DONE - Json
    - DONE -      - DONE - JsonFieldAttribute
    - IP   -      - DONE - JsonMatcher          May not be needed / To Be Removed
    - DONE -      - DONE - JsonNameAttribute
    - DONE - IP   - DONE - JsonOutput
    - DONE - DONE - DONE - JsonOutputType
    - IP   -      - DONE - JsonReader
    - IP   -      - DONE - JsonSkimmer          May not be needed / To Be Removed
    - DONE - IP   - DONE - JsonString
    - DONE - IP   - DONE - JsonValue
    - DONE -      - DONE - JsonWriter

UTILS/LOGGING
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - FPSLogger
    - DONE - DONE - DONE - IPreferences
    - DONE - DONE - DONE - Logger
    - DONE - DONE - DONE - Preferences
    - DONE - DONE - DONE - Stats

UTILS/POOLING
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - FlushablePool
    - DONE - DONE - DONE - IPoolable
    - DONE - DONE - DONE - Pool
    - DONE - DONE - DONE - PooledLinkedList
    - DONE - DONE - DONE - Pools

UTILS/XML
---------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - XmlReader
    - DONE - DONE - DONE - XmlWriter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

