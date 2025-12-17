# decorators.py
# Mimics IronPython decorator behavior if needed, or provides new ones for Python 3
import functools

def comicrack_command(name, key=None, image=None, hook=None):
    """
    Decorator for ComicRack commands. 
    In Python.NET, we can use these to help the initializer discover methods.
    """
    def decorator(func):
        func._cr_metadata = {
            'Name': name,
            'Key': key or func.__name__,
            'Image': image,
            'Hook': hook
        }
        @functools.wraps(func)
        def wrapper(*args, **kwargs):
            return func(*args, **kwargs)
        return wrapper
    return decorator
