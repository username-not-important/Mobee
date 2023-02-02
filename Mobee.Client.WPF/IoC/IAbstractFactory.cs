using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Client.WPF.IoC
{
    public interface IAbstractFactory<T>
    {
        T Create();
    }
}
