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
        private string[] BlockNames;
        private Mode mode;


        public RepeatedBlockAction(double interval, bool messageAdminsOnly, string[] blockNames,
            TGridContext initialBlockActionContext) : base(interval)
        {
            this.mode = messageAdminsOnly ? Mode.Admin : Mode.Silent;
            this.context = initialBlockActionContext;
            this.BlockNames = blockNames;
        }

        public RepeatedBlockAction(int interval, Mode mode, string[] blockNames,
            TGridContext initialBlockActionContext) : base(interval)
        {
            this.mode = mode;
            this.BlockNames = blockNames;
            this.context = initialBlockActionContext;
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

                    if (context.PlayerDistanceThresholdForAct > 0 && Utilities.AnyWithinDistance(
                            untypedEntity.GetPosition(), context.PlayerPositions,
                            context.PlayerDistanceThresholdForAct))
                        continue;

                    if (!BeforeAction(entity, context))
                        continue;

                    context.GridsForUpdate.Add(untypedEntity);
                }

                if (context.GridsForUpdate.Count > 0)
                {
                    Logger.WriteLine("{0}: toggling {1} entities", GetType().Name,
                        context.Grids.Count); // TODO: log more details

                    foreach (var entity in context.GridsForUpdate)
                    {
                        var cubeGrid = (IMyCubeGrid) entity;
                        var blocks = new List<IMySlimBlock>();
                        cubeGrid.GetBlocks(blocks, f => f.FatBlock != null);

                        foreach (var block in blocks)
                        {
                            if (BlockNames.Contains(block.FatBlock.BlockDefinition.TypeIdString))
                            {
                                ((IMyFunctionalBlock) block.FatBlock).Enabled = false;
                            }
                        }

                        context.GridsForUpdateNames.Add(entity.DisplayName);
                    }
                }

                AfterAction(context);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in RepeatedBlockAction.Run: {0}", ex);
                ShowMessageFromServer(
                    "Oh no, there was an error while I was toggling stuff, let's hope nothing broke: " + ex.Message);
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
            if (mode == Mode.Admin)
                Utilities.ShowMessageFromServerToAdmins(format, args);
        }
    }
}