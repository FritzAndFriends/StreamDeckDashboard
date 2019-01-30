using StreamDeckLib;
using StreamDeckLib.Messages;
using System;
using System.Diagnostics;
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

			this.State = args.payload.settings.State;

			return base.OnWillAppear(args);
		}

		private Task StartTests()
		{
			throw new NotImplementedException();
		}

		private Task StopTests()
		{
			throw new NotImplementedException();
		}

	}
}