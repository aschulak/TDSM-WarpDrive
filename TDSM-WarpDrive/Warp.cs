using System;
using Terraria_Server.Misc;

namespace Envoy.TDSM_WarpDrive
{
    public static class WarpType
    {
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

        public override bool Equals(Object other)
        {
            Warp otherWarp = (Warp)other;
            return (name.Equals(otherWarp.name) && owner.Equals(otherWarp.owner) && type.Equals(otherWarp.type));
        }
     
        public override int GetHashCode()
        {
            return name.GetHashCode() ^ owner.GetHashCode() ^ type.GetHashCode();
        }

    }

}