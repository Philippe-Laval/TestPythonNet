using System;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace TestPythonNet;

public class BokehPlot
{
    private StringBuilder script;
    private string filePath;

    public BokehPlot(string filePath)
    {
        this.filePath = filePath;
        script = new StringBuilder();
        script.AppendLine("from bokeh.plotting import figure, output_file, show");
        script.AppendLine($"output_file('{filePath}')");
        script.AppendLine("p = figure(title='Interactive Bokeh Plot')");
    }

    public void AddPoints((double, double)[] points, string layerName, string color)
    {
        script.AppendLine($"p.scatter(x={string.Join(",", points.Select(p => p.Item1))}, y={string.Join(",", points.Select(p => p.Item2))}, legend_label='{layerName}', fill_color='{color}', size=8)");
    }

    public void AddLine((double, double)[] points, string layerName, string color)
    {
        script.AppendLine($"p.line(x={string.Join(",", points.Select(p => p.Item1))}, y={string.Join(",", points.Select(p => p.Item2))}, legend_label='{layerName}', line_color='{color}', line_width=2)");
    }

    public void AddPolygon((double, double)[] points, string layerName, string color)
    {
        script.AppendLine($"p.patch(x={string.Join(",", points.Select(p => p.Item1))}, y={string.Join(",", points.Select(p => p.Item2))}, legend_label='{layerName}', fill_color='{color}', fill_alpha=0.6)");
    }

    public void AddAxis(string axisType, double[] positions, string layerName)
    {
        script.AppendLine($"p.{axisType}_axis.axis_label = '{layerName}'");
        script.AppendLine($"p.{axisType}_axis.ticker = {string.Join(",", positions)}");
    }

    public void Show()
    {
        script.AppendLine("show(p)");
        string scriptPath = "bokeh_script.py";
        File.WriteAllText(scriptPath, script.ToString());

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = scriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process? process = Process.Start(start))
        {
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the Python process.");
            }

            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.WriteLine(result);
            }
        }
    }
}
