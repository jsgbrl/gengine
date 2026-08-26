// Where a thing is, and where it is relative to whatever it hangs from. There is no
// rotation here on purpose: the console renderer cannot blit a rotated sprite, and an
// engine that carries an angle nothing reads is carrying dead code.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Scenes;

/// <summary>A position and a scale, optionally expressed relative to a parent transform.</summary>
public sealed class Transform2D
{
    private readonly List<Transform2D> _children = [];

    /// <summary>Position relative to the parent, or in the world when there is no parent.</summary>
    public Vector2 LocalPosition { get; set; }

    /// <summary>Scale relative to the parent, or in the world when there is no parent.</summary>
    public Vector2 LocalScale { get; set; } = Vector2.One;

    /// <summary>The transform this one hangs from, or null when it is a root.</summary>
    public Transform2D? Parent { get; private set; }

    /// <summary>The transforms hanging from this one.</summary>
    public IReadOnlyList<Transform2D> Children => _children;

    /// <summary>Position in world space.</summary>
    public Vector2 Position
    {
        get => Parent is null ? LocalPosition : Parent.ToWorld(LocalPosition);
        set => LocalPosition = Parent is null ? value : Parent.ToLocal(value);
    }

    /// <summary>Scale in world space: this transform's scale multiplied by every ancestor's.</summary>
    public Vector2 Scale
    {
        get
        {
            if (Parent is null)
            {
                return LocalScale;
            }

            Vector2 parentScale = Parent.Scale;
            return new Vector2(parentScale.X * LocalScale.X, parentScale.Y * LocalScale.Y);
        }
    }

    /// <summary>Attaches a child, detaching it from its previous parent first.</summary>
    /// <param name="child">The transform to attach.</param>
    /// <exception cref="ArgumentException">The child is this transform or one of its ancestors.</exception>
    public void AddChild(Transform2D child)
    {
        ArgumentNullException.ThrowIfNull(child);
        RejectCycle(child);
        child.Parent?.RemoveChild(child);
        child.Parent = this;
        _children.Add(child);
    }

    /// <summary>Detaches a child, which becomes a root transform.</summary>
    /// <param name="child">The transform to detach.</param>
    /// <returns>True when the child was attached to this transform.</returns>
    public bool RemoveChild(Transform2D child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (!_children.Remove(child))
        {
            return false;
        }

        child.Parent = null;
        return true;
    }

    /// <summary>Converts a point expressed in this transform's space into world space.</summary>
    /// <param name="localPoint">The point, relative to this transform.</param>
    /// <returns>The same point in world space.</returns>
    public Vector2 ToWorld(Vector2 localPoint)
    {
        Vector2 scale = Scale;
        return Position + new Vector2(localPoint.X * scale.X, localPoint.Y * scale.Y);
    }

    /// <summary>Converts a world-space point into this transform's space.</summary>
    /// <param name="worldPoint">The point, in world space.</param>
    /// <returns>The same point relative to this transform.</returns>
    public Vector2 ToLocal(Vector2 worldPoint)
    {
        Vector2 scale = Scale;
        Vector2 offset = worldPoint - Position;
        return new Vector2(offset.X / scale.X, offset.Y / scale.Y);
    }

    private void RejectCycle(Transform2D child)
    {
        for (Transform2D? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor, child))
            {
                throw new ArgumentException("a transform cannot become its own ancestor", nameof(child));
            }
        }
    }
}
