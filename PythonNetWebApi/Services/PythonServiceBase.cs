using System;
using System.Collections.Generic;
using Python.Runtime;
    
namespace PythonNetWebApi.Services;

/*

Three Rules for Leak-Free PyObject Management


Scenario    How to Handle PyObject	Why It Matters
Long-lived objects (fields like _sqlglot in Scoped services)	Track them and call .Dispose() inside IDisposable.Dispose() within a using (Py.GIL()) block.	Calling .Dispose() without holding the GIL can cause access violations or queue delays.
Short-lived objects (inside methods, like AST trees or result lists)	Assign to using PyObject x = ...; instead of dynamic x = ...; before converting to C# primitives (string, int, List<string>).	Immediately decrements Python's reference count when the method exits rather than waiting for .NET's GC.
Returning data from methods	Always convert Python types to native .NET types (string, bool, List<T>) before exiting RunWithGil().	Returning a dynamic or PyObject outside the GIL block risks accessing Python memory without the lock.


Best Practice Summary for DI Lifetimes

• Use AddSingleton (Recommended): Python modules (sqlglot) are cached in memory once, and ImportModule only runs at startup.
• Use AddScoped (Only if per-request Python state is required): Inheriting from PythonServiceBase ensures ASP.NET Core automatically invokes Dispose() at the end of every HTTP request, cleanly releasing any per-request Python variables.
 
 */


public abstract class PythonServiceBase : IDisposable
{
    // Track all imported modules or long-lived PyObjects for safe disposal
    private readonly List<PyObject> _trackedObjects = new();
    private bool _disposed;

    /// <summary>
    /// Safely imports a Python module under the GIL and registers it for disposal.
    /// </summary>
    protected dynamic ImportModule(string moduleName)
    {
        return RunWithGil(() =>
        {
            PyObject module = Py.Import(moduleName);
            _trackedObjects.Add(module);
            return module;
        });
    }

    /// <summary>
    /// Executes a Python operation under the GIL and returns a result.
    /// </summary>
    protected T RunWithGil<T>(Func<T> action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using (Py.GIL())
        {
            return action();
        }
    }

    /// <summary>
    /// Executes a void Python operation under the GIL.
    /// </summary>
    protected void RunWithGil(Action action)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using (Py.GIL())
        {
            action();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Disposing PyObjects MUST happen while holding the GIL!
        using (Py.GIL())
        {
            foreach (var obj in _trackedObjects)
            {
                obj?.Dispose();
            }
            _trackedObjects.Clear();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
