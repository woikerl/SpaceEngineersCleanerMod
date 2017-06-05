using Sandbox.ModAPI;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.ModAPI;
using Scripts.jukes;

namespace ServerCleaner.Updatables
{
    /// <summary>
    /// stops all grids unless they use a period in the grid name.
    /// </summary>
    public class StopallShips : RepeatedAction
    {
        public static MessageHandler MsgHandle;
        private string Stoptag = ".";
        private HashSet<IMyEntity> SelectedShips;

        public StopallShips(double interval) : base(interval)
        {
        /*  
            var Ships = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(Ships, (x) => x is VRage.Game.ModAPI.Ingame.IMyCubeGrid && x.Physics != null && x.Physics.LinearVelocity.Length() > 1f && !x.DisplayName.Contains(Stoptag));
            if (Ships.Count == 0) return;
            foreach (var s in Ships)
            {
                s.Physics.ClearSpeed();
            }
            MsgHandle.SendNotificationAll("Server: Auto-stopped ships!", 20000);
            MsgHandle.SendNotificationAll("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000);
            MsgHandle.SendNotificationAll("Example: .Myshipname", 20000);
        */
        }

        protected override bool ShouldRun()
        {
            try
            {
                var Ships = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(Ships, (x) => x is VRage.Game.ModAPI.Ingame.IMyCubeGrid && x.Physics != null && x.Physics.LinearVelocity.Length() > 1f && !x.DisplayName.Contains(Stoptag));
                if (Ships.Count != 0)
                {

                    SelectedShips = Ships;
                }

                return true;
                //return !string.IsNullOrWhiteSpace(nextMessage);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in StopAllShips.ShouldRun: {0}", ex);
                return false;
            }
            //return false;
        }


        protected override void Run()
        {
            //var Ships = new HashSet<IMyEntity>();
            //MyAPIGateway.Entities.GetEntities(Ships, (x) => x is VRage.Game.ModAPI.Ingame.IMyCubeGrid && x.Physics != null && x.Physics.LinearVelocity.Length() > 1f && !x.DisplayName.Contains(Stoptag));
            //if (Ships.Count == 0) return;
            //foreach (var s in Ships)
            //{
             //   s.Physics.ClearSpeed();
            //}

            //MyAPIGateway.Entities.GetEntities(Selected Ships, (x) => x is VRage.Game.ModAPI.Ingame.IMyCubeGrid && x.Physics != null && x.Physics.LinearVelocity.Length() > 1f && !x.DisplayName.Contains(Stoptag));
            //if (Ships.Count == 0) return;
            foreach (var s in SelectedShips)
            {
                s.Physics.ClearSpeed();
            }
            MsgHandle.SendNotificationAll("Server: Auto-stopped ships!", 20000);
            MsgHandle.SendNotificationAll("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000);
            MsgHandle.SendNotificationAll("Example: .Myshipname", 20000);
        }


    }
}
