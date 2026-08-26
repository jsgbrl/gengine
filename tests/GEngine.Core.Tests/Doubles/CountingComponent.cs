// Counts every callback the Template Method in Component is supposed to make.

using GEngine.Core.Scenes;

namespace GEngine.Core.Tests.Doubles;

internal sealed class CountingComponent : Component
{
    public int AttachCount { get; private set; }

    public int UpdateCount { get; private set; }

    public int FixedUpdateCount { get; private set; }

    public int DetachCount { get; private set; }

    protected override void OnAttach() => AttachCount++;

    protected override void OnUpdate(float deltaSeconds) => UpdateCount++;

    protected override void OnFixedUpdate(float fixedDeltaSeconds) => FixedUpdateCount++;

    protected override void OnDetach() => DetachCount++;
}
