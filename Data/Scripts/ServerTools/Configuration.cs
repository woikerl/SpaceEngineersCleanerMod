using System.Xml.Serialization;

namespace ServerTools
{
    public enum Mode
    {
        Off = 0,
        Warn = 1,
        Delete = 2,
        Admin = 3,
        Silent = 4
    }

    public class Configuration
    {
        public bool BlockToggle_Enabled = false;
        public int BlockToggle_Interval = 33 * 60 * 1000;

        [XmlArrayItem(ElementName = "BlockType", Type = typeof(string))]
        public string[] BlockToggle_BlockNames =
        {
            "MyObjectBuilder_MyProgrammableBlock",
            "MyObjectBuilder_Projector",
            "MyObjectBuilder_TimerBlock",
            "MyObjectBuilder_SensorBlock",
            "MyObjectBuilder_VirtualMass",
            "MyObjectBuilder_SpaceBall",
            "MyObjectBuilder_OreDetector",
            "MyObjectBuilder_GravityGenerator",
            "MyObjectBuilder_GravityGeneratorSphere",
            "MyObjectBuilder_Decoy",
            "MyObjectBuilder_MotorStator",
            "MyObjectBuilder_MotorAdvancedStator",
            "MyObjectBuilder_ExtendedPistonBase",
            "MyObjectBuilder_PistonBase",
            "MyObjectBuilder_Drill",
            "MyObjectBuilder_ShipWelder",
            "MyObjectBuilder_ShipGrinder"
        };

        public double BlockToggle_PlayerDistanceThresholdForWarning = 500;
        public double BlockToggle_PlayerDistanceThresholdForToggle = 1000;
        public bool BlockToggle_MessageAdminsOnly = true;

        public bool DeletePirates_Enabled = false;
        public int DeletePirates_Interval = 58 * 60 * 1000;
        public long DeletePirates_NPC_IdentityId = 0;
        public double DeletePirates_PlayerDistanceThresholdForWarning = 1000;
        public double DeletePirates_PlayerDistanceThresholdForDeletion = 8000;
        public bool DeletePirates_MessageAdminsOnly = true;

        public bool StopAllShips_Enabled = true;
        public int StopAllShips_Interval = 42 * 60 * 1000;

        public bool CleanPlanets_Enabled = true;
        public int CleanPlanets_Interval = 14 * 60 * 1000;

        public bool FloatingObjectDeletion_Enabled = true;
        public int FloatingObjectDeletion_Interval = 7 * 60 * 1000;
        public double FloatingObjectDeletion_PlayerDistanceThreshold = 100;
        public bool FloatingObjectDeletion_MessageAdminsOnly = true;

        public bool UnownedGridDeletion_Enabled = true;
        public int UnownedGridDeletion_Interval = 21 * 60 * 1000;
        public double UnownedGridDeletion_PlayerDistanceThreshold = 500;
        public int UnownedGridDeletion_BlockCountThreshold = 50;
        public bool UnownedGridDeletion_MessageAdminsOnly = true;

        public bool DamagedGridDeletion_Enabled = true;
        public int DamagedGridDeletion_Interval = 11 * 60 * 1000;
        public double DamagedGridDeletion_PlayerDistanceThreshold = 500;
        public int DamagedGridDeletion_BlockCountThreshold = 5;
        public bool DamagedGridDeletion_MessageAdminsOnly = true;

        public bool RespawnShipDeletion_Enabled = true;
        public int RespawnShipDeletion_Interval = 12 * 60 * 1000;

        [XmlArrayItem(ElementName = "GridName", Type = typeof(string))]
        public string[] RespawnShipDeletion_GridNames =
        {
            "Respawn Planet Pod",
            "Moon Drop Pod",
            "Respawn Space Pod"
        };

        public double RespawnShipDeletion_PlayerDistanceThresholdForWarning = 50;
        public double RespawnShipDeletion_PlayerDistanceThresholdForDeletion = 1000;
        public bool RespawnShipDeletion_MessageAdminsOnly = false;

        public string DeleterModes = "0 = off | 1 = warn only | 2 = delete | 3 = only inform admins | 4 = silent delete";
       
        public Mode UnrenamedGridDeletion_Mode = Mode.Warn;
        public int UnrenamedGridDeletion_Interval_sec = 12 * 60;
        public int UnrenamedGridDeletion_DeletionMinDistance_m = 1000;

        public Mode UnmarkedGridDeletion_Mode = Mode.Warn;
        public int UnmarkedGridDeletion_Interval_sec = 13 * 60;
        public int UnmarkedGridDeletion_DeletionMinDistance_m = 1000;

        [XmlArrayItem(ElementName = "BlockType", Type = typeof(string))]
        public string[] MarkerBlocks =
        {
            "MyObjectBuilder_Beacon",
            "MyObjectBuilder_GridSafer"
        };

        public bool MessagesFromFile_Enabled = true;
        public int MessagesFromFile_Interval = 60 * 1000;

        public bool PopupsFromFile_Enabled = true;
        public int PopupsFromFile_Interval = 60 * 1000;
    }
}