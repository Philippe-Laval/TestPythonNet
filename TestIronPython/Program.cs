using System;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

// Triggering Python Code from C#: A Practical Guide
// Compare differentes possibilités :
// - Process
// - Python.NET (pythonnet)
// - IronPython
// - Hosting a Python Server (e.g., FastAPI)

// https://medium.com/@hanxuyang0826/triggering-python-code-from-c-a-practical-guide-84b17d593dc6

namespace TestIronPython
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test1();
            Test2();
            //Test3();
            //Test4();

            ExecutePythonCode();
        }

        static void Test1()
        {
            // Create a Python engine
            var engine = Python.CreateEngine();

            // Execute Python code
            var scope = engine.CreateScope();
            engine.Execute("x = 5", scope);
            engine.Execute("result = x**2", scope);

            // Retrieve the result from Python
            dynamic result = scope.GetVariable("result");

            // Print the result
            Console.WriteLine($"The square of 5 is {result}");
        }

        static void Test2()
        {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();

            // Load the referenced .NET assembly directly; this avoids DLL path-resolution issues
            engine.Runtime.LoadAssembly(typeof(DotNetLibrary.Calculator).Assembly);

            // Create an instance of the Calculator class
            engine.Execute("from DotNetLibrary import Calculator", scope);
            engine.Execute("calculator = Calculator()", scope);

            // Call the Add method
            // Note that we can setup a breakpoint in debugger in the Add method.
            var result = engine.Execute<int>("calculator.Add(10, 5)", scope);

            Console.WriteLine($"10 + 5 = {result}");
        }

        static void Test3()
        {
            using var httpClient = new System.Net.Http.HttpClient();
            var content = httpClient
                .GetStringAsync("https://jsonplaceholder.typicode.com/posts/1")
                .GetAwaiter()
                .GetResult();

            Console.WriteLine(content);
        }

        static void Test4()
        {
            var engine = Python.CreateEngine();

            var searchPaths = engine.GetSearchPaths();
            searchPaths.Add(Path.Combine(
                AppContext.BaseDirectory,
                "Lib"));

            engine.SetSearchPaths(searchPaths);

            var scope = engine.CreateScope();
            engine.Execute("import requests", scope);
            engine.Execute(
                "response = requests.get('https://jsonplaceholder.typicode.com/posts/1')",
                scope);

            var content = engine.Execute<string>("response.text", scope);
            Console.WriteLine(content);
        }

        public static void ExecutePythonCode()
        {
            // Create the IronPython scripting engine
            ScriptEngine engine = Python.CreateEngine();

            // Add the directory containing the Python file to the search paths
            ICollection<string> searchPaths = engine.GetSearchPaths();
            searchPaths.Add(Path.Combine(
                AppContext.BaseDirectory,
                "PythonFiles"));
            engine.SetSearchPaths(searchPaths);

            // Import the Python file
            dynamic calculatorModule = engine.ImportModule("pythonExample");

            // Create an instance of the Calculator class
            dynamic calculator = calculatorModule.Calculator();

            // Call the AddInPython method
            int result = calculator.AddInPython(5, 10);
            Console.WriteLine($"Result of adding in Python: {result}");
        }

    }
}
