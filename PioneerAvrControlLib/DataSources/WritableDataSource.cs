namespace PioneerAvrControlLib.DataSources {
	public abstract class WritableDataSource : DataSource {
		public abstract void Write(byte[] data);
	}
}