using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SquareTimeProcessingService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ISquareTimeProcessingService" in both code and config file together.
    [ServiceContract]
    public interface ISquareTimeProcessingService
    {
        [OperationContract]
        void Start(string pathFile);
        [OperationContract]
        void Suspend(bool run);
        [OperationContract]
        bool isFirstLTSecund(byte FirstCase, byte SecundCase);
        [OperationContract]
        void SetFirstHit(byte noCase);
        [OperationContract]
        DateTime GetFirstHit(byte nocase);
        [OperationContract]
        bool NewGame();
    }
}
