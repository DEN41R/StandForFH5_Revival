using System;
using System.Collections.Generic;
using StandForFH5Revival.Interfaces;

namespace StandForFH5Revival
{
    public static class Cheats
    {
        private static readonly Dictionary<Type, object> _cachedInstances = new Dictionary<Type, object>();
        private static readonly object _lock = new object();

        public static T GetClass<T>() where T : class
        {
            var classType = typeof(T);

            if (_cachedInstances.TryGetValue(classType, out var cachedInstance))
            {
                return (T)cachedInstance;
            }

            lock (_lock)
            {
                if (_cachedInstances.TryGetValue(classType, out cachedInstance))
                {
                    return (T)cachedInstance;
                }

                try
                {
                    var newInstance = Activator.CreateInstance(classType) as T;
                    if (newInstance == null)
                    {
                        throw new InvalidOperationException($"Failed to create instance of type {classType.Name}");
                    }

                    _cachedInstances[classType] = newInstance;
                    return newInstance;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Unable to create instance of {classType.Name}: {ex.Message}", ex);
                }
            }
        }

        public static void ClearCache()
        {
            lock (_lock)
            {
                foreach (var instance in _cachedInstances.Values)
                {
                    if (instance is IDisposable disposable)
                    {
                        try { disposable.Dispose(); } catch { }
                    }

                    if (instance is ICheatsBase cheat)
                    {
                        try { cheat.Cleanup(); } catch { }
                    }
                }

                _cachedInstances.Clear();
            }
        }

        public static int CachedInstanceCount
        {
            get
            {
                lock (_lock)
                {
                    return _cachedInstances.Count;
                }
            }
        }

        public static bool IsInstanceCached<T>() where T : class
        {
            lock (_lock)
            {
                return _cachedInstances.ContainsKey(typeof(T));
            }
        }

        public static bool RemoveFromCache<T>() where T : class
        {
            lock (_lock)
            {
                var classType = typeof(T);
                if (_cachedInstances.TryGetValue(classType, out var instance))
                {
                    if (instance is IDisposable disposable)
                    {
                        try { disposable.Dispose(); } catch { }
                    }

                    if (instance is ICheatsBase cheat)
                    {
                        try { cheat.Cleanup(); } catch { }
                    }

                    return _cachedInstances.Remove(classType);
                }
                return false;
            }
        }
    }
}