using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Bounds
{
    abstract class GenericBound : IBounds
    {
        public abstract object GetCurrentBounds();
        public abstract void RegisterNewBounds(object data);
    }
}
