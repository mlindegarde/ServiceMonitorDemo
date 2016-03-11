using System.ServiceModel;
using ServiceMonitorDemo.Model;

namespace ServiceMonitorDemo.Service.Contracts
{
    [ServiceContract]
    public interface IDemoServiceCallbackChannel
    {
        [OperationContract(IsOneWay = true)]
        void UpdateStatus(StatusUpdate status);

        [OperationContract(IsOneWay = true)]
        void ServiceShutdown();
    }
}
