using System;
using System.Data.Common;

namespace SensateIoT.Platform.Router.DataAccess.Abstract
{
	public interface IAuthorizationDbContext : IDisposable
	{
		DbConnection Connection { get; }
	}
}
