using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet;

public sealed class PythonCalculator : IDisposable
{
    private readonly PyObject _module;

    public PythonCalculator()
    {
        using (Py.GIL())
        {
            SetupPath();

            _module = Py.Import("calculator2");
        }
    }

    private void SetupPath()
    {
        //dynamic sys = Py.Import("sys");
        //sys.path.append(@"C:\Scripts");

        string pythonFilesPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "PythonFiles");

        dynamic sys = Py.Import("sys");

        Console.WriteLine($"sys.path before: {sys.path}");

        if (!(bool)sys.path.__contains__(pythonFilesPath))
        {
            sys.path.append(pythonFilesPath);
        }

        Console.WriteLine($"sys.path after: {sys.path}");
    }

    public int Add(int a, int b)
    {
        using (Py.GIL())
        {
            dynamic calculator = _module;

            return (int)calculator.add(a, b);
        }
    }

    public int Multiply(int a, int b)
    {
        using (Py.GIL())
        {
            dynamic calculator = _module;

            return (int)calculator.multiply(a, b);
        }
    }

    public void Dispose()
    {
        using (Py.GIL())
        {
            _module.Dispose();
        }
    }
}
