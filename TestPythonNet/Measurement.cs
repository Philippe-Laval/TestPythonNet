using System;
using System.Collections.Generic;
using System.Text;

namespace TestPythonNet
{
    public class Measurement
    {
        public string SensorName { get; set; } = "";
        public double RawValue { get; set; }
        public double CorrectedValue { get; set; }
        public bool IsValid { get; set; }

        public void ApplyCalibration(double gain, double offset)
        {
            CorrectedValue = RawValue * gain + offset;
        }

        public override string ToString()
        {
            return $"{SensorName}: Raw={RawValue}, " +
                   $"Corrected={CorrectedValue}, Valid={IsValid}";
        }
    }
}
