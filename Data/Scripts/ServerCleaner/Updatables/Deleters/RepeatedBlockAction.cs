using Sandbox.ModAPI;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using VRage.Game.ModAPI;

namespace ServerCleaner.Updatables.Deleters
{
	public abstract class RepeatedBlockAction<TEntity, TGridContext> : RepeatedAction
		where TEntity : class
		where TGridContext : GridContext<TEntity>
	{
		private TGridContext context;
		private bool messageAdminsOnly;
        private string[] BlockNames;

        [Flags]
        private enum KindofBlock
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

                    var config = GetConfiguration();
                    var vipNames = GetVipNames();



                    foreach (var entity in context.GridsForUpdate)
					{

                        //shit is about to get ugly. you need to study in midspace's code (CommandShipSwitch.cs) how he uses IMyCubeGrid cubeGrid 
                        //parm in SwitchShipSystemsOnOff. also you need to come up with a way to use BlockNames array here to validate the blocks 
                        //desired are getting updated.

                        //your IMyCubeGrid is passed to a function that then passes to another function to get the blocks
                        //(midspace code - commandshipswitch.cs, SwitchSystemsOnOff, SwitchShipSystemsOnOff).
                        //you may need to duplicate this. you are doing this in blocktoggle.cs, beforeaction.

                        //once this is figured out all thats left is to test!

                        //int counter = 0;
                        //var blocks = new List<IMySlimBlock>();
                        //entity.GetBlocks(blocks, f => f.FatBlock != null);
                        //entity.GetChildren(blocks, f => f.GetChildren.blocks != null);
                        //entity.Flags.Equals

                        // you are ready to write the logic to toggle blocks. you wrote the check in blocktoggle.beforeaction
                        //if (entity.SyncObject == null)
                        //    foreach (var block in blocks)
                        //    {
                        // now here we need the logic to loop through the blocks to identify if any of the names match the configuration list.
                        //        if (BlockNames.Contains(block.FatBlock.BlockDefinition.TypeIdString))
                        //            counter++;
                        //    }
                        //else
                        //entity.SyncObject.SendCloseRequest();

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

        private Configuration GetConfiguration()
        {
            var config = new Configuration();

            try
            {
                var fileName = string.Format("Config_{0}.xml", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));

                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(fileName, GetType()))
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(fileName, GetType()))
                    {
                        config = MyAPIGateway.Utilities.SerializeFromXML<Configuration>(reader.ReadToEnd());
                    }
                }

                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, GetType()))
                {
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in MainLogic.GetConfiguration(), using the default settings: {0}", ex);
            }

            return config;
        }

        private List<string> GetVipNames()
        {
            var vipNames = new List<string>();

            try
            {
                var fileName = string.Format("VIP_Names_{0}.txt", Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));

                if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(fileName, GetType()))
                {
                    using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, GetType())) // Create an empty file
                    {
                    }
                }

                using (var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(fileName, GetType()))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length == 0)
                            continue;

                        vipNames.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in MainLogic.GetVipNames(), using an empty list: {0}", ex);
            }

            return vipNames;
        }


    }
}
