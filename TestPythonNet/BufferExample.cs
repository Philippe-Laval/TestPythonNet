using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    /// <summary>
    /// Shows how to pass array between python and C# without converting each value
    /// </summary>
    public class BufferExample
    {
        /// <summary>
        /// Basic example: C# → Python → C#
        /// </summary>
        public void TestPyBuffer()
        {
            /*
            Python.Runtime.PyBuffer is useful when you need to exchange binary data, images, 
            numerical arrays or other large buffers between C# and Python without converting 
            every element individually into a Python object.

            A typical workflow is:
            1. C# creates a byte[] containing data.
            2. C# transfers the data into a Python buffer.
            3. Python processes the buffer in place.
            4. C# retrieves the modified data using PyBuffer.
            The important distinction is that PyBuffer provides C# access to an existing Python buffer 
            through the Python buffer protocol. It is not itself a buffer allocator.
             */

            byte[] input = { 0, 50, 100, 200, 255 };
            byte[] output = new byte[input.Length];

            using (Py.GIL())
            {
                using var scope = Py.CreateScope();

                // 1. Allocate a Python bytearray
                scope.Set("size", input.Length);
                scope.Exec("buffer = bytearray(size)");

                // 2. Retrieve the Python object
                using PyObject pyArray = scope.Get("buffer");

                // 3. Get access to its underlying memory
                using PyBuffer pyBuffer = pyArray.GetBuffer();

                // 4. Copy C# data into Python memory
                pyBuffer.Write(input, 0, input.Length, 0);

                // 5. Process the buffer in Python (in place)
                scope.Exec(
"""
for i in range(len(buffer)):
    buffer[i] = 255 - buffer[i]
""");

                // 6. Copy Python data back into C#
                pyBuffer.Read(output, 0, output.Length, 0);
            }

            Console.WriteLine(string.Join(", ", input));
            Console.WriteLine(string.Join(", ", output));
        }

        /// <summary>
        /// Échange un tableau de valeurs `double` entre C# et un tableau NumPy via un tampon brut, applique des calculs
        /// NumPy en place, puis récupère les résultats en `double[]`.
        /// </summary>
        /// <remarks>Utilise `Buffer.BlockCopy` pour convertir entre `double[]` et `byte[]`, puis
        /// `PyBuffer.Write`/`PyBuffer.Read` pour transférer les données. Nécessite NumPy (`float64`) dans
        /// l’environnement Python.</remarks>
        public void TestBufferWithNumpy()
        {
            /*
More useful example: exchanging a double[] with NumPy
If you are working with numerical computations, signal processing or scientific data, 
you probably want to exchange arrays of double, float or int instead of bytes.

Since PyBuffer.Write() and PyBuffer.Read() operate on byte[], you can use Buffer.BlockCopy() 
to convert between raw bytes and numerical arrays.

Install NumPy in your Python environment:
pip install numpy 
 */

            try
            {
                double[] input = { 1.5, 2.5, 3.5, 4.5 };
                double[] output = new double[input.Length];

                int byteCount = input.Length * sizeof(double);

                byte[] rawData = new byte[byteCount];

                // Convert double[] to raw bytes
                Buffer.BlockCopy(input, 0, rawData, 0, byteCount);

                using (Py.GIL())
                {
                    using var scope = Py.CreateScope();

                    // Allocate a NumPy array
                    scope.Set("size", input.Length);

                    // Allocate a buffer in Python
                    scope.Exec(@"
import numpy as np

data = np.frombuffer(bytearray(size * 8), dtype=np.float64)
raw = data.base
");
                    // Get the Python object
                    using PyObject pyArray = scope.Get("raw");
                    // Get access to Python memory
                    using PyBuffer pyBuffer = pyArray.GetBuffer();

                    // Validate the buffer
                    if (pyBuffer.ReadOnly ||
                        !pyBuffer.IsContiguous(BufferOrderStyle.C) ||
                        pyBuffer.ItemSize != sizeof(byte) ||
                        pyBuffer.Length != input.Length * sizeof(double))
                    {
                        throw new InvalidOperationException(
                            "Invalid Python buffer");
                    }

                    // C# -> NumPy
                    pyBuffer.Write(rawData, 0, byteCount, 0);

                    // NumPy computation in place
                    //
                    // Correct — modifies the existing buffer: data *= 2
                    // Incorrect — creates a new array and discards the original: data = data * 2
                    //
                    // In the second case, the C# span still points to the original buffer, not the newly created NumPy array.
                    // Also, never access the span after disposing of the PyBuffer, and avoid concurrent access to the shared memory without synchronization.

                    scope.Exec(
"""
data *= 2
data += 10
""");

                    // NumPy -> C#
                    pyBuffer.Read(rawData, 0, byteCount, 0);
                }

                // Convert raw bytes back to double[]
                Buffer.BlockCopy(rawData, 0, output, 0, byteCount);

                Console.WriteLine(string.Join(", ", output));
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

        public void TestBufferWithNumpyAndSpan()
        {
            /*
             Here is a complete example using PyBuffer and Span<double> to exchange data between C# and Python using shared memory, without intermediate byte[] arrays.

            In this example:
            - Python allocates a NumPy array.
            - C# accesses its memory directly through a Span<double>.
            - C# writes values into the buffer.
            - Python modifies the values in place.
            - C# reads the modified values directly from the same memory.
             */

            try
            {
                using (Py.GIL())
                {
                    using var scope = Py.CreateScope();

                    // 1. Allocate a buffer in Python
                    scope.Exec(
"""
import numpy as np

data = np.frombuffer(bytearray(5 * 8), dtype=np.float64)
raw = data.base
""");

                    // 2. Get the underlying bytearray (shares memory with the NumPy array)
                    using PyObject pyArray = scope.Get("raw");

                    // 3. Get access to Python memory
                    using PyBuffer buffer = pyArray.GetBuffer();

                    // 4. Validate the buffer
                    if (buffer.ReadOnly ||
                        !buffer.IsContiguous(BufferOrderStyle.C) ||
                        buffer.Length != 5 * sizeof(double))
                    {
                        throw new InvalidOperationException(
                            "Invalid Python buffer");
                    }

                    // 5. Create a Span over Python memory
                    Span<double> span;
                    unsafe
                    {
                        span = new Span<double>(
                            buffer.Buffer.ToPointer(),
                            5
                        );
                    }

                    // 6. Write data directly from C#
                    for (int i = 0; i < span.Length; i++)
                    {
                        span[i] = i + 1;
                    }

                    // 7. Process data in Python
                    scope.Exec(@"
data *= 2
data += 10
");

                    // 8. Read the result directly from C#
                    Console.WriteLine("C# reads:");

                    foreach (double value in span)
                    {
                        Console.WriteLine(value);
                    }

                    // 9. Modify the buffer again from C#
                    span[0] = 999;

                    // 10. Verify that Python sees the modification
                    double first = scope.Eval<double>(
                        "float(data[0])"
                    );

                    Console.WriteLine($"Python reads: {first}");
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


        public void TestBufferWithNumpyAndSpan2()
        {
            /*
             Here is a complete example using PyBuffer and Span<double> to exchange data between C# and Python using shared memory, without intermediate byte[] arrays.

            In this example:
            - Python allocates a NumPy array.
            - C# accesses its memory directly through a Span<double>.
            - C# writes values into the buffer.
            - Python modifies the values in place.
            - C# reads the modified values directly from the same memory.
             */

            try
            {
                using (Py.GIL())
                {
                    using var scope = Py.CreateScope();

                    // 1. Allocate a buffer in Python
                    scope.Exec(
"""
import numpy as np

data = np.frombuffer(bytearray(5 * 8), dtype=np.float64)
raw = data.base
""");

                    // 2. Get the underlying bytearray (shares memory with the NumPy array)
                    using PyObject pyArray = scope.Get("raw");

                    using PyBuffer buffer = pyArray.GetBuffer();

                    Span<double> span = AsSpan<double>(buffer);

                    span.Fill(42.0);

                    scope.Exec("data *= 2");

                    Console.WriteLine(span[0]); // 84

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


        /*
        C# Span type	NumPy dtype	Bytes per element
        Span<byte>	    np.uint8	1
        Span<short>	    np.int16	2
        Span<int>	    np.int32	4
        Span<long>	    np.int64	8
        Span<float>	    np.float32	4
        Span<double>	np.float64	8
         */

        /// <summary>
        /// The helper checks memory layout and element size, 
        /// but the caller must still ensure that the actual Python data type matches T. 
        /// For example, an int64 and a float64 have the same element size but different 
        /// binary representations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static unsafe Span<T> AsSpan<T>(PyBuffer buffer) where T : unmanaged
        {
            if (buffer.ReadOnly)
                throw new InvalidOperationException(
                    "Buffer is read-only");

            if (!buffer.IsContiguous(BufferOrderStyle.C))
                throw new InvalidOperationException(
                    "Buffer is not contiguous");

            //if (buffer.ItemSize != sizeof(T))
            //    throw new InvalidOperationException(
            //        "Incompatible element size");

            if (buffer.Length % sizeof(T) != 0)
                throw new InvalidOperationException(
                    "Invalid buffer length");

            return new Span<T>(
                buffer.Buffer.ToPointer(),
                checked((int)(buffer.Length / sizeof(T)))
            );
        }
    }
}
