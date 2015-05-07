using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    public interface IMapper
    {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }
}
