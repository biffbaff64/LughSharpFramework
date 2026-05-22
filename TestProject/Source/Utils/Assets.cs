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

using LughSharp.Source.IO;

namespace TestProject.Source.Utils;

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
    public static string Background       => @$"{Files.ContentRoot}\water_background.png";
    public static string TitleBackground  => @$"{Files.ContentRoot}\title_background.png";
    public static string WindowBackground => @$"{Files.ContentRoot}\title_background.png";
    public static string HudPanel         => @$"{Files.ContentRoot}\hud_panel_rework.png";
    public static string CompleteStar     => @$"{Files.ContentRoot}\packedimages\objects\complete_star.png";
    public static string KeyCollected     => @$"{Files.ContentRoot}\packedimages\text\key_collected.png";
    public static string Bar9             => @$"{Files.ContentRoot}\packedimages\objects\bar9.png";
    public static string Barrel4          => @$"{Files.ContentRoot}\packedimages\animations\barrel4.png";
    public static string Bouncer          => @$"{Files.ContentRoot}\packedimages\animations\bouncer.png";

    // ========================================================================
    // Input / Output
    public static string ButtonBUp       => @$"{Files.ContentRoot}\packedimages\input\button_drop.png";
    public static string ButtonBDown     => @$"{Files.ContentRoot}\packedimages\input\button_fire.png";
    public static string ButtonBOver     => @$"{Files.ContentRoot}\packedimages\input\button_x.png";
    public static string ButtonBChecked  => @$"{Files.ContentRoot}\packedimages\input\button_y.png";
    public static string ButtonBDisabled => @$"{Files.ContentRoot}\packedimages\input\button_y_pressed.png";
    public static string ToggleOn        => @$"{Files.ContentRoot}\packedimages\input\toggle_on.png";
    public static string ToggleOff       => @$"{Files.ContentRoot}\packedimages\input\toggle_off.png";

    // ========================================================================
    // UI Skins
    public static string UiSkin          => @$"{Files.ContentRoot}\Skins\uiskin.json";
    public static string UiskinAtlas     => @$"{Files.ContentRoot}\Skins\uiskin.atlas";
    public static string DefaultSkinFont => @$"{Files.ContentRoot}\Skins\default.fnt";

    // ========================================================================
    // TMX Maps
    public static string Room1Map => @$"{Files.ContentRoot}\Maps\map1.tmx";

    // ========================================================================
    // Fonts
    public static string ArialFont          => @$"{Files.ContentRoot}\Fonts\arial-15.fnt";
    public static string ArialLatinFont     => @$"{Files.ContentRoot}\Fonts\arial-latin.fnt";
    public static string Arial15Font        => @$"{Files.ContentRoot}\Fonts\arial-15.fnt";
    public static string AmbleRegular26Font => @$"{Files.ContentRoot}\Fonts\Amble-Regular-26.fnt";
    public static string Isans15Font        => @$"{Files.ContentRoot}\Fonts\Isans-15.fnt";
}

// ============================================================================
// ============================================================================