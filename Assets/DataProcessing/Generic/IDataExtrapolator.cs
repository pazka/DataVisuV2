using System.Collections.Generic;

namespace DataProcessing.Generic
{
    public interface IDataExtrapolator
    {
         void InitExtrapolation(IEnumerable<IData> inputData);
         IEnumerable<IData> RetrieveExtrapolation();
    }
}