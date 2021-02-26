using System;
using System.Data.Common;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface INetworkingDbContext : IDisposable
	{
		DbConnection Connection { get; }
	}
}
