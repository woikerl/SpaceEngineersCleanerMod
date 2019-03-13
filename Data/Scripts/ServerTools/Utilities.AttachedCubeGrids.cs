using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ServerTools
{
    public partial class Utilities
    {
        public static bool IsAttachedWheelGrid(this List<IMySlimBlock> slimBlocks)
        {
            var gridHasStators = slimBlocks.Any(slimBlock =>
                slimBlock.FatBlock != null && slimBlock.FatBlock is IMyMotorStator);

            if (gridHasStators)
                return false;

            return slimBlocks.Any(slimBlock =>
            {
                if (slimBlock.FatBlock == null) return false;
                var wheel = slimBlock.FatBlock as IMyWheel;
                if (wheel == null || !wheel.IsAttached) return false;

                // rename wheels, so they won't have to be checked and are easier to notice
                wheel.CubeGrid.DisplayName = "Wheel";
                return true;
            });
        }

        public static List<IMyCubeGrid> GetAttachedCubeGrids(IMyCubeGrid cubeGrid)
        {
            var attachedCubeGrids = new List<IMyCubeGrid>();

            var cubeGridsToVisit = new Queue<IMyCubeGrid>();
            cubeGridsToVisit.Enqueue(cubeGrid);

            var slimBlocks = new List<IMySlimBlock>();

            while (cubeGridsToVisit.Count > 0)
            {
                var currentCubeGrid = cubeGridsToVisit.Dequeue();

                slimBlocks.Clear();
                currentCubeGrid.GetBlocks(slimBlocks, slimBlock => slimBlock?.FatBlock != null);

                foreach (var slimBlock in slimBlocks)
                {
                    var fatBlock = slimBlock.FatBlock;
                    // keen is moving all physics blocks to the below two... wheels are still broke
                    {
                        var motorBase = fatBlock as IMyMechanicalConnectionBlock;
                        if (motorBase != null && TryAddDistinctCubeGrid(motorBase.TopGrid, attachedCubeGrids))
                            cubeGridsToVisit.Enqueue(motorBase.TopGrid);
                    }

                    {
                        var motorRotor = fatBlock as IMyAttachableTopBlock;

                        if (motorRotor != null && motorRotor.Base != null &&
                            TryAddDistinctCubeGrid(motorRotor.Base.CubeGrid, attachedCubeGrids))
                            cubeGridsToVisit.Enqueue(motorRotor.Base.CubeGrid);
                    }
                }
            }

            return attachedCubeGrids;
        }

        private static bool TryAddDistinctCubeGrid(IMyCubeGrid cubeGrid, List<IMyCubeGrid> cubeGrids)
        {
            if (cubeGrid == null)
                return false;

            if (cubeGrids.Contains(cubeGrid))
                return false;

            cubeGrids.Add(cubeGrid);
            return true;
        }

        public static void GetBlocksIncludingFromStaticallyAttachedCubeGrids(this IMyCubeGrid cubeGrid,
            List<IMySlimBlock> slimBlocks, Func<IMySlimBlock, bool> collect = null)
        {
            cubeGrid.GetBlocks(slimBlocks, collect);

            var attachedCubeGrids = GetAttachedCubeGrids(cubeGrid);

            // Just in case Keen makes GetBlocks() clear the list in the future,
            // let's get blocks to a temporary list first
            var attachedCubeGridSlimBlocks = new List<IMySlimBlock>();

            foreach (var attachedCubeGrid in attachedCubeGrids)
            {
                attachedCubeGrid.GetBlocks(attachedCubeGridSlimBlocks, collect);
                slimBlocks.AddList(attachedCubeGridSlimBlocks);
            }
        }
    }
}