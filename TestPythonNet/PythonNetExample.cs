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

        public void TestImportName()
        {
            /*
             * Python equivalent:
import math
result = math.sqrt(25)
             */

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // Imports a Python module by its name
                using var math = scope.Import("math");

                double result = scope.Eval<double>("math.sqrt(25)");

                Console.WriteLine(result); // 5
            }

            /*
             * Python equivalent:
import math as m
result = m.sqrt(25)
             */

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // Imports a Python module by its name and assigns it to a alias
                using PyObject? math = scope.Import("math", "m");

                double result = scope.Eval<double>("m.sqrt(25)");

                Console.WriteLine(result); // 5
            }
        }

        /// <summary>
        /// Imports an existing PyModule into another scope under a given name.
        /// This is useful for creating your own reusable Python modules directly from C#.
        /// The advantage is that you don't need a physical calculator.py file
        /// </summary>
        public void TestImportPyModule()
        {
            using (Py.GIL())
            {
                using var module = Py.CreateScope("calculator");

                module.Exec(
"""
def add(a, b):
    return a + b

def multiply(a, b):
    return a * b
""");

                // Create another scope
                using var scope = Py.CreateScope();

                scope.Import(module, "calc");

                int sum = scope.Eval<int>("calc.add(10, 20)");
                int product = scope.Eval<int>("calc.multiply(10, 20)");

                Console.WriteLine(sum);     // 30
                Console.WriteLine(product); // 200
            }
        }


        public void TestImportPyObject()
        {
            using (Py.GIL())
            {
                using PyObject math = Py.Import("math");

                using var scope1 = Py.CreateScope();
                using var scope2 = Py.CreateScope();

                scope1.Import(math, "m");
                scope2.Import(math, "math");

                double result1 = scope1.Eval<double>("m.sqrt(16)");
                double result2 = scope2.Eval<double>("math.sqrt(81)");

                Console.WriteLine(result1); // 4
                Console.WriteLine(result2); // 9
            }


            using (Py.GIL())
            {
                using PyObject module = Py.Import("math");
                using var scope = Py.CreateScope();

                // Without an alias, the module's own name is used
                scope.Import(module);

                double result = scope.Eval<double>("math.sqrt(100)");

                Console.WriteLine(result); // 10
            }
        }

        public void TestImportAll()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using PyObject math = Py.Import("math");

                // Equivalent to "from math import *" in Python
                // However, ImportAll() actually copies all module dictionary entries,
                // including private and special variables, unlike Python's wildcard import
                scope.ImportAll(math);

                // Notice that you can call sqrt() directly instead of math.sqrt().
                double result = scope.Eval<double>("sqrt(25)");

                Console.WriteLine(result); // 5
            }
        }



        /// <summary>
        /// Basic example: C# → Python → C#
        /// </summary>
        public void TestPyBuffer()
        {
            /*
            Python.Runtime.PyBuffer is useful when you need to exchange binary data, images, 
            numerical arrays or other large buffers between C# and Python without converting 
            every element individually into a Python object.

            A typical workflow is:
            1. C# creates a byte[] containing data.
            2. C# transfers the data into a Python buffer.
            3. Python processes the buffer in place.
            4. C# retrieves the modified data using PyBuffer.
            The important distinction is that PyBuffer provides C# access to an existing Python buffer 
            through the Python buffer protocol. It is not itself a buffer allocator.
             */

            byte[] input = { 0, 50, 100, 200, 255 };
            byte[] output = new byte[input.Length];

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // 1. Allocate a Python bytearray
                scope.Set("size", input.Length);
                scope.Exec("buffer = bytearray(size)");

                // 2. Retrieve the Python object
                using PyObject pyArray = scope.Get("buffer");

                // 3. Get access to its underlying memory
                using PyBuffer pyBuffer = pyArray.GetBuffer();

                // 4. Copy C# data into Python memory
                pyBuffer.Write(input, 0, input.Length, 0);

                // 5. Process the buffer in Python (in place)
                scope.Exec(
"""
for i in range(len(buffer)):
    buffer[i] = 255 - buffer[i]
""");

                // 6. Copy Python data back into C#
                pyBuffer.Read(output, 0, output.Length, 0);
            }

            Console.WriteLine(string.Join(", ", input));
            Console.WriteLine(string.Join(", ", output));
        }

        /// <summary>
        /// Échange un tableau de valeurs `double` entre C# et un tableau NumPy via un tampon brut, applique des calculs
        /// NumPy en place, puis récupère les résultats en `double[]`.
        /// </summary>
        /// <remarks>Utilise `Buffer.BlockCopy` pour convertir entre `double[]` et `byte[]`, puis
        /// `PyBuffer.Write`/`PyBuffer.Read` pour transférer les données. Nécessite NumPy (`float64`) dans
        /// l’environnement Python.</remarks>
        public void TestBufferWithNumpy()
        {
            /*
More useful example: exchanging a double[] with NumPy
If you are working with numerical computations, signal processing or scientific data, you probably want to exchange arrays of double, float or int instead of bytes.

Since PyBuffer.Write() and PyBuffer.Read() operate on byte[], you can use Buffer.BlockCopy() to convert between raw bytes and numerical arrays.

Install NumPy in your Python environment:
pip install numpy 
 */

            double[] input = { 1.5, 2.5, 3.5, 4.5 };
            double[] output = new double[input.Length];

            int byteCount = input.Length * sizeof(double);

            byte[] rawData = new byte[byteCount];

            // Convert double[] to raw bytes
            Buffer.BlockCopy(input, 0, rawData, 0, byteCount);

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // Allocate a NumPy array
                scope.Set("size", input.Length);

                scope.Exec(@"
import numpy as np

data = np.zeros(size, dtype=np.float64)
");

                using PyObject pyArray = scope.Get("data");
                using PyBuffer pyBuffer = pyArray.GetBuffer();

                // C# -> NumPy
                pyBuffer.Write(rawData, 0, byteCount, 0);

                // NumPy computation in place
                scope.Exec(
"""
data *= 2
data += 10
""");

                // NumPy -> C#
                pyBuffer.Read(rawData, 0, byteCount, 0);
            }

            // Convert raw bytes back to double[]
            Buffer.BlockCopy(rawData, 0, output, 0, byteCount);

            Console.WriteLine(string.Join(", ", output));
        }

        public void TestBufferWithNumpyAndSpan()
        {
            /*
             Here is a complete example using PyBuffer and Span<double> to exchange data between C# and Python using shared memory, without intermediate byte[] arrays.

            In this example:
            - Python allocates a NumPy array.
            - C# accesses its memory directly through a Span<double>.
            - C# writes values into the buffer.
            - Python modifies the values in place.
            - C# reads the modified values directly from the same memory.
             */

            try
            {
                using (Py.GIL())
                {
                    using var scope = Py.CreateScope();

                    // 1. Allocate a buffer in Python
                    scope.Exec(
"""
import numpy as np

data = np.zeros(5, dtype=np.float64)
""");

                    // 2. Get the Python object
                    using PyObject pyArray = scope.Get("data");

                    // 3. Get access to Python memory
                    using PyBuffer buffer = pyArray.GetBuffer();

                    // 4. Validate the buffer
                    if (buffer.ReadOnly ||
                        !buffer.IsContiguous(BufferOrderStyle.C) ||
                        buffer.ItemSize != sizeof(double) ||
                        buffer.Length != 5 * sizeof(double))
                    {
                        throw new InvalidOperationException(
                            "Invalid Python buffer");
                    }

                    // 5. Create a Span over Python memory
                    Span<double> span;
                    unsafe
                    {
                        span = new Span<double>(
                            buffer.Buffer.ToPointer(),
                            5
                        );
                    }

                    // 6. Write data directly from C#
                    for (int i = 0; i < span.Length; i++)
                    {
                        span[i] = i + 1;
                    }

                    // 7. Process data in Python
                    scope.Exec(@"
data *= 2
data += 10
");

                    // 8. Read the result directly from C#
                    Console.WriteLine("C# reads:");

                    foreach (double value in span)
                    {
                        Console.WriteLine(value);
                    }

                    // 9. Modify the buffer again from C#
                    span[0] = 999;

                    // 10. Verify that Python sees the modification
                    double first = scope.Eval<double>(
                        "float(data[0])"
                    );

                    Console.WriteLine($"Python reads: {first}");
                }
            }
            catch(PythonException ex)
            {
                Console.WriteLine("An error occurred while executing Python code:");
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred:");
                Console.WriteLine(ex.Message);
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
