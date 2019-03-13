﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sandbox.ModAPI;
using ServerTools.Updateables;
using ServerTools.Updateables.Deleter;
using VRage.Game.Components;

namespace ServerTools
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class MainLogic : MySessionComponentBase
    {
        // TODO: spider/dog/general consolidated npc periodic deletion.
        // TODO: Pirate majority owned and damaged grids perdiodic deletion.
        // TODO: Optional popups (rather than chat warnings) when online owners are in danger of having grids deleted.
        // TODO: Configurable player (& faction?) and owned grids deletion based on last log in time.
        // TODO: add player proximity to the pirate majority owner deletion rule to allow for ignore if player is close.
        // TODO: add in game commands and secure them to admins only.


        private bool initialized, triedToInitialize, unloaded, registeredMessageHandlers;

        private IUpdatableAfterSimulation[] updatables;


        public override void UpdateAfterSimulation()
        {
            try
            {
                base.UpdateAfterSimulation();

                if (unloaded || !Utilities.IsGameRunning())
                    return;

                if (!initialized && !triedToInitialize)
                {
                    triedToInitialize = true;
                    Initialize();
                    initialized = true;
                }

                if (!initialized && triedToInitialize)
                    return;

                if (updatables == null) return;

                foreach (var t in updatables)
                {
                    t.UpdateAfterSimulation();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in MainLogic.UpdateAfterSimulation: {0}", ex);
            }
        }

        private Configuration GetConfiguration()
        {
            var config = new Configuration();

            try
            {
                var fileName = string.Format("Config_{0}.xml",
                    Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));

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
                Logger.WriteLine("Exception in MainLogic.GetConfiguration(), using the default settings:\n{0}", ex);
            }

            return config;
        }

        private List<string> GetVipNames()
        {
            var vipNames = new List<string>();

            try
            {
                var fileName = string.Format("VIP_Names_{0}.txt",
                    Path.GetFileNameWithoutExtension(MyAPIGateway.Session.CurrentPath));

                if (!MyAPIGateway.Utilities.FileExistsInLocalStorage(fileName, GetType()))
                {
                    using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(fileName, GetType())
                    ) // Create an empty file
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

        private void Initialize()
        {
            if (!MyAPIGateway.Multiplayer.MultiplayerActive || MyAPIGateway.Multiplayer.IsServer)
            {
                Logger.Initialize();
                var config = GetConfiguration();
                var vipNames = GetVipNames();

                var updatables = new List<IUpdatableAfterSimulation>();


                if (config.BlockToggle_Enabled)
                    updatables.Add(new BlockToggle(
                        config.BlockToggle_Interval,
                        config.BlockToggle_BlockNames,
                        config.BlockToggle_PlayerDistanceThresholdForWarning,
                        config.BlockToggle_PlayerDistanceThresholdForToggle,
                        config.BlockToggle_MessageAdminsOnly,
                        vipNames));


                if (config.DeletePirates_Enabled)
                    updatables.Add(new DeletePirates(
                        config.DeletePirates_Interval,
                        config.DeletePirates_NPC_IdentityId,
                        config.DeletePirates_PlayerDistanceThresholdForWarning,
                        config.DeletePirates_PlayerDistanceThresholdForDeletion,
                        config.DeletePirates_MessageAdminsOnly));


                if (config.StopAllShips_Enabled)
                    updatables.Add(new StopallShips(config.StopAllShips_Interval));

                if (config.CleanPlanets_Enabled)
                    updatables.Add(new CleanPlanets(config.CleanPlanets_Interval));


                if (config.FloatingObjectDeletion_Enabled)
                    updatables.Add(new FloatingObjectDeleter(
                        config.FloatingObjectDeletion_Interval,
                        config.FloatingObjectDeletion_PlayerDistanceThreshold,
                        config.FloatingObjectDeletion_MessageAdminsOnly));

                if (config.UnownedGridDeletion_Enabled)
                    updatables.Add(new UnownedGridDeleter(
                        config.UnownedGridDeletion_Interval,
                        config.UnownedGridDeletion_PlayerDistanceThreshold,
                        config.UnownedGridDeletion_BlockCountThreshold,
                        config.UnownedGridDeletion_MessageAdminsOnly));

                if (config.DamagedGridDeletion_Enabled)
                    updatables.Add(new DamagedGridDeleter(
                        config.DamagedGridDeletion_Interval,
                        config.DamagedGridDeletion_PlayerDistanceThreshold,
                        config.DamagedGridDeletion_BlockCountThreshold,
                        config.DamagedGridDeletion_MessageAdminsOnly));

                if (config.RespawnShipDeletion_Enabled)
                    updatables.Add(new RespawnShipDeleter(
                        config.RespawnShipDeletion_Interval,
                        config.RespawnShipDeletion_GridNames,
                        config.RespawnShipDeletion_PlayerDistanceThresholdForWarning,
                        config.RespawnShipDeletion_PlayerDistanceThresholdForDeletion,
                        config.RespawnShipDeletion_MessageAdminsOnly));

                if (IsActive(config.UnrenamedGridDeletion_Mode))
                    updatables.Add(new UnnamedGridDeleter(
                        config.UnrenamedGridDeletion_Mode,
                        config.UnrenamedGridDeletion_Interval_sec,
                        config.UnrenamedGridDeletion_DeletionMinDistance_m,
                        vipNames));

                if (IsActive(config.UnmarkedGridDeletion_Mode))
                    updatables.Add(new UnmarkedGridDeleter(
                        config.UnmarkedGridDeletion_Mode,
                        config.UnmarkedGridDeletion_Interval_sec,
                        config.UnmarkedGridDeletion_DeletionMinDistance_m,
                        config.MarkerBlocks,
                        vipNames));

                if (config.MessagesFromFile_Enabled)
                    updatables.Add(new MessageFromFileShower(config.MessagesFromFile_Interval));

                if (config.PopupsFromFile_Enabled)
                    updatables.Add(new PopupFromFileShower(config.PopupsFromFile_Interval));

                this.updatables = updatables.ToArray();
            }

            if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
            {
                MyAPIGateway.Multiplayer.RegisterMessageHandler(MessageIds.MessageFromServer, ShowMessageFromServer);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(MessageIds.PopupFromServer, ShowPopupFromServer);

                registeredMessageHandlers = true;
            }
        }

        private bool IsActive(Mode mode)
        {
            return mode > Mode.Off;
        }


        protected override void UnloadData()
        {
            if (!unloaded)
            {
                if (updatables != null)
                    foreach (var updatable in updatables)
                        updatable.Close();

                if (registeredMessageHandlers)
                {
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(MessageIds.MessageFromServer,
                        ShowMessageFromServer);
                    MyAPIGateway.Multiplayer.UnregisterMessageHandler(MessageIds.PopupFromServer, ShowPopupFromServer);

                    registeredMessageHandlers = false;
                }

                Logger.Close();

                unloaded = true;
            }

            base.UnloadData();
        }

        private void ShowMessageFromServer(byte[] encodedMessage)
        {
            try
            {
                if (encodedMessage == null)
                {
                    Logger.WriteLine("ShowMessageFromServer: null byte array");
                    return;
                }

                Utilities.ShowMessageFromServerOnClient(Encoding.Unicode.GetString(encodedMessage));
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in ShowMessageFromServer: {0}", ex);
            }
        }

        private void ShowPopupFromServer(byte[] encodedMessage)
        {
            try
            {
                if (encodedMessage == null)
                {
                    Logger.WriteLine("ShowPopupFromServer: null byte array");
                    return;
                }

                var message = Encoding.Unicode.GetString(encodedMessage);
                var splitMessage = message.Split('\0');

                if (splitMessage.Length != 3)
                    Logger.WriteLine("ShowPopupFromServer: invalid popup message");
                else
                    Utilities.ShowPopupFromServerOnClient(splitMessage[0], splitMessage[1], splitMessage[2]);
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in ShowPopupFromServer: {0}", ex);
            }
        }
    }
}