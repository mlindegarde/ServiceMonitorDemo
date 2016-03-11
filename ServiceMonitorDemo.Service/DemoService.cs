using System.Collections.Generic;
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
        private readonly List<IDemoServiceCallbackChannel> _callbackChannels;

        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        #endregion

        #region Constructor
        public DemoService()
        {
            _callbackChannels = new List<IDemoServiceCallbackChannel>();
        }
        #endregion

        public void AddCallbackChannel(IDemoServiceCallbackChannel channel)
        {
            if(_callbackChannels == null)
                return;

            lock(_callbackChannels)
            {
                _callbackChannels.Add(channel);
            }
        }

        public void Start()
        {
            if(_tokenSource == null)
                _tokenSource = new CancellationTokenSource();

            _token = _tokenSource.Token;

            Task.Run(async () =>
            {
                while(!_token.IsCancellationRequested)
                {
                    UpdateCallbackChannels();
                    await Task.Delay(1000, _token);
                }
            }, _token);
        }

        public void Stop()
        {
            if(_tokenSource == null)
                return;

            _tokenSource.Cancel();
            _tokenSource.Dispose();

            ShutdownCallbackChannels();
        }

        #region Utility Methods
        private void UpdateCallbackChannels()
        {
            if(_callbackChannels == null)
                return;

            List<IDemoServiceCallbackChannel> badChannels = new List<IDemoServiceCallbackChannel>();

            lock(_callbackChannels)
            {
                _callbackChannels.ForEach(c =>
                {
                    try
                    {
                        c.UpdateStatus(new StatusUpdate
                        {
                            Message = "Up and runing"
                        });
                    }
                    catch(CommunicationObjectAbortedException)
                    {
                        badChannels.Add(c);
                    }
                });

                _callbackChannels.RemoveAll(x => badChannels.Contains(x));
            }
        }

        private void ShutdownCallbackChannels()
        {
            if(_callbackChannels == null)
                return;

            lock(_callbackChannels)
            {
                _callbackChannels.ForEach(c => c.ServiceShutdown());
            }
        }
        #endregion
    }
}
