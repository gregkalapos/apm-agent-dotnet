using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Foo
{
	public class Sample
	{
		public static void M1()
		{
			try
			{
				// run some code
				throw new Exception("Bamm1");
			}
			catch (Exception e)
			{
				if (e.Message == "Bamm")
				{
					Console.WriteLine("BammException");
				}
				else
				{
					throw;
				}
			}
		}















		public static void M2()
		{
			try
			{
				// runs code
				throw new Exception("Bamm1");
			}
			catch (Exception e) when (CaptureException(e)) //(e.Message == "Bamm")
			{
				Console.WriteLine("BammException");
			}
		}

		public static bool CaptureException(Exception e) => false;
	}
}
