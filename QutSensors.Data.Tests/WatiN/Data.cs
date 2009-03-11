using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WatiN.Core;

namespace QutSensors.Data.Tests.WatiN
{
	[TestClass]
	public class DataTests : WatiNTest
	{
		[TestMethod]
		public void GetDefaultPage()
		{
			using (var ie = new IE())
			{
				ie.GoTo(ServerPath + "/Data.aspx");
			}
		}
	}
}