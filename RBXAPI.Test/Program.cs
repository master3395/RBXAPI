using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RBXAPI;

namespace RBXAPI.Test
{
	class Program
	{
		static void Main(string[] args)
		{
			User login = new User("Shedletsky", "hunter2");
			try
			{
				login.Login();
			}
			catch (InvalidOperationException e)
			{
				Console.WriteLine("ERROR: {0}", e.Message);
			}
			Console.Write("Logged in: ");
			Console.WriteLine(login.IsLoggedIn);
			Group TheGroup = new Group(238595);
			//Console.Write("Setting rank: ");
			//Console.WriteLine(TheGroup.SetRole(login, new User("b6e"), 3909726));
			Console.Write("Posting to group wall: ");
			Console.WriteLine(login.PostToGroupWall(TheGroup, "Hey testing here."));
			Console.Read();
		}
	}
}
