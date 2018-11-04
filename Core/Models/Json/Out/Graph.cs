/*
 * Generic graph model.
 *
 * @author Michel Megens
 * @email  dev@bietje.net
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SensateService.Models.Json.Out
{
	public class Graph<X, Y>
	{
		public IList<Node<X, Y>> Data { get; }

		public Graph()
		{
			this.Data = new List<Node<X, Y>>();
		}

		public void Add(X xcoord, Y ycoord)
		{
			var tuple = new Node<X, Y> {
				Xcoord = xcoord,
				Ycoord = ycoord
			};

			this.Data.Append(tuple);
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this);
		}

		public class Node<A, B>
		{
			public A Xcoord { get; set; }
			public B Ycoord { get; set; }
		}
	}
}