using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
	public class DeletePirates : RepeatedDeleter<IMyCubeGrid, ComplexCubeGridDeletionContext>
	{
        private long NPCid;

        public DeletePirates(double interval, long pirateid, double playerDistanceThresholdForWarning, double playerDistanceThresholdForDeletion, bool messageAdminsOnly)
			: base(interval, messageAdminsOnly, new ComplexCubeGridDeletionContext()
		{
			PlayerDistanceThreshold = playerDistanceThresholdForWarning,
			PlayerDistanceThresholdForActualDeletion = playerDistanceThresholdForDeletion
		})
		{
			this.NPCid = pirateid;
		}


        protected override bool BeforeDelete(IMyCubeGrid grid, ComplexCubeGridDeletionContext context)
		{

            string Stoptag = ".";
            
            // Is it a pirate ship?

            if (!grid.BigOwners.Contains(NPCid))
                return false;

            // Are any of the owners online?

            var nameString = string.Format("{0} (owned by {1})", grid.DisplayName, Utilities.GetOwnerNameString(grid, context.PlayerIdentities));
            /* this does not work because Pirates are always online.
            foreach (var ownerID in entity.SmallOwners)
			{
				if (!context.OnlinePlayerIds.Contains(ownerID))
					continue;

				// At least one owner is online, warn him

				context.NameStringsForLaterDeletion.Add(nameString);
				return false;
			}
            */

            // Are any other players nearby?

            if (context.PlayerDistanceThresholdForActualDeletion > 0 && Utilities.AnyWithinDistance(grid.GetPosition(), context.PlayerPositions, context.PlayerDistanceThresholdForActualDeletion))
                return false;

            // Does it have a stoptag?

            if (grid.DisplayName.Contains(Stoptag))
                return false;

            context.NameStringsForDeletion.Add(nameString);
			return true;
		}

		protected override void AfterDeletion(ComplexCubeGridDeletionContext context)
		{
			if (context.EntitiesForDeletion.Count > 0)
			{
				ShowMessageFromServer("Deleted {0} NPC ship(s) that had no player owner online and no players within {1} m: {2}.",
					context.EntitiesForDeletion.Count, context.PlayerDistanceThresholdForActualDeletion, string.Join(", ", context.NameStringsForDeletion));
                MyAPIGateway.Utilities.ShowNotification("Server: NPC grids have been deleted!", 40000, MyFontEnum.Green);
                MyAPIGateway.Utilities.ShowNotification("Use ignore tag '.' (no quotes) in ship name to be skipped.", 40000, MyFontEnum.Green);
                MyAPIGateway.Utilities.ShowNotification("Example: .MyCapturedShipName", 40000, MyFontEnum.Green);
            }

            if (context.NameStringsForLaterDeletion.Count > 0)
			{
				ShowMessageFromServer("The following NPC ship(s) will be deleted later unless they are fully claimed or an ignore tag is used in the name !! : {0}",
					string.Join(", ", context.NameStringsForLaterDeletion));
			}
		}
	}
}
