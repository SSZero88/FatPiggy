using System;
using System.Collections.Generic;
namespace FatPiggy
{
    public class UIPlayer
    {
        public int UserID;
        public Dictionary<string, UICharacter> Characters = new Dictionary<string, UICharacter>();

        public UIPlayer(int userId)
        {
            UserID = userId;
            Characters = new Dictionary<string, UICharacter>();
        }

        public UIPlayer(int userId, Dictionary<string, UICharacter> characters)
        {
            UserID = userId;
            Characters = characters;
        }

    }
}
