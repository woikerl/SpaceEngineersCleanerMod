using System.Xml.Serialization;

namespace ServerCleaner
{
	public class Configuration
	{

        public bool BlockToggle_Enabled = false;
        public int BlockToggle_Interval = 30 * 60 * 1000;
        [XmlArrayItem(ElementName = "BlockType", Type = typeof(string))]
        public string[] BlockToggle_BlockNames = { "MyObjectBuilder_MyProgrammableBlock", "MyObjectBuilder_Projector", "MyObjectBuilder_TimerBlock", "MyObjectBuilder_SensorBlock","MyObjectBuilder_VirtualMass","MyObjectBuilder_SpaceBall","MyObjectBuilder_OreDetector","MyObjectBuilder_GravityGenerator","MyObjectBuilder_GravityGeneratorSphere","MyObjectBuilder_Decoy","MyObjectBuilder_MotorStator","MyObjectBuilder_MotorAdvancedStator","MyObjectBuilder_ExtendedPistonBase","MyObjectBuilder_PistonBase","MyObjectBuilder_Drill","MyObjectBuilder_ShipWelder","MyObjectBuilder_ShipGrinder" };
        /*
        public bool BlockToggle_Power_Enabled = false;
        public bool BlockToggle_Prod_Enabled = false;
        public bool BlockToggle_Prog_Enabled = false;
        public bool BlockToggle_Proj_Enabled = false;
        public bool BlockToggle_Sensor_Enabled = false;
        public bool BlockToggle_Spot_Enabled = false;
        public bool BlockToggle_Timer_Enabled = false;
        public bool BlockToggle_Weapons_Enabled = false;
        //Set Medical block share to faction if set to all;
        //Set Medical block power to off if nobody owns it.
        public bool BlockToggle_MediFacNobody_Enabled = false;
        public bool BlockToggle_Mass_Enabled = false;
        public bool BlockToggle_Grav_Enabled = false;
        public bool BlockToggle_Grinder_Enabled = false;
        public bool BlockToggle_Welder_Enabled = false;
        */
        public double BlockToggle_PlayerDistanceThresholdForWarning = 1000;
        public double BlockToggle_PlayerDistanceThresholdForDeletion = 8000;
        public bool BlockToggle_MessageAdminsOnly = true;

        public bool DeletePirates_Enabled = false;
        public int DeletePirates_Interval = 60 * 60 * 1000;
        public long DeletePirates_NPC_IdentityId = 0;
        public double DeletePirates_PlayerDistanceThresholdForWarning = 1000;
        public double DeletePirates_PlayerDistanceThresholdForDeletion = 8000;
        public bool DeletePirates_MessageAdminsOnly = true;

        public bool StopAllShips_Enabled = true;
        public int StopAllShips_Interval = 40 * 60 * 1000;
        
		public bool FloatingObjectDeletion_Enabled = true;
		public int FloatingObjectDeletion_Interval = 7 * 60 * 1000;
		public double FloatingObjectDeletion_PlayerDistanceThreshold = 100;
		public bool FloatingObjectDeletion_MessageAdminsOnly = true;

		public bool UnownedGridDeletion_Enabled = true;
		public int UnownedGridDeletion_Interval = 9 * 60 * 1000;
		public double UnownedGridDeletion_PlayerDistanceThreshold = 500;
		public int UnownedGridDeletion_BlockCountThreshold = 50;
		public bool UnownedGridDeletion_MessageAdminsOnly = true;

		public bool DamagedGridDeletion_Enabled = true;
		public int DamagedGridDeletion_Interval = 10 * 60 * 1000;
		public double DamagedGridDeletion_PlayerDistanceThreshold = 500;
		public int DamagedGridDeletion_BlockCountThreshold = 5;
		public bool DamagedGridDeletion_MessageAdminsOnly = true;

		public bool RespawnShipDeletion_Enabled = true;
		public int RespawnShipDeletion_Interval = 11 * 60 * 1000;
		[XmlArrayItem(ElementName = "GridName", Type = typeof(string))]
		public string[] RespawnShipDeletion_GridNames = { "Atmospheric Lander mk.1", "RespawnShip", "RespawnShip2" };
		public double RespawnShipDeletion_PlayerDistanceThresholdForWarning = 50;
		public double RespawnShipDeletion_PlayerDistanceThresholdForDeletion = 1000;
		public bool RespawnShipDeletion_MessageAdminsOnly = false;

		public bool UnrenamedGridDeletion_Enabled = true;
		public int UnrenamedGridDeletion_Interval = 13 * 60 * 1000;
		public double UnrenamedGridDeletion_PlayerDistanceThresholdForWarning = 0;
		public double UnrenamedGridDeletion_PlayerDistanceThresholdForDeletion = 1000;
		public bool UnrenamedGridDeletion_WarnOnly = true;
		public bool UnrenamedGridDeletion_MessageAdminsOnly = false;

		public bool MessagesFromFile_Enabled = true;
		public int MessagesFromFile_Interval = 60 * 1000;

		public bool PopupsFromFile_Enabled = true;
		public int PopupsFromFile_Interval = 60 * 1000;
	}
}
