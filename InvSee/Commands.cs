using System;
using System.Linq;
using System.Text.RegularExpressions;
using InvSee.Extensions;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace InvSee
{
	internal class Commands
	{
		public static void DoCreatePiggy(CommandArgs args)
		{
			if (!Main.ServerSideCharacter)
			{
				args.Player.PluginErrorMessage("ServerSideCharacters must be enabled.");
				return;
			}


		}
	}
}
