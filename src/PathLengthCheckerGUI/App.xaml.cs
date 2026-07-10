using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PathLengthChecker;

namespace PathLengthCheckerGUI
{
	public partial class App : Application
	{
		public App() : base()
		{
			SetupUnhandledExceptionHandling();
		}

		private void SetupUnhandledExceptionHandling()
		{
			AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
				ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

			TaskScheduler.UnobservedTaskException += (sender, args) =>
				ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

			Dispatcher.UnhandledException += (sender, args) =>
			{
				if (!Debugger.IsAttached)
				{
					args.Handled = true;
					ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
				}
			};
		}

		void ShowUnhandledException(Exception? e, string unhandledExceptionType, bool promptUserForShutdown)
		{
			var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
			var messageBoxMessage = $"The following exception occurred:\n\n{e}";
			var messageBoxButtons = MessageBoxButton.OK;

			if (promptUserForShutdown)
			{
				messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
				messageBoxButtons = MessageBoxButton.YesNo;
			}

			if (MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons) == MessageBoxResult.Yes)
			{
				Current.Shutdown();
			}
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var mainWindow = new MainWindow();

			if (e.Args.Length == 1 && Directory.Exists(e.Args[0]))
			{
				mainWindow.txtRootDirectory.Text = e.Args[0];
				mainWindow.btnGetPathLengths.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
			}
			else if (e.Args.Length >= 1)
			{
				try
				{
					var searchOptions = ArgumentParser.ParseArgs(e.Args);
					mainWindow.SetUIControlsFromSearchOptions(searchOptions);

					if (!string.IsNullOrEmpty(searchOptions?.RootDirectory))
					{
						mainWindow.btnGetPathLengths.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
					}
				}
				catch (ArgumentException ex)
				{
					string title = "Incorrect arguments";
					string message = "Incorrectly-formatted arguments were passed to the program.\n\n";
					message += ex.Message + "\n\n" + ArgumentParser.ArgumentUsage;
					MessageBox.Show(message, title);
				}
			}

			mainWindow.Show();
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			// Window saves its own settings on close.
		}
	}
}
