using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fritz.StreamDeckDashboard
{
	internal class UnitTestPlugin : BaseStreamDeckPlugin
	{

		// Cheer 100 Auth0bobby  January 29, 2019
		// Cheer 100 roberttables  January 29, 2019
		// Cheer 100 cpayette  January 29, 2019

		public enum UnitTestButtonState {
			NoTestsRunning,
			TestsRunning,
			TestsPassed,
			TestsPassedWithWarnings,
			TestsFailed
		}

		private Stopwatch _ButtonHoldTimer;
		private Process _UnitTestProcess;
		private static readonly Regex _GetDigits = new Regex(@"(\d+)");

		~UnitTestPlugin() {

			if (_UnitTestProcess != null) _UnitTestProcess.Dispose();

		}

		private string _Context;

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

			if (State != UnitTestButtonState.NoTestsRunning && _ButtonHoldTimer.Elapsed.TotalSeconds > 2)
			{
				await StopTests();
				return;
			} else if (State == UnitTestButtonState.NoTestsRunning) {
				await StartTests();
				return;
			}

			// TODO:  Figure out what to do in each of the other states...
			// NOTE: If tests have completed, perhaps open a report of those tests

		}

		public override Task OnKeyDown(StreamDeckEventPayload args)
		{
			
			if (State != UnitTestButtonState.NoTestsRunning) {
				_ButtonHoldTimer = Stopwatch.StartNew();
			}

			return Task.CompletedTask;

		}

		public override async Task OnWillDisappear(StreamDeckEventPayload args)
		{

			var currentState = new
			{
				State = (int)(this.State)
			};

			await this.Manager.SetSettingsAsync(args.context, currentState);

		}

		public override Task OnWillAppear(StreamDeckEventPayload args)
		{

			this._Context = args.context;

			this.State = (UnitTestButtonState)(args.payload.settings.State ?? UnitTestButtonState.NoTestsRunning);

			return base.OnWillAppear(args);
		}

		private Task StartTests()
		{

			_UnitTestProcess = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					WorkingDirectory = @"c:\dev\StreamDeck_First\src\StreamDeckLib.Test",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					FileName = "dotnet",
					Arguments = "watch test"
				}
			};


			//_UnitTestProcess.WaitForExit();
			_UnitTestProcess.Disposed += _UnitTestProcess_Disposed;
			_UnitTestProcess.OutputDataReceived += _UnitTestProcess_OutputDataReceived;
			_UnitTestProcess.ErrorDataReceived += _UnitTestProcess_ErrorDataReceived;
			_UnitTestProcess.Exited += _UnitTestProcess_Exited;
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

		private void _UnitTestProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{

			if (e.Data.Contains("watch : Started")) {
				State = UnitTestButtonState.TestsRunning;
				Manager.SetImageAsync(_Context, "images/Test-Running.png");
				Manager.SetTitleAsync(_Context, "");
			}
			else if (e.Data.StartsWith("Total tests:")) {

				SetButtonFromTestResults(e.Data);

			}

			Debug.WriteLine("Data: " + e.Data);
		}

		private void SetButtonFromTestResults(string data)
		{
			var results = _GetDigits.Matches(data);
			var newTitle = $"Passed: {results[1].Value}\nFailed: {results[2].Value}";
			Manager.SetTitleAsync(_Context, newTitle);

			var newImage = results[2].Value == "0" && results[3].Value == "0" ? "images/Test-Successful.png" : results[2].Value != "0" ? "images/Test-Failed.png" : "images/Test-Warning.png";

			Manager.SetImageAsync(_Context, newImage);

		}

		private Task StopTests()
		{
			_UnitTestProcess.Kill();
			State = UnitTestButtonState.NoTestsRunning;
			Manager.SetTitleAsync(_Context, "");
			Manager.SetImageAsync(_Context, "images/UnitTest.png");
			return Task.CompletedTask;
		}

	}
}