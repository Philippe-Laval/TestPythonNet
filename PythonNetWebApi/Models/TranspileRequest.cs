namespace PythonNetWebApi.Models;

public record TranspileRequest(string Sql, string FromDialect, string ToDialect);
