using System;
using System.Collections.Generic;
using System.Linq;

using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace ServerCleaner
{
	public partial class Utilities
	{
		// Workaround for bug in 01_149_005: IMyWheel.Stator is always null, even when .IsAttached = true
		public static bool IsAttachedWheelGrid(this List<IMySlimBlock> slimBlocks)
		{
            //bool gridHasStators = slimBlocks.Any(slimBlock => slimBlock.FatBlock != null && slimBlock.FatBlock is IMyMotorStator);

            //if (gridHasStators)
            //	return false;

            //if (slimBlocks.Count > 1)
            //    return false;

			return slimBlocks.Any(slimBlock =>
			{
				if (slimBlock.FatBlock == null)
					return false;

				var TopPart = slimBlock.FatBlock as IMyAttachableTopBlock;

                //if (wheel != null)
                 //   return true;

                return TopPart != null && TopPart.IsAttached;

                //return false;
			});
		}

        public static bool withStators(this List<IMySlimBlock> slimBlocks)
        {
            var gridHasStators = slimBlocks.Any(slimBlock => slimBlock.FatBlock != null && slimBlock.FatBlock is IMyMotorStator);


            if (gridHasStators)
                return false;

            return true;
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
				currentCubeGrid.GetBlocks(slimBlocks, slimBlock => slimBlock != null && slimBlock.FatBlock != null);

				foreach (var slimBlock in slimBlocks)
				{
					var fatBlock = slimBlock.FatBlock;
                    // keen deprecated IMyMotorBase and IMyMotorRotor
					{
                        var MechBase = fatBlock as IMyMechanicalConnectionBlock;
                        //var motorBase = fatBlock as IMyMotorBase;
						if (MechBase != null && TryAddDistinctCubeGrid(MechBase.TopGrid, attachedCubeGrids))
						//if (motorBase != null && TryAddDistinctCubeGrid(motorBase.RotorGrid, attachedCubeGrids))
							cubeGridsToVisit.Enqueue(MechBase.TopGrid);
							//cubeGridsToVisit.Enqueue(motorBase.RotorGrid);
					}

					{
                        var MechTopPart = fatBlock as IMyAttachableTopBlock;
                        //var motorRotor = fatBlock as IMyMotorRotor;

						if (MechTopPart != null && MechTopPart.Base != null && TryAddDistinctCubeGrid(MechTopPart.Base.CubeGrid, attachedCubeGrids))
						//if (motorRotor != null && motorRotor.Stator != null && TryAddDistinctCubeGrid(motorRotor.Stator.CubeGrid, attachedCubeGrids))
							cubeGridsToVisit.Enqueue(MechTopPart.Base.CubeGrid);
                            //cubeGridsToVisit.Enqueue(motorRotor.Stator.CubeGrid);
					}




                    /*
					{
						var pistonBase = fatBlock as IMyPistonBase;
						if (pistonBase != null && TryAddDistinctCubeGrid(pistonBase.TopGrid, attachedCubeGrids))
							cubeGridsToVisit.Enqueue(pistonBase.TopGrid);
					}

					{
						var pistonTop = fatBlock as IMyPistonTop;
						if (pistonTop != null && pistonTop.Piston != null && TryAddDistinctCubeGrid(pistonTop.Piston.CubeGrid, attachedCubeGrids))
							cubeGridsToVisit.Enqueue(pistonTop.Piston.CubeGrid);
					}
                    */

                    //{
                    //    var gridHasStators = fatBlock as IMyMotorStator
                    
                    //    slimBlocks.Any(slimBlock => slimBlock.FatBlock != null && slimBlock.FatBlock is IMyMotorStator);
                    //    if (gridHasStators)

                    //}                    

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

		public static void GetBlocksIncludingFromStaticallyAttachedCubeGrids(this IMyCubeGrid cubeGrid, List<IMySlimBlock> slimBlocks, Func<IMySlimBlock, bool> collect = null)
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
