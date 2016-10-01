using System;

namespace ToyMQ.MessageQueue.Adapter {
	public interface IAdapterFactory {
		bool IsProtocolSupported(string url);
		IAdapterServer CreateServer(string url);
		IAdapter CreateClient(string url);
	}

	public interface IAdapterServer {
		IAdapter WaitForNewClient();
	}

	public interface IAdapter {
		int Send(byte[] data);
		int Receive(byte[] data);
	}
}
