# Embedding .NET into Python
# https://pythonnet.github.io/pythonnet/python.html

import clr
from System import String, Char, Int32, Type, Array, Environment
#from System import *
#from System.Collections import *
from System.Collections import Hashtable
from System.Collections.Generic import Dictionary

#clr.AddReference("System.Windows.Forms")
#from System.Windows.Forms import Form

# Add a reference to the C# assembly (DLL) that contains the Calculator class
clr.AddReference("DotNetLibrary")
from DotNetLibrary import Calculator
from DotNetLibrary import InformationProvider, Message, Response

def get_response(message):
    print(f"Message: StatusCode={message.StatusCode}, Message={message.Information}")
    provider = InformationProvider()
    response = provider.GetResponse(message)
    print(f"Response: StatusCode={response.StatusCode}, Message={response.Message}")
    return response

def add(a, b):
    # Create an instance of the C# Calculator class
    calculator = Calculator()
    # Call the Add method
    result = calculator.Add(a, b)
    # Print the result
    print(f"Result of Add: {result}")
    return result

def sub(a, b):
    # Create an instance of the C# Calculator class
    calculator = Calculator()
    # Call the Subtract method
    result = calculator.Subtract(a, b)
    # Print the result
    print(f"Result of Subtract: {result}")
    return result

def test():
    #s = String.__overloads__[Char, Int32]('A', 10)
    dict1 = Dictionary[String, String]()
    dict2 = Dictionary[String, Int32]()
    dict3 = Dictionary[String, Type]()

    myarray = Array[Int32](10)

    name = Environment.MachineName
    drives = Environment.GetLogicalDrives()

    for drive in drives:
        print(drive)

    table = Hashtable()
    table["key 1"] = "value 1"
    
    # print(Environment.GetFolderPath.__doc__)
    # help(Environment)
    
    return (drives, name)