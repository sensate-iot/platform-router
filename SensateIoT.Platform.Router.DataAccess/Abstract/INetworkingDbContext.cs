using System;
using System.Data.Common;

namespace SensateIoT.Platform.Router.DataAccess.Abstract
{
	public interface INetworkingDbContext : IDisposable
	{
		DbConnection Connection { get; }
	}
}
