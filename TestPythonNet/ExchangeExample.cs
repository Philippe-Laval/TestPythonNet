using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    /// <summary>
    /// Shows how to use C# object in python or Python object in C#
    /// </summary>
    public class ExchangeExample
    {
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

        /// <summary>
        /// Shows how to create a C# object in python
        /// </summary>
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
        /// Shows how to call a python method passing C# object as parameter
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

        /// <summary>
        /// Show how to pass a list of C# objects to python code
        /// </summary>
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

        public void TestUsingPythonObjectsFromCsharp()
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

                    // Use dynamic to access attributes and methods
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

        /// <summary>
        /// Shows how to manually convert a python object to a C# object
        /// </summary>
        public void TestConvertAPythonObjectToACharpObject()
        {
            MeasurementResult result;

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Exec("""
from measurement import analyze

result = analyze([10, 20, 30, 40, 50])
""");

                using PyObject pyResult = scope.Get("result");

                result = MeasurementResult.ConvertResult(pyResult);
            }

            // No GIL required anymore
            Console.WriteLine(result.Average); // 30

            // The result C# object can even be used after PythonEngine.Shutdown(),
            // because it no longer depends on Python.
        }
    }
}
