/*
 * Audit log repository.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Threading;
using System.Threading.Tasks;

using SensateIoT.Platform.Network.Data.Models;

namespace SensateIoT.Platform.Network.DataAccess.Abstract
{
	public interface IAuditLogRepository
	{
		Task CreateAsync(AuditLog log, CancellationToken ct = default);
	}
}
