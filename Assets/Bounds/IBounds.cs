using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Bounds
{
    interface IBounds
    {
         object GetCurrentBounds();
         void RegisterNewBounds(object data);
    }
}
