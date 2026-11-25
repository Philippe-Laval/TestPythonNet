using System;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace TestIronPython
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test1();
            Test2();
            Test3();
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

    }
}
