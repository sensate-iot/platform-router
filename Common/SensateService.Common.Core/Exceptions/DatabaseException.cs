/*
 * Exeption which is thrown when database reads / writes fail.
 *
 * @author Michel Megens
 * @email  michel.megens@sonatolabs.com
 */

using System;

namespace SensateService.Exceptions
{
	public class DatabaseException : SystemException
	{
		public string Database { get; private set; }

		public DatabaseException() : base("Database failure!")
		{
		}

		public DatabaseException(string msg) : base(msg)
		{ }

		public DatabaseException(string msg, string database) : base(message: msg)
		{
			this.Database = database;
		}

		public DatabaseException(string msg, string database, Exception inner) : base(msg, inner)
		{
			this.Database = database;
		}
	}
}
