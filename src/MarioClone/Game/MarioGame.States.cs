// The six states, as a table of legal moves plus a method for each shape of state.
//
// Declaring the transitions rather than writing them as ifs means an illegal one - unpausing
// into a death, restarting from the title - is refused by the machine instead of being a bug
// nobody finds until somebody pauses at exactly the wrong moment.

using GEngine.Core.Contracts;
using GEngine.Core.Patterns;

namespace MarioClone.Game;

/// <content>The state machine and what each state does.</content>
public sealed partial class MarioGame
{
    private StateMachine<GameStateKind> BuildStates()
    {
        var machine = new StateMachine<GameStateKind>(
            _settings.StartsImmediately ? GameStateKind.Playing : GameStateKind.Title);

        machine.AddTransition(GameStateKind.Title, GameStateKind.Playing);
        machine.AddTransition(GameStateKind.Playing, GameStateKind.Paused);
        machine.AddTransition(GameStateKind.Playing, GameStateKind.Death);
        machine.AddTransition(GameStateKind.Playing, GameStateKind.LevelComplete);
        machine.AddTransition(GameStateKind.Paused, GameStateKind.Playing);
        machine.AddTransition(GameStateKind.Paused, GameStateKind.Title);
        machine.AddTransition(GameStateKind.Death, GameStateKind.Playing);
        machine.AddTransition(GameStateKind.Death, GameStateKind.GameOver);
        machine.AddTransition(GameStateKind.LevelComplete, GameStateKind.Title);
        machine.AddTransition(GameStateKind.GameOver, GameStateKind.Title);
        return machine;
    }

    private void UpdateState()
    {
        switch (State)
        {
            case GameStateKind.Title:
                UpdateTitle();
                break;
            case GameStateKind.Playing:
                UpdatePlaying();
                break;
            case GameStateKind.Paused:
                UpdatePaused();
                break;
            default:
                UpdateWaiting();
                break;
        }
    }

    private void UpdateTitle()
    {
        if (_input.WasPressedThisFrame(InputAction.Cancel))
        {
            Quit();
            return;
        }

        if (_input.WasPressedThisFrame(InputAction.Confirm) || _input.WasPressedThisFrame(InputAction.Jump))
        {
            StartLevel();
        }
    }

    // Both Pause and Cancel pause, so a player reaching for Escape gets something rather than
    // nothing. From the pause screen, Cancel again goes back to the title, and from the title
    // it quits - three presses of the same key to leave, each of which shows what it did.
    private void UpdatePlaying()
    {
        if (_input.WasPressedThisFrame(InputAction.Pause) || _input.WasPressedThisFrame(InputAction.Cancel))
        {
            _states.TryTransitionTo(GameStateKind.Paused);
        }
    }

    private void UpdatePaused()
    {
        if (_input.WasPressedThisFrame(InputAction.Pause))
        {
            _states.TryTransitionTo(GameStateKind.Playing);
        }

        if (_input.WasPressedThisFrame(InputAction.Cancel))
        {
            _states.TryTransitionTo(GameStateKind.Title);
        }
    }

    // Death, LevelComplete and GameOver are one state with three words on it: hold the screen
    // for a moment, then move on. Where they move on to is the only difference between them.
    private void UpdateWaiting()
    {
        if (_states.TimeInStateSeconds < PauseBeforeContinuingSeconds)
        {
            return;
        }

        if (State != GameStateKind.Death)
        {
            _states.TryTransitionTo(GameStateKind.Title);
            return;
        }

        if (Session.IsGameOver)
        {
            _states.TryTransitionTo(GameStateKind.GameOver);
            return;
        }

        StartLevel();
    }

    // Reaching the flag and falling down a pit are the same shape of event: the level is over,
    // and the state machine decides what that means.
    private void CheckForAnEnding()
    {
        if (_world.Player is null)
        {
            return;
        }

        if (_world.Player.HasReachedGoal)
        {
            _states.TryTransitionTo(GameStateKind.LevelComplete);
            return;
        }

        if (_world.Player.IsDead || Session.IsOutOfTime)
        {
            Session.LoseLife();
            _states.TryTransitionTo(GameStateKind.Death);
        }
    }

    private void StartLevel()
    {
        _world = BuildWorld();
        _camera = BuildCamera();
        Session.RestartLevel();
        _input.Clear();
        _states.TryTransitionTo(GameStateKind.Playing);
    }
}
