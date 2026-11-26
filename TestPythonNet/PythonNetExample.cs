using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

// https://github.com/pythonnet/pythonnet#embedding-python-in-net

// pip install sqlglot
// dotnet add package pythonnet

// C:\Users\philippe.laval\AppData\Roaming\Python\Python314\site-packages

// https://github.com/pythonnet/pythonnet/wiki
// https://pythonnet.github.io/pythonnet/dotnet.html
// These classes include PyObject, PyList, PyDict, PyTuple
// https://pythonnet.github.io/pythonnet/reference.html
// https://pythonnet.github.io/pythonnet/reference.html#class-Python.Runtime.PyModule
// https://pythonnet.github.io/pythonnet/reference.html#class-Python.Runtime.PyDict

namespace TestPythonNet
{
    public class PythonNetExample
    {
        public void Test1()
        {
            using (Py.GIL())
            {
                // Import the Python file

                string pythonFilesPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "PythonFiles");

                dynamic sys = Py.Import("sys");
                sys.path.append(pythonFilesPath);

                dynamic calculatorModule = Py.Import("test2");
                // Create an instance of the Calculator class
                dynamic calculator = calculatorModule.Calculator();

                // Call the AddInPython method
                int result = calculator.AddInPython(5, 10);
                Console.WriteLine($"Result of adding in Python: {result}");

              

            }
        }

        public void Test2()
        {
            using (Py.GIL())
            {
                dynamic np = Py.Import("numpy");
                Console.WriteLine(np.cos(np.pi * 2));

                dynamic sin = np.sin;
                Console.WriteLine(sin(5));

                double c = (double)(np.cos(5) + sin(5));
                Console.WriteLine(c);

                dynamic a = np.array(new List<float> { 1, 2, 3 });
                Console.WriteLine(a.dtype);

                dynamic b = np.array(new List<float> { 6, 5, 4 }, dtype: np.int32);
                Console.WriteLine(b.dtype);

                Console.WriteLine(a * b);
                Console.ReadKey();
            }
        }

        public void TestPyTuple()
        {
            using (Py.GIL())
            {
                var items = new PyObject[] { new PyInt(1), new PyFloat(2.5), new PyString("test") };
                PyTuple pyTuple = new PyTuple(items);
                bool isTuple = PyTuple.IsTupleType(pyTuple);
                Console.WriteLine($"Is pyTuple a tuple? {isTuple}");

                PyTuple typel2 = PyTuple.AsTuple(pyTuple);
            }
        }

        public void TestPyList()
        {
            using (Py.GIL())
            {
                var items = new PyObject[] { new PyInt(1), new PyInt(2), new PyInt(3) };
                PyList pyList = new PyList(items);
                bool isList = PyList.IsListType(pyList);
                Console.WriteLine($"Is pyList a list? {isList}");

                pyList.Append(new PyInt(4));
                pyList.Insert(0, new PyInt(0));
                pyList.Reverse();

                // This is equivalent to the Python expression “list(object)”
                PyList list2 = PyList.AsList(pyList);

                pyList.Sort();
            }
        }

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

                string result = scope.Execute<string>(script);

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

        public void TestNewScope()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Exec(
                    """
                    factor = 10

                    def square(x):
                        return x ** 2
                    """);

                using var newScope = scope.NewScope();

                int result = newScope.Eval<int>("square(5) * factor");

                Console.WriteLine(result); // 250

                newScope.Exec("factor = 20");

                Console.WriteLine(scope.Get<int>("factor"));    // 10
                Console.WriteLine(newScope.Get<int>("factor")); // 20
            }
        }


        public void Test3()
        {
            // create a person object
            Person person = new Person("John", "Smith");

            // acquire the GIL before using the Python interpreter
            using (Py.GIL())
            {
                // create a Python scope
                using (PyModule scope = Py.CreateScope())
                {
                    // convert the Person object to a PyObject
                    PyObject pyPerson = person.ToPython();

                    PyDict pyDict = new PyDict();
                    pyDict.SetItem("key1", new PyString("value1"));
                    pyDict.SetItem("key2", new PyInt(42));

                    scope.Set("pyDict", pyDict);

                    // create a Python variable "person"
                    scope.Set("person", pyPerson);

                    // the person object may now be used in Python
                    string code = "fullName = person.FirstName + ' ' + person.LastName";
                    PyModule module = scope.Exec(code);

                    var p = scope.Get<Person>("person");
                }
            }
        }


        public void ExecutePythonCode()
        {

            using (Py.GIL())
            {
                // Import the Python file
                dynamic calculatorModule = Py.Import("pythonExample");

                // Create an instance of the Calculator class
                dynamic calculator = calculatorModule.Calculator();

                // Call the AddInPython method
                int result = calculator.AddInPython(5, 10);
                Console.WriteLine($"Result of adding in Python: {result}");
            }

        }


        public void ExecuteSqlglot()
        {
            try
            {
                // 3. Acquérir le Global Interpreter Lock (GIL) avant tout appel Python
                using (Py.GIL())
                {
                    // Utilisation d'un environnement virtuel (venv).
                    // Si sqlglot est installé dans un venv plutôt que dans le Python global,
                    // ajoutez le dossier site-packages au sys.path juste après PythonEngine.Initialize() 
                    //
                    //dynamic sys = Py.Import("sys");
                    //sys.path.append(@"C:\chemin\vers\votre\venv\Lib\site-packages");

                    // Importer le module sqlglot
                    dynamic sqlglot = Py.Import("sqlglot");

                    // --- Exemple A : Transpilation SQL ---
                    string sqlSource = "SELECT EPOCH_MS(1618088028295) AS ts, ARRAY[1, 2, 3] AS arr";

                    // Appel à sqlglot.transpile(sql, read="duckdb", write="tsql")[0]
                    dynamic transpiledList = sqlglot.transpile(
                        sqlSource,
                        read: "duckdb",
                        write: "tsql"
                    );

                    string tsqlResult = transpiledList[0].ToString();
                    Console.WriteLine("SQL transpilé (T-SQL) :");
                    Console.WriteLine(tsqlResult);

                    // --- Exemple B : Parsing et inspection de l'AST ---
                    dynamic exp = Py.Import("sqlglot.expressions");
                    dynamic parsed = sqlglot.parse_one("SELECT a, b FROM ma_table WHERE c = 1");

                    Console.WriteLine("\nColonnes détectées :");
                    foreach (dynamic col in parsed.find_all(exp.Column))
                    {
                        Console.WriteLine($"- {col.name}");
                    }
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine("Une erreur s'est produite lors de l'exécution du code Python :");
                Console.WriteLine(ex.Message);
            }
        }


        public void ExecuteSqlglot2()
        {
            try
            {
                // 3. Acquérir le Global Interpreter Lock (GIL) avant tout appel Python
                using (Py.GIL())
                {
                    // Utilisation d'un environnement virtuel (venv).
                    // Si sqlglot est installé dans un venv plutôt que dans le Python global,
                    // ajoutez le dossier site-packages au sys.path juste après PythonEngine.Initialize() 
                    //
                    //dynamic sys = Py.Import("sys");
                    //sys.path.append(@"C:\chemin\vers\votre\venv\Lib\site-packages");

                    // Importer le module sqlglot
                    dynamic sqlglot = Py.Import("sqlglot");

                    dynamic where = sqlglot.condition("x=1").and_("y=1");
                    var sql = sqlglot.select("*").from_("y").where(where).sql();

                    sql = sqlglot.parse_one("SELECT x FROM y").from_("z").sql();


                    dynamic result = sqlglot.diff(sqlglot.parse_one("SELECT a + b, c, d"), sqlglot.parse_one("SELECT c, a - b, d"));
                    foreach (dynamic a in result)
                    {
                        Console.WriteLine($"{a}");
                    }


                    result = sqlglot.diff(
                        sqlglot.parse_one("SELECT [a] + [b] FROM t", read: "tsql"),
                        sqlglot.parse_one("SELECT [a] - [c] FROM t", read: "tsql"));


                    foreach(dynamic a in result)
                    {
                        Console.WriteLine($"{a}");
                    }
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine("Une erreur s'est produite lors de l'exécution du code Python :");
                Console.WriteLine(ex.Message);
            }
        }

    }
}
