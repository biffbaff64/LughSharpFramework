// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using LughSharp.Core.Files;

namespace TestProject.Source;

public static class Assets
{
    // ========================================================================
    // TextureAtlases
    public static string AnimationsAtlas => @$"\packedimages\output\animations.atlas";
    public static string ObjectsAtlas    => @$"\packedimages\output\objects.atlas";
    public static string InputAtlas      => @$"\packedimages\output\input.atlas";
    public static string TextAtlas       => @$"\packedimages\output\text.atlas";

    // ========================================================================
    // Miscellaneous
    public static string Background       => @$"{Files.ContentRoot}\background.png";
    public static string TitleBackground  => @$"{Files.ContentRoot}\title_background.png";
    public static string HudPanel         => @$"{Files.ContentRoot}\hud_panel.png";
    public static string CompleteStar     => @$"{Files.ContentRoot}\complete_star.png";
    public static string KeyCollected     => @$"{Files.ContentRoot}\key_collected.png";
    public static string WindowBackground => @$"{Files.ContentRoot}\title_background.png";
    public static string PauseExitButton  => @$"{Files.ContentRoot}\pause_exit_button.png";
    public static string ButtonRight      => @$"{Files.ContentRoot}\packedimages\input\button_right.png";
    public static string ButtonBUp        => @$"{Files.ContentRoot}\packedimages\input\button_a.png";
    public static string ButtonBDown      => @$"{Files.ContentRoot}\packedimages\input\button_b.png";
    public static string ButtonBOver      => @$"{Files.ContentRoot}\packedimages\input\button_x.png";
    public static string ButtonBChecked   => @$"{Files.ContentRoot}\packedimages\input\button_y.png";
    public static string ButtonBDisabled  => @$"{Files.ContentRoot}\packedimages\input\button_y_pressed.png";
    public static string Bar9             => @$"{Files.ContentRoot}\packedimages\objects\bar9.png";
    public static string Boulder32X32     => @$"{Files.ContentRoot}\packedimages\animations\boulder32x32.png";
    public static string Boulder48X48     => @$"{Files.ContentRoot}\packedimages\animations\boulder48x48.png";
    public static string Boulder64X64     => @$"{Files.ContentRoot}\packedimages\animations\boulder64x64.png";
    public static string Solid112X112     => @$"{Files.ContentRoot}\solid112x112.png";
    public static string Icon11112X112    => @$"{Files.ContentRoot}\ic_icon11_112x112.png";

    // ========================================================================
    // UI Skins
    public static string UiSkin      => @$"{Files.ContentRoot}\Skins\uiskin.json";
    public static string UiskinAtlas => @$"{Files.ContentRoot}\Skins\uiskin.atlas";

    // ========================================================================
    // TMX Maps
    public static string Room1Map => @$"{Files.ContentRoot}\Maps\map1.tmx";

    // ========================================================================
    // Fonts
    public static string ArialFont          => @$"{Files.ContentRoot}\Fonts\arial-15.fnt";
    public static string ArialLatinFont     => @$"{Files.ContentRoot}\Fonts\arial-latin.fnt";
    public static string Arial15Font        => @$"{Files.ContentRoot}\Fonts\arial-15.fnt";
    public static string AmbleRegular26Font => @$"{Files.ContentRoot}\Fonts\Amble-Regular-26.fnt";
}

// ============================================================================
// ============================================================================