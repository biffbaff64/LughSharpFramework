
C# 2D Game Framework project inspired by, but not a direct copy of, the Java LibGDX Game Framework.

( Heavily Work In Progress )

WORKING:

- Asset Management.
- TexturePacker support.
- OpenGL 2D Graphics.
- TiledMap support.
- BitmapFont support.
- Json handling to support Scene2D Skins.
- Ninepatch support
- Sprite2D animations.
- ImagePacker
- Input System works.
- TiledMap Animated Tiles work.
- Sprite Scrolling.

IN PROGRESS:

- Audio needs testing and/or finishing.
- TiledMapPacker support.
  - Scene2D UI is not yet complete.
        1. I'm currently testing the UI Actors and their functionality.
        2. I'm currently working on adding StyleRegistry and StyleFactory classes to enable
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

  - Scene2D UI Actor Styles must initialise all properties.
    
          - ButtonStyle            - Done
          - CheckBoxStyle          - Done
          - DialogStyle            - Done
          - ImageButtonStyle       - Done
          - ImageTextButtonStyle   - Done
          - LabelStyle             - Done
          - ListBoxStyle           - Done
          - ProgressBarStyle       - Done
          - ScrollPaneStyle        - Done
          - SelectBoxStyle         - Done
          - SliderStyle            - Done
          - SplitPaneStyle         - Done
          - TextAreaStyle          - Done 
          - TextButtonStyle        - Done
          - TextFieldStyle         - Done
          - TextTooltipStyle       - Done
          - TouchpadStyle          - Done
          - TreeStyle              - Done
          - WindowStyle            - Done

          - ButtonStyleRecord            - 
          - CheckBoxStyleRecord          - 
          - DialogStyleRecord            - 
          - ImageButtonStyleRecord       - 
          - ImageTextButtonStyleRecord   - 
          - LabelStyleRecord             - 
          - ListBoxStyleRecord           - 
          - ProgressBarStyleRecord       - 
          - ScrollPaneStyleRecord        - 
          - SelectBoxStyleRecord         - 
          - SliderStyleRecord            - 
          - SplitPaneStyleRecord         - 
          - TextAreaStyleRecord          - Done
          - TextButtonStyleRecord        - 
          - TextFieldStyleRecord         - 
          - TextTooltipStyleRecord       - 
          - TouchpadStyleRecord          - 
          - TreeStyleRecord              - 
          - WindowStyleRecord            - 

TODO:

- Add Copy() methods for Mutable classes.
- GameSprite helpers.
- Network / HTTP etc.
- Update Logger.Debug and Logger.Error to acceot NULL messages and report them as such.
- Box2D Physics support.
- 3D Graphics needs testing and/or finishing.
- 2D Particle System needs testing and/or finishing.
- Freetype Font generation.
- SpriteFont support.

FUTURE UPDATES:
- Migrate to EventHandler based event system for Scene2D. This is a BIG task and will require a lot of work.
- Use NUnit for testing properly.
- 