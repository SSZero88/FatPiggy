using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using FatPiggy;

namespace FatPiggy
{
	public static class UICommands
	{
		public static void PiggyCommand(CommandArgs args)
		{
			if (!args.Player.IsLoggedIn)
			{
				args.Player.SendErrorMessage("You must be logged in to do that.");
				return;
			}

			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax:");
				args.Player.SendErrorMessage("{0}piggy new - creates new piggy slot and switches to it", TShock.Config.CommandSpecifier);
				args.Player.SendErrorMessage("{0}piggy load <id> - loads a piggy slot", TShock.Config.CommandSpecifier);
				return;
			}

			switch (args.Parameters[0].ToLower())
			{
				case "new":
					#region Create Piggy
					{
						if (args.Parameters.Count < 1)
						{
							args.Player.SendErrorMessage("Invalid syntax: {0}piggy new", TShock.Config.CommandSpecifier);
							return;
						}
						else
						{
							if (!FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID))
								FatPiggy.Instance.Players.Add(args.Player.User.ID, new UIPlayer(args.Player.User.ID, new Dictionary<int, NetItem[]>()));

							args.Parameters.RemoveRange(0, 1); // Remove "save"
							string inventoryName = string.Join(" ", args.Parameters.Select(x => x));
							if (FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID) && FatPiggy.Instance.Players[args.Player.User.ID].PiggyCount() >= FatPiggy.Instance.Configuration.MaxPiggies && !args.Player.HasPermission(FatPiggy.Instance.Configuration.BypassMaxPermission))
							{
								args.Player.SendErrorMessage("You have reached the max number of piggies.");
								return;
							}
							else
							{
                                FatPiggy.Instance.Players[args.Player.User.ID].NewPiggy();
							}
						}
					}
					#endregion
					break;
				case "load":
					#region Load Piggy
					{
						if (args.Parameters.Count != 2)
						{
							args.Player.SendErrorMessage("Invalid syntax: {0}piggy load <id>", TShock.Config.CommandSpecifier);
							return;
						}
						else if (!FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID))
						{
							args.Player.SendErrorMessage("You don't have any piggies saved!");
							return;
						}
						else
						{
							args.Parameters.RemoveRange(0, 1);
							string inventoryIndexStr = string.Join(" ", args.Parameters.Select(x => x));
						    int index = Convert.ToInt32(inventoryIndexStr);
							if (!FatPiggy.Instance.Players[args.Player.User.ID].HasPiggy(index))
							{
								args.Player.SendErrorMessage("No piggies with the id '"+inventoryIndexStr+"' were found.");
								return;
							}
							else
							{
								FatPiggy.Instance.Players[args.Player.User.ID].LoadPiggy(index);
								args.Player.SendSuccessMessage("Loaded piggy '"+index+"'!");
							}
						}
					}
					#endregion
					break;
                case "next":
                    #region Load Next Piggy
                    {
                        if (!FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID))
                        {
                            args.Player.SendErrorMessage("You don't have any piggies saved!");
                            return;
                        }
                        FatPiggy.Instance.Players[args.Player.User.ID].LoadNextPiggy();
                        break;
                    }
                #endregion
                case "prev":
                    #region Load Next Piggy
                    {
                        if (!FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID))
                        {
                            args.Player.SendErrorMessage("You don't have any piggies saved!");
                            return;
                        }
                        FatPiggy.Instance.Players[args.Player.User.ID].LoadPrevPiggy();
                        break;
                    }
                #endregion
                case "curr":
                    #region Load Next Piggy
                    {
                        if (!FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID))
                        {
                            args.Player.SendErrorMessage("You don't have any piggies saved!");
                            return;
                        }
                        var current = FatPiggy.Instance.Players[args.Player.User.ID].GetCurrentPiggyId();
                        args.Player.SendInfoMessage("You're currently using piggy " + current + ".");
                        break;
                    }
                #endregion
                case "list":
					#region List Inventories
					{
						if (!FatPiggy.Instance.Players.ContainsKey(args.Player.User.ID))
						{
							args.Player.SendErrorMessage("You don't have any piggies saved!");
							return;
						}

						if (args.Parameters.Count > 2)
						{
							args.Player.SendErrorMessage("Invalid syntax: {0}piggy list [page]", TShock.Config.CommandSpecifier);
							return;
						}
						else
						{
							int pageNum;
							if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNum))
								return;

							PaginationTools.SendPage(args.Player, pageNum, PaginationTools.BuildLinesFromTerms(FatPiggy.Instance.Players[args.Player.User.ID].GetPiggies()),
								new PaginationTools.Settings
								{
									HeaderFormat = "Piggies ({0}/{1})",
									FooterFormat = "Type {0}piggy list {{0}} for more.".SFormat(TShock.Config.CommandSpecifier),
									NothingToDisplayString = "You have no piggies to display."
                                });
						}
					}
					#endregion
					break;

            }
        }
	}
}