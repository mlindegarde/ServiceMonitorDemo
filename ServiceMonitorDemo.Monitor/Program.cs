using System;
using System.ServiceModel;
using System.Threading;
using ServiceMonitorDemo.Model;
using ServiceMonitorDemo.Service.Contracts;

namespace ServiceMonitorDemo.Monitor
{
    class Program : IDemoServiceCallbackChannel
    {
        #region Constants
        public const string Uri = "net.pipe://localhost/service-monitor-demo";
        #endregion

        #region Member Variables
        private IDemoServiceChannel _channel;
        private bool _isConnected;
        #endregion

        #region IDemoServiceCallbackChannel Implementation
        public void UpdateStatus(StatusUpdate status)
        {
            Console.WriteLine($"Status: {status.Message} | Connections: {status.ConnectedClients}");
        }

        public void ServiceShutdown()
        {
            _isConnected = false;
        }
        #endregion

        private void Connect()
        {
            while(!_isConnected)
            {
                try
                {
                    DuplexChannelFactory<IDemoServiceChannel> channelFactory = new DuplexChannelFactory<IDemoServiceChannel>(
                        new InstanceContext(this),
                        new NetNamedPipeBinding(),
                        new EndpointAddress(Uri));

                    _channel = channelFactory.CreateChannel();
                    _channel.Connect();

                    _isConnected = true;
                    Console.WriteLine("Channel connected.");
                }
                catch(Exception)
                {
                    Console.WriteLine("Failed to connect to channel.");
                }

                Thread.Sleep(1000);
            }
        }

        public void Run()
        {
            Random rand = new Random();
            int id = rand.Next(1000);

            while(true)
            {
                try
                {
                    if(_isConnected)
                    {
                        if(rand.Next(100) < 5)
                        {
                            string not = _channel.DisplayMessage($"Hello from {id}")
                                ? String.Empty 
                                : "not ";

                            Console.WriteLine($"Mesage {not}displayed.");
                        }
                    }
                    else
                        Connect();

                    Thread.Sleep(100);
                }
                catch(CommunicationObjectFaultedException)
                {
                    _isConnected = false;
                }
            }
        }

        static void Main()
        {
            Program demoApp = new Program();

            demoApp.Run();
        }
    }
}
