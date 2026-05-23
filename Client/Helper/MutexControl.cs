using System;
using System.Threading;

namespace Client.Helper
{
	
	public static class MutexControl
	{
		
		public static bool CreateMutex(string mtx)
		{
			try
			{
				if (string.IsNullOrEmpty(mtx)) return false;
				MutexControl.currentApp = new Mutex(false, mtx, out createdNew);
				return MutexControl.createdNew;
			}
			catch
			{
				createdNew = true; 
				return true;
			}
		}

		
		public static void Exit()
		{
			if (MutexControl.currentApp != null)
			{
				MutexControl.currentApp.Dispose();
			}
		}

		
		public static Mutex currentApp;

		
		public static bool createdNew;
	}
}


