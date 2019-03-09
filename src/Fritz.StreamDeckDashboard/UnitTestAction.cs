using Fritz.StreamDeckDashboard.Models;
using Microsoft.Extensions.Logging;
using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Fritz.StreamDeckDashboard
{
	
	[ActionUuid("com.csharpfritz.plugin.unittest.action")]
	internal class UnitTestAction : BaseStreamDeckActionWithSettingsModel<UnitTestSettingsModel>
	{

		// Cheer 100 Auth0bobby  January 29, 2019
		// Cheer 100 roberttables  January 29, 2019
		// Cheer 100 cpayette  January 29, 2019
		// Cheer 100 devlead 19/2/19 

		public enum UnitTestButtonState {
			NoTestsRunning,
			TestsRunning,
			TestsPassed,
			TestsPassedWithWarnings,
			TestsFailed
		}

		private Stopwatch _ButtonHoldTimer;
		private Process _UnitTestProcess;
        private string _Context;
        private static readonly Regex _GetDigits = new Regex(@"(\d+)");

		~UnitTestAction() {

			Logger.LogDebug("Disposing the Unit Test Action");

			if (_UnitTestProcess != null) _UnitTestProcess.Dispose();

		}
        	
		/**
		 * 
		 * Not Running -- Unit Test Button
		 * Currently Running
		 * Green button with check -- Success
		 * Yellow -- success with warnings
		 * Red button with big X -- failure
		 */

		public UnitTestButtonState State { get; set; }

		public override async Task OnKeyUp(StreamDeckEventPayload args)
		{

			var ellapsedSeconds = _ButtonHoldTimer != null ? _ButtonHoldTimer.Elapsed.TotalSeconds : 0;
			_ButtonHoldTimer?.Reset();

			if (State != UnitTestButtonState.NoTestsRunning && ellapsedSeconds > 2)
			{
				await StopTestsAsync();
				return;
			} else if (State == UnitTestButtonState.NoTestsRunning) {
				await StartTests(args);
				return;
			}

			// TODO:  Figure out what to do in each of the other states...
			// NOTE: If tests have completed, perhaps open a report of those tests

		}

		public override Task OnKeyDown(StreamDeckEventPayload args)
		{

			Logger.LogInformation($"Unit test pressed: context({args.context})");

			if (State != UnitTestButtonState.NoTestsRunning) {
				_ButtonHoldTimer = Stopwatch.StartNew();
			}

			return Task.CompletedTask;

		}

		//public override async Task OnWillDisappear(StreamDeckEventPayload args)
		//{

		//	// These values persist with the class scope fields

		//	//var currentState = new UnitTestSettings
		//	//{
		//	//	State = this.State,
		//	//	ProjectFileName = _ProjectFileName
		//	//};

		//	//await this.Manager.SetSettingsAsync(args.context, currentState);

		//}

		public override Task OnWillAppear(StreamDeckEventPayload args)
		{

			this._Context = args.context;

		//	//this.State = args.GetPayloadSettingsValue<UnitTestButtonState>("State");
		//	//this._ProjectFileName = args.GetPayloadSettingsValue<string>("ProjectFileName");

			return base.OnWillAppear(args);
		}

		//public override Task OnPropertyInspectorConnected(PropertyInspectorEventPayload args)
		//{

		//	Manager.SendToPropertyInspectorAsync(args.context, new { ProjectName = _ProjectFileName });

		//	return base.OnPropertyInspectorConnected(args);
		//}

		//public override Task OnPropertyInspectorMessageReceived(PropertyInspectorEventPayload args)
		//{

		//	// Cheer 100 pharewings 22/2/19 
		//	// Cheer 200 cpayette 22/2/19 
		//	// Cheer 900 cpayette 24/2/19 
		//	// Cheer 100 gep13 24/2/19 
		//	// Cheer 100 phrakberg 24/2/19 
		//	// Cheer 100 lannonbr 24/2/19 

		//	_ProjectFileName = args.GetSettingsPayloadValue<string>("test_project_file");
		//	return Task.CompletedTask;

		//}

		private Task StartTests(StreamDeckEventPayload args)
		{

			// Cheer 1000 themichaeljolley 24/2/19 
			// Cheer 100 ElectricHavoc 24/2/19 

			if (string.IsNullOrEmpty(SettingsModel.ProjectFileName)) Manager.ShowAlertAsync(args.context);


			var projectFolder = new FileInfo(SettingsModel.ProjectFileName).DirectoryName;

			_UnitTestProcess = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					WorkingDirectory = projectFolder,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					FileName = "dotnet",
					Arguments = "watch test"
				}
			};


			//_UnitTestProcess.WaitForExit();
			_UnitTestProcess.Disposed += _UnitTestProcess_Disposed;
            _UnitTestProcess.OutputDataReceived += _UnitTestProcess_OutputDataReceivedAsync;
            _UnitTestProcess.ErrorDataReceived += _UnitTestProcess_ErrorDataReceived;
			_UnitTestProcess.Exited += _UnitTestProcess_Exited;

			Logger.LogDebug("Beginning Unit Tests");

			_UnitTestProcess.Start();

			_UnitTestProcess.BeginOutputReadLine();
			State = UnitTestButtonState.TestsRunning;

			return Task.CompletedTask;

		}

		private void _UnitTestProcess_Disposed(object sender, EventArgs e)
		{
			Debug.WriteLine("Process Disposed");
		}

		private void _UnitTestProcess_Exited(object sender, EventArgs e)
		{
			Debug.WriteLine("Process exited");
		}

		private void _UnitTestProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Debug.WriteLine("Error: " + e.Data);
		}

		private async void _UnitTestProcess_OutputDataReceivedAsync(object sender, DataReceivedEventArgs e)
		{

			// Cheer 200 roberttables 19/2/19 

			if (e.Data.Contains("watch : Started")) {
				State = UnitTestButtonState.TestsRunning;
				await Manager.SetImageAsync(_Context, "images/TestRunning.png");
				await Manager.SetTitleAsync(_Context, "");
			}
			else if (e.Data.StartsWith("Total tests:")) {

				await SetButtonFromTestResultsAsync(e.Data);

			}

			Debug.WriteLine("Data: " + e.Data);
		}

		private async Task SetButtonFromTestResultsAsync(string data)
		{
			var results = _GetDigits.Matches(data);
			var newTitle = $"Passed: {results[1].Value}\nFailed: {results[2].Value}";
			await Manager.SetTitleAsync(_Context, newTitle);

			var newImage = results[2].Value == "0" && results[3].Value == "0" ? "images/Test-Successful.png" : results[2].Value != "0" ? "images/Test-Failed.png" : "images/Test-Warning.png";

			await Manager.SetImageAsync(_Context, newImage);
            State = UnitTestButtonState.NoTestsRunning;
		}

		private async Task StopTestsAsync()
		{
			_UnitTestProcess.Kill();
			State = UnitTestButtonState.NoTestsRunning;
			await Manager.SetTitleAsync(_Context, "");
			await Manager.SetImageAsync(_Context, "images/UnitTest.png");
			//return Task.CompletedTask;
		}

		public class UnitTestSettings {

			public UnitTestButtonState State { get; set; }

			public string ProjectFileName { get; set; }

		}

	}
}