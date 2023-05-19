using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

namespace CardGameCore;

abstract class Core
{
	public TcpListener listener;
	public abstract void HandleNetworking();
	public abstract void Init(PipeStream? pipeStream);

	public Core()
	{
		listener = new TcpListener(IPAddress.Any, Program.config.port);
	}
}