using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.Views;
using NLog;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Unity;
using Windows.Services.Store;
using WinRT;

namespace boilersGraphics
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : PrismApplication
    {
        public static bool IsTest { get; set; }

        public static App Instance { get; set; }

        public StoreContext StoreContext { get; private set; }

        public App()
        {
            Instance = this;
        }

        public static Application GetCurrentApp()
        {
            return App.Current != null ? App.Current : new Application();
        }

        public Window GetDialog()
        {
            foreach (var window in this.Windows)
            {
                if (window.GetType() == typeof(Prism.Services.Dialogs.DialogWindow))
                    return window as Window;
            }
            return null;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics {version}");
            LogManager.GetCurrentClassLogger().Info($"Copyright (C) dhq_boiler 2018-2022. All rights reserved.");
            LogManager.GetCurrentClassLogger().Info($"boiler's Graphics IS LAUNCHING");

            StoreContext context = StoreContext.GetDefault();
            IInitializeWithWindow initWindow = context.As<IInitializeWithWindow>();
            initWindow.Initialize(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            StoreContext = context;

            base.OnStartup(e);
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            var container = new UnityContainer();
            container.AddExtension(new Diagnostic());
            container.AddExtension(new LogResolvesUnityContainerExtension());
            return new UnityContainerExtension(container);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<Layers, ViewModels.LayersViewModel>();
        }

        protected override Window CreateShell()
        {
            var w = Container.Resolve<MainWindow>();
            return w;
        }

        public static string GetAppNameAndVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var majorMinorBuild = $"{version.Major}.{version.Minor}.{version.Build}";
            var appnameAndVersion = $"boiler's Graphics {majorMinorBuild}";
            return appnameAndVersion;
        }
    }
}
