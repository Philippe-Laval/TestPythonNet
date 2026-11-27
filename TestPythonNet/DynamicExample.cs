using DotNetLibrary;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;



namespace TestPythonNet
{
    public class DynamicExample
    {
        /// <summary>
        /// Calling Python functions in the math module
        /// </summary>
        public void Test1()
        {
            using (Py.GIL())
            {
                dynamic math = Py.Import("math");

                double result = (double)math.sqrt(25);

                Console.WriteLine(result); // 5
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

        /// <summary>
        /// Calling your own Python functions
        /// </summary>
        public void Test2()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic calculator = Py.Import("calculator2");

                int sum = (int)calculator.add(10, 20);
                int product = (int)calculator.multiply(5, 6);
                string message = (string)calculator.greet("David");

                Console.WriteLine(sum);      // 30
                Console.WriteLine(product);  // 30
                Console.WriteLine(message);  // Hello David!
            }
        }

        /// <summary>
        /// Calling functions with named and optional arguments
        /// </summary>
        public void Test3()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic calculator = Py.Import("calculator2");
                double a = (double)calculator.calculate(100);

                double b = (double)calculator.calculate(
                    100,
                    quantity: 3,
                    discount: 0.10
                );

                Console.WriteLine(a); // 100
                Console.WriteLine(b); // 270.0 (100 * 3 * (1 - 0.10))

                double result = (double)calculator.calculate(
                    100,
                    Py.kw("quantity", 3, "discount", 0.10)
                );
                Console.WriteLine(result); // 270.0 (100 * 3 * (1 - 0.10))
            }
        }

        /// <summary>
        /// Instantiating Python classes
        /// </summary>
        public void Test4()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic module = Py.Import("device");

                dynamic device = module.Device("Device001");

                device.connect();

                // Read attributes
                string name = (string)device.name;
                bool connected = (bool)device.connected;

                // Read property
                string status = (string)device.status;

                // Modify attribute
                device.name = "Device002";

                // Call method
                device.disconnect();

                Console.WriteLine(device.status); // Offline
            }
        }

        /// <summary>
        /// Returning multiple values (tuples)
        /// </summary>
        public void Test5()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic module = Py.Import("calculator2");
                dynamic result = module.get_measurement();

                double voltage = (double)result[0];
                double temperature = (double)result[1];
                bool success = (bool)result[2];

                Console.WriteLine(voltage);     // 12.5
                Console.WriteLine(temperature); // 24.8
                Console.WriteLine(success);     // True
            }
        }

        /// <summary>
        /// Working with Python lists
        /// </summary>
        public void Test6()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic builtins = Py.Import("builtins");
                dynamic module = Py.Import("calculator2");

                dynamic values = module.get_values();

                // Access an element
                int first = (int)values[0];

                // Add an element
                values.append(60);

                // Remove an element
                values.remove(20);

                // Get list length
                int count = (int)builtins.len(values);

                // Iterate over the list
                for (int i = 0; i < count; i++)
                {
                    int value = (int)values[i];
                    Console.WriteLine(value);
                }

                var result = new List<int>();

                // Convert the Python list into a native C# list
                foreach (PyObject item in values)
                {
                    using (item)
                    {
                        result.Add(item.As<int>());
                    }
                }

            }
        }

        /// <summary>
        /// Working with Python dictionaries
        /// </summary>
        public void Test7()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic module = Py.Import("calculator2");

                dynamic config = module.get_configuration();

                string hostname = (string)config["hostname"];
                int port = (int)config["port"];
                bool enabled = (bool)config["enabled"];

                // Modify a value
                config["port"] = new PyInt(9090);

                // Add a new value
                config["timeout"] = new PyInt(5000);

                // Access a potentially missing key
                dynamic timeout = config.get("timeout", new PyInt(1000));

                Console.WriteLine(config);
            }
        }

        /// <summary>
        /// Passing C# collections to Python
        /// </summary>
        public void Test8()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic builtins = Py.Import("builtins");
                dynamic module = Py.Import("calculator2");

                var values = new List<int> { 10, 20, 30, 40 };

                // Convert the .NET collection into a Python list
                dynamic pythonList = builtins.list(values);

                dynamic result = module.process_values(pythonList);

                int min = (int)result["min"];
                int max = (int)result["max"];
                int sum = (int)result["sum"];
                double average = (double)result["average"];

                Console.WriteLine(average); // 25s
            }
        }

        /// <summary>
        /// Using NumPy directly from C#
        /// This is particularly useful for scientific calculations, signal processing and measurement analysis.
        /// </summary>
        public void Test9()
        {
            using (Py.GIL())
            {
                dynamic np = Py.Import("numpy");

                dynamic array = np.array(
                    new double[] { 10, 20, 30, 40, 50 }
                );

                double mean = (double)np.mean(array);
                double std = (double)np.std(array);
                double max = (double)np.max(array);

                dynamic squared = np.square(array);

                Console.WriteLine(mean);    // 30
                Console.WriteLine(std);     // 14.142...
                Console.WriteLine(max);     // 50
                Console.WriteLine(squared); // [100 400 900 1600 2500]
            }
        }

        public void Test10()
        {
            using (Py.GIL())
            {
                dynamic np = Py.Import("numpy");

                dynamic a = np.array(new double[] { 1, 2, 3 });
                dynamic b = np.array(new double[] { 4, 5, 6 });

                dynamic addition = a + b;
                dynamic multiplication = a * b;
                dynamic scaling = a * 10;

                Console.WriteLine(addition);       // [5 7 9]
                Console.WriteLine(multiplication); // [4 10 18]
                Console.WriteLine(scaling);        // [10 20 30]
            }
        }


        /// <summary>
        /// Executing Python code without a Python file
        /// You can create a Python scope and execute code directly from a C# string.
        /// </summary>
        public void Test11()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Exec(@"
def calculate(a, b):
    return a * b + 10
");

                dynamic calculate = scope.Get("calculate");

                int result = (int)calculate(5, 6);

                Console.WriteLine(result); // 40
            }
        }


        /// <summary>
        /// exchange variables between C# and Python
        /// </summary>
        public void Test12()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Set("x", 10);
                scope.Set("y", 20);

                scope.Exec("result = x * y");

                int result = scope.Get<int>("result");

                Console.WriteLine(result); // 200
            }
        }

        /// <summary>
        /// Handling Python exceptions in C#
        /// </summary>
        public void Test13()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic module = Py.Import("calculator2");

                try
                {
                    dynamic result = module.divide(10, 0);
                }
                catch (PythonException ex)
                {
                    Console.WriteLine(ex.Message);
                    // ex.Format() is especially useful because it provides
                    // the formatted Python exception, including its traceback.
                    Console.WriteLine(ex.Format());
                }
            }
        }

        /// <summary>
        /// Calling C# callbacks from Python
        /// Communication can also be bidirectional. You can pass a C# delegate to Python.
        /// 
        /// This is useful for progress reporting, logging, notifications 
        /// and integrating long-running Python operations into a C# application.
        /// </summary>
        public void Test14()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic module = Py.Import("calculator2");

                Action<int> callback = progress =>
                {
                    Console.WriteLine($"Progress: {progress}");
                };

                dynamic result = module.execute_task(callback);

                Console.WriteLine(result);
            }
        }

        /// <summary>
        /// Handling Python None
        /// </summary>
        public void Test15()
        {
            using (Py.GIL())
            {
                SetupPath();

                dynamic module = Py.Import("calculator2");

                dynamic result = module.find_device();
                if (result == null || result is PyObject pyObj && pyObj.IsNone())
                {
                    Console.WriteLine("Device not found");
                }
                else
                {
                    Console.WriteLine($"Device found: {result}");
                }

                //if (((PyObject)result).IsNone())
                //{
                //    Console.WriteLine("Device not found");
                //}
            }
        }

        /// <summary>
        /// Calling Python from multiple C# threads
        /// If you are developing a multithreaded application, such as WPF, WinForms or ASP.NET, release the initial GIL after initializing Python.
        /// Runtime.PythonDLL = @"C:\Python312\python312.dll";
        /// PythonEngine.Initialize();
        /// PythonEngine.BeginAllowThreads();
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double Calculate(double value)
        {
            using (Py.GIL())
            {
                dynamic math = Py.Import("math");

                return (double)math.sqrt(value);
            }
        }

        public async Task<double> Test16(double value)
        {
            double result = await Task.Run(() => Calculate(value));
            return result;
        }

        /// <summary>
        /// Encapsulating Python's dynamic objects inside strongly typed C# wrapper classes
        /// </summary>
        public void Test17()
        {
            // Recommended architecture for a C# application
            // For a larger application, I recommend encapsulating Python's dynamic objects
            // inside strongly typed C# wrapper classes.

            // The rest of your C# application does not need to know that Python is being used
            using var calculator = new PythonCalculator();

            int sum = calculator.Add(10, 20);
            int product = calculator.Multiply(5, 6);
        }

    }
}
