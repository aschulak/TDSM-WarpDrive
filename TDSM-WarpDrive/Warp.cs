using System;

using Terraria_Server;

namespace WarpDrive
{
    public class Warp
    {
        public string name = "";
        public string owner = "";
        public Vector2 loc = new Vector2 ();
     
        public string ToXml ()
        {
            string warpXml = "<warp>";
            warpXml += "<name>" + name + "</name>";
            warpXml += "<owner>" + owner + "</owner>";                               
            warpXml += "<x>" + loc.X + "</x><y>" + loc.Y + "</y>";
            warpXml += "</warp>";            
            return warpXml;
        }
    }
}

