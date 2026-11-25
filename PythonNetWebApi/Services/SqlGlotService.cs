using Python.Runtime;

namespace PythonNetWebApi.Services;

public class SqlGlotService : PythonServiceBase, ISqlGlotService
{
    private readonly dynamic _sqlglot;
    private readonly dynamic _expressions;

    public SqlGlotService()
    {
        _sqlglot = ImportModule("sqlglot");
        _expressions = ImportModule("sqlglot.expressions");
    }

    public string Transpile(string sql, string readDialect, string writeDialect)
    {
        return RunWithGil(() =>
        {
            // Wrap intermediate PyObjects in 'using' so they don't linger until GC
            using PyObject resultList = _sqlglot.transpile(sql, read: readDialect, write: writeDialect);
            return resultList.ToString();
        }) ?? "";
    }

    public List<string> ExtractColumns(string sql, string dialect = "")
    {
        return RunWithGil(() =>
        {
            var columns = new List<string>();

            // 1. Keep 'parsed' as dynamic so C# can resolve Python's .find_all() method at runtime
            dynamic parsed = string.IsNullOrWhiteSpace(dialect)
                ? _sqlglot.parse_one(sql)
                : _sqlglot.parse_one(sql, read: dialect);

            try
            {
                // 2. Keep 'foundColumns' as dynamic so C# can use Python's iterator protocol in foreach
                dynamic foundColumns = parsed.find_all(_expressions.Column);
                try
                {
                    foreach (dynamic col in foundColumns)
                    {
                        try
                        {
                            columns.Add((string)col.name);
                        }
                        finally
                        {
                            ((PyObject)col).Dispose();
                        }
                    }
                }
                finally
                {
                    ((PyObject)foundColumns).Dispose();
                }
            }
            finally
            {
                ((PyObject)parsed).Dispose();
            }

            return columns;
        });
    }


    public List<string> ExtractColumns2(string sql, string dialect = "")
    {
        return RunWithGil(() =>
        {
            var columns = new List<string>();

            dynamic parsed = string.IsNullOrWhiteSpace(dialect)
                ? _sqlglot.parse_one(sql)
                : _sqlglot.parse_one(sql, read: dialect);

            using (parsed as IDisposable)
            {
                // Convert Python generator to a PyList so we can index it cleanly
                using var foundColumns = new PyList(parsed.find_all(_expressions.Column));
                foreach (PyObject colObj in foundColumns)
                {
                    using (colObj)
                    {
                        dynamic col = colObj;
                        columns.Add((string)col.name);
                    }
                }
            }

            return columns;
        });
    }

}
