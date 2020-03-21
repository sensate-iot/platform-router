/*
 * Middleware to fetch user data based on a
 * claims principle.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SensateService.Helpers;
using SensateService.Infrastructure.Repositories;

namespace SensateService.ApiCore.Middleware
{
	public class UserFetchMiddleware
	{
		private readonly RequestDelegate m_next;
		private readonly IServiceProvider m_services;
		private readonly ILogger<UserFetchMiddleware> m_logger;

		public UserFetchMiddleware(RequestDelegate next,
								   IServiceProvider sp,
								   ILogger<UserFetchMiddleware> logger)
		{
			this.m_logger = logger;
			this.m_services = sp;
			this.m_next = next;
		}

		public async Task Invoke(HttpContext ctx)
		{
			var cp = ctx.User;

			if(cp == null) {
				this.m_logger.LogInformation("ClaimsPrincipal not found!");
				ctx.Items["UserData"] = null;
				await this.m_next(ctx).AwaitBackground();

				return;
			}

			using var scope = this.m_services.CreateScope();
			var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
			var user = await users.GetByClaimsPrincipleAsync(cp).AwaitBackground();

			ctx.Items["UserData"] = user;
			await this.m_next(ctx).AwaitBackground();
		}
	}
}