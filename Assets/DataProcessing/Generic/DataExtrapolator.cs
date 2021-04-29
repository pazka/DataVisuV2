using System.Collections.Generic;

namespace DataProcessing.Generic
{
    public abstract class DataExtrapolator : IDataExtrapolator
    {
        public abstract void InitExtrapolation(IEnumerable<IData> inputData);
        public abstract IEnumerable<IData> RetreiveExtrapolation();
    }
}