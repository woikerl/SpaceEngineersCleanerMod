﻿using System;

namespace ServerTools.Updateables.Deleter

{
    public abstract class RepeatedDeleter<TEntity, TDeletionContext> : RepeatedAction
        where TEntity : class
        where TDeletionContext : DeletionContext<TEntity>
    {
        private TDeletionContext context;
        private bool messageAdminsOnly;
        protected Mode mode = Mode.Warn;

        public RepeatedDeleter(double interval, bool messageAdminsOnly, TDeletionContext initialDeletionContext) :
            base(interval)
        {
            this.messageAdminsOnly = messageAdminsOnly;
            this.context = initialDeletionContext;
        }

        public RepeatedDeleter(Mode mode, int interval, TDeletionContext initialDeletionContext) : base(interval * 1000)
        {
            this.mode = mode;
            this.messageAdminsOnly = mode == Mode.Admin;
            this.context = initialDeletionContext;
        }

        protected override void Run()
        {
            try
            {
                context.Prepare();

                foreach (var untypedEntity in context.Entities)
                {
                    var entity = untypedEntity as TEntity;

                    if (entity == null)
                        continue;

                    if (untypedEntity.MarkedForClose || untypedEntity.Closed)
                        continue;

                    if (untypedEntity.Physics == null)
                        continue; // projection/block placement indicator?

                    if (context.PlayerDistanceThreshold > 0 && Utilities.AnyWithinDistance(untypedEntity.GetPosition(),
                            context.PlayerPositions, context.PlayerDistanceThreshold))
                        continue;

                    if (!BeforeDelete(entity, context))
                        continue;

                    context.EntitiesForDeletion.Add(untypedEntity);
                }

                if (context.EntitiesForDeletion.Count > 0)
                {
                    Logger.WriteLine("{0}: deleting {1} entities", GetType().Name,
                        context.EntitiesForDeletion.Count); // TODO: log more details

                    foreach (var entity in context.EntitiesForDeletion)
                    {
                        if (entity.SyncObject == null)
                            entity.Delete();
                        else
                            entity.SyncObject.Entity.Close();
                        //entity.SyncObject.SendCloseRequest();

                        context.EntitiesForDeletionNames.Add(entity.DisplayName);
                    }
                }

                AfterDeletion(context);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in RepeatedDeleter.Run: {0}", ex);
                ShowMessageFromServer(
                    "Oh no, there was an error while I was deleting stuff, let's hope nothing broke: " + ex.Message);
            }
        }

        protected virtual bool BeforeDelete(TEntity entity, TDeletionContext context)
        {
            return true;
        }

        protected virtual void AfterDeletion(TDeletionContext context)
        {
        }

        protected void ShowMessageFromServer(string format, params object[] args)
        {
            if (mode == Mode.Silent) return;
            
            if (messageAdminsOnly)
                Utilities.ShowMessageFromServerToAdmins(format, args);
            else
                Utilities.ShowMessageFromServerToEveryone(format, args);
        }
    }
}