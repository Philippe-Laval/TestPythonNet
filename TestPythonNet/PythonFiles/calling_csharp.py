import clr
from System import String
from System.Collections import *

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

