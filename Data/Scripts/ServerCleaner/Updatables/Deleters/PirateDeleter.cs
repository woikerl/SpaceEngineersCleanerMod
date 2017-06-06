using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace ServerCleaner.Updatables.Deleters
{
    /// <summary>
    /// Delete Pirate and NPC entities (preferably at server reboot).
    /// </summary>
    /// 

    public class DeleteNPCs : RepeatedAction
    {
        //private string Stoptag = ".";
        //private HashSet<IMyEntity> SelectedShips;

        public DeleteNPCs(double interval, long playerid) : base(interval)
        {
            
            var m_data = new MissionData();
            NPCid = playerid;
            MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && x.IdentityId == NPCid);  //pirates have always been 144115188075855873 identity
            SelectedNPCs = m_data;

        }

        class MissionData
        {
            public List<IMyIdentity> players = new List<IMyIdentity>();
            public List<IMyCubeGrid> CheckedShips = new List<IMyCubeGrid>();
            public DateTime StartTime;
        }
        private long NPCid;
        private MissionData SelectedNPCs;
        private HashSet<IMyEntity> SelectedShips;

        private bool checkOwners(IMyEntity ent)
        {
            foreach (var p in SelectedNPCs.players)
            {
                if ((ent as IMyCubeGrid).BigOwners.Contains(p.IdentityId)) return true;
            }
            return false;
        }

        protected override bool ShouldRun()
        {

           try
          {
                SelectedShips = null;
                SelectedNPCs = null; 
                //m_data.players.Clear();
                //MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && !x.DisplayName.Contains("Space Pirates"));
                string Stoptag = ".";
                var m_data = new MissionData();
                MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && x.IdentityId == NPCid);  //pirates have always been 144115188075855873 identity
                SelectedNPCs = m_data;
                var Ships = new HashSet<IMyEntity>();
                MyAPIGateway.Entities.GetEntities(Ships, (x) => x is IMyCubeGrid && x.Physics != null && !x.DisplayName.Contains(Stoptag));
                if (Ships.Count == 0) return false;

                SelectedShips = Ships;
                foreach (var s in SelectedShips)
                {
                    //if (!checkOwners(s))
                    if (checkOwners(s))
                    {
                        if (s == null) return false;
                        //(s as IMyCubeGrid).SyncObject.SendCloseRequest();
                        //write to log if ship is deleted
                        //string tested = null;
                        //tested = s.ToString();
                        //Logger.WriteLine("NPC has been deleted: {0} ", tested);
                    }
                    if (!checkOwners(s))
                    { return false; }
                }
                return true;

            }
            catch (Exception ex)
          {
              Logger.WriteLine("Exception in PirateDeleter.ShouldRun: {0}", ex);
              return false;
          }
            
        }

        protected override void Run()
        {


            foreach (var s in SelectedShips)
            {
                //if (!checkOwners(s))
                if (checkOwners(s))
                {
                    if (s == null) return;
                    (s as IMyCubeGrid).SyncObject.SendCloseRequest();
                    //write to log if ship is deleted
                    string tested = null;
                    tested = s.ToString();
                    Logger.WriteLine("NPC has been deleted: {0} ", tested);
                }

            }


            MyAPIGateway.Utilities.ShowNotification("Server: NPC's have been deleted!", 20000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Use ignore tag '.' (no quotes) in ship name to be skipped.", 20000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Example: .MyCapturedShipName", 20000, MyFontEnum.Green);
        }


    }
}
