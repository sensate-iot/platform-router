/*
 * Generic graph model.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SensateService.Common.Data.Dto.Json.Out
{
	public class Graph<X, Y> where X : IComparable<X>
	{
		public List<Node<X, Y>> Data { get; }

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

			this.Data.Add(tuple);
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this.Data);
		}

		public class Node<A, B> : IComparable<Node<A, B>> where A : IComparable<A>
		{
			public A Xcoord { get; set; }
			public B Ycoord { get; set; }

			public int CompareTo(Node<A, B> other)
			{
				return this.Xcoord.CompareTo(other.Xcoord);
			}
		}
	}
}