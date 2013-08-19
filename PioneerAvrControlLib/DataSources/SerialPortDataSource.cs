using System;
using System.Globalization;
using System.IO.Ports;
using System.Threading;
using System.Xml;

namespace PioneerAvrControlLib.DataSources {
	public enum ReadMethod {
		BlockingPoll,
		EventBased
	};

	public class SerialPortDataSource : WritableDataSource {
		public SerialPort SerialPort { get; private set; }

		public SerialPortDataSource(string portname, int baudrate) {
			SerialPort = new SerialPort {
				BaudRate = baudrate,
				PortName = portname,
				ReceivedBytesThreshold = 1,
				ReadTimeout = 1050
			};
			SerialPort.DataReceived += SerialPortDataReceived;
			SerialPort.Handshake = Handshake.None;
		}

		#region Buffer Handling

		public string PortName {
			get { return SerialPort.PortName; }
			set { SerialPort.PortName = value; }
		}

		public int BaudRate {
			get { return SerialPort.BaudRate; }
			set { SerialPort.BaudRate = value; }
		}

		void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e) {
			try {
				int rs = SerialPort.BytesToRead;
				byte[] b = new byte[rs];
				rs = SerialPort.Read(b, 0, rs);
				if (rs != b.Length)
					Array.Resize(ref b, rs);
				OnDataReceived(b);
			}
			catch (ThreadAbortException) {
			}
			catch (InvalidOperationException) {
			}
		}

		public void Write(byte[] buffer, int offset, int count) {
			SerialPort.Write(buffer, offset, count);
		}
		public override void Write(byte[] buffer) {
			SerialPort.Write(buffer, 0, buffer.Length);
		}

		#endregion Buffer Handling

		public override bool IsEnabled {
			get { return SerialPort != null && SerialPort.IsOpen; }
		}

		public override string Type {
			get { return "sp://" + SerialPort.PortName + "@" + SerialPort.BaudRate; }
		}

		public override void Start() {
			if (!SerialPort.IsOpen)
				SerialPort.Open();
			OnConnectionEstablished();
			base.Start(); // start UniversalLogfileWriter
		}

		public override void Stop() {
			if (SerialPort.IsOpen)
				SerialPort.Close();
			OnConnectionLost();
			base.Stop(); // stop UniversalLogfileWriter
		}

		public override string ToString() {
			return PortName + "@" + BaudRate;
		}

		#region xml serializing/deserializing
		public static DataSource LoadFrom(XmlElement node) {
			return (new SerialPortDataSource(
				node["portname"].InnerText, int.Parse(node["baudrate"].InnerText)
			)).LoadBaseSettings(node);
		}

		public override void SaveTo(XmlTextWriter xtr) {
			xtr.WriteStartElement("datasource");
			xtr.WriteAttributeString("type", "serialport");
			xtr.WriteElementString("portname", PortName ?? "");
			xtr.WriteElementString("baudrate", BaudRate.ToString(CultureInfo.InvariantCulture));
			WriteBaseSettings(xtr);
			xtr.WriteEndElement();
		}
		#endregion

		public event DataReceivedEventHandler DataReceived;
		public void OnDataReceived(byte[] data) {
			DataReceived(this, new DataReceivedEventArgs(data));
		}
	}
}
