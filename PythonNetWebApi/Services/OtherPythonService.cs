using Python.Runtime;

namespace PythonNetWebApi.Services;

public interface IOtherPythonService
{
    string GetPythonVersion();
}

public class OtherPythonService : IOtherPythonService
{
    private readonly dynamic _sys;

    public OtherPythonService()
    {
        using (Py.GIL())
        {
            _sys = Py.Import("sys");
        }
    }

    public string GetPythonVersion()
    {
        using (Py.GIL())
        {
            return (string)_sys.version;
        }
    }
}
