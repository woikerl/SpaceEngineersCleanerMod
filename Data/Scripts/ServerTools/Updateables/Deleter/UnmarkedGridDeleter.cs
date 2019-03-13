using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
    /// <summary>
    /// Deleter of grids that have no whitelisted block set, built and/or active
    /// </summary>
    public class UnmarkedGridDeleter : RepeatedDeleter<IMyCubeGrid, ComplexCubeGridDeletionContext>
    {
        private List<string> whitelistedPlayers;
        private string[] markerBlockNames;

        public UnmarkedGridDeleter(Mode mode, int interval, int deletionMinDistance, string[] markerBlockNames,
            List<string> whitelistedPlayers)
            : base(mode, interval, new ComplexCubeGridDeletionContext()
            {
                PlayerDistanceThreshold = 0,
                PlayerDistanceThresholdForActualDeletion = deletionMinDistance
            })
        {
            this.mode = mode;
            this.whitelistedPlayers = whitelistedPlayers;
            this.markerBlockNames = markerBlockNames;
        }

        protected override bool BeforeDelete(IMyCubeGrid grid, ComplexCubeGridDeletionContext context)
        {
            // Is there a whitelisted block?
            context.CurrentEntitySlimBlocks.Clear();
            grid.GetBlocksIncludingFromStaticallyAttachedCubeGrids(context.CurrentEntitySlimBlocks);

            if (context.CurrentEntitySlimBlocks.Any(slimBlock =>
                slimBlock.FatBlock != null &&
                markerBlockNames.Contains(slimBlock.FatBlock.BlockDefinition.TypeIdString)))
                return false;

            // Wheel stator=null workaround
            if (context.CurrentEntitySlimBlocks.IsAttachedWheelGrid())
            {
                return false;
            }

            // Are any of the owners online or VIP?
            var nameString = string.Format("{0} (owned by {1})", grid.DisplayName,
                Utilities.GetOwnerNameString(grid, context.PlayerIdentities));

            foreach (var ownerId in grid.SmallOwners)
            {
                if (context.PlayerIdentities.Any(identity =>
                    identity.IdentityId == ownerId && whitelistedPlayers.Contains(identity.DisplayName)))
                    return false;

                if (!context.OnlinePlayerIds.Contains(ownerId))
                    continue;

                // At least one owner is online, warn him

                context.NameStringsForLaterDeletion.Add(nameString);
                return false;
            }

            // Are any other players nearby?

            if (context.PlayerDistanceThresholdForActualDeletion > 0 && Utilities.AnyWithinDistance(
                    grid.GetPosition(), context.PlayerPositions, context.PlayerDistanceThresholdForActualDeletion))
                return false;

            if (mode == Mode.Warn)
                return false;

            context.NameStringsForDeletion.Add(nameString);
            return true;
        }

        protected override void AfterDeletion(ComplexCubeGridDeletionContext context)
        {
            if (context.EntitiesForDeletion.Count > 0)
            {
                ShowMessageFromServer("Deleted {0} unmarked grid(s):\n{1}.",
                    context.EntitiesForDeletion.Count, string.Join("\n", context.NameStringsForDeletion));
            }

            if (context.NameStringsForLaterDeletion.Count <= 0) return;

            ShowMessageFromServer("{0} unmarked grid(s) may get deleted soon:\n{1}",
                context.NameStringsForLaterDeletion.Count, string.Join("\n", context.NameStringsForLaterDeletion));

            MyAPIGateway.Utilities.ShowNotification("Server: Offline owner grid cleanup is configured!", 30000,
                MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification(
                "Unmarked Grids will be deleted!", 30000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Remember to attach a marker block.",
                30000, MyFontEnum.Green);
        }
    }
}