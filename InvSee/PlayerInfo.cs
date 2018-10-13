using TShockAPI;

namespace InvSee
{
	public class PlayerInfo
	{
		public const string KEY = "FatPiggy_Data";

		public PlayerData Backup { get; set; }

		public int PiggyIndex { get; set; }

		public PlayerInfo()
		{
			Backup = null;
		}

		public bool Restore(TSPlayer player)
		{
			if (Backup == null)
				return false;

			Backup.RestoreCharacter(player);
			Backup = null;
			PiggyIndex = 0;
			return true;
		}
	}
}
