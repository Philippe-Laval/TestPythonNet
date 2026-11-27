using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    public class NewScopeExample
    {
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
    }
}
