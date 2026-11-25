using PolyglotSql.Bundle;
using PolyglotSql.Models;
using System.Text.Json;
using System.Text.Json.Nodes;
using static PolyglotSql.Models.BuilderOutput;

/*

Comparatif : Python.NET + SQLGlot vs PolyglotSql (.NET natif)


Critère	Python.NET + SQLGlot	PolyglotSql.Bundle (Rust FFI)
Déploiement	Nécessite un runtime Python (python-embed) et la gestion du ._pth	Simple package NuGet, binaires natifs copiés automatiquement
Concurrence (ASP.NET)	Exécution séquentielle bridée par le GIL Python (Py.GIL())	100 % multithread natif, sans blocage global
Gestion mémoire	Nécessite de libérer manuellement les PyObject (Dispose())	Géré automatiquement par le wrapper FFI
Maturité & couverture SQL	Référence absolue (toutes les fonctionnalités avancées d'AST, optimiseur, lineage)	Couvre 30+ dialectes, mais le wrapper .NET est plus récent (v0.2.x) et expose moins d'outils d'analyse d'AST poussés que l'original Python
 
 */


// Microsoft Visual C++ Redistributable latest supported downloads
// https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170

// Polyglot SQL .NET
// A .NET FFI wrapper for the Polyglot, a Rust SQL transpiler. Enables high-performance parsing, formatting, AST generation, and translation between more than 30 SQL dialects.
// https://github.com/KS-DB-Merge-Tools/polyglot-sql-net/blob/main/README.md
// look at INativePolyglot to see what is avaliable in the Polyglot API
// https://github.com/KS-DB-Merge-Tools/polyglot-sql-net/blob/main/src/PolyglotSql.Core/INativePolyglot.cs
// models : DiffEdit
// https://github.com/KS-DB-Merge-Tools/polyglot-sql-net/tree/main/src/PolyglotSql.Core/Models

namespace PolyglotSql
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialiser le fournisseur natif embarqué (obligatoire une seule fois)
            BundleInitializer.Initialize();

            // 2. Exécuter vos transpilations normalement

            string mysqlQuery = "SELECT `id`, `name` FROM `person` LIMIT 10;";

            // Transpilation directe de MySQL vers T-SQL
            string[] tsqlQueries = Polyglot.Transpile(
                mysqlQuery,
                Dialect.MySQL,
                Dialect.TSQL
            );

            Console.WriteLine(tsqlQueries[0]);
            // Résultat : SELECT TOP 10 [id], [name] FROM [person]

            var options = new TranspileOptions { Pretty = true };
            string? result = Polyglot
                .TranspileWithOptions("SELECT `a` FROM `t`", Dialect.MySQL, Dialect.TSQL, options)
                .FirstOrDefault();

            Console.WriteLine(result);


            string sql = "SELECT [a], b FROM t";
            var dialect = Models.Dialect.TSQL;

            Test1(sql, dialect);
            Test2(sql, dialect);
            Test3();
            Test4();
        }

        private static void Test1(string sql, Dialect dialect)
        {
            Expression expression = Polyglot.ParseOne(sql, dialect);

            expression.TransformAll(node =>
            {
                if (node is JsonObject obj && obj.ContainsKey("quoted"))
                {
                    obj["quoted"] = true;
                }
            });

            var generated = Polyglot.GenerateOne(expression, dialect); // SELECT [a], [b] FROM [t]
            Console.WriteLine(generated);
        }

        private static void Test2(string sql, Dialect dialect)
        {
            var parsedExpression = Polyglot.ParseOne(sql, dialect);
            JsonNode? rootNode = JsonNode.Parse(parsedExpression.ToJsonString());
            // ..
            // .. modify rootNode or its children ..
            // ..
            string modifiedJson = rootNode!.ToJsonString();
            var modifiedExpression = JsonSerializer.Deserialize(
                modifiedJson, PolyglotJsonContext.Default.Expression);
            string[] result = Polyglot.Generate(new[] { modifiedExpression }, dialect);
        }

        private static void Test3()
        {
            var plan = BuilderPlan.FromJson(@"{
	            ""base"": { ""kind"": ""select"", ""expressions"": [ { ""kind"": ""column"", ""name"": ""a"" } ] },
	            ""operations"": [
		            { ""kind"": ""from"", ""source"": { ""kind"": ""table"", ""name"": ""t"" } },
		            { ""kind"": ""limit"", ""expression"": { ""kind"": ""literal"", ""value"": { ""kind"": ""integer"", ""value"": 10 } } }
	            ]
            }");

            string sql = Polyglot.BuildSql(plan, Dialect.Generic, Dialect.TSQL); // SELECT TOP 10 a FROM t
           Console.WriteLine(sql);

            Expression ast = Polyglot.BuildAst(plan);
            Console.WriteLine(ast.ToJsonString());
        }

        private static void Test4()
        {
            string sql1 = "SELECT [a] + [b] FROM t";
            string sql2 = "SELECT [a] - [b] FROM t";

            string[] o1 = Polyglot.Optimize(sql1, Dialect.TSQL);
            string[] o2 = Polyglot.Optimize(sql2, Dialect.TSQL);



            DiffResult result = Polyglot.Diff(o1[0], o2[0], Dialect.TSQL);

            Console.WriteLine(result.AreEqual);

            foreach (var r in result.Edits)
            {
                Console.WriteLine(r.ToString());
            }

        }


    }
}
