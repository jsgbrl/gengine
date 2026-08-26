// A component that adds and removes entities from inside its own update. This is the
// exact shape of the bug Scene defers changes to avoid.

using GEngine.Core.Scenes;

namespace GEngine.Core.Tests.Doubles;

internal sealed class SceneEditingComponent : Component
{
    public Entity? EntityToAdd { get; set; }

    public Entity? EntityToRemove { get; set; }

    protected override void OnUpdate(float deltaSeconds)
    {
        Scene scene = Entity.Scene!;
        if (EntityToAdd is not null)
        {
            scene.Add(EntityToAdd);
        }

        if (EntityToRemove is not null)
        {
            scene.Remove(EntityToRemove);
        }
    }
}
