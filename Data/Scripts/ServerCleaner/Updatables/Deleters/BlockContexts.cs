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

	public class BlockContext<TEntity>
	{
		public double PlayerDistanceThresholdForAct;

		public HashSet<IMyEntity> Blocks = new HashSet<IMyEntity>();
		public List<IMyPlayer> Players = new List<IMyPlayer>();
		public List<Vector3D> PlayerPositions = new List<Vector3D>();

		public List<IMyEntity> BlocksForUpdate = new List<IMyEntity>();
		public List<string> BlocksForUpdateNames = new List<string>();

		public virtual void Prepare()
		{
			Blocks.Clear();
			MyAPIGateway.Entities.GetEntities(Blocks, entity => entity is TEntity);

			Players.Clear();
			MyAPIGateway.Players.GetPlayers(Players, player => player != null);

			PlayerPositions.Clear(); // Player positions are used by some deleters even when PlayerDistanceThreshold == 0
			foreach (var player in Players)
				PlayerPositions.Add(player.GetPosition());

			BlocksForUpdate.Clear();
			BlocksForUpdateNames.Clear();
		}
	}
    //changing this to IMyCubeBlock clears error in blocktoggle.cs but i doubt this is going to yield the desired function
	public class CubeGridBlockContext : BlockContext<IMyCubeGrid>
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
