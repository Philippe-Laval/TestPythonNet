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
        public void TestChangeSysPathThenInstanciatePythonObjectAndUseIt()
        {
            using (Py.GIL())
            {
                // Import the Python file

                string pythonFilesPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "PythonFiles");

                dynamic sys = Py.Import("sys");
                sys.path.append(pythonFilesPath);

                dynamic calc = Py.Import("calculator");
                // Create an instance of the Calculator class
                dynamic calcObject = calc.Calculator();

                // Call the AddInPython method
                int result = calcObject.AddInPython(5, 10);
                Console.WriteLine($"Result of adding in Python: {result}");
            }
        }

        public void TestImportNumpy()
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
            }
        }

        public void TestToPython()
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

                    // create a Python variable "person"
                    scope.Set("person", pyPerson);

                    PyDict pyDict = new PyDict();
                    pyDict.SetItem("key1", new PyString("value1"));
                    pyDict.SetItem("key2", new PyInt(42));

                    scope.Set("pyDict", pyDict);

                    // the person object may now be used in Python
                    string code = """
                        fullName = person.FirstName + ' ' + person.LastName
                        person.FirstName = 'Jane'
                        person.LastName = 'Doe'
                        """;
                        
                    PyModule module = scope.Exec(code);

                    Person? p = scope.Get<Person>("person");
                    if (p != null)
                    {
                        Console.WriteLine($"Updated person: {p.FirstName} {p.LastName}");
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve the updated person object.");
                    }

                }
            }
        }

        public void ExecutePythonCode()
        {
            using (Py.GIL())
            {
                // Import the Python module "pythonExample.py"
                dynamic calculatorModule = Py.Import("pythonExample");

                // Create an instance of the Calculator class
                dynamic calculator = calculatorModule.Calculator();

                // Call the AddInPython method
                int result = calculator.AddInPython(5, 10);
                Console.WriteLine($"Result of adding in Python: {result}");
            }
        }


        

    }
}
