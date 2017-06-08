
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

    public class OldPirateDeleter : RepeatedAction
    {

        public OldPirateDeleter(double interval, long pirateid) : base(interval)
        {
            var m_data = new MissionData();
            NPCid = pirateid;
            MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && x.IdentityId == NPCid);  
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
                //if ((ent as IMyCubeGrid).SmallOwners.Contains(p.IdentityId)) return false;
                //if ((ent as IMyCubeGrid).SmallOwners == null) return false;
            }
            //return true;
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
                MyAPIGateway.Players.GetAllIdentites(m_data.players, (x) => x != null && x.IdentityId != null && x.IdentityId == NPCid);  
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
                        if (s != null) return true;
                        //(s as IMyCubeGrid).SyncObject.SendCloseRequest();
                        //write to log if ship is deleted
                        //string tested = null;
                        //tested = s.ToString();
                        //Logger.WriteLine("NPC has been deleted: {0} ", tested);
                    }
                }
                return false;

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


            MyAPIGateway.Utilities.ShowNotification("Server: NPC's have been deleted!", 40000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Use ignore tag '.' (no quotes) in ship name to be skipped.", 40000, MyFontEnum.Green);
            MyAPIGateway.Utilities.ShowNotification("Example: .MyCapturedShipName", 40000, MyFontEnum.Green);
        }


    }
}
