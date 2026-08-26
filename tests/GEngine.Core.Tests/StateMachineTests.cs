using System.Collections.Generic;
using GEngine.Core.Patterns;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="StateMachine{TState}"/>.</summary>
public sealed class StateMachineTests
{
    private enum Mood
    {
        Idle,
        Running,
        Jumping,
    }

    [Test]
    public void ANewMachine_SitsInItsInitialStateWithoutRunningTheEnterHook()
    {
        var entered = new List<Mood>();
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.OnEnter(Mood.Idle, () => entered.Add(Mood.Idle));
        Assert.AreEqual(Mood.Idle, machine.CurrentState);
        Assert.AreEqual(0, entered.Count);
    }

    [Test]
    public void WithNoTransitionsDeclared_EveryChangeIsAllowed()
    {
        var machine = new StateMachine<Mood>(Mood.Idle);
        Assert.IsFalse(machine.IsStrict);
        Assert.IsTrue(machine.TryTransitionTo(Mood.Jumping));
        Assert.AreEqual(Mood.Jumping, machine.CurrentState);
    }

    [Test]
    public void DeclaringOneTransition_MakesTheMachineStrict()
    {
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.AddTransition(Mood.Idle, Mood.Running);
        Assert.IsTrue(machine.IsStrict);
        Assert.IsTrue(machine.CanTransitionTo(Mood.Running));
        Assert.IsFalse(machine.CanTransitionTo(Mood.Jumping));
    }

    [Test]
    public void AnUndeclaredChange_IsRefusedAndChangesNothing()
    {
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.AddTransition(Mood.Idle, Mood.Running);
        Assert.IsFalse(machine.TryTransitionTo(Mood.Jumping));
        Assert.AreEqual(Mood.Idle, machine.CurrentState);
    }

    [Test]
    public void AChange_RunsExitThenEnter_InThatOrder()
    {
        var log = new List<string>();
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.OnExit(Mood.Idle, () => log.Add("exit idle"));
        machine.OnEnter(Mood.Running, () => log.Add("enter running"));
        machine.TryTransitionTo(Mood.Running);
        Assert.MatchesSnapshot(string.Join("\n", log), "exit idle\nenter running");
    }

    [Test]
    public void ARefusedChange_RunsNeitherHook()
    {
        var log = new List<string>();
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.AddTransition(Mood.Idle, Mood.Running);
        machine.OnExit(Mood.Idle, () => log.Add("exit"));
        machine.OnEnter(Mood.Jumping, () => log.Add("enter"));
        machine.TryTransitionTo(Mood.Jumping);
        Assert.AreEqual(0, log.Count);
    }

    [Test]
    public void TimeInState_AccumulatesAndResetsOnEveryChange()
    {
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.Update(0.5f);
        machine.Update(0.25f);
        Assert.ApproximatelyEqual(0.75f, machine.TimeInStateSeconds);
        machine.TryTransitionTo(Mood.Running);
        Assert.ApproximatelyEqual(0.0f, machine.TimeInStateSeconds);
    }

    [Test]
    public void AStateCanDeclareSeveralTargets()
    {
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.AddTransition(Mood.Idle, Mood.Running);
        machine.AddTransition(Mood.Idle, Mood.Jumping);
        Assert.IsTrue(machine.CanTransitionTo(Mood.Running));
        Assert.IsTrue(machine.CanTransitionTo(Mood.Jumping));
    }

    [Test]
    public void AStateWithNoDeclaredTargets_IsADeadEndInAStrictMachine()
    {
        var machine = new StateMachine<Mood>(Mood.Idle);
        machine.AddTransition(Mood.Idle, Mood.Jumping);
        machine.TryTransitionTo(Mood.Jumping);
        Assert.IsFalse(machine.CanTransitionTo(Mood.Idle));
    }
}
