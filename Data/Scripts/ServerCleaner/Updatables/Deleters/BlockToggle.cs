using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ServerCleaner.Updatables.Deleters
{
	public class BlockToggle : RepeatedDeleter<IMyCubeGrid, ComplexCubeGridDeletionContext>
	{

        [Flags]
        private enum SwitchSystems
        {
            None = 0x0,
            Power = 0x1, // (reactors, batteries)
            Production = 0x2, // (refineries, arc furnaces, assemblers)
            Programmable = 0x4,
            Projectors = 0x8,
            Timers = 0x10,
            Weapons = 0x20, // all types.
            SpotLights = 0x40,
            Sensors = 0x80,
            Medical = 0x100,
            Mass = 0x200,
            Welder = 0x400,
            Grinder = 0x800
        };

        public BlockToggle(double interval, string[] BlockNames, double playerDistanceThresholdForWarning, double playerDistanceThresholdForDeletion, bool messageAdminsOnly)
			: base(interval, messageAdminsOnly, new ComplexCubeGridDeletionContext()
		{
			PlayerDistanceThreshold = playerDistanceThresholdForWarning,
			PlayerDistanceThresholdForActualDeletion = playerDistanceThresholdForDeletion
		})
		{
			this.BlockNames = BlockNames;
		}


        protected override bool BeforeDelete(IMyCubeGrid entity, ComplexCubeGridDeletionContext context)
		{

            string Stoptag = ".";
            
            // Is it a pirate ship?

            if (!entity.BigOwners.Contains(NPCid))
                return false;

            // Are any of the owners online?

            var nameString = string.Format("{0} (owned by {1})", entity.DisplayName, Utilities.GetOwnerNameString(entity, context.PlayerIdentities));
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

            if (context.PlayerDistanceThresholdForActualDeletion > 0 && Utilities.AnyWithinDistance(entity.GetPosition(), context.PlayerPositions, context.PlayerDistanceThresholdForActualDeletion))
                return false;

            // Does it have a stoptag?

            if (entity.DisplayName.Contains(Stoptag))
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
