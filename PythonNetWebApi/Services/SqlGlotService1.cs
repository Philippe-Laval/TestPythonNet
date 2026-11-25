using System;
using System.Collections.Generic;
using System.IO;
using Python.Runtime;

namespace PythonNetWebApi.Services;

public interface ISqlGlotService
{
    string Transpile(string sql, string readDialect, string writeDialect);
    List<string> ExtractColumns(string sql, string dialect = "");
}

public class SqlGlotService1 : ISqlGlotService
{
    private readonly dynamic _sqlglot;
    private readonly dynamic _expressions;
    private IntPtr _threadState;

    public SqlGlotService1()
    {
        // Acquire the GIL briefly to import and cache Python modules
        using (Py.GIL())
        {
            _sqlglot = Py.Import("sqlglot");
            _expressions = Py.Import("sqlglot.expressions");
        }
    }

    public string Transpile(string sql, string readDialect, string writeDialect)
    {
        // Chaque thread HTTP acquiert le verrou Python le temps de l'exécution
        using (Py.GIL())
        {
            dynamic resultList = _sqlglot.transpile(
                sql,
                read: readDialect,
                write: writeDialect
            );

            // sqlglot.transpile retourne une liste Python de requêtes
            return resultList[0].ToString();
        }
    }

    public List<string> ExtractColumns(string sql, string dialect = "")
    {
        var columns = new List<string>();

        using (Py.GIL())
        {
            dynamic parsed = string.IsNullOrWhiteSpace(dialect)
                ? _sqlglot.parse_one(sql)
                : _sqlglot.parse_one(sql, read: dialect);

            foreach (dynamic col in parsed.find_all(_expressions.Column))
            {
                columns.Add((string)col.name);
            }
        }

        return columns;
    }
}
