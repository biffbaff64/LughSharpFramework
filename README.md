
( Heavily Work In Progress )


C# 2D Game Framework project inspired by, but not a direct copy of, the Java LibGDX Game Framework.#
The original reason for this project was to learn C# and to gain experience with C#, as my exposure
that language, and OOP in general was very limited ( I've spent most of my career with C and Assembly
languages ).

I realise and accept that the code within this project is not the best, and that it is not production
ready. I'm working on it, and I'm hoping to release a version 1.0.0 at some point.


COMPLETED:
----------

- Asset Management.
- TexturePacker support.
- OpenGL 2D Graphics.
- TiledMap support.
- BitmapFont support.
- Json handling to support Scene2D Skins.
- Ninepatch support
- Sprite2D animations.
- ImagePacker
- Input System.
- TiledMap Animated Tiles.
- Sprite Scrolling.
- Update Logger.Debug and Logger.Error to acceot NULL messages and report them as such.


IN PROGRESS:
------------

- Audio needs testing and/or finishing.
- TiledMapPacker support.
- Scene2D UI is not yet complete.
      1. I'm currently testing the UI Actors and their functionality.
      2. I'm working on adding StyleRegistry and StyleFactory classes to enable
         creation and use of Scene2D UI Actors without the need for Json Skin files,
         although support for Json Skins will still be retained.

        - Button              - Done
        - CheckBox            - Done
        - Dialog              - 
        - ImageButton         - Done
        - ImageTextButton     - Done
        - Label               - Done
        - ListBox             - Done
        - ProgressBar         - Done
        - Scene2DImage        - Done
        - ScrollPane          - Done
        - SelectBox           - 
        - Slider              - Done
        - SplitPane           - 
        - Table               - 
        - TextArea            - Done
        - TextButton          - Done
        - TextField           - Done
        - TextTooltip         - 
        - Tooltip             - 
        - Touchpad            - 
        - Tree                - 
        - Window              - 

- Something for consideration:

  All Styles should provide a fully defined default style. This will remove any possibility
  of problems with the default style not being defined..


TODO:
-----

- Box2D Physics support.
- 3D Graphics needs testing and/or finishing.
- 2D Particle System needs testing and/or finishing.
- Distance Field Font generation.

TESTS NEEDED:
-------------

- Maths
- Scene2D
- Network
- TiledMap
- Audio
- Asset Management
- Collections
- Input
- Scene2D UI
- Particle Effects
- 

FUTURE UPDATES:
---------------

- Migrate to EventHandler based event system for Scene2D. This is a BIG task and will require a lot of work.
- Use NUnit for testing properly.
- Freetype Font Support
- SpriteFont support.
- Add AnimatedTileBuilder to TiledMap support.
- Network / HTTP etc.


-------------------------------------------------------------------------------
-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

ref (method parameters)
-----------------------

- pass by reference. Variable passed must be initialised first. (check other uses of ref).
- the method MAY modify parameter.

out (method parameters)
-----------------------

- Like ref but variable does not need to be initialised.
- the method MUST modify parameter.

in (method parameters)
----------------------

- pass by reference but disable modification.
- the method CANNOT modify parameter.


