using System;
using System.Collections.Generic;
using System.Text;
using Python.Runtime;

// https://github.com/pythonnet/pythonnet#embedding-python-in-net

// pip install sqlglot
// dotnet add package pythonnet

// C:\Users\philippe.laval\AppData\Roaming\Python\Python314\site-packages

namespace TestPythonNet
{
    public class PythonNetExample
    {
        public void ExecutePythonCode()
        {

            using (Py.GIL())
            {
                // Import the Python file
                dynamic calculatorModule = Py.Import("pythonExample");

                // Create an instance of the Calculator class
                dynamic calculator = calculatorModule.Calculator();

                // Call the AddInPython method
                int result = calculator.AddInPython(5, 10);
                Console.WriteLine($"Result of adding in Python: {result}");
            }

        }


        public void ExecuteSqlglot()
        {
            try
            {
                // 3. Acquérir le Global Interpreter Lock (GIL) avant tout appel Python
                using (Py.GIL())
                {
                    // Utilisation d'un environnement virtuel (venv).
                    // Si sqlglot est installé dans un venv plutôt que dans le Python global,
                    // ajoutez le dossier site-packages au sys.path juste après PythonEngine.Initialize() 
                    //
                    //dynamic sys = Py.Import("sys");
                    //sys.path.append(@"C:\chemin\vers\votre\venv\Lib\site-packages");

                    // Importer le module sqlglot
                    dynamic sqlglot = Py.Import("sqlglot");

                    // --- Exemple A : Transpilation SQL ---
                    string sqlSource = "SELECT EPOCH_MS(1618088028295) AS ts, ARRAY[1, 2, 3] AS arr";

                    // Appel à sqlglot.transpile(sql, read="duckdb", write="tsql")[0]
                    dynamic transpiledList = sqlglot.transpile(
                        sqlSource,
                        read: "duckdb",
                        write: "tsql"
                    );

                    string tsqlResult = transpiledList[0].ToString();
                    Console.WriteLine("SQL transpilé (T-SQL) :");
                    Console.WriteLine(tsqlResult);

                    // --- Exemple B : Parsing et inspection de l'AST ---
                    dynamic exp = Py.Import("sqlglot.expressions");
                    dynamic parsed = sqlglot.parse_one("SELECT a, b FROM ma_table WHERE c = 1");

                    Console.WriteLine("\nColonnes détectées :");
                    foreach (dynamic col in parsed.find_all(exp.Column))
                    {
                        Console.WriteLine($"- {col.name}");
                    }
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine("Une erreur s'est produite lors de l'exécution du code Python :");
                Console.WriteLine(ex.Message);
            }
        }


        public void ExecuteSqlglot2()
        {
            try
            {
                // 3. Acquérir le Global Interpreter Lock (GIL) avant tout appel Python
                using (Py.GIL())
                {
                    // Utilisation d'un environnement virtuel (venv).
                    // Si sqlglot est installé dans un venv plutôt que dans le Python global,
                    // ajoutez le dossier site-packages au sys.path juste après PythonEngine.Initialize() 
                    //
                    //dynamic sys = Py.Import("sys");
                    //sys.path.append(@"C:\chemin\vers\votre\venv\Lib\site-packages");

                    // Importer le module sqlglot
                    dynamic sqlglot = Py.Import("sqlglot");

                    dynamic where = sqlglot.condition("x=1").and_("y=1");
                    var sql = sqlglot.select("*").from_("y").where(where).sql();

                    sql = sqlglot.parse_one("SELECT x FROM y").from_("z").sql();


                    dynamic result = sqlglot.diff(sqlglot.parse_one("SELECT a + b, c, d"), sqlglot.parse_one("SELECT c, a - b, d"));
                    foreach (dynamic a in result)
                    {
                        Console.WriteLine($"{a}");
                    }


                    result = sqlglot.diff(
                        sqlglot.parse_one("SELECT [a] + [b] FROM t", read: "tsql"),
                        sqlglot.parse_one("SELECT [a] - [c] FROM t", read: "tsql"));


                    foreach(dynamic a in result)
                    {
                        Console.WriteLine($"{a}");
                    }
                }
            }
            catch (PythonException ex)
            {
                Console.WriteLine("Une erreur s'est produite lors de l'exécution du code Python :");
                Console.WriteLine(ex.Message);
            }
        }

    }
}
