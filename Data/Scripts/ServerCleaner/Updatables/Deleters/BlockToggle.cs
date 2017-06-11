using Sandbox.ModAPI;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ServerCleaner.Updatables.Deleters
{
	public class BlockToggle : RepeatedBlockAction<IMyCubeBlock, ComplexCubeGridBlockContext>
    {
        private string[] BlockNames;


        public BlockToggle(double interval, string[] BlockNames, double playerDistanceThresholdForWarning, double playerDistanceThresholdForAction, bool messageAdminsOnly)
			: base(interval, messageAdminsOnly, new ComplexCubeGridBlockContext()
		{
			PlayerDistanceThresholdForAct = playerDistanceThresholdForWarning,
			PlayerDistanceThresholdForActualAction = playerDistanceThresholdForAction
		})
		{
			this.BlockNames = BlockNames;
		}


        protected override bool BeforeAction(IMyCubeBlock entity, ComplexCubeGridBlockContext context)
        {
                // Is it a toggle block?

                if (!BlockNames.Contains(entity.BlockDefinition.TypeIdString))
                    return false;

                // Are any of the owners online?

                var nameString = string.Format("{0} (owned by {1})", entity.DisplayName, Utilities.GetOwnerNameString(entity, context.PlayerIdentities));

                if (!context.OnlinePlayerIds.Contains(entity.OwnerId))
                {
                    // owner is online, warn him

                    context.NameStringsForLaterBlockAction.Add(nameString);
                    return false;
                }
                // Are any other players nearby?

                if (context.PlayerDistanceThresholdForActualAction > 0 && Utilities.AnyWithinDistance(entity.GetPosition(), context.PlayerPositions, context.PlayerDistanceThresholdForActualAction))
                    return false;

                context.NameStringsForBlockAction.Add(nameString);
                return true;
        }

        protected override void AfterAction(ComplexCubeGridBlockContext context)
        {
                if (context.BlocksForUpdate.Count > 0)
                {
                    ShowMessageFromServer("Turned off {0} block(s) that had no owner online and no players within {1} m: {2}.",
                        context.BlocksForUpdate.Count, context.PlayerDistanceThresholdForActualAction, string.Join(", ", context.NameStringsForBlockAction));
                }

                if (context.NameStringsForLaterBlockAction.Count > 0)
                {
                    ShowMessageFromServer("has configured some blocks to be switched off when their owners are offline.",
                        string.Join(", ", context.NameStringsForLaterBlockAction));
                }
        }
    }
}
