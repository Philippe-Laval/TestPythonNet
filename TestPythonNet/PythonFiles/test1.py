from pythonnet import load
load("coreclr")

import clr
from System import String
from System.Collections import *

clr.AddReference("System.Windows.Forms")
from System.Windows.Forms import Form

# Calling .NET code from Python

class Calculator:
    def AddInPython(self, a, b):
        return a + b