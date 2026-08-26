// State, as a table rather than a switch. The value of writing it this way is that the
// legal transitions are data you can print, test and draw, instead of an if-chain spread
// over whichever methods happened to need it.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Patterns;

/// <summary>
/// Tracks one current state, the callbacks to run when it changes, and which changes are
/// legal. Declaring no transitions at all leaves every change legal, which is what a
/// machine used only for its enter and exit hooks wants; declaring one turns the machine
/// strict, and an undeclared change is refused from then on.
/// </summary>
/// <typeparam name="TState">The state type, usually an enum.</typeparam>
public sealed class StateMachine<TState>
    where TState : notnull
{
    private readonly Dictionary<TState, List<TState>> _transitions = [];
    private readonly Dictionary<TState, Action> _onEnter = [];
    private readonly Dictionary<TState, Action> _onExit = [];

    /// <summary>Creates a machine sitting in a state, without running its enter callback.</summary>
    /// <param name="initialState">The state to start in.</param>
    public StateMachine(TState initialState)
    {
        CurrentState = initialState;
    }

    /// <summary>The state the machine is in.</summary>
    public TState CurrentState { get; private set; }

    /// <summary>Seconds spent in the current state, advanced by <see cref="Update"/>.</summary>
    public float TimeInStateSeconds { get; private set; }

    /// <summary>True once any transition has been declared, which makes the machine strict.</summary>
    public bool IsStrict => _transitions.Count > 0;

    /// <summary>Declares a legal change of state.</summary>
    /// <param name="from">State to leave.</param>
    /// <param name="to">State to enter.</param>
    public void AddTransition(TState from, TState to)
    {
        if (!_transitions.TryGetValue(from, out List<TState>? targets))
        {
            targets = [];
            _transitions[from] = targets;
        }

        targets.Add(to);
    }

    /// <summary>Registers what to run when a state is entered.</summary>
    /// <param name="state">The state.</param>
    /// <param name="action">What to run.</param>
    public void OnEnter(TState state, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _onEnter[state] = action;
    }

    /// <summary>Registers what to run when a state is left.</summary>
    /// <param name="state">The state.</param>
    /// <param name="action">What to run.</param>
    public void OnExit(TState state, Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _onExit[state] = action;
    }

    /// <summary>Whether a change from the current state is legal.</summary>
    /// <param name="to">State to enter.</param>
    /// <returns>True when the machine would accept the change.</returns>
    public bool CanTransitionTo(TState to)
    {
        if (!IsStrict)
        {
            return true;
        }

        return _transitions.TryGetValue(CurrentState, out List<TState>? targets) && targets.Contains(to);
    }

    /// <summary>Changes state, running the exit callback and then the enter callback.</summary>
    /// <param name="to">State to enter.</param>
    /// <returns>False when the change is not legal, in which case nothing happened.</returns>
    public bool TryTransitionTo(TState to)
    {
        if (!CanTransitionTo(to))
        {
            return false;
        }

        Run(_onExit, CurrentState);
        CurrentState = to;
        TimeInStateSeconds = 0.0f;
        Run(_onEnter, to);
        return true;
    }

    /// <summary>Advances the time spent in the current state.</summary>
    /// <param name="deltaSeconds">Seconds since the previous call.</param>
    public void Update(float deltaSeconds) => TimeInStateSeconds += deltaSeconds;

    private static void Run(Dictionary<TState, Action> callbacks, TState state)
    {
        if (callbacks.TryGetValue(state, out Action? action))
        {
            action();
        }
    }
}
