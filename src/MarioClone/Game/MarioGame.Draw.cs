// Drawing, one method per state, and none of them changes anything. Render is called once per
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
        switch (State)
        {
            case GameStateKind.Title:
                _titleScreen.Draw(_frame, _settings.Router.ActiveBackend);
                break;
            case GameStateKind.Paused:
                DrawLevel();
                Hud.DrawBanner(_frame, "PAUSED", Palette.White);
                break;
            case GameStateKind.Death:
                DrawLevel();
                Hud.DrawBanner(_frame, "OUCH", Palette.LightRed);
                break;
            case GameStateKind.LevelComplete:
                DrawLevel();
                Hud.DrawBanner(_frame, "COURSE CLEAR", Palette.Yellow);
                break;
            case GameStateKind.GameOver:
                _frame.Clear(Palette.Black);
                Hud.DrawBanner(_frame, "GAME OVER", Palette.White);
                break;
            default:
                DrawLevel();
                break;
        }
    }

    private void DrawLevel()
    {
        _levelView.Draw(_frame, _camera, _world);
        Hud.Draw(_frame, Session);
    }
}
