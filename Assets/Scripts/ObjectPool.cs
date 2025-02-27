using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPool<T> where T : Component
{
    private readonly Stack<T> objects;
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
        objects = new Stack<T>(defaultCapacity);
    }

    public T Get()
    {
        T obj = objects.Count > 0 ? objects.Pop() : createFunc();
        actionOnGet?.Invoke(obj);
        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null) return;
        actionOnRelease?.Invoke(obj);
        objects.Push(obj);
    }

    public void Clear()
    {
        while (objects.Count > 0)
        {
            T obj = objects.Pop();
            actionOnDestroy?.Invoke(obj);
        }
    }
}