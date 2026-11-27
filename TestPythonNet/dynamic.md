# Python.NET — `dynamic` Quick Reference

## Common operations

| Operation | C# syntax |
|---|---|
| Import module | `dynamic m = Py.Import("module");` |
| Call function | `m.function(10, 20)` |
| Named argument | `m.function(value: 10)` |
| Create object | `m.MyClass()` |
| Read attribute | `obj.name` |
| Write attribute | `obj.name = "test"` |
| Call method | `obj.execute()` |
| List indexing | `obj[0]` |
| Dictionary indexing | `obj["key"]` |
| Convert to integer | `(int)obj` |
| Convert to double | `(double)obj` |
| Convert to string | `(string)obj` |
| Convert to boolean | `(bool)obj` |
| Explicit conversion | `((PyObject)obj).As<T>()` |
| Check for `None` | `((PyObject)obj).IsNone()` |
| Get Python attributes | `((PyObject)obj).Dir()` |
| Test for attribute | `((PyObject)obj).HasAttr("name")` |
| Dispose object | `((PyObject)obj).Dispose()` |

## Basic usage

```csharp
using Python.Runtime;

Runtime.PythonDLL = @"C:\Python312\python312.dll";

PythonEngine.Initialize();

using (Py.GIL())
{
    using PyObject module = Py.Import("math");
    dynamic math = module;

    double result = (double)math.sqrt(25);

    Console.WriteLine(result); // 5
}

PythonEngine.Shutdown();
```

## Best practices

- Initialize the Python engine only once.
- Always acquire the GIL before accessing Python objects.
- Cache frequently used Python modules.
- Dispose Python objects when no longer needed.
- Convert Python results to native C# types when possible.
- Catch `PythonException` to handle Python errors.
- Never hold the GIL across an `await`.
- Encapsulate `dynamic` objects in strongly typed C# wrappers.
- Remember that `dynamic` member resolution occurs at runtime.

## References

- [Python.NET GitHub](https://github.com/pythonnet/pythonnet)
- [Python.NET Documentation](https://pythonnet.github.io/)
- [Python.NET NuGet Package](https://www.nuget.org/packages/pythonnet/)
