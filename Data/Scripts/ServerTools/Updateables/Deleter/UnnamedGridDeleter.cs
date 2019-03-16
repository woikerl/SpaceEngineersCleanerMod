using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
    /// <summary>
    /// Deleter of grids that have default names. Does not delete the ships matched by the RespawnShipDeleter class.
    /// </summary>
    public class UnnamedGridDeleter : RepeatedDeleter<IMyCubeGrid, ComplexCubeGridDeletionContext>
    {
        public static Regex[] DefaultNameRegexes = new[]
        {
            new Regex("^Small Grid [0-9]+$"),
            new Regex("^Small Ship [0-9]+$"),
            new Regex("^Large Grid [0-9]+$"),
            new Regex("^Large Ship [0-9]+$"),
            new Regex("^Static Grid [0-9]+$"),
            new Regex("^Platform [0-9]+$")
        };

        private List<string> vipNames;

        public UnnamedGridDeleter(Mode mode, int interval, int deletionMinDistance, List<string> vipNames)
            : base(mode, interval, new ComplexCubeGridDeletionContext()
            {
                PlayerDistanceThreshold = 0,
                PlayerDistanceThresholdForActualDeletion = deletionMinDistance
            })
        {
            this.mode = mode;
            this.vipNames = vipNames;
        }

        protected override bool BeforeDelete(IMyCubeGrid grid, ComplexCubeGridDeletionContext context)
        {
            // Is the grid unrenamed?
            if (!IsNameDefault(grid.DisplayName))
                return false;

            // Is there a beacon or an antenna? Merge blocks reset ship names, renaming can get quite tedious

            context.CurrentEntitySlimBlocks.Clear();
            grid.GetBlocksIncludingFromStaticallyAttachedCubeGrids(context.CurrentEntitySlimBlocks);

            //if (context.CurrentEntitySlimBlocks.Any(slimBlock => slimBlock.FatBlock != null && (slimBlock.FatBlock is IMyRadioAntenna || slimBlock.FatBlock is IMyBeacon)))
            //    return false;

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
                    identity.IdentityId == ownerId && vipNames.Contains(identity.DisplayName)))
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

        public static bool IsNameDefault(string name)
        {
            foreach (var regex in DefaultNameRegexes)
            {
                if (regex.IsMatch(name))
                    return true;
            }

            return false;
        }

        protected override void AfterDeletion(ComplexCubeGridDeletionContext context)
        {
            if (context.EntitiesForDeletion.Count > 0)
            {
                ShowMessageFromServer("Deleted {0} unrenamed grid(s):\n{1}.",
                    context.EntitiesForDeletion.Count,
                    string.Join(",\n", context.NameStringsForDeletion));
            }

            if (context.NameStringsForLaterDeletion.Count <= 0) return;

            ShowMessageFromServer("{0} unrenamed grid(s) may be deleted later:\n{1}",
                context.NameStringsForLaterDeletion.Count, string.Join(",\n", context.NameStringsForLaterDeletion));
            MyAPIGateway.Utilities.ShowNotification("Server: Offline owner grid cleanup is configured!", 30000,
                MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Unrenamed grids will be deleted!", 30000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Remember to rename your ships.", 30000, MyFontEnum.Green);
        }
    }
}