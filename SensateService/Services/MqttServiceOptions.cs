/*
 * MQTT background service
 *
 * @author: Michel Megens
 * @email:  dev@bietje.net
 */

using System;
using System.Collections.Generic;

namespace SensateService.Services
{
	public class MqttServiceOptions
	{
		public string Host {get;set;}
		public int Port {get;set;}
		public bool Ssl {get;set;}
		public string Id {get;set;}
		public string Username {get;set;}
		public string Password {get;set;}
		public string TopicShare {get;set;}
	}
}
