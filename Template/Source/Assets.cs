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

namespace Template.Source;

public static class Assets
{
    public static string BackgroundImage  => @$"{IOConfig.AssetsRoot}\title_background.png";
    public static string HudPanel         => @$"{IOConfig.AssetsRoot}\hud_panel.png";
    public static string CompleteStar     => @$"{IOConfig.AssetsRoot}\complete_star.png";
    public static string KeyCollected     => @$"{IOConfig.AssetsRoot}\key_collected.png";
    public static string WindowBackground => @$"{IOConfig.AssetsRoot}\title_background.png";
    public static string PauseExitButton  => @$"{IOConfig.AssetsRoot}\pause_exit_button.png";
    public static string ButtonBUp        => @$"{IOConfig.AssetsRoot}\packedimages\input\button_b.png";
    public static string ButtonBDown      => @$"{IOConfig.AssetsRoot}\packedimages\input\button_b_pressed.png";

    // ========================================================================
    // UI Skins
    public static string UiSkin      => @$"{IOConfig.AssetsRoot}\Skins\uiskin.json";
    public static string UiskinAtlas => @$"{IOConfig.AssetsRoot}\Skins\uiskin.atlas";

    // ========================================================================
    // TMX Maps
    public static string Room1Map => @$"{IOConfig.AssetsRoot}\Maps\room1.tmx";

    // ========================================================================
    // Fonts
    public static string ArialFont          => @$"{IOConfig.AssetsRoot}\Fonts\arial-15.fnt";
    public static string ArialLatinFont     => @$"{IOConfig.AssetsRoot}\Fonts\arial-latin.fnt";
    public static string Arial15Font        => @$"{IOConfig.AssetsRoot}\Fonts\arial-15.fnt";
    public static string AmbleRegular26Font => @$"{IOConfig.AssetsRoot}\Fonts\Amble-Regular-26.fnt";
}

// ============================================================================
// ============================================================================