using System;

namespace PioneerAvrControlLib.DataSources {
	public delegate void DataReceivedEventHandler(DataSource o, DataReceivedEventArgs e);

	public class DataReceivedEventArgs : EventArgs {
		public readonly byte[] NewData;

		public DataReceivedEventArgs(byte[] data) {
			NewData = data;
		}

	}
}
