# clr_bridge.py
# Provides a common bridge for Python.NET scripts to access ComicRack APIs
import clr
import sys

# Common references
clr.AddReference('ComicRack.Engine')
clr.AddReference('cYo.Common')
clr.AddReference('cYo.Common.Windows')
clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')

from cYo.Projects.ComicRack.Engine import *
from System.Windows.Forms import *
from System.Drawing import *

# This will be populated by the PythonRuntimeManager
ComicRack = None

def setup_comicrack(api_object):
    global ComicRack
    ComicRack = api_object
