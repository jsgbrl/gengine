// Drawing: four pictures for six states, and none of them changes anything. Render is called once per
// frame after the fixed steps, so what it draws is the world as it stands - and because it
// writes nothing, a frame that is dropped costs nothing but the frame.

using GEngine.Rendering;
using MarioClone.View;

namespace MarioClone.Game;

/// <content>What each state looks like.</content>
public sealed partial class MarioGame
{
    private void DrawState()
    {
        if (State == GameStateKind.Title)
        {
            _titleScreen.Draw(_frame, _settings.Router.ActiveBackend);
            return;
        }

        if (State == GameStateKind.GameOver)
        {
            _frame.Clear(Palette.Black);
            Hud.DrawBanner(_frame, "GAME OVER", Palette.White);
            return;
        }

        DrawLevel();
        DrawBanner();
    }

    // Three of the six states are the level with a word over it, and the word is the only
    // difference between them. Playing is the fourth, and its word is nothing at all.
    private void DrawBanner()
    {
        switch (State)
        {
            case GameStateKind.Paused:
                Hud.DrawBanner(_frame, "PAUSED", Palette.White);
                break;
            case GameStateKind.Death:
                Hud.DrawBanner(_frame, "OUCH", Palette.LightRed);
                break;
            case GameStateKind.LevelComplete:
                Hud.DrawBanner(_frame, "COURSE CLEAR", Palette.Yellow);
                break;
            default:
                break;
        }
    }

    private void DrawLevel()
    {
        _levelView.Draw(_frame, _camera, _world);
        Hud.Draw(_frame, Session);
    }
}
