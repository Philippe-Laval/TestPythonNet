using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    public class MeasurementResult
    {
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public double Average { get; set; }

        public static MeasurementResult ConvertResult(PyObject obj)
        {
            using (Py.GIL())
            {
                using var min = obj.GetAttr("minimum");
                using var max = obj.GetAttr("maximum");
                using var avg = obj.GetAttr("average");

                return new MeasurementResult
                {
                    Minimum = min.As<double>(),
                    Maximum = max.As<double>(),
                    Average = avg.As<double>()
                };
            }
        }
    }
}
