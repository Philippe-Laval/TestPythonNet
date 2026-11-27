using Python.Runtime;

namespace TestPythonNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Indiquer le chemin vers la DLL Python (à adapter selon votre OS/version)
            // Windows : @"C:\Users\VotreNom\AppData\Local\Programs\Python\Python312\python312.dll"
            // Linux   : "/usr/lib/x86_64-linux-gnu/libpython3.12.so"
            // macOS   : "/Library/Frameworks/Python.framework/Versions/3.12/lib/libpython3.12.dylib"
            Runtime.PythonDLL = @"C:\Python314\python314.dll";

            // 2. Initialiser le moteur Python
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            // Enable GIL debugging assistance
            //PythonEngine.DebugGIL = true;


            PrimitivesExample primitivesExample = new PrimitivesExample();
            primitivesExample.TestPyTuple();
            primitivesExample.TestPyList();
            primitivesExample.TestPyDict();

            PythonNetExample example = new PythonNetExample();

            example.TestChangeSysPathThenInstanciatePythonObjectAndUseIt();
            example.TestImportNumpy();
            example.TestToPython();
            example.ExecutePythonCode();

            SqlglotExample sqlglotExample = new SqlglotExample();
            sqlglotExample.ExecuteSqlglot2();
            sqlglotExample.ExecuteSqlglot();

            PySequenceExample pySequenceExample = new PySequenceExample();
            pySequenceExample.TestPySequence1();
            pySequenceExample.TestPySequence2();
            pySequenceExample.TestPySequence3();
            pySequenceExample.TestPySequence4();
            pySequenceExample.TestPySequence5();
            pySequenceExample.TestPySequence6();
            pySequenceExample.TestPySequence7();
            pySequenceExample.TestPySequence8();
            pySequenceExample.TestPySequence9();
            pySequenceExample.TestPySequence10();
            pySequenceExample.TestPySequence11();
            pySequenceExample.TestPySequence12();

            ExecOrEvalExample execOrEvalExample = new ExecOrEvalExample();
            execOrEvalExample.TestExec();
            execOrEvalExample.TestEval();
            execOrEvalExample.TestExecuteWithEval();
            execOrEvalExample.TestExecuteWithFile();

            NewScopeExample newScopeExample = new NewScopeExample();
            newScopeExample.TestNewScope();

            ImportExample importExample = new ImportExample();
            importExample.TestImportName();
            importExample.TestImportPyModule();
            importExample.TestImportPyObject();
            importExample.TestImportAll();

            BufferExample bufferExample = new BufferExample();
            bufferExample.TestPyBuffer();
            bufferExample.TestBufferWithNumpy();
            bufferExample.TestBufferWithNumpyAndSpan();
            bufferExample.TestBufferWithNumpyAndSpan2();

            ExchangeExample exchangeExample = new ExchangeExample();
            exchangeExample.TestUsingASharpObjectWithPython();
            exchangeExample.TestCreatingANewCSharpObjectFromPython();
            exchangeExample.CallPythonFonctionWithCSharpObject();
            exchangeExample.PassingListOfCSharpObjectsToPython();
            exchangeExample.ConvertingANativePythonObjectToCsharp();
            exchangeExample.TestUsingPythonObjectsFromCsharp();
            exchangeExample.TestSimplerAccessUsingDynamic();
            exchangeExample.TestConvertAPythonObjectToACharpObject();


            try
            {
                // Shutdown the Python engine
                PythonEngine.Shutdown();
            }
            catch (PythonException ex)
            {
                Console.WriteLine("Une erreur s'est produite lors de l'arrêt du moteur Python :");
                Console.WriteLine(ex.Message);
            }
        }

        static void TestLauchingPythonWithProcess()
        {
            BokehPlot bokeh = new BokehPlot(filePath: "C:/myplots/myplot");

            var pointsA = new (double, double)[] { (1, 2), (2, 4), (3, 6) };
            var pointsB = new (double, double)[] { (1, 3), (2, 6), (3, 9) };
            var line = new (double, double)[] { (0, 0), (4, 8) };
            var polygon = new (double, double)[] { (1, 1), (1, 4), (4, 4), (4, 1) };

            bokeh.AddPoints(pointsA, layerName: "points-A", color: "red");
            bokeh.AddPoints(pointsB, layerName: "points-B", color: "blue");
            bokeh.AddLine(line, layerName: "line", color: "green");
            bokeh.AddPolygon(polygon, layerName: "polygon", color: "yellow");

            bokeh.AddAxis("x", new double[] { 0, 1, 2, 3, 4 }, "X Axis");
            bokeh.AddAxis("y", new double[] { 0, 2, 4, 6, 8 }, "Y Axis");

            bokeh.Show();
        }
    }
}
