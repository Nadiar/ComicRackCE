# clr_bridge.py
# Provides a common bridge for Python.NET scripts to access ComicRack APIs
import clr
import sys
import os

# Common references
clr.AddReference('ComicRack.Engine')
clr.AddReference('cYo.Common')
clr.AddReference('cYo.Common.Windows')
clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')

from cYo.Projects.ComicRack.Engine import *
from System.Windows.Forms import *
from System.Drawing import *
import faulthandler

# Enable fault handler to dump stack traces on hard crashes (segfaults)
try:
    _crash_log_path = os.path.join(os.environ.get('APPDATA', '.'), 'cYo', 'ComicRack Community Edition', 'python_crash.log')
    _crash_file = open(_crash_log_path, 'w')
    faulthandler.enable(file=_crash_file)
except:
    pass # If we can't open the file, just ignore it

# This will be populated by the PythonRuntimeManager
ComicRack = None

# Trace support
_trace_enabled = False
_original_trace = None

def _safe_repr(obj):
    """Safely get string representation of an object."""
    try:
        if obj is None:
            return 'None'
        return str(obj)[:200]
    except:
        return '<repr failed>'

def _trace_func(frame, event, arg):
    """Trace function that prints execution events for script files."""
    # Check if a C# callback is registered in the global scope
    callback = globals().get('_cr_trace_callback', None)
    
    try:
        code = frame.f_code
        filename = code.co_filename
    
        # Only trace script files, not system libraries
        if 'Scripts' in filename or (filename.endswith('.py') and '<' not in filename and 'lib' not in filename.lower()):
            lineno = frame.f_lineno
            name = code.co_name
            
            message = None
            if event == 'call':
                message = f'CALL: {name}() at {filename}:{lineno}'
            elif event == 'line':
                message = f'LINE: {filename}:{lineno}'
            elif event == 'return':
                result_str = _safe_repr(arg)
                message = f'RETURN: {name}() -> {result_str}'
            elif event == 'exception':
                exc_type, exc_value, exc_tb = arg
                message = f'EXCEPTION: {exc_type.__name__}: {exc_value}'
            
            if message:
                if callback:
                    try:
                        callback('PythonTrace', message)
                    except:
                        pass # avoid recursion or crashes in logging
                else:
                    print(f'[TRACE] {message}')
                    
    except:
        pass # Swallow errors to avoid breaking execution
        
    return _trace_func

def enable_trace():
    """Enable execution tracing for all scripts."""
    global _trace_enabled, _original_trace
    if not _trace_enabled:
        _original_trace = sys.gettrace()
        sys.settrace(_trace_func)
        _trace_enabled = True
        callback = globals().get('_cr_trace_callback', None)
        if callback:
            callback('System', '=== Tracing ENABLED (clr_bridge) ===')
        else:
            print('[TRACE] === Tracing ENABLED ===')

def disable_trace():
    """Disable execution tracing."""
    global _trace_enabled, _original_trace
    if _trace_enabled:
        sys.settrace(_original_trace)
        _trace_enabled = False
        callback = globals().get('_cr_trace_callback', None)
        if callback:
            callback('System', '=== Tracing DISABLED (clr_bridge) ===')
        else:
            print('[TRACE] === Tracing DISABLED ===')


def is_trace_enabled():
    """Check if tracing is currently enabled."""
    return _trace_enabled

def setup_comicrack(api_object):
    global ComicRack
    ComicRack = api_object

    # Check if trace should be enabled (set by PythonRuntimeManager)
    if os.environ.get('COMICRACK_TRACE_ENABLED', '').lower() == 'true':
        enable_trace()
