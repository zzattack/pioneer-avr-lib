using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PioneerAvrControlLib;
using System.Threading;

namespace PioneerAvrControlLibTest {
	public class Program {
		public static void Main() {
			var conn = new PioneerAvrControlLib.TCPConnection("10.31.45.25");
			conn.Open();
			Thread.Sleep(Timeout.Infinite);
		}
	}
}
