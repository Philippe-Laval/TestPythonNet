using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    public class ImportExample
    {
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
    }
}
