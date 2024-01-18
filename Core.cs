using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;

namespace CardGameCore;

abstract class Core(int port)
{
	public TcpListener listener = new(IPAddress.Any, port);
	public abstract void HandleNetworking();
	public abstract void Init(PipeStream? pipeStream);
}
