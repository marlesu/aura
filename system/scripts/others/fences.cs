//--- Aura Script -----------------------------------------------------------
// Fences
//--- Description -----------------------------------------------------------
// Restrict access to specific areas that are off-limits until certain
// features are enabled.
//---------------------------------------------------------------------------

public class FeatureFencesScript : GeneralScript
{
	public override void Load()
	{
		if (!IsEnabled("G4S1"))
		{
			// Bangor -> Morva Aisle
			SpawnProp(41277, 31, 12400, 4900, 4.712389f);
		}

		if (!IsEnabled("G10S1"))
		{
			// Emain Macha -> Blago Prairie
			SpawnProp(41894, 52, 18131, 46040, 5.85575f);
		}

		// Dugald Aisle -> Dugald Residential Area
		var prop = ChannelServer.Instance.World.GetRegion(16).GetProp(a => a.Info.Id == 25219); // toggleable fence
		if (prop != null) prop.SetState(IsEnabled("Housing") ? "open" : "close");
	}
}