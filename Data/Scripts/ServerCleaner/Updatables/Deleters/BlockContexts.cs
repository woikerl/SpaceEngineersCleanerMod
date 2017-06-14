using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace ServerCleaner.Updatables.Deleters
{
	// These classes were orginally created to prevent extra object allocations,
	// but the code might be cleaner without them. Alternatively, they could
	// be expanded to include some kind of caching...

	public class GridContext<TEntity>
	{
		public double PlayerDistanceThresholdForAct;

		public HashSet<IMyEntity> Grids = new HashSet<IMyEntity>();
		public List<IMyPlayer> Players = new List<IMyPlayer>();
		public List<Vector3D> PlayerPositions = new List<Vector3D>();

		public List<IMyEntity> GridsForUpdate = new List<IMyEntity>();
		public List<string> GridsForUpdateNames = new List<string>();

		public virtual void Prepare()
		{
			Grids.Clear();
            //MyAPIGateway.Entities.GetEntities(Grids, entity => entity is TEntity);
            MyAPIGateway.Entities.GetEntities(Grids, e => e is IMyCubeGrid);

            Players.Clear();
			MyAPIGateway.Players.GetPlayers(Players, player => player != null);

			PlayerPositions.Clear(); // Player positions are used by some deleters even when PlayerDistanceThreshold == 0
			foreach (var player in Players)
				PlayerPositions.Add(player.GetPosition());

			GridsForUpdate.Clear();
			GridsForUpdateNames.Clear();
		}
	}
    //changing this to IMyCubeBlock clears error in blocktoggle.cs but i doubt this is going to yield the desired function
	public class CubeGridBlockContext : GridContext<IMyCubeGrid>
	{
		public List<IMySlimBlock> CurrentEntitySlimBlocks = new List<IMySlimBlock>();
	}

	public class ComplexCubeGridBlockContext : CubeGridBlockContext
	{
		public double PlayerDistanceThresholdForActualAction;
        
		public List<IMyIdentity> PlayerIdentities = new List<IMyIdentity>();
		public List<long> OnlinePlayerIds = new List<long>();

		public List<string> NameStringsForBlockAction = new List<string>();
		public List<string> NameStringsForLaterBlockAction = new List<string>();

		public override void Prepare()
		{
			base.Prepare();

			PlayerIdentities.Clear();
			MyAPIGateway.Players.GetAllIdentites(PlayerIdentities);

			OnlinePlayerIds.Clear();

			foreach (var player in Players)
			{
				if (player.Client == null)
					continue;

				OnlinePlayerIds.Add(player.IdentityId);
			}

			NameStringsForBlockAction.Clear();
			NameStringsForLaterBlockAction.Clear();
		}
	}
}
