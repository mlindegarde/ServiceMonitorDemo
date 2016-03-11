using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using ServiceMonitorDemo.Model;
using ServiceMonitorDemo.Service.Contracts;

namespace ServiceMonitorDemo.Service
{
    public class DemoService
    {
        #region Member Variables
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        #endregion

        #region Properties
        public IDemoServiceCallbackChannel CallbackChannel {get; set;}
        #endregion

        public void Start()
        {
            if(_tokenSource == null)
                _tokenSource = new CancellationTokenSource();

            _token = _tokenSource.Token;

            Task.Run(async () =>
            {
                while(!_token.IsCancellationRequested)
                {
                    try
                    {
                        CallbackChannel?.UpdateStatus(new StatusUpdate
                        {
                            Message = "Up and runing"
                        });
                        await Task.Delay(1000, _token);
                    }
                    catch(CommunicationObjectAbortedException)
                    {
                        CallbackChannel = null;
                    }
                }
            }, _token);
        }

        public void Stop()
        {
            if(_tokenSource == null)
                return;

            _tokenSource.Cancel();
            _tokenSource.Dispose();

            CallbackChannel?.ServiceShutdown();
        }
    }
}
