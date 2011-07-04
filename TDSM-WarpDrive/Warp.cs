using System;
using Terraria_Server.Misc;

namespace WarpDrive
{
    public static class WarpType {
        public const string GLOBAL = "global";
        public const string PERSONAL = "personal";
    }

    public class Warp
    {
        public string name = "";
        public string owner = "";
        public string type = "";
        public Vector2 loc = new Vector2();

        public bool isGlobal()
        {
            return type == WarpType.GLOBAL;
        }

        public bool isPersonal()
        {
            return type == WarpType.PERSONAL;
        }

        public string ToXml()
        {
            string warpXml = "<warp>";
            warpXml += "<name>" + name + "</name>";
            warpXml += "<owner>" + owner + "</owner>";
            warpXml += "<type>" + type + "</type>";
            warpXml += "<x>" + loc.X + "</x><y>" + loc.Y + "</y>";
            warpXml += "</warp>";
            return warpXml;
        }
    }

}