using Topshelf;
using Topshelf.Hosts;

namespace ServiceMonitorDemo.Service
{
    class Program
    {
        public void Run()
        {
            Host host = HostFactory.New(x =>
            {
                x.Service<DemoService>(s =>
                {
                    s.ConstructUsing(_ => DemoService.GetInstance());

                    s.WhenStarted(svc => svc.Start());
                    s.WhenStopped(svc => svc.Stop());
                });

                x.RunAsLocalSystem();
                
                x.SetDescription("WCF Service Monitor Demo - Service");
                x.SetDisplayName("WCF Service Monitor Demo - Service");
                x.SetServiceName("WCFServiceMonitorDemoService");
            });

            DemoService.GetInstance().IsRunningAsService = !(host is ConsoleRunHost);
            host.Run();
        }

        static void Main()
        {
            Program demoSvc = new Program();

            demoSvc.Run();
        }
    }
}
