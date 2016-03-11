using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceMonitorDemo.Service.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IDemoServiceCallbackChannel))]
    public interface IDemoServiceChannel
    {
        [OperationContract(IsOneWay = true)]
        void Connect();
    }
}
