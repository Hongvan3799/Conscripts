﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;

namespace PowerShortcut
{
    public static class Program
    {
        private static App _app;

        [STAThread]
        public static void Main(string[] args)
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

            var isRedirect = DecideRedirection().GetAwaiter().GetResult();

            if (!isRedirect)
            {
                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);
                    _app = new App();
                });
            }
        }

        private static async Task<bool> DecideRedirection()
        {
            AppInstance keyInstance = AppInstance.FindOrRegisterForKey("POWERSHORTCUT-F78E9363-9B49-4C08-998E-F26350ED4D26");
            AppActivationArguments args = AppInstance.GetCurrent().GetActivatedEventArgs();

            bool isRedirect = false;
            if (keyInstance.IsCurrent)
            {
                keyInstance.Activated += OnActivated;
            }
            else
            {
                isRedirect = true;
                await keyInstance.RedirectActivationToAsync(args);
            }
            return isRedirect;
        }

        private static void OnActivated(object sender, AppActivationArguments args)
        {
            _app?.ShowMainWindow();
        }
    }
}
