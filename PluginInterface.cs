using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlcoholicDrinks
{
    public interface IPlugin
    {
        string Activate(string input);
    }
}
