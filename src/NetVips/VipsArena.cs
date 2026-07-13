using System;
using System.Collections.Generic;
using System.Threading;

namespace NetVips;

/// <summary>
/// Manages the lifetime of native <see cref="VipsObject"/> instances within a scoped execution context.
/// </summary>
public sealed class VipsArena : IDisposable
{
    private static readonly AsyncLocal<VipsArena> Current = new();

    private readonly List<VipsObject> _objects = [];
    private readonly object _lock = new();
    private readonly VipsArena _parent;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="VipsArena"/> class and sets it as the active scope.
    /// </summary>
    public VipsArena()
    {
        // Capture parent arena if we are nesting them
        _parent = Current.Value;
        Current.Value = this;
    }

    /// <summary>
    /// Internally called by <see cref="VipsObject"/> constructors to track objects under the current arena.
    /// </summary>
    /// <param name="vipsObject">The native object instance to track.</param>
    internal static void Track(VipsObject vipsObject)
    {
        var arena = Current.Value;
        if (arena == null)
        {
            return;
        }

        // Skip operations, they manage their own lifetimes inside Operation.Call
        if (vipsObject is Operation)
        {
            return;
        }

        lock (arena._lock)
        {
            if (Volatile.Read(ref arena._disposed) == 0)
            {
                arena._objects.Add(vipsObject);
            }
        }
    }

    /// <summary>
    /// Saves a <see cref="VipsObject"/> from automatic disposal when the current arena scope closes.
    /// </summary>
    /// <typeparam name="T">A type derived from <see cref="VipsObject"/>.</typeparam>
    /// <param name="vipsObject">The object instance to keep alive.</param>
    /// <returns>The same <paramref name="vipsObject"/> instance passed into the method.</returns>
    public T Keep<T>(T vipsObject) where T : VipsObject
    {
        lock (_lock)
        {
            _objects.Remove(vipsObject);
        }

        return vipsObject;
    }

    /// <summary>
    /// Releases the tracked <see cref="VipsObject"/> instances held by this <see cref="VipsArena"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        // Restore parent context only if this arena is still active
        if (ReferenceEquals(Current.Value, this))
        {
            Current.Value = _parent;
        }

        lock (_lock)
        {
            // Dispose in LIFO order
            for (var i = _objects.Count - 1; i >= 0; i--)
            {
                _objects[i].Dispose();
            }

            _objects.Clear();
        }
    }
}