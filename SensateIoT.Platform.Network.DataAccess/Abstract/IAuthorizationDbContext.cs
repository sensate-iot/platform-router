using System;
using System.Data.Common;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IAuthorizationDbContext : IDisposable
	{
		DbConnection Connection { get; }
	}
}
