using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    public class PrimitivesExample
    {
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

        public void TestPyDict()
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

            }
        }

    }
}
