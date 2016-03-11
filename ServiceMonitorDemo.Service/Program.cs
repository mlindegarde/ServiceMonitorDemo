using System;
using System.ServiceModel;
using ServiceMonitorDemo.Service.Contracts;
using Topshelf;

namespace ServiceMonitorDemo.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class Program : IDemoServiceChannel
    {
        #region Constants
        public const string Uri = "net.pipe://localhost/service-monitor-demo";
        #endregion

        #region Member Variables
        private ServiceHost _host;
        private DemoService _demoSvc;
        #endregion

        #region IDemoServiceChannel Implementation
        public void Connect()
        {
            _demoSvc?.AddCallbackChannel(OperationContext.Current.GetCallbackChannel<IDemoServiceCallbackChannel>());
        }
        #endregion

        public void StartWcf()
        {
            _host = new ServiceHost(this);

            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.ReceiveTimeout = TimeSpan.MaxValue;

            _host.AddServiceEndpoint(typeof(IDemoServiceChannel),
                binding,
                new Uri(Uri));

            _host.Open();
        }

        public void StopWcf()
        {
            _host.Close();
        }

        public void Run()
        {
            HostFactory.Run(x =>
            {
                x.Service<DemoService>(s =>
                {
                    _demoSvc = new DemoService();

                    s.ConstructUsing(_ => _demoSvc);

                    s.WhenStarted(svc =>
                    {
                        StartWcf();
                        svc.Start();
                    });

                    s.WhenStopped(svc =>
                    {
                        svc.Stop();
                        StopWcf();
                    });
                });

                x.RunAsLocalSystem();
                
                x.SetDescription("Demo Service");
                x.SetDisplayName("Demo Service");
                x.SetServiceName("DemoService");
            });
        }

        static void Main()
        {
            Program demoSvc = new Program();

            demoSvc.Run();
        }
    }
}
