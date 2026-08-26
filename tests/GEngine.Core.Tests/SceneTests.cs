using GEngine.Core.Scenes;
using GEngine.Core.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>
/// Covers <see cref="Scene"/>, and above all the reason it defers: an entity added or
/// removed from inside an update must not disturb the walk that is happening.
/// </summary>
public sealed class SceneTests
{
    private Scene _scene = new("test");

    [Setup]
    public void Setup() => _scene = new Scene("test");

    [Test]
    public void Add_IsDeferredUntilTheNextUpdate()
    {
        _scene.Add(new Entity("a"));
        Assert.AreEqual(0, _scene.Entities.Count);
        Assert.AreEqual(1, _scene.PendingCount);
        _scene.Update(0.1f);
        Assert.AreEqual(1, _scene.Entities.Count);
        Assert.AreEqual(0, _scene.PendingCount);
    }

    [Test]
    public void ApplyPendingChanges_DrainsTheQueueWithoutRunningAFrame()
    {
        var entity = new Entity("a");
        CountingComponent component = entity.Add(new CountingComponent());
        _scene.Add(entity);
        _scene.ApplyPendingChanges();
        Assert.AreEqual(1, _scene.Entities.Count);
        Assert.AreEqual(0, component.UpdateCount);
    }

    [Test]
    public void AddedEntities_KnowWhichSceneTheyAreIn()
    {
        var entity = new Entity("a");
        _scene.Add(entity);
        _scene.ApplyPendingChanges();
        Assert.AreSame(_scene, entity.Scene);
        _scene.Remove(entity);
        _scene.ApplyPendingChanges();
        Assert.IsNull(entity.Scene);
    }

    [Test]
    public void AnEntityAddedDuringAnUpdate_FirstRunsOnTheNextFrame()
    {
        var newcomer = new Entity("newcomer");
        CountingComponent newcomerCounter = newcomer.Add(new CountingComponent());
        AddEditor(entityToAdd: newcomer, entityToRemove: null);

        _scene.Update(0.1f);
        Assert.AreEqual(0, newcomerCounter.UpdateCount);
        _scene.Update(0.1f);
        Assert.AreEqual(1, newcomerCounter.UpdateCount);
    }

    [Test]
    public void AnEntityRemovedDuringAnUpdate_StillFinishesThatFrame()
    {
        var doomed = new Entity("doomed");
        CountingComponent doomedCounter = doomed.Add(new CountingComponent());

        // The editor goes in first, so the removal is queued before the doomed entity has
        // had its turn. Without deferral this is where the walk would skip it.
        AddEditor(entityToAdd: null, entityToRemove: doomed);
        _scene.Add(doomed);
        _scene.ApplyPendingChanges();

        _scene.Update(0.1f);
        Assert.AreEqual(1, doomedCounter.UpdateCount);
        _scene.Update(0.1f);
        Assert.AreEqual(1, doomedCounter.UpdateCount);
        Assert.IsNull(_scene.Find("doomed"));
    }

    [Test]
    public void RemovingAnEntityThatIsNotInTheScene_ChangesNothing()
    {
        _scene.Remove(new Entity("stranger"));
        _scene.ApplyPendingChanges();
        Assert.AreEqual(0, _scene.Entities.Count);
    }

    [Test]
    public void FixedUpdate_AlsoDrainsTheQueueAndSkipsInactiveEntities()
    {
        var active = new Entity("active");
        CountingComponent activeCounter = active.Add(new CountingComponent());
        var sleeping = new Entity("sleeping") { IsActive = false };
        CountingComponent sleepingCounter = sleeping.Add(new CountingComponent());
        _scene.Add(active);
        _scene.Add(sleeping);

        _scene.FixedUpdate(1.0f / 60.0f);
        Assert.AreEqual(1, activeCounter.FixedUpdateCount);
        Assert.AreEqual(0, sleepingCounter.FixedUpdateCount);
    }

    [Test]
    public void Find_ReturnsTheFirstEntityWithThatName_OrNull()
    {
        var first = new Entity("coin");
        _scene.Add(first);
        _scene.Add(new Entity("coin"));
        _scene.ApplyPendingChanges();
        Assert.AreSame(first, _scene.Find("coin"));
        Assert.IsNull(_scene.Find("mushroom"));
    }

    [Test]
    public void Name_IsKept()
    {
        Assert.AreEqual("test", _scene.Name);
    }

    private void AddEditor(Entity? entityToAdd, Entity? entityToRemove)
    {
        var editor = new Entity("editor");
        editor.Add(new SceneEditingComponent { EntityToAdd = entityToAdd, EntityToRemove = entityToRemove });
        _scene.Add(editor);
        _scene.ApplyPendingChanges();
    }
}
