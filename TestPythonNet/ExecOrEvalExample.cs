using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    public class ExecOrEvalExample
    {
        public void TestExec()
        {
            using (Py.GIL())
            {
                PyInt pyInt = new PyInt(42L);
                long value1 = pyInt.ToInt64();
                long value2 = pyInt.As<long>();

                PyString pyString = new PyString("Hello, Python!");

                PyFloat pyFloat = new PyFloat(3.14);

                var items1 = new PyObject[] { new PyInt(1), new PyInt(2), new PyInt(3) };
                PyList pyList = new PyList(items1);

                var items2 = new PyObject[] { new PyInt(1), new PyFloat(2.5), new PyString("test") };
                PyTuple pyTuple = new PyTuple(items2);

                PyDict pyDict = new PyDict();
                pyDict.SetItem("key1", new PyFloat(1.1));
                pyDict.SetItem("key2", new PyInt(1));

                using (PyModule scope = Py.CreateScope())
                {
                    scope.Set("my_int", pyInt);
                    scope.Set("my_string", pyString);
                    scope.Set("my_float", pyFloat);
                    scope.Set("my_list", pyList);
                    scope.Set("my_tuple", pyTuple);
                    scope.Set("my_dict", pyDict);

                    string code = """

                        print(f'my_int: {my_int}, my_string: {my_string}')

                        # create a local variable
                        my_int2 = my_int + 10

                        # Modify the variable in the scope
                        my_int = 123

                        """;

                    PyDict locals = new PyDict();

                    // Execute the code within the scope, passing the locals dictionary
                    // This allows you to retrieve variables defined in the executed code (like my_int2)
                    scope.Exec(code, locals);

                    if (locals.HasKey("my_int"))
                    {
                        PyInt myInt2 = locals.GetItem("my_int").As<PyInt>();
                        Console.WriteLine($"my_int from locals: {myInt2.ToInt64()}");
                    }

                    if (locals.HasKey("my_int2"))
                    {
                        PyInt myInt2 = locals.GetItem("my_int2").As<PyInt>();
                        Console.WriteLine($"my_int2 from locals: {myInt2.ToInt64()}");
                    }

                    // Get 42
                    if (scope.TryGet("my_int", out PyObject? myInt2Obj))
                    {
                        PyInt myInt2 = myInt2Obj!.As<PyInt>();
                        Console.WriteLine($"my_int after execution: {myInt2.ToInt64()}");
                    }

                    // Get 42
                    if (scope.TryGet<long?>("my_int", out long? myLong2))
                    {
                        Console.WriteLine($"my_int after execution: {myLong2}");
                    }

                    // Get 42
                    if (scope.Contains("my_int"))
                    {
                        PyInt myInt2 = scope.Get("my_int").As<PyInt>();
                        Console.WriteLine($"my_int after execution: {myInt2.ToInt64()}");
                    }

                    // Get 42
                    if (scope.Contains("my_int"))
                    {
                        long myLong1 = scope.Get<long>("my_int");
                        Console.WriteLine($"my_int after execution: {myLong1}");
                    }

                    PyDict variables = scope.Variables();
                }
            }
        }

        public void TestEval()
        {
            using (Py.GIL())
            {
                PyInt pyInt = new PyInt(10L);
                PyFloat pyFloat = new PyFloat(3.14);

                using (PyModule scope = Py.CreateScope())
                {
                    scope.Set("my_int", pyInt);
                    scope.Set("my_float", pyFloat);

                    // Using variables
                    string code = "my_float + my_int";

                    PyObject result = scope.Eval(code);
                    Console.WriteLine($"Result of evaluating '{code}': {result}");

                    result = scope.Eval("10 * 5");
                    result = scope.Eval("2 ** 3");
                    result = scope.Eval("10 > 5");
                    result = scope.Eval("len('hello')");

                    PyDict locals = new PyDict();
                    locals.SetItem("local1", new PyInt(5));
                    locals.SetItem("local2", new PyFloat(5.1));

                    code = "my_float + my_int + local1 + local2";

                    // 23.24
                    result = scope.Eval(code, locals);
                    Console.WriteLine($"Result of evaluating '{code}': {result}");

                    // 23,24 (French locale)
                    double? result2 = scope.Eval<double?>(code, locals);
                    Console.WriteLine($"Result of evaluating '{code}': {result2}");
                }
            }
        }

        public void TestExecuteWithEval()
        {
            // Simple mathematical expression
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // In Python.NET, RunFlagType.File and RunFlagType.Eval determine how Python code is compiled and what it returns when executed from C#.
                // -RunFlagType.Eval: Evaluates a Python expression and returns its value.
                // -RunFlagType.File: Executes Python statements, just like running a .py file.

                using var script = PythonEngine.Compile(
                    "10 + 20 * 2",
                    mode: RunFlagType.Eval
                );

                int result = scope.Execute<int>(script);

                Console.WriteLine(result);
            }

            // Expression with variables
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Set("x", 10);
                scope.Set("y", 20);

                using var script = PythonEngine.Compile(
                    "x + y",
                    mode: RunFlagType.Eval
                );

                int result = scope.Execute<int>(script);

                Console.WriteLine(result); // 30
            }

            // Conditional expression
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Set("x", 15);

                using var script = PythonEngine.Compile(
                    "'Positive' if x > 0 else 'Negative or zero'",
                    mode: RunFlagType.Eval
                );

                string? result = scope.Execute<string>(script);

                Console.WriteLine(result); // Positive
            }

            // List comprehension
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var script = PythonEngine.Compile(
                    "[x ** 2 for x in range(5)]",
                    mode: RunFlagType.Eval
                );

                using var result = scope.Execute(script);

                Console.WriteLine(result.ToString());
                // [0, 1, 4, 9, 16]
            }
        }

        public void TestExecuteWithFile()
        {
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    string code = """
x = 10
y = 20
result = x + y
""";

                    using var script = PythonEngine.Compile(
                        code,
                        mode: RunFlagType.File
                    );

                    using var executionResult = scope.Execute(script);

                    int result = scope.Get<int>("result");

                    Console.WriteLine(result); // 30
                }
            }

            // Unlike Eval, File returns Python None.
            // The calculated result must be retrieved from the scope
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    string code =
                        """
                        result = 0

                        for i in range(1, 6):
                            result += i
                        """;

                    using var script = PythonEngine.Compile(
                        code,
                        mode: RunFlagType.File
                    );

                    using var executionResult = scope.Execute(script);

                    int result = scope.Get<int>("result");

                    Console.WriteLine(result); // 15
                }
            }

            // Function definition and import
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    string code =
                    """
                    import math

                    def calculate(x):
                        return math.sqrt(x) * 2

                    result = calculate(25)
                    """;

                    using var script = PythonEngine.Compile(
                        code,
                        mode: RunFlagType.File
                    );

                    using var executionResult = scope.Execute(script);

                    double result = scope.Get<double>("result");

                    Console.WriteLine(result); // 10
                }
            }

            // Combining File and Eval in the same scope
            // You can use File to initialize variables, import modules and define functions,
            // then use Eval to evaluate expressions within the same scope.
            //
            // This is particularly useful when you want to initialize a Python environment once
            // and evaluate expressions repeatedly from C#.

            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    string code =
                    """
                    import math

                    factor = 10

                    def calculate(x):
                        return math.sqrt(x) * factor
                    """;

                    // Initialize Python environment
                    using var initScript = PythonEngine.Compile(code,
                        mode: RunFlagType.File
                    );

                    using var initResult = scope.Execute(initScript);

                    // Compile an expression
                    using var evalScript = PythonEngine.Compile(
                        "calculate(x)",
                        mode: RunFlagType.Eval
                    );

                    // Execute multiple times
                    scope.Set("x", 25);
                    Console.WriteLine(scope.Execute<double>(evalScript)); // 50

                    scope.Set("x", 100);
                    Console.WriteLine(scope.Execute<double>(evalScript)); // 100
                }
            }
        }
    }
}
