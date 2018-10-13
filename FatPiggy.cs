using System;
using System.Collections.Generic;
using System.IO;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;
using TShockAPI.Hooks;

namespace FatPiggy
{
    [ApiVersion(2, 1)]
    public class FatPiggy : TerrariaPlugin
    {
        public static FatPiggy Instance;

        public override string Name { get { return "FatPiggy"; } }
        public override string Author { get { return "SSZero88"; } }
        public override string Description { get { return "Enables multiple piggy bank slots for greedy players!."; } }
        public override Version Version { get { return new Version(0, 2, 0, 0); } }

        public Config Configuration = new Config();
        public Database Database = new Database();
        public Dictionary<int, UIPlayer> Players = new Dictionary<int, UIPlayer>();

        public FatPiggy(Main game) : base(game)
        {
            Instance = this;
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
        }

        private void OnInitialize(EventArgs args)
        {
            LoadConfig();
            Database.DBConnect();
            Database.LoadDatabase();

            Commands.ChatCommands.Add(new Command("fatpiggy.core", UICommands.PiggyCommand, "piggy") { AllowServer = false, HelpText = "Creates or loads piggy slots." });
        }

        private void OnReload(ReloadEventArgs args)
        {
            LoadConfig();
            Database.LoadDatabase();
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(TShock.SavePath, "FatPiggy.json");
            (Configuration = Config.Read(configPath)).Write(configPath);
        }
    }
}
