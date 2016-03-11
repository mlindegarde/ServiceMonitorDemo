using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using ServiceMonitorDemo.Model;
using ServiceMonitorDemo.Service.Contracts;

namespace ServiceMonitorDemo.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class DemoService : IDemoServiceChannel
    {
        #region Constants
        public const string Uri = "net.pipe://localhost/service-monitor-demo";
        #endregion

        #region Member Variables
        private static DemoService _instance;

        private readonly List<IDemoServiceCallbackChannel> _callbackChannels;

        private ServiceHost _host;
        private CancellationTokenSource _tokenSource;
        private CancellationToken _token;
        #endregion

        #region Properties
        public bool IsRunningAsService {get; set;}
        #endregion

        #region Constructor
        public static DemoService GetInstance()
        {
            return _instance ?? (_instance = new DemoService());
        }

        private DemoService()
        {
            _callbackChannels = new List<IDemoServiceCallbackChannel>();
        }
        #endregion

        #region IDemoServiceChannel Implementation
        public void Connect()
        {
            AddCallbackChannel(OperationContext.Current.GetCallbackChannel<IDemoServiceCallbackChannel>());
        }

        public bool DisplayMessage(string message)
        {
            if(!IsRunningAsService)
            {
                Console.WriteLine(message);
                return true;
            }

            return false;
        }
        #endregion

        public void Start()
        {
            // wcf
            _host = new ServiceHost(this);

            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            _host.AddServiceEndpoint(typeof(IDemoServiceChannel),
                binding,
                new Uri(Uri));

            _host.Open();

            // service
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
            // service
            if(_tokenSource == null)
                return;

            _tokenSource.Cancel();
            _tokenSource.Dispose();

            // wcf
            ShutdownCallbackChannels();
            
            _host.Close();
        }

        #region Utility Methods
        private void AddCallbackChannel(IDemoServiceCallbackChannel channel)
        {
            if(_callbackChannels == null)
                return;

            lock(_callbackChannels)
            {
                _callbackChannels.Add(channel);
            }
        }

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
                            Message = "Up",
                            ConnectedClients = _callbackChannels.Count
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
