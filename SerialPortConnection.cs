using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace PioneerAvrControlLib {
	class SerialPortConnection : PioneerConnection {
		// Specs say:
		// Communication Speed: 9600bps 
		// Character length：8bits
		// Parity: No
		// Start bit: 1bits 
		// Stop bit: 1bit

		// I have only tested the TCP implementation though

		SerialPort sp;
		public SerialPortConnection(string portName) {
			sp = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
			sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
		}

		protected override void Open() {
			sp.Open();
		}

		void sp_DataReceived(object sender, SerialDataReceivedEventArgs e) {
			if (e.EventType == SerialData.Chars) {
				int btr = sp.BytesToRead;
				byte[] b = new byte[btr];
				sp.Read(b, 0, btr);
				AddToBuffer(b);
			}
		}
		
		protected override void Write(IEnumerable<byte> b) {
			var arr = b.ToArray();
			sp.Write(arr, 0, arr.Length);
		}

	}
}
