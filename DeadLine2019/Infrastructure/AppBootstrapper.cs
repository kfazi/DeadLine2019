namespace DeadLine2019.Infrastructure
{
    using System;
    using System.Threading;
    using System.Windows;

    using Autofac;

    using Caliburn.Micro.Autofac;

    public class AppBootstrapper : AutofacBootstrapper<MainViewModel>
    {
        private Thread _thread;

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(MainLoop).Assembly)
                .Where(x => x != typeof(AppBootstrapper))
                .Where(x => x != typeof(AppSettings))
                .Where(x => x != typeof(ConnectionData))
                .Where(x => !x.IsAssignableTo<Exception>())
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.RegisterInstance(ConnectionData.Load(@"connection.json"));
            builder.RegisterInstance(AppSettings.Create());
        }

        protected override void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
            EnforceNamespaceConvention = false;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            Application.ShutdownMode = ShutdownMode.OnMainWindowClose;

            DisplayRootViewFor<MainViewModel>();

            var mainLoop = Container.Resolve<MainLoop>();
            _thread = new Thread(mainLoop.Run);
            _thread.Start();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            if (_thread != null)
            {
                var mainLoop = Container.Resolve<MainLoop>();
                mainLoop.Finish();

                if (!_thread.Join(TimeSpan.FromMilliseconds(500.0)))
                {
                    _thread.Abort();
                }
            }

            base.OnExit(sender, e);
        }
    }
}