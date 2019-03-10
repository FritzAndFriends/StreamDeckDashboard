using System;
using System.IO;
using static Fritz.StreamDeckDashboard.UnitTestAction;

namespace Fritz.StreamDeckDashboard.Models
{
	[Serializable]
	public class UnitTestSettings
	{

		public UnitTestButtonState State { get; set; }

		public string ProjectFileName { get; set; }

		// Cheer 342 cpayette 10/3/19 

		/// <summary>
		/// Name of the project without the full path and extension
		/// </summary>
		public string ProjectName => string.IsNullOrEmpty(ProjectFileName) ? 
			"" : Path.GetFileNameWithoutExtension(ProjectFileName);

		public int PassedTestCount { get; set; }

		public int FailedTestCount { get; set; }

		public int IgnoredTestCount { get; set; }


	}

}