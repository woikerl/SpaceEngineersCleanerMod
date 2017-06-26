using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace ServerCleaner.Updatables
{
    /// <summary>
    /// stops all grids unless they use a period in the grid name.
    /// </summary>
    public class CleanPlanets : RepeatedAction
    {
        HashSet<MyPlanet> planetList = new HashSet<MyPlanet>();
        HashSet<IMyEntity> entityList = new HashSet<IMyEntity>();
        HashSet<IMyEntity> gridList = new HashSet<IMyEntity>();

        double MeasureDistance(Vector3D coordsStart, Vector3D coordsEnd)
        {

            double distance = Math.Round(Vector3D.Distance(coordsStart, coordsEnd), 2);
            return distance;

        }

        public CleanPlanets(double interval) : base(interval)
        {
        }

        protected override bool ShouldRun()
        {


            try
            {

                entityList.Clear();
                planetList.Clear();
                gridList.Clear();

                MyAPIGateway.Entities.GetEntities(entityList);

                if (entityList.Count != 0)
                {
                    foreach (var entity in entityList)
                    {
                        var planet = entity as MyPlanet;
                        if (planet != null)
                        {

                            //return true;
                            planetList.Add(planet);

                        }

                    }

                }

                if (planetList != null) { 
                foreach (var planet in planetList)
                {

                    var planetEntity = planet as IMyEntity;
                    double minRadius = (double)planet.MinimumRadius;

                    foreach (var entity in entityList)
                    {

                        var cubeGrid = entity as IMyCubeGrid;

                        if (cubeGrid != null)
                        {

                            if (MeasureDistance(cubeGrid.GetPosition(), planetEntity.GetPosition()) < minRadius / 1.5)
                            {

                                gridList.Add(cubeGrid);
                                //cubeGrid.Delete();

                            }

                        }

                    }

                }

                if (gridList == null) { return false; }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.WriteLine("Exception in CleanPlanets.ShouldRun: {0}", ex);
                return false;
            }


        }


        protected override void Run()
        {


            foreach (var g in gridList)
            {

                g.Delete();
            }
            MyAPIGateway.Utilities.ShowNotification("Server: Planets have been cleaned.", 20000, MyFontEnum.Green);
        }


    }
}
