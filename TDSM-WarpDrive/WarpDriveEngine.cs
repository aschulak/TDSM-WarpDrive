using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Terraria_Server;
using WarpDrive;
 
namespace WarpDrive
{
	public class WarpDriveEngine
	{
		private WarpDrivePlugin warpDrivePlugin;
		public bool enabled;		
		public string xmlFile;
		public XmlDocument warpFile;
		private XmlReader reader;
		private XPathNavigator navi;
		private XmlWriter writer;
		private XmlWriterSettings wSettings;
		private Dictionary<string, Warp> globalWarplist;
		private Dictionary<string, Dictionary<string, Warp>> personalWarplistByPlayer;
     
		public WarpDriveEngine(WarpDrivePlugin warpDrivePlugin, string xmlFile)
		{
			this.warpDrivePlugin = warpDrivePlugin;
			this.xmlFile = xmlFile;          
			globalWarplist = new Dictionary<string, Warp>();       
			personalWarplistByPlayer = new Dictionary<string, Dictionary<string, Warp>>();
			SetupWarps();
		}
     
		public void SetupWarps()
		{
			warpFile = new XmlDocument();
			wSettings = new XmlWriterSettings();
			wSettings.OmitXmlDeclaration = true;
			wSettings.Indent = true;
			int warpCount = 0;
         
			if (!(File.Exists(xmlFile))) {
				FileStream file = File.Create(xmlFile);
				file.Close();
				warpFile.LoadXml("<warps></warps>");
				navi = warpFile.CreateNavigator();
				WriteXML();
			} else {
				reader = XmlTextReader.Create(xmlFile);
				warpFile.Load(reader);
				navi = warpFile.CreateNavigator();
             
				foreach (XmlElement e in warpFile.SelectNodes("/warps/warp")) {
					Warp warp = new Warp();
					warp.name = e.ChildNodes[0].InnerText;
					warp.owner = e.ChildNodes[1].InnerText;
					warp.loc.X = float.Parse(e.ChildNodes[2].InnerText);
					warp.loc.Y = float.Parse(e.ChildNodes[3].InnerText);
                 
					if (warp.owner.Length == 0) {						
						this.globalWarplist.Add(warp.name, warp);
					} else {						
						Dictionary<string, Warp > personalWarplist = new Dictionary<string, Warp>();
						personalWarplistByPlayer.TryGetValue(warp.owner, out personalWarplist);
						if (personalWarplist == null) {
							personalWarplist = new Dictionary<string, Warp>();
							personalWarplistByPlayer.Add(warp.owner, personalWarplist);                         
						}
						personalWarplist.Add(warp.name, warp);
					}
					warpCount++;
                    
				}
				reader.Close();             
			}
			warpDrivePlugin.Log("Loaded " + warpCount + " warps.");
		}

		public void DelWarp(Player player, string warpName, bool isGlobal)
		{
			Dictionary<string, Warp > warplist = new Dictionary<string, Warp>();
			if (isGlobal) {
				warplist = globalWarplist;   
			} else {             
				personalWarplistByPlayer.TryGetValue(player.getName(), out warplist);
			}
         
			if (warplist != null && warplist.ContainsKey(warpName)) {
				XmlNodeList node = warpFile.SelectNodes("/warps");
				for (int i = 0; i < node.Count; i++) {
					for (int j = 0; j < node[i].ChildNodes.Count; j++) {
						if (node[i].ChildNodes[j].FirstChild.InnerText == warpName) {
							node[i].RemoveChild(node[i].ChildNodes[j]);
							break;
						}
					}
				}
				WriteXML();
				warplist.Remove(warpName);
				player.sendMessage("Warp " + warpName + " removed.", 255, 0f, 255f, 255f);
				Program.tConsole.WriteLine(player.getName() + " removed warp " + warpName);
			} else {             
				player.sendMessage("Error: Warp " + warpName + " does not exist.", 255, 0f, 255f, 255f);
			}
		}
     
		// Warps the player to the warp
		private void WarpPlayerTo(Player player, Warp warp)
		{
			player.teleportTo(warp.loc.X, warp.loc.Y);
			player.sendMessage("Warped to " + warp.name + ".", 255, 0f, 255f, 255f);
			warpDrivePlugin.Log(player.getName() + " used /warp " + warp.name);
		}
     
		public void Warp(Player player, string warpName)
		{
			warpDrivePlugin.Log("Attempting to warp to [" + warpName + "]");
             
			Warp warp = null;
         
			// always try personal warp first                                    
			Dictionary<string, Warp > personalWarplist = new Dictionary<string, Warp>();
			personalWarplistByPlayer.TryGetValue(player.getName(), out personalWarplist);              
			if (personalWarplist != null && personalWarplist.ContainsKey(warpName)) {                   
				personalWarplist.TryGetValue(warpName, out warp);                   
			}                
         
			// if we fail to find a personal warp, look in the global warps
			if (warp == null) {
				globalWarplist.TryGetValue(warpName, out warp);             
			}
         
			if (warp != null) {
				WarpPlayerTo(player, warp); 
			} else {
				player.sendMessage("Error: warp " + warpName + " does not exist.", 255, 0f, 255f, 255f);
			}
             
		}
        
		public void WriteWarp(Player player, string warpName, bool isGlobal)
		{
			if (!WarpAlreadyExists(player, warpName, isGlobal)) {
				// create Warp
				Warp warp = new Warp();
				warp.name = warpName;
				warp.loc.X = player.getLocation().X;
				warp.loc.Y = player.getLocation().Y;
                             
				if (!isGlobal) {                 
					warp.owner = player.getName();
				}
				
				// write warp to disk
				string warpXml = warp.ToXml();             
				navi.MoveToRoot();
				navi.MoveToFirstChild();                
				navi.AppendChild(warpXml);
				WriteXML();
             
				// add warp to memory
				if (isGlobal) {
					globalWarplist.Add(warpName, warp);
					player.sendMessage("Global warp " + warpName + " created.", 255, 0f, 255f, 255f);
					warpDrivePlugin.Log(player.getName() + " created global warp " + warpName + " at " + warp.loc.X + "," + warp.loc.Y);
				} else {
					Dictionary<string, Warp > personalWarplist = new Dictionary<string, Warp>();
					personalWarplistByPlayer.TryGetValue(warp.owner, out personalWarplist);
					if (personalWarplist == null) {
						personalWarplist = new Dictionary<string, Warp>();
						personalWarplistByPlayer.Add(warp.owner, personalWarplist);                         
					}
					personalWarplist.Add(warp.name, warp);
					player.sendMessage("Personal warp " + warpName + " created.", 255, 0f, 255f, 255f);
					warpDrivePlugin.Log(player.getName() + " created personal warp " + warpName + " at " + warp.loc.X + "," + warp.loc.Y);
				}                                               
			} else {
				player.sendMessage("Error: Warp " + warpName + " already exists.", 255, 0f, 255f, 255f);
			}
		}
     
		public bool WarpAlreadyExists(Player player, string warpName, bool isGlobal)
		{
			bool warpAlreadyExists = false;
         
			if (isGlobal) {
				warpAlreadyExists = globalWarplist.ContainsKey(warpName);
			} else {
				Dictionary<string, Warp > personalWarplist = new Dictionary<string, Warp>();
				personalWarplistByPlayer.TryGetValue(player.getName(), out personalWarplist);
				if (personalWarplist != null) {
					warpAlreadyExists = personalWarplist.ContainsKey(warpName); 
				}
			}            
         
			return warpAlreadyExists;
		}
     
		/**
      * Sends a list of all valid warp locations to the player
      */      
		public void WarpList(Player player)
		{
			GlobalWarpList(player);
			player.sendMessage("", 255, 0f, 255f, 255f);
			PersonalWarpList(player);           
		}

		/**
      * Sends a list of all valid global warp locations to the player
      */      
		public void GlobalWarpList(Player player)
		{
			player.sendMessage("Available global warps:", 255, 0f, 255f, 255f);
			foreach (KeyValuePair<string, Warp> pair in globalWarplist) {
				player.sendMessage("  " + pair.Key, 255, 0f, 255f, 255f);               
			}                        
		}
     
		/**
      * Sends a list of all valid personal warp locations to the player
      */      
		public void PersonalWarpList(Player player)
		{            
			player.sendMessage("Available personal warps:", 255, 0f, 255f, 255f);
			Dictionary<string, Warp > personalWarplist = new Dictionary<string, Warp>();
			personalWarplistByPlayer.TryGetValue(player.getName(), out personalWarplist);
			if (personalWarplist != null) {		
				foreach (KeyValuePair<string, Warp> pair in personalWarplist) {
					player.sendMessage("  " + pair.Key, 255, 0f, 255f, 255f);               
				}

			}            
		}

		private void WriteXML()
		{
			writer = XmlWriter.Create(xmlFile, wSettings);
			warpFile.WriteTo(writer);
			writer.Flush();
			writer.Close();
		}

	}
}
