using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private readonly Stack<T> inactiveObjects; 
    private readonly HashSet<T> activeObjects; 
    private readonly Func<T> createFunc;
    private readonly Action<T> actionOnGet;
    private readonly Action<T> actionOnRelease;
    private readonly Action<T> actionOnDestroy;

    public ObjectPool(
        Func<T> createFunc,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        int defaultCapacity = 100)
    {
        this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        this.actionOnGet = actionOnGet;
        this.actionOnRelease = actionOnRelease;
        this.actionOnDestroy = actionOnDestroy;
        inactiveObjects = new Stack<T>(defaultCapacity);
        activeObjects = new HashSet<T>();
    }

    public T Get()
    {
        T obj = inactiveObjects.Count > 0 ? inactiveObjects.Pop() : createFunc();
        actionOnGet?.Invoke(obj);

        // Track the object as active
        activeObjects.Add(obj);

        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null) return;

        // Stop tracking the object as active
        activeObjects.Remove(obj);

        actionOnRelease?.Invoke(obj);
        inactiveObjects.Push(obj);
    }

    public void Clear()
    {
        while (inactiveObjects.Count > 0)
        {
            T obj = inactiveObjects.Pop();
            actionOnDestroy?.Invoke(obj);
        }

        // Clear all active objects
        activeObjects.Clear();
    }

    // Property to get the number of active objects
    public int CountActive => activeObjects.Count;

    // Property to get the number of inactive objects
    public int CountInactive => inactiveObjects.Count;

    // Method to get all active objects
    public HashSet<T> GetActiveObjects()
    {
        return activeObjects;
    }
}