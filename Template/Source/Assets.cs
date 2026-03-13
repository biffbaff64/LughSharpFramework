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
    public static string BackgroundImage  => @$"{Files.ContentRoot}\title_background.png";
    public static string HudPanel         => @$"{Files.ContentRoot}\hud_panel.png";
    public static string CompleteStar     => @$"{Files.ContentRoot}\complete_star.png";
    public static string KeyCollected     => @$"{Files.ContentRoot}\key_collected.png";
    public static string WindowBackground => @$"{Files.ContentRoot}\title_background.png";
    public static string PauseExitButton  => @$"{Files.ContentRoot}\pause_exit_button.png";
    public static string ButtonBUp        => @$"{Files.ContentRoot}\packedimages\input\button_b.png";
    public static string ButtonBDown      => @$"{Files.ContentRoot}\packedimages\input\button_b_pressed.png";

    // ========================================================================
    // UI Skins
    public static string UiSkin      => @$"{Files.ContentRoot}\Skins\uiskin.json";
    public static string UiskinAtlas => @$"{Files.ContentRoot}\Skins\uiskin.atlas";

    // ========================================================================
    // TMX Maps
    public static string Room1Map => @$"{Files.ContentRoot}\Maps\room1.tmx";

    // ========================================================================
    // Fonts
    public static string ArialFont          => @$"{Files.ContentRoot}\Fonts\arial-15.fnt";
    public static string ArialLatinFont     => @$"{Files.ContentRoot}\Fonts\arial-latin.fnt";
    public static string Arial15Font        => @$"{Files.ContentRoot}\Fonts\arial-15.fnt";
    public static string AmbleRegular26Font => @$"{Files.ContentRoot}\Fonts\Amble-Regular-26.fnt";
}

// ============================================================================
// ============================================================================