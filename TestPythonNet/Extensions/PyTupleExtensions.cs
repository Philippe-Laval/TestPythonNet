using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet.Extensions;

public static class PyTupleExtensions
{
    public static void Deconstruct(
        this PyTuple tuple,
        out dynamic first,
        out dynamic second)
    {
        first = tuple[0];
        second = tuple[1];
    }

    public static void Deconstruct(
        this PyTuple tuple,
        out dynamic first,
        out dynamic second,
        out dynamic third)
    {
        first = tuple[0];
        second = tuple[1];
        third = tuple[2];
    }

    public static void Deconstruct(
        this PyTuple tuple,
        out dynamic first,
        out dynamic second,
        out dynamic third,
        out dynamic fourth)
    {
        first = tuple[0];
        second = tuple[1];
        third = tuple[2];
        fourth = tuple[3];
    }
}
