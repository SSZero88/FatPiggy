using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using TShockAPI;
using TShockAPI.DB;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using FatPiggy;

namespace FatPiggy
{
	public class Database
	{
		public static IDbConnection db;

		public void DBConnect()
		{
			switch (TShock.Config.StorageType.ToLower())
			{
				case "mysql":
					string[] dbHost = TShock.Config.MySqlHost.Split(':');
					db = new MySqlConnection()
					{
						ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
							dbHost[0],
							dbHost.Length == 1 ? "3306" : dbHost[1],
							TShock.Config.MySqlDbName,
							TShock.Config.MySqlUsername,
							TShock.Config.MySqlPassword)

					};
					break;

				case "sqlite":
					string sql = Path.Combine(TShock.SavePath, "tshock.sqlite");
					db = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
					break;

			}

			SqlTableCreator sqlcreator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

			sqlcreator.EnsureTableStructure(new SqlTable("FatPiggy",
				new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("CharacterName", MySqlDbType.Text),
                new SqlColumn("PiggyID", MySqlDbType.Int32),
				new SqlColumn("PiggyContents", MySqlDbType.Text)));

            sqlcreator.EnsureTableStructure(new SqlTable("FatPiggy_State",
                new SqlColumn("UserID", MySqlDbType.Int32),
                new SqlColumn("CharacterName", MySqlDbType.Text),
                new SqlColumn("CurrentPiggyID", MySqlDbType.Int32)));
        }

		public void LoadDatabase()
		{
			FatPiggy.Instance.Players.Clear();

            // load piggies
			using (QueryResult reader = db.QueryReader("SELECT * FROM FatPiggy"))
			{
				while (reader.Read())
				{
					int UserID = reader.Get<int>("UserID");
                    string CharName = reader.Get<string>("CharacterName");
                    int PiggyId = reader.Get<int>("PiggyID");
					NetItem[] PiggyContents = reader.Get<string>("PiggyContents").Split('~').Select(NetItem.Parse).ToArray();


                    if (!FatPiggy.Instance.Players.ContainsKey(UserID))
                    {
                        FatPiggy.Instance.Players.Add(UserID, new UIPlayer(UserID));
                    }

                    if (FatPiggy.Instance.Players[UserID].Characters.ContainsKey(CharName))
                    {
                        FatPiggy.Instance.Players[UserID].Characters[CharName].Piggies.Add(PiggyId, PiggyContents);

                    }
                    else
                    {
                        FatPiggy.Instance.Players[UserID].Characters.Add(CharName, new UICharacter(UserID, CharName, new Dictionary<int, NetItem[]>() { { PiggyId, PiggyContents } }));
                    }

                }
			}

            // load current piggy
            using (QueryResult reader = db.QueryReader("SELECT * from FatPiggy_State"))
            {
                while (reader.Read())
                {
                    int UserID = reader.Get<int>("UserID");
                    string CharName = reader.Get<string>("CharacterName");
                    int CurrentPiggyId = reader.Get<int>("CurrentPiggyID");

                    if (FatPiggy.Instance.Players.ContainsKey(UserID) && FatPiggy.Instance.Players[UserID].Characters.ContainsKey(CharName))
                    {
                        FatPiggy.Instance.Players[UserID].Characters[CharName].PiggyCurrent = CurrentPiggyId;
                    }
                }
            }
		}

        public void SavePiggy(int UserID, string CharName, int index, NetItem[] Piggy, bool updateExisting = false)
		{

            // if this is the 1st new piggy slot, create the piggy state
            if(FatPiggy.Instance.Players[UserID].Characters[CharName].PiggyCurrent == 1)
            {
                db.Query("INSERT INTO FatPiggy_State (UserID, CharacterName, CurrentPiggyID) VALUES (@0, @1, @2);", UserID.ToString(), CharName, index);
            }else
            {
                db.Query("UPDATE FatPiggy_State SET CurrentPiggyId=@2 WHERE UserID=@0 AND CharacterName=@1;", UserID.ToString(), CharName, index);
            }

            if (!updateExisting)
			{
				FatPiggy.Instance.Players[UserID].Characters[CharName].Piggies.Add(index, Piggy);
				db.Query("INSERT INTO FatPiggy (UserID, CharacterName, PiggyID, PiggyContents) VALUES (@0, @1, @2, @3);", UserID.ToString(), CharName, index, string.Join("~", Piggy));
			}
			else
			{
				FatPiggy.Instance.Players[UserID].Characters[CharName].Piggies[index] = Piggy;
				db.Query("UPDATE FatPiggy SET PiggyContents=@0 WHERE UserID=@1 AND CharacterName=@2 AND PiggyID=@3;", string.Join("~", Piggy), UserID.ToString(), CharName, index);
			}
		}

        public int GetCurrentPiggyId(int UserId, string CharName)
        {
            using (QueryResult reader = db.QueryReader("SELECT * FROM FatPiggy_State where UserId=@0 and CharacterName=@1;", UserId.ToString(), CharName))
            {
                int CurrentPiggyId = FatPiggy.Instance.Players[UserId].Characters[CharName].PiggyCurrent;

                while (reader.Read())
                {
                    CurrentPiggyId = reader.Get<int>("CurrentPiggyID");
                }
                return CurrentPiggyId;
            }
        }

        public int GetLastPiggyId(int UserId, string CharName)
        {
            using (QueryResult reader = db.QueryReader("SELECT max(PiggyID) as PiggyID FROM FatPiggy where UserId=@0 AND CharacterName=@1;", UserId.ToString(), CharName))
            {
                int lastPiggyId = 0;

                while (reader.Read())
                {
                    lastPiggyId = reader.Get<int>("PiggyID");
                }
                return lastPiggyId;
            }
        }
    }
}