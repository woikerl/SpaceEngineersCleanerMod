﻿using System.Linq;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
	/// <summary>
	/// Deleter of cubegrids that have few blocks and some of which are damaged.
	/// </summary>
	public class DamagedGridDeleter : RepeatedDeleter<IMyCubeGrid, CubeGridDeletionContext>
	{
		private readonly int blockCountThreshold;

		public DamagedGridDeleter(double interval, double playerDistanceThreshold, int blockCountThreshold, bool messageAdminsOnly)
			: base(interval, messageAdminsOnly, new CubeGridDeletionContext() { PlayerDistanceThreshold = playerDistanceThreshold })
		{
			this.blockCountThreshold = blockCountThreshold;
		}

		protected override bool BeforeDelete(IMyCubeGrid grid, CubeGridDeletionContext context)
		{
			context.CurrentEntitySlimBlocks.Clear();
			grid.GetBlocksIncludingFromStaticallyAttachedCubeGrids(context.CurrentEntitySlimBlocks);

			if (context.CurrentEntitySlimBlocks.Count > blockCountThreshold)
				return false;

			if (context.CurrentEntitySlimBlocks.IsAttachedWheelGrid())
				return false;
    
            return context.CurrentEntitySlimBlocks.Any(slimBlock => slimBlock.CurrentDamage > 0);
		}

		protected override void AfterDeletion(CubeGridDeletionContext context)
		{
			if (context.EntitiesForDeletion.Count == 0)
				return;

			ShowMessageFromServer("Deleted {0} damaged grid(s) with less than {1} blocks:\n{3}.",
				context.EntitiesForDeletion.Count, blockCountThreshold, context.PlayerDistanceThreshold, string.Join(",\n", context.EntitiesForDeletionNames));
		}
	}
}
