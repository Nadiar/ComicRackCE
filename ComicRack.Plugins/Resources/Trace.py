import sys

def _trace(frame, event, arg):
    try:
        # PERFORMANCE OPTIMIZATION:
        # Skip tracing for internal IronPython libraries and Standard Library
        # This prevents 1000s of redundant calls for internal logic.
        fname = frame.f_code.co_filename
        if not fname or 'IronPython' in fname or 'Lib' in fname:
            return _trace

        mod = frame.f_globals.get('__name__')
        if not mod:
            # Fallback to filename if module name is missing
            if '\\' in fname:
                mod = fname.split('\\')[-1]
            elif '/' in fname:
                mod = fname.split('/')[-1]
            else:
                mod = fname
        
        func = frame.f_code.co_name
        if func == '<module>':
            name = mod
        else:
            name = mod + '.' + func

        # host is injected by the C# engine
        if event == 'line':
           host.Trace(str(fname), str(frame.f_lineno), name)
        elif event == 'call':
           host.Trace(str(fname), str(frame.f_lineno), 'CALL: ' + name)
        elif event == 'return':
           host.Trace(str(fname), str(frame.f_lineno), 'RETURN: ' + name)
        elif event == 'exception':
           try:
               exc_type, exc_value, exc_traceback = arg
               msg = str(exc_value)
           except:
               msg = 'Unknown Exception'
           host.Trace(str(fname), str(frame.f_lineno), 'EXCEPTION: ' + msg)
    except:
        pass
    return _trace

# Assign the trace function
sys.settrace(_trace)
