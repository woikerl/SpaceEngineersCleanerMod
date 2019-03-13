﻿using System.Linq;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
    public class RespawnShipDeleter : RepeatedDeleter<IMyCubeGrid, ComplexCubeGridDeletionContext>
    {
        private string[] respawnShipNames;

        public RespawnShipDeleter(double interval, string[] respawnShipNames, double playerDistanceThresholdForWarning,
            double playerDistanceThresholdForDeletion, bool messageAdminsOnly)
            : base(interval, messageAdminsOnly, new ComplexCubeGridDeletionContext()
            {
                PlayerDistanceThreshold = playerDistanceThresholdForWarning,
                PlayerDistanceThresholdForActualDeletion = playerDistanceThresholdForDeletion
            })
        {
            this.respawnShipNames = respawnShipNames;
        }

        protected override bool BeforeDelete(IMyCubeGrid grid, ComplexCubeGridDeletionContext context)
        {
            // Is it a respawn ship?

            if (!respawnShipNames.Contains(grid.DisplayName))
                return false;

            // Are any of the owners online?

            var nameString = string.Format("{0} (owned by {1})", grid.DisplayName,
                Utilities.GetOwnerNameString(grid, context.PlayerIdentities));

            foreach (var ownerID in grid.SmallOwners)
            {
                if (!context.OnlinePlayerIds.Contains(ownerID))
                    continue;

                // At least one owner is online, warn him

                context.NameStringsForLaterDeletion.Add(nameString);
                return false;
            }

            // Are any other players nearby?

            if (context.PlayerDistanceThresholdForActualDeletion > 0 && Utilities.AnyWithinDistance(
                    grid.GetPosition(), context.PlayerPositions, context.PlayerDistanceThresholdForActualDeletion))
                return false;

            context.NameStringsForDeletion.Add(nameString);
            return true;
        }

        protected override void AfterDeletion(ComplexCubeGridDeletionContext context)
        {
            if (context.EntitiesForDeletion.Count > 0)
            {
                ShowMessageFromServer(
                    "Deleted {0} respawn ship(s) that had no owner online and no players within {1} m: {2}.",
                    context.EntitiesForDeletion.Count, context.PlayerDistanceThresholdForActualDeletion,
                    string.Join(", ", context.NameStringsForDeletion));
            }

            if (context.NameStringsForLaterDeletion.Count > 0)
            {
                ShowMessageFromServer(
                    "I'm going to delete the following respawn ship(s) later unless they are renamed: {0}",
                    string.Join(", ", context.NameStringsForLaterDeletion));
            }
        }
    }
}