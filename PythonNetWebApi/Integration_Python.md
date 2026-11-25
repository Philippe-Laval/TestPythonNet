# Integration de SQLGlot dans une API ASP.NET Core avec PythonNet

Pour intégrer SQLGlot dans une API ASP.NET Core prête pour la production sans dépendre 
d'une installation Python externe, il faut combiner un Singleton thread-safe 
(gérant le GIL Python) et un package Python Embeddable copié lors de la compilation.


1. Préparer et embarquer Python (Embeddable Package)

L'objectif est d'inclure un dossier python-embed autonome directement dans le répertoire de sortie (bin/ et publish/) de votre application .NET.

Étape A : Télécharger et préparer le bundle (exemple Windows x64)

1. Téléchargez le Windows embeddable package (64-bit) depuis python.org (ex. Python 3.14.10) et extrayez-le dans un dossier python-embed/ à la racine de votre projet .NET.
2. Activer site-packages (étape cruciale) : Ouvrez le fichier python314._pth dans ce dossier et décommentez la ligne import site 
(si on veut utiliser "C:\Users\philippe.laval\AppData\Roaming\Python\Python314\site-packages" 
sinon mettre Lib\site-packages dans le fichier pour etre isolé) :

```text
python314.zip
.

# Uncomment to run site.main() automatically
#import site
Lib\site-packages
```

3. Installer pip et sqlglot dans ce dossier :powershell

```powershell
# Télécharger get-pip.py dans le dossier python-embed
cd C:\CodeGithubPerso\TestPythonNet\PythonNetWebApi
curl -L -o python-embed\get-pip.py https://bootstrap.pypa.io/get-pip.py

# Alternativement, vous pouvez installer sqlglot directement dans le dossier python-embed\Lib\site-packages
# Remplacez "python" par le chemin vers votre installation Python habituelle
python -m pip install sqlglot --target=.\python-embed\Lib\site-packages

# Installer pip puis sqlglot directement dans le Python embarqué
.\python-embed\python.exe .\python-embed\get-pip.py
.\python-embed\python.exe -m pip install sqlglot --target=.\python-embed\Lib\site-packages --upgrade --no-warn-script-location

# Verify isolation
.\python-embed\python.exe -c "import sqlglot; print(sqlglot.__file__)"
```


Étape B : Configurer le .csproj pour copier le dossier au build

Ajoutez ces lignes dans votre fichier .csproj afin que le dossier python-embed suive automatiquement l'application lors du dotnet build et dotnet publish :

```xml
<ItemGroup>
  <None Include="python-embed\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```


2. Encapsuler SQLGlot dans un service Singleton ASP.NET Core

En ASP.NET Core, les requêtes HTTP arrivent en parallèle sur plusieurs threads. Or, Python utilise un GIL (Global Interpreter Lock).
La règle d'or :
• Initialiser le moteur une seule fois dans le constructeur du Singleton, puis relâcher le thread principal avec PythonEngine.BeginAllowThreads().
• Acquérir le verrou via using (Py.GIL()) à chaque appel de méthode.

Services/SqlGlotService.cs

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Python.Runtime;

public interface ISqlGlotService
{
    string Transpile(string sql, string readDialect, string writeDialect);
    List<string> ExtractColumns(string sql, string dialect = "");
}

public class SqlGlotService : ISqlGlotService
{
    private readonly dynamic _sqlglot;
    private readonly dynamic _expressions;
    private IntPtr _threadState;

    public SqlGlotService()
    {
        // 1. Localiser la DLL Python embarquée dans le dossier de l'application
        string baseDir = AppContext.BaseDirectory;
        string pythonHome = Path.Combine(baseDir, "python-embed");
        string pythonDll = Path.Combine(pythonHome, "python312.dll");

        if (!File.Exists(pythonDll))
            throw new FileNotFoundException($"Runtime Python introuvable : {pythonDll}");

        // 2. Configurer les chemins avant l'initialisation
        Runtime.PythonDLL = pythonDll;
        PythonEngine.PythonHome = pythonHome;

        // 3. Démarrer le moteur Python
        PythonEngine.Initialize();

        // 4. Importer les modules une seule fois et les mettre en cache
        using (Py.GIL())
        {
            _sqlglot = Py.Import("sqlglot");
            _expressions = Py.Import("sqlglot.expressions");
        }

        // 5. TRÈS IMPORTANT : Relâcher le GIL pour permettre aux threads ASP.NET de l'acquérir
        _threadState = PythonEngine.BeginAllowThreads();
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

```


3. Enregistrement et utilisation dans Program.cs

Enregistrez le service en tant que Singleton (et idéalement instanciez-le dès le démarrage pour éviter une latence sur la première requête HTTP) :

```csharp

var builder = WebApplication.CreateBuilder(args);

// Enregistrement en Singleton
builder.Services.AddSingleton<ISqlGlotService, SqlGlotService>();

var app = builder.Build();

// Optionnel : Forcer l'initialisation de Python dès le démarrage de l'API
app.Services.GetRequiredService<ISqlGlotService>();

// Endpoint Minimal API de démonstration
app.MapPost("/api/sql/transpile", (TranspileRequest req, ISqlGlotService sqlGlot) =>
{
    try
    {
        string converted = sqlGlot.Transpile(req.Sql, req.FromDialect, req.ToDialect);
        return Results.Ok(new { Sql = converted });
    }
    catch (PythonException ex)
    {
        // Capture proprement les erreurs de syntaxe SQL (sqlglot.errors.ParseError)
        return Results.BadRequest(new { Error = ex.Message });
    }
});

app.Run();

public record TranspileRequest(string Sql, string FromDialect, string ToDialect);

```


Points clés pour la mise en production

- Performance : La mise en cache de _sqlglot = Py.Import("sqlglot") dans le constructeur évite de recharger le module à chaque appel API. Une transpilation prend généralement moins d'une milliseconde.
- Concurrence : Grâce au GIL, les appels Python s'exécutent de manière séquentielle (thread-safe). Si votre API subit une très forte charge CPU sur des requêtes SQL géantes, envisagez d'augmenter le nombre d'instances (containers) plutôt que d'ajouter des threads sur un seul processus.
