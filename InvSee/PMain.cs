using System;
using InvSee.Extensions;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace InvSee
{
	[ApiVersion(1, 23)]
	public class PMain : TerrariaPlugin
	{
		public override string Author
		{
			get { return "SSZero88"; }
		}

		public override string Description
		{
			get { return "Allows you to create additional piggy slots for expanding your piggy bank!"; }
		}

		public override string Name
		{
			get { return "FatPiggy"; }
		}

		public override Version Version
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public static string Tag = TShock.Utils.ColorTag("FatPiggy:", Color.Teal);

		public PMain(Main game)
			: base(game)
		{
			// A lower order ensures commands are replaced properly
			Order--;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
				PlayerHooks.PlayerLogout -= OnLogout;
			}
		}

		public override void Initialize()
		{
			ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
			PlayerHooks.PlayerLogout += OnLogout;
		}

		void OnInitialize(EventArgs e)
		{
			Action<Command> Add = (command) =>
				{
					TShockAPI.Commands.ChatCommands.RemoveAll(c =>
						{
							foreach (string s in c.Names)
							{
								if (command.Names.Contains(s))
									return true;
							}
							return false;
						});
					TShockAPI.Commands.ChatCommands.Add(command);
				};

			Add(new Command(Permissions.FatPiggy, Commands.DoCreatePiggy, "piggy")
				{
					HelpDesc = new[]
					{
						"Allows you to create or switch piggy slots.",
						$"Use '{TShockAPI.Commands.Specifier}piggy create' to create piggy slots and '{TShockAPI.Commands.Specifier}piggy <index>' to switch piggy slots."
					}
				});
		}

		void OnLeave(LeaveEventArgs e)
		{
			if (e.Who < 0 || e.Who > Main.maxNetPlayers)
				return;

			TSPlayer player = TShock.Players[e.Who];
			if (player != null)
			{
				PlayerInfo info = player.GetPlayerInfo();
				info.Restore(player);
			}
		}

		void OnLogout(PlayerLogoutEventArgs e)
		{
			if (e.Player == null || !e.Player.Active || !e.Player.RealPlayer)
				return;

			e.Player.GetPlayerInfo().Restore(e.Player);
		}
	}
}
