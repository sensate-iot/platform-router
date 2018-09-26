/*
 * Command line interface parser.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace SensateService.DatabaseTool
{
	public class CliParser
	{
		private readonly Dictionary<string, CliCommand> _commands;

		public CliParser()
		{
			this._commands = new Dictionary<string, CliCommand> {
				{ "GetMeasurementsBySensor", Commands.GetMeasurementsBySensor },
				{ "GetMeasurementsAfterYesterday", Commands.GetMeasurementsAfterYesterday },
				{ "GetMeasurementsBeforeToday", Commands.GetMeasurementsBeforeToday }
			};
		}

		public async Task Run(IServiceProvider provider)
		{
			Console.WriteLine("");

			while(true) {
				Console.Write("db cli $ ");
				var raw = Console.ReadLine();

				if(raw == null)
					continue;

				var tokens = Regex.Matches(raw, @"[\""].+?[\""]|[^ ]+")
					.Select(m => m.Value).ToList();

				if(tokens.Count <= 0)
					continue;

				if(tokens[0] == "exit" || tokens[0] == "quit") {
					Program.Application.Reset();
					break;
				}

				if(!this._commands.TryGetValue(tokens[0], out var cmd)) {
					Console.WriteLine("> Command not found!");
					continue;
				}

				using(var scope = provider.CreateScope()) {
					tokens.RemoveAt(0);
					var args = string.Join(' ', tokens);
					await cmd(scope, args);
				}

			}
		}
	}
}