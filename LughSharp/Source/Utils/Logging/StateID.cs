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

namespace LughSharp.Source.Utils.Logging;

/// <summary>
/// Enum holding State Identifiers for use in combination with
/// <see cref="EnumStateManager"/>.
/// </summary>
[PublicAPI]
public enum StateID
{
    StateSetup,
    StatePaused,
    StateStartupBegin,
    StateStartupEnd,
    
    // ------------------------------------------
    Inactive,
    Limbo,
    Init,
    Update,
    Close,

    // ------------------------------------------
    StateOpen,
    StateOpening,
    StateClosing,
    StateClosed,

    // ------------------------------------------
    StateFlashing,
    StateSteady,

    // ------------------------------------------
    StateMenuBegin,
    StateMenuUpdate,
    StateMenuEnd,
    
    // ------------------------------------------
    StateZoomIn,
    StateZoomOut,
    StateFadeIn,
    StateFadeOut,
    StateFadeInTrigger,
    StateFadeOutTrigger,
    
    // ------------------------------------------
    StatePowerUp,
    StatePowerDown,

    // ------------------------------------------
    StateGame,
    StateGameOver,
    StateGameFinished,
    StateGameEnd,
    StateGameWin,
    StateGameLose,

    StateGameOverMessagePrepare,
    StateGameOverMessage,
    
    // ------------------------------------------
    StateLevelPrepare,
    StateLevelPrepareRetry,
    StateLevelRetry,
    StateLevelPrepareFinished,
    StateLevelFinished,
    
    // ------------------------------------------
    StateDebugHang,

    // ------------------------------------------
    StateEnabled,
    StateDisabled,
    
    // ------------------------------------------
    StateMessagePanel,
    StateSettingsPanel,
    StateDeveloperPanel,
    StateWelcomePanel,

    // ------------------------------------------
    
    StatePanelStart,
    StatePanelIntro,
    StatePanelUpdate,
    StatePanelClose,
    
    // ------------------------------------------

    /// <summary>
    /// The next available StateID for extension.
    /// </summary>
    NextStateID,
}

// ============================================================================
// ============================================================================

