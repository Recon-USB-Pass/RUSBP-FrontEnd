using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Core/Services/PinValidator.cs
namespace RUSBP_Admin.Core.Services;
public class PinValidator
{
    private readonly string _pin; private readonly int _max; private int _fails;
    public event Action? MaxReached;
    public PinValidator(string pin, int max) { _pin = pin; _max = max; }
    public bool Check(string input)
    {
        if (input == _pin) { _fails = 0; return true; }
        if (++_fails >= _max) MaxReached?.Invoke();
        return false;
    }
}

