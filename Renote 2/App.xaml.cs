using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Renote_2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Exception);
            e.Handled = false;
        }

        /// <summary>
        /// Gets the main application window
        /// </summary>
        public Window? Window
        {
            get { return _window; }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var launchFiles = ParseLaunchFilePaths(args.Arguments);

            // In unpackaged WinUI desktop activation, LaunchActivatedEventArgs.Arguments can be empty
            // even when the app is started with a file path (for example via Explorer Open With).
            if (launchFiles.Count == 0)
            {
                launchFiles = ParseLaunchFilePaths(Environment.GetCommandLineArgs().Skip(1));
            }

            _window = new MainWindow(launchFiles);
            _window.Activate();
        }

        private static IReadOnlyList<string> ParseLaunchFilePaths(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
                return Array.Empty<string>();

            var filePaths = new List<string>();
            foreach (Match match in Regex.Matches(arguments, "\"([^\"]+)\"|([^\\s]+)"))
            {
                var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                if (File.Exists(value))
                    filePaths.Add(value);
            }

            return filePaths;
        }

        private static IReadOnlyList<string> ParseLaunchFilePaths(IEnumerable<string> arguments)
        {
            if (arguments == null)
                return Array.Empty<string>();

            var filePaths = new List<string>();
            foreach (var argument in arguments)
            {
                if (!string.IsNullOrWhiteSpace(argument) && File.Exists(argument))
                    filePaths.Add(argument);
            }

            return filePaths;
        }
    }
}

