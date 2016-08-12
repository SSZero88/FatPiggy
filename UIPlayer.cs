using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;
using NetItem = TShockAPI.NetItem;
using FatPiggy;

namespace FatPiggy
{
	public class UIPlayer
	{
		public int UserID;
		public Dictionary<int, NetItem[]> Piggies = new Dictionary<int, NetItem[]>();
        public string CharacterName;

	    public int PiggyCurrent = 1;
		public UIPlayer()
		{
			UserID = new int();
			Piggies = new Dictionary<int, NetItem[]>();
            CharacterName = "";
		}

		public UIPlayer(int userID, Dictionary<int, NetItem[]> piggies)
		{
			UserID = userID;
			Piggies = piggies;
        }

		public int PiggyCount()
		{
			return this.Piggies.Keys.Count();
		}

		public bool HasPiggy(int id)
		{
			return this.Piggies.Keys.Contains(id);
		}

		public List<int> GetPiggies()
		{
			return this.Piggies.Keys.ToList();
		}

		public void NewPiggy()
		{
            bool SSC = Main.ServerSideCharacter;

			TSPlayer player = FindTSPlayerByUserID(UserID);

            int index = PiggyCurrent;

            int piggyOffset = (58 + player.TPlayer.armor.Length + player.TPlayer.dye.Length + player.TPlayer.miscEquips.Length + player.TPlayer.miscDyes.Length) + 1;

            if (player != null)
			{
                if (!SSC)
                {
                    player.SendErrorMessage("SSC not enabled");
                    Main.ServerSideCharacter = true;
                    NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");
                }

                NetItem[] PiggyCurr = ItemToNetItem(player.TPlayer.bank.item);
                
                FatPiggy.Instance.Database.SavePiggy(UserID, index, PiggyCurr, HasPiggy(index));
                player.SendSuccessMessage("Piggy " + index + " saved.");
                // clear piggy
                for (int i = 0; i < NetItem.PiggySlots; i++)
                {
                    player.TPlayer.bank.item[i].netDefaults(0);

                    player.TPlayer.bank.item[i].prefix = 0;
                    player.TPlayer.bank.item[i].stack = 0;

                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.bank.item[i].name, player.Index, piggyOffset + i, player.TPlayer.bank.item[i].prefix);
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.bank.item[i].name, player.Index, piggyOffset + i, player.TPlayer.bank.item[i].prefix);
                }

                if (!SSC)
                {
                    Main.ServerSideCharacter = false;
                    NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");
                }

                NetItem[] PiggyNew = ItemToNetItem(player.TPlayer.bank.item);

                PiggyCurrent = FatPiggy.Instance.Database.GetLastPiggyId(UserID) + 1;

                FatPiggy.Instance.Database.SavePiggy(UserID, PiggyCurrent, PiggyNew, HasPiggy(PiggyCurrent));
                player.SendSuccessMessage("Piggy " + PiggyCurrent + " created.");

            }
		}


        public void LoadNextPiggy()
        {
            int maxPiggy = FatPiggy.Instance.Database.GetLastPiggyId(UserID);
            if (PiggyCurrent < maxPiggy)
            {
                LoadPiggy(PiggyCurrent + 1);
            }
            else
            {
                LoadPiggy(1);
            }
        }

        public void LoadPrevPiggy()
        {
            int maxPiggy = FatPiggy.Instance.Database.GetLastPiggyId(UserID);
            if (PiggyCurrent > 1)
            {
                LoadPiggy(PiggyCurrent - 1);
            }
            else
            {
 
                LoadPiggy(maxPiggy);
            }
        }

        public int GetCurrentPiggyId()
        {
            return PiggyCurrent;
        }

        public void LoadPiggy(int index)
		{

            bool SSC = Main.ServerSideCharacter;
            TSPlayer player = FindTSPlayerByUserID(UserID);

            player.SendErrorMessage(player.TPlayer.name);

            if (!HasPiggy(index))
            {
                player.SendErrorMessage("No piggy found for index " + index);
                return;
            }

            NetItem[] Piggy = this.Piggies[index];

            //save current piggy
            player.SendInfoMessage("Saved Piggy "+ PiggyCurrent);
            NetItem[] CurrentPiggy = ItemToNetItem(player.TPlayer.bank.item);
            FatPiggy.Instance.Database.SavePiggy(UserID, PiggyCurrent, CurrentPiggy, HasPiggy(PiggyCurrent));

            if (player != null)
			{
				if (!SSC)
				{
                    player.SendErrorMessage("SSC not enabled");
                    Main.ServerSideCharacter = true;
					NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");
				}

                int piggyOffset = (58 + player.TPlayer.armor.Length + player.TPlayer.dye.Length + player.TPlayer.miscEquips.Length + player.TPlayer.miscDyes.Length) + 1;

                player.SendInfoMessage("Loading piggy "+ index);
                for (int i = 0; i < NetItem.PiggySlots; i++)
				{
                    player.TPlayer.bank.item[i].netDefaults(Piggy[i].NetId);

					if (player.TPlayer.bank.item[i].netID != 0)
					{
						player.TPlayer.bank.item[i].prefix = Piggy[i].PrefixId;
						player.TPlayer.bank.item[i].stack = Piggy[i].Stack;
					}

                    if(player.TPlayer.bank.item[i].prefix > 0)
                    {
                        player.SendInfoMessage("Prefix of "+ player.TPlayer.bank.item[i].name + " is " + player.TPlayer.bank.item[i].prefix);
                    }
                    NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, player.TPlayer.bank.item[i].name, player.Index, piggyOffset + i, player.TPlayer.bank.item[i].prefix);
					NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, player.TPlayer.bank.item[i].name, player.Index, piggyOffset + i, player.TPlayer.bank.item[i].prefix);
				}

				if (!SSC)
				{
					Main.ServerSideCharacter = false;
					NetMessage.SendData((int)PacketTypes.WorldInfo, player.Index, -1, "");
				}
                
                // save loaded piggy (might not need to do this)
                NetItem[] loadedPiggy = ItemToNetItem(player.TPlayer.bank.item);
                FatPiggy.Instance.Database.SavePiggy(UserID, index, loadedPiggy, HasPiggy(index));

                // set current piggy id
                PiggyCurrent = index;
            }
        }

		#region Miscellaneous
		private TSPlayer FindTSPlayerByUserID(int UserID)
		{
			return TShock.Players.FirstOrDefault(p => p != null && p.User != null && p.User.ID == UserID);
		}

        private NetItem[] ItemToNetItem(Item[] bankItems)
        {
            NetItem[] Piggy = new NetItem[bankItems.Length];

            for (int i = 0; i < NetItem.PiggySlots; i++)
            {
                Piggy[i] = (NetItem)bankItems[i];

            }
            return Piggy;
        }

        #endregion
    }
}