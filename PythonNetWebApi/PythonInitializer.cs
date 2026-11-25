using Python.Runtime;

/*
Important Rules for Multi-Service Architectures

- Never call BeginAllowThreads() inside individual services: 
It must only be called once right after PythonEngine.Initialize() in Program.cs. 
Calling it multiple times will corrupt Python's thread state.

- Keep services as Singleton: 
Importing Python modules (Py.Import) has overhead and holds references to Python objects (PyObject). 
Registering Python-backed services as AddSingleton ensures modules are imported once 
and prevents memory leaks from uncollected dynamic PyObject references across scoped requests. 
 */

namespace PythonNetWebApi
{
    public static class PythonInitializer
    {
        public static void InitializePython()
        {
            // 1. Locate the embedded Python DLL
            string baseDir = AppContext.BaseDirectory;
            string pythonHome = Path.Combine(baseDir, "python-embed");
            string pythonDll = Path.Combine(pythonHome, "python314.dll");

            if (!File.Exists(pythonDll))
                throw new FileNotFoundException($"Runtime Python introuvable : {pythonDll}");

            // 2. Configure paths and start the Python engine ONCE
            Runtime.PythonDLL = pythonDll;
            PythonEngine.PythonHome = pythonHome;

            // 3. Démarrer le moteur Python
            PythonEngine.Initialize();

            // 4. TRÈS IMPORTANT : Relâcher le GIL pour permettre aux threads ASP.NET de l'acquérir
            PythonEngine.BeginAllowThreads();
        }
    }
}
