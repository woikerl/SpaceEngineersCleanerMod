using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ServerTools.Updateables.Deleter
{
	public abstract class RepeatedBlockAction<TEntity, TGridContext> : RepeatedAction
		where TEntity : class
		where TGridContext : GridContext<TEntity>
	{
		private TGridContext context;
		private bool messageAdminsOnly;
        private string[] BlockNames;


        public RepeatedBlockAction(double interval, bool messageAdminsOnly, string[] BlockNames, TGridContext initialBlockActionContext) : base(interval)
		{
			this.messageAdminsOnly = messageAdminsOnly;
            this.context = initialBlockActionContext;
            this.BlockNames = BlockNames;
        }

        protected override void Run()
		{
			try
			{
				context.Prepare();

				foreach (var untypedEntity in context.Grids)
				{

                    var entity = untypedEntity as TEntity;

					if (entity == null)
						continue;

					if (untypedEntity.MarkedForClose || untypedEntity.Closed)
						continue;

					if (untypedEntity.Physics == null)
						continue; // projection/block placement indicator?

					if (context.PlayerDistanceThresholdForAct > 0 && Utilities.AnyWithinDistance(untypedEntity.GetPosition(), context.PlayerPositions, context.PlayerDistanceThresholdForAct))
						continue;

					if (!BeforeAction(entity, context))
						continue;

                    

					context.GridsForUpdate.Add(untypedEntity);
				}

				if (context.GridsForUpdate.Count > 0)
				{

                    Logger.WriteLine("{0}: toggling {1} entities", GetType().Name, context.Grids.Count); // TODO: log more details

                    //var allShips = context.GridsForUpdate;
                    //MyAPIGateway.Entities.GetEntities(allShips, e => e is IMyCubeGrid);
                    foreach (var entity in context.GridsForUpdate)
					{

                        //MyAPIGateway.Entities.GetEntities(entity, e => e is IMyCubeGrid);
                        //entity.GetChildren(blocks, f => f. != null);
                        //entity.GetChildren(blocks, f => f.GetChildren.blocks != null);
                        //entity.Flags.Equals

                        // you are ready to write the logic to toggle blocks. you wrote the check in blocktoggle.beforeaction
                        //if (entity.SyncObject == null)
                        //{
                        var cubeGrid = (IMyCubeGrid)entity;
                        var blocks = new List<IMySlimBlock>();
                        cubeGrid.GetBlocks(blocks, f => f.FatBlock != null);
                          foreach (var block in blocks)
                          {
                      // now here we need the logic to loop through the blocks to identify if any of the names match the configuration list.
                              if (BlockNames.Contains(block.FatBlock.BlockDefinition.TypeIdString))
                              {
                                if (((IMyFunctionalBlock)block.FatBlock).Enabled == true)
                                {
                                    ((IMyFunctionalBlock)block.FatBlock).Enabled = false; // turn power on/off.
                                    //var ex = "";
                                    //ShowMessageFromServer("Debug: enable code reached" + ex);
                                    //counter++;
                                }
                              }
                          } 
                        //}
                        context.GridsForUpdateNames.Add(entity.DisplayName);
					}
				}

				AfterAction(context);
			}
			catch (Exception ex)
			{
				Logger.WriteLine("Exception in RepeatedBlockAction.Run: {0}", ex);
				ShowMessageFromServer("Oh no, there was an error while I was toggling stuff, let's hope nothing broke: " + ex.Message);
			}
		}

		protected virtual bool BeforeAction(TEntity entity, TGridContext context)
		{
			return true;
		}

		protected virtual void AfterAction(TGridContext context)
		{
		}

		protected void ShowMessageFromServer(string format, params object[] args)
		{
			if (messageAdminsOnly)
				Utilities.ShowMessageFromServerToAdmins(format, args);
			else
				Utilities.ShowMessageFromServerToEveryone(format, args);
		}



    }
}
