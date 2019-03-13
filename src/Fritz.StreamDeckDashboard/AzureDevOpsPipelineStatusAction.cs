using StreamDeckLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fritz.StreamDeckDashboard
{

	[ActionUuid("com.csharpfritz.plugin.azuredevopsstatus.action")]
	public class AzureDevOpsPipelineStatusAction : BaseStreamDeckActionWithSettingsModel<Models.AzureDevopsPipelineSettings>
	{

		/*
		 * Fetch from URL similar to:  https://dev.azure.com/FritzAndFriends/StreamDeckTools/_apis/build/status/StreamDeckTools-CI?api-version=5.1-preview.1

				200 per 5 minute interval
				Once every 5 seconds
			*/

		// Cheer 1242 roberttables 12/3/19 
		// Cheer 142 cpayette 12/3/19 





	}
}
