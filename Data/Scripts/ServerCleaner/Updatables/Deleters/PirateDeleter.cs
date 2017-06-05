using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.ModAPI;

namespace ServerCleaner.Updatables.Deleters
{
    /// <summary>
    /// Delete Pirate and NPC entities (preferably at server reboot).
    /// </summary>
    public class DeleteNPCs : RepeatedAction
    {
        //private string Stoptag = ".";
        //private HashSet<IMyEntity> SelectedShips;

        public DeleteNPCs(double interval, double playerid) : base(interval)
        {
        }

        protected override bool ShouldRun()
        {
            /*
            try
            {
                var Ships = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(Ships, (x) => x is VRage.Game.ModAPI.Ingame.IMyCubeGrid && x.Physics != null && x.Physics.LinearVelocity.Length() > 30f && !x.DisplayName.Contains(Stoptag));
                if (Ships.Count != 0)
                {

                    SelectedShips = Ships;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in StopAllShips.ShouldRun: {0}", ex);
                return false;
            }
            */
        }


        protected override void Run()
        {
            /*
            foreach (var s in SelectedShips)
            {

                s.Physics.ClearSpeed();
            }
            MyAPIGateway.Utilities.ShowNotification("Server: Auto-stopped ships!", 20000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Example: .Myshipname", 20000, MyFontEnum.Green);
            */
        }


    }
}
