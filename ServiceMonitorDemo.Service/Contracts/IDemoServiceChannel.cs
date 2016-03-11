using System.ServiceModel;

namespace ServiceMonitorDemo.Service.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IDemoServiceCallbackChannel))]
    public interface IDemoServiceChannel
    {
        [OperationContract(IsOneWay = true)]
        void Connect();
    }
}
