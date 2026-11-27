using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

/*
Summary of PySequence methods

C# method	Python equivalent
IsSequenceType(obj)	Sequence protocol check
Length()	len(seq)
GetItem(i)	seq[i]
SetItem(i, value)	seq[i] = value
GetSlice(i, j)	seq[i:j]
SetSlice(i, j, obj)	seq[i:j] = obj
DelSlice(i, j)	del seq[i:j]
Contains(obj)	obj in seq
Index(obj)	seq.index(obj)
Index32(obj)	seq.index(obj)
Index64(obj)	seq.index(obj)
Concat(obj)	seq + obj
Repeat(n)	seq * n 
*/

namespace TestPythonNet
{
    public class PySequenceExample
    {
        public void TestPySequence1()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using PyObject obj = scope.Eval("[10, 20, 30, 40, 50]");
                using var sequence = new PySequence(obj);

                Console.WriteLine(sequence);
                Console.WriteLine(sequence.Length());
            }
        }

        public void TestPySequence2()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var list = scope.Eval("[1, 2, 3]");
                using var tuple = scope.Eval("(1, 2, 3)");
                using var text = scope.Eval("'Hello'");
                using var range = scope.Eval("range(10)");
                using var number = scope.Eval("42");
                using var dict = scope.Eval("{'x': 10}");

                Console.WriteLine(PySequence.IsSequenceType(list));
                Console.WriteLine(PySequence.IsSequenceType(tuple));
                Console.WriteLine(PySequence.IsSequenceType(text));
                Console.WriteLine(PySequence.IsSequenceType(range));
                Console.WriteLine(PySequence.IsSequenceType(number));
                Console.WriteLine(PySequence.IsSequenceType(dict));
            }
        }

        public void TestPySequence3()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[10, 20, 30, 40, 50]");
                using var sequence = new PySequence(obj);

                // python equivalent: slice = sequence[1:4]
                using var slice = sequence.GetSlice(1, 4);

                Console.WriteLine(slice);
            }
        }

        public void TestPySequence4()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[10, 20, 30, 40, 50]");
                using var sequence = new PySequence(obj);

                using var replacement = scope.Eval("[200, 300]");

                // python equivalent: sequence[1:3] = [200, 300]
                sequence.SetSlice(1, 3, replacement);

                Console.WriteLine(sequence);
                Console.WriteLine(obj);
            }
        }

        public void TestPySequence5()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[10, 20, 30, 40, 50]");
                using var sequence = new PySequence(obj);

                // python equivalent: del sequence[1:4]
                sequence.DelSlice(1, 4);

                Console.WriteLine(sequence);
            }
        }

        public void TestPySequence6()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("['red', 'green', 'blue']");
                using var sequence = new PySequence(obj);

                using var green = new PyString("green");
                using var yellow = new PyString("yellow");

                // "green" in sequence
                Console.WriteLine(sequence.Contains(green));
                Console.WriteLine(sequence.Contains(yellow));
            }
        }

        public void TestPySequence7()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[10, 20, 30, 20, 50]");
                using var sequence = new PySequence(obj);

                using var value = new PyInt(20);

                // Python equivalent: index = sequence.index(20)
                nint index = sequence.Index(value);
                int index32 = sequence.Index32(value);
                long index64 = sequence.Index64(value);

                Console.WriteLine(index);
                Console.WriteLine(index32);
                Console.WriteLine(index64);

                using var value2 = new PyInt(999);

                int index2 = sequence.Contains(value2)
                    ? sequence.Index32(value2)
                    : -1;
                Console.WriteLine(index2);
            }
        }

        public void TestPySequence8()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj1 = scope.Eval("[10, 20, 30]");
                using var obj2 = scope.Eval("[40, 50]");

                using var sequence = new PySequence(obj1);

                using var result = sequence.Concat(obj2);

                Console.WriteLine(result);
            }
        }

        public void TestPySequence9()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[1, 2, 3]");
                using var sequence1 = new PySequence(obj);

                using var result1 = sequence1.Repeat(3);

                Console.WriteLine(result1);


                using var text = new PyString("ABC");
                using var sequence2 = new PySequence(text);

                using var result2 = sequence2.Repeat(3);

                Console.WriteLine(result2); // ABCABCABC
            }
        }

        public void TestPySequence10()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[10, 20, 30]");
                using var sequence = new PySequence(obj);

                // Read an element
                using var item = sequence.GetItem(1);

                int value = item.As<int>();

                Console.WriteLine(value); // 20

                // Modify an element
                using var replacement = new PyInt(999);

                sequence.SetItem(1, replacement);

                Console.WriteLine(sequence); // [10, 999, 30]
            }
        }

        public void TestPySequence11()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                using var obj = scope.Eval("[10, 20, 30, 40]");
                using var sequence = new PySequence(obj);

                foreach (PyObject item in sequence)
                {
                    using (item)
                    {
                        int value = item.As<int>();

                        Console.WriteLine(value);
                    }
                }
            }
        }

        public void TestPySequence12()
        {
            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                scope.Exec(@"
def get_temperatures():
    return [21.5, 22.1, 23.4, 24.2, 22.8, 21.9]
");

                // Retrieve measurements from Python
                using var obj = scope.Eval("get_temperatures()");
                using var sequence = new PySequence(obj);

                // Select measurements 1 to 3
                using var slice = sequence.GetSlice(1, 4);
                using var selected = new PySequence(slice);

                double sum = 0;
                int count = 0;

                foreach (PyObject item in selected)
                {
                    using (item)
                    {
                        sum += item.As<double>();
                        count++;
                    }
                }

                double average = sum / count;

                Console.WriteLine($"Measurements: {selected}");
                Console.WriteLine($"Average: {average:F2}");

                // Send the result back to Python
                scope.Set("average", average);

                scope.Exec("is_too_hot = average > 23");

                bool isTooHot = scope.Get<bool>("is_too_hot");

                Console.WriteLine($"Too hot: {isTooHot}");
            }
        }
    }
}
