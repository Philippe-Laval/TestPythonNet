using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

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

        



        

        

        

        /// <summary>
        /// Passing a measurement object from C# to Python
        /// The important point is that Python manipulates the original C# object, not a copy.
        /// FromManagedObject() creates a proxy for the managed object and bypasses the normal conversion mechanisms.
        /// </summary>
        public void TestUsingASharpObjectWithPython()
        {
            try
            {
                var measurement = new Measurement
                {
                    SensorName = "TemperatureSensor",
                    RawValue = 25.0
                };

                using (Py.GIL())
                {
                    using var scope = Py.CreateScope();

                    // 1. Convert the C# object to a Python proxy
                    using PyObject pyMeasurement =
                        PyObject.FromManagedObject(measurement);

                    // 2. Expose the object to Python
                    scope.Set("measurement", pyMeasurement);

                    // 3. Process the object in Python
                    scope.Exec(@"
def process(m):
    m.ApplyCalibration(1.2, -2.0)
    m.IsValid = 0 <= m.CorrectedValue <= 100
    return m

result = process(measurement)
");

                    // 4. Retrieve the Python result
                    using PyObject pyResult = scope.Get("result");

                    // 5. Recover the managed C# object
                    var result = (Measurement)pyResult.AsManagedObject(
                        typeof(Measurement)
                    )!;

                    Console.WriteLine(result);

                    // 6. Check object identity
                    Console.WriteLine(
                        ReferenceEquals(measurement, result)
                    );

                    // FromManagedObject() creates a proxy for the managed object and bypasses the normal conversion mechanisms.
                    // Consequently, you could even access the modified object directly from C# without calling AsManagedObject()
                    Console.WriteLine(measurement.CorrectedValue); // 28
                    Console.WriteLine(measurement.IsValid);        // True
                }
            }
            catch (PythonException ex)
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

        public void TestCreatingANewCSharpObjectFromPython()
        {
            var input = new Measurement
            {
                SensorName = "Sensor1",
                RawValue = 25.0
            };

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var pyInput = PyObject.FromManagedObject(input);

                scope.Set("input", pyInput);

                scope.Exec("""
def process(m):
    # Create a new instance of the same managed type
    result = type(m)()

    result.SensorName = m.SensorName
    result.RawValue = m.RawValue

    result.ApplyCalibration(2.0, 5.0)
    result.IsValid = True

    return result

output = process(input)
""");

                using var pyOutput = scope.Get("output");

                var output = (Measurement)pyOutput.AsManagedObject(
                    typeof(Measurement)
                )!;

                Console.WriteLine(input);
                Console.WriteLine(output);

                Console.WriteLine(ReferenceEquals(input, output));
            }
        }

        /// <summary>
        /// This is particularly useful if you want to call the same Python processing function 
        /// many times with different C# objects.
        /// </summary>
        public void CallPythonFonctionWithCSharpObject()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Exec("""
def process(m):
    m.ApplyCalibration(2.0, 10.0)
    m.IsValid = True
    return m
""");

                using PyObject function = scope.Get("process");

                var input = new Measurement
                {
                    SensorName = "Sensor1",
                    RawValue = 50
                };

                // C# -> Python
                using PyObject pyInput = PyObject.FromManagedObject(input);

                // Execute Python function
                using PyObject pyOutput = function.Invoke(pyInput);

                // Python -> C#
                var output = (Measurement)pyOutput.AsManagedObject(
                    typeof(Measurement)
                )!;

                Console.WriteLine(output.CorrectedValue); // 110
            }
        }

        public void PassingListOfCSharpObjectsToPython()
        {
            var measurements = new List<Measurement>
            {
                new Measurement { SensorName = "S1", RawValue = 10 },
                new Measurement { SensorName = "S2", RawValue = 20 },
                new Measurement { SensorName = "S3", RawValue = 30 }
            };

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var pyList =
                    PyObject.FromManagedObject(measurements);

                scope.Set("measurements", pyList);

                scope.Exec(@"
for m in measurements:
    m.ApplyCalibration(1.5, 5.0)
    m.IsValid = m.CorrectedValue < 40

result = measurements
");

                using var pyResult = scope.Get("result");

                var result = (List<Measurement>)pyResult.AsManagedObject(
                    typeof(List<Measurement>)
                )!;

                foreach (Measurement m in result)
                {
                    Console.WriteLine(m);
                }

                Console.WriteLine(ReferenceEquals(measurements, result));
            }
        }

        public void ConvertingANativePythonObjectToCsharp()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var pyNumber = scope.Eval("42");
                using var pyText = scope.Eval("'Hello'");
                using var pyList = scope.Eval("[10, 20, 30]");

                int number = (int)pyNumber.AsManagedObject(typeof(int))!;

                string text = (string)pyText.AsManagedObject(typeof(string))!;

                int[] array = (int[])pyList.AsManagedObject(typeof(int[]))!;

                Console.WriteLine(number);
                Console.WriteLine(text);
                Console.WriteLine(string.Join(", ", array));
            }
        }

        public void TestUsingPythonObjects()
        {
            try
            {
                using (Py.GIL())
                {
                    // Import the Python module
                    using var module = Py.Import("measurement");

                    // Retrieve the Python function
                    using var analyze = module.GetAttr("analyze");

                    // Create input data
                    using var values = new PyList();

                    foreach (double value in new double[] { 10, 20, 30, 40, 50 })
                    {
                        using var item = new PyFloat(value);
                        values.Append(item);
                    }

                    // Execute Python and retrieve the class instance
                    using PyObject result = analyze.Invoke(values);

                    // Display the Python object
                    Console.WriteLine(result.ToString());

                    // Read Python attributes
                    using var min = result.GetAttr("minimum");
                    using var max = result.GetAttr("maximum");
                    using var avg = result.GetAttr("average");

                    Console.WriteLine($"Minimum: {min.As<double>()}");
                    Console.WriteLine($"Maximum: {max.As<double>()}");
                    Console.WriteLine($"Average: {avg.As<double>()}");

                    // Invoke a method of the Python instance
                    using var threshold = new PyFloat(60);
                    using var valid = result.InvokeMethod(
                        "is_valid", threshold
                    );

                    Console.WriteLine($"Valid: {valid.As<bool>()}");
                }
            }
            catch (PythonException ex)
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

        /// <summary>
        /// This approach is more concise, but attribute names and method signatures are checked at runtime 
        /// rather than at compile time.
        /// </summary>
        public void TestSimplerAccessUsingDynamic()
        {
            try
            {
                using (Py.GIL())
                {
                    using var module = Py.Import("measurement");
                    using var analyze = module.GetAttr("analyze");

                    using var values = new PyList();

                    foreach (double value in new double[] { 10, 20, 30 })
                    {
                        using var item = new PyFloat(value);
                        values.Append(item);
                    }

                    using PyObject pyResult = analyze.Invoke(values);

                    dynamic result = pyResult;

                    double minimum = (double)result.minimum;
                    double maximum = (double)result.maximum;
                    double average = (double)result.average;

                    bool valid = (bool)result.is_valid(100.0);

                    Console.WriteLine(minimum); // 10
                    Console.WriteLine(maximum); // 30
                    Console.WriteLine(average); // 20
                    Console.WriteLine(valid);   // True

                    // Modify a Python attribute from C#
                    result.average = 999.0;

                    Console.WriteLine((double)result.average); // 999
                }
            }
            catch (PythonException ex)
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

        public void Test()
        {
            MeasurementResult result;

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Exec("""
from measurement import analyze

result = analyze([10, 20, 30, 40, 50])
""");

                using var pyResult = scope.Get("result");

                result = MeasurementResult.ConvertResult(pyResult);
            }

            // No GIL required anymore
            Console.WriteLine(result.Average); // 30

            // The resulting C# object can even be used after PythonEngine.Shutdown(),
            // because it no longer depends on Python.
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
                // Acquérir le Global Interpreter Lock (GIL) avant tout appel Python
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
