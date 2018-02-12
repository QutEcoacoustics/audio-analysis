using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acoustics.Shared.ConfigFile
{
    public interface IProfile<T>
    {
        T Profiles { get; set; }
    }
}
