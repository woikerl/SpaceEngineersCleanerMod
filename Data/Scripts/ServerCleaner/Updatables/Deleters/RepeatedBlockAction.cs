using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerCleaner.Updatables.Deleters
{
	public abstract class RepeatedBlockAction<TEntity, TBlockContext> : RepeatedAction
		where TEntity : class
		where TBlockContext : BlockContext<TEntity>
	{
		private TBlockContext context;
		private bool messageAdminsOnly;

		public RepeatedBlockAction(double interval, bool messageAdminsOnly, TBlockContext initialBlockActionContext) : base(interval)
		{
			this.messageAdminsOnly = messageAdminsOnly;
			this.context = initialBlockActionContext;
		}

		protected override void Run()
		{
			try
			{
				context.Prepare();

				foreach (var untypedEntity in context.Blocks)
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

					context.BlocksForUpdate.Add(untypedEntity);
				}

				if (context.BlocksForUpdate.Count > 0)
				{
					Logger.WriteLine("{0}: toggling {1} entities", GetType().Name, context.Blocks.Count); // TODO: log more details

                    var config = GetConfiguration();
                    var vipNames = GetVipNames();



                    foreach (var entity in context.BlocksForUpdate)
					{
						if (entity.SyncObject == null)
							//entity.Delete();
						else
							//entity.SyncObject.SendCloseRequest();

						context.BlocksForUpdateNames.Add(entity.DisplayName);
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

		protected virtual bool BeforeAction(TEntity entity, TBlockContext context)
		{
			return true;
		}

		protected virtual void AfterAction(TBlockContext context)
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
