﻿using System.Timers;

namespace ServerTools.Updateables
{
	public abstract class RepeatedAction : IUpdatableAfterSimulation
	{
		private bool runOnNextUpdate;
		private readonly Timer timer;

		protected RepeatedAction(double interval)
		{
			timer = new Timer();
			timer.AutoReset = true;
			timer.Interval = interval;
			timer.Elapsed += (sender, e) => runOnNextUpdate = ShouldRun();
			timer.Start();
		}

		public void UpdateAfterSimulation()
		{
			if (!runOnNextUpdate)
				return;

			runOnNextUpdate = false;
			Run();
		}

		public void Close()
		{
			timer.Close();
		}

		protected virtual bool ShouldRun()
		{
			return true;
		}

		protected abstract void Run();
	}
}
