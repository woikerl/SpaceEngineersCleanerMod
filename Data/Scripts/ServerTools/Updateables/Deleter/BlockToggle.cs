using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
    public class BlockToggle : RepeatedBlockAction<IMyCubeGrid, ComplexCubeGridBlockContext>
    {
        private string[] BlockNames;
        private List<string> vipNames;

        public BlockToggle(double interval, string[] BlockNames, double playerDistanceThresholdForWarning,
            double playerDistanceThresholdForAction, bool messageAdminsOnly, List<string> vipNames)
            : base(interval, messageAdminsOnly, BlockNames, new ComplexCubeGridBlockContext()
            {
                PlayerDistanceThresholdForAct = playerDistanceThresholdForWarning,
                PlayerDistanceThresholdForActualAction = playerDistanceThresholdForAction
            })
        {
            this.BlockNames = BlockNames;
            this.vipNames = vipNames;
        }


        protected override bool BeforeAction(IMyCubeGrid entity, ComplexCubeGridBlockContext context)
        {
            //trying to integrate midspace' strategy here
            int counter = 0;
            var blocks = new List<IMySlimBlock>();
            entity.GetBlocks(blocks, f => f.FatBlock != null);


            // Are any of the owners online or VIP?

            var nameString = string.Format("{0} (owned by {1})", entity.DisplayName,
                Utilities.GetOwnerNameString(entity, context.PlayerIdentities));

            foreach (var ownerID in entity.SmallOwners)
            {
                if (context.PlayerIdentities.Any(identity =>
                    identity.IdentityId == ownerID && vipNames.Contains(identity.DisplayName)))
                    return false;


                if (!context.OnlinePlayerIds.Contains(ownerID))
                    continue;

                // At least one owner is online, warn him

                context.NameStringsForLaterBlockAction.Add(nameString);
                return false;
            }

            // Are any other players nearby?
            if (context.PlayerDistanceThresholdForActualAction > 0 && Utilities.AnyWithinDistance(entity.GetPosition(),
                    context.PlayerPositions, context.PlayerDistanceThresholdForActualAction))
                return false;

            // locate all toggle blocks if any
            foreach (var block in blocks)
            {
                // now here we need the logic to loop through the blocks to identify if any of the names match the configuration list.
                if (!BlockNames.Contains(block.FatBlock.BlockDefinition.TypeIdString)) continue;

                if (((IMyFunctionalBlock) block.FatBlock).Enabled == true)
                {
                    //    ((IMyFunctionalBlock)block.FatBlock).Enabled = false; // turn power on/off.
                    counter++;
                }
            }

            if (counter == 0)
                return false;


            context.NameStringsForBlockAction.Add(nameString);
            return true;
        }

        protected override void AfterAction(ComplexCubeGridBlockContext context)
        {
            if (context.GridsForUpdate.Count > 0)
            {
                ShowMessageFromServer(
                    "Turned off block(s) on {0} grids that had no owner online and no players within {1} m: {2}.",
                    context.GridsForUpdate.Count, context.PlayerDistanceThresholdForActualAction,
                    string.Join(", ", context.NameStringsForBlockAction));
            }

            if (context.NameStringsForLaterBlockAction.Count > 0)
            {
                ShowMessageFromServer(
                    "Some blocks will be switched off on the following grids when their owners are offline: {0}",
                    string.Join(", ", context.NameStringsForLaterBlockAction));
            }
        }
    }
}