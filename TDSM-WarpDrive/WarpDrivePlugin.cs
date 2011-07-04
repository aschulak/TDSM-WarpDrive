using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Terraria_Server.Plugin;
using Terraria_Server;
using Terraria_Server.Commands;
using Terraria_Server.Events;
using Terraria_Server.Misc;
using WarpDrive;

/**
 * Based on core functionality developed in the Essentials plugin.
 * 	   http://www.tdsm.org/index.php?topic=130.0
 * 	   https://github.com/LukeTDSM/Essentials-TDSM
 * 	
 * -Envoy
 */

namespace WarpDrive
{
	public class WarpDrivePlugin : Plugin
	{
		/*
         * @Developers
         * 
         * Plugins need to be in .NET 3.5
         * Otherwise TDSM will be unable to load it. 
         * 
         * As of June 16, 1:15 AM, TDSM should now load Plugins Dynamically.
         */

		public Properties properties;
		public WarpDriveEngine warpDriveEngine;
		public bool isEnabled = false;
		public bool requiresOp = true;
        public bool globalOwnershipEnforced = true;
     
		public override void Load()
		{
			Name = "WarpDrive";
			Description = "Warp commands for TDSM";
			Author = "Envoy"; // see credits above, most of this is borrowed
			Version = "1.1.0";
			TDSMBuild = 19;
         
			Log("version " + base.Version + " Loading...");
			
			string pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + Name;
			//Create folder if it doesn't exist
			CreateDirectory(pluginFolder);

			//setup a new properties file
			properties = new Properties(pluginFolder + Path.DirectorySeparatorChar + "warpdrive.properties");
			properties.Load();
			properties.pushData(); //Creates default values if needed.
			properties.Save();

            //read properties data
            requiresOp = properties.requiresOp();
            globalOwnershipEnforced = properties.globalOwnershipEnforced();

            // spit out useful property info
            Log("Requires Ops: " + requiresOp);
            Log("Global Ownership Enforced: " + globalOwnershipEnforced);

			//setup new WarpDriveEngine        
			warpDriveEngine = new WarpDriveEngine(this, pluginFolder + Path.DirectorySeparatorChar + "warps.xml");

			isEnabled = true;
		}

		private static void CreateDirectory(string dirPath)
		{
			if (!Directory.Exists(dirPath)) {
				Directory.CreateDirectory(dirPath);
			}
		}

		public void Log(string message)
		{
			Program.tConsole.WriteLine("[" + base.Name + "] " + message);
		}
     
		public override void Enable()
		{
			Log("Enabled");
			this.registerHook(Hooks.PLAYER_COMMAND);            
		}

		public override void Disable()
		{
			Log("Disabled");
			isEnabled = false;
		}

		public override void onPlayerCommand(PlayerCommandEvent Event)
		{
			if (isEnabled == false) {
				return;
			}
			
			string[] commands = Event.Message.ToLower().Split(' '); //Split into sections (to lower case to work with it better)
			if (commands.Length > 0) {
				if (commands[0] != null && commands[0].Trim().Length > 0) { //If it is not nothing, and the string is actually something
					
					Player sendingPlayer = Event.Player;
										
					// usage
					if (commands[0].Equals("/warpdrive")) {					
						// always honor requiresOp for everything
						if (requiresOp && !(sendingPlayer.Op)) {
							sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 0f, 255f, 255f);
							Event.Cancelled = true;
							return;
						}					

						sendingPlayer.sendMessage("WarpDrive version " + base.Version + " usage:", 255, 0f, 255f, 255f);
						sendingPlayer.sendMessage("  /warplist: Lists all available global and personal warps", 255, 0f, 255f, 255f);
						sendingPlayer.sendMessage("  /warp + <warpname>: Adds a personal warp named <warpname>", 255, 0f, 255f, 255f);
						sendingPlayer.sendMessage("  /warp - <warpname>: Removes the personal warp named <warpname>", 255, 0f, 255f, 255f);
						sendingPlayer.sendMessage("  /warp g+ <warpname>: Adds a global warp named <warpname>", 255, 0f, 255f, 255f);
						sendingPlayer.sendMessage("  /warp g- <warpname>: Removes the global warp named <warpname>", 255, 0f, 255f, 255f);
						sendingPlayer.sendMessage("  /warpdrive: Displays plugin usage (this text)", 255, 0f, 255f, 255f);
						Event.Cancelled = true;
						return;
					}
					
					// list of warps
					if (commands[0].Equals("/warplist")) {
						// always honor requiresOp for everything
						if (requiresOp && !(sendingPlayer.Op)) {
							sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 0f, 255f, 255f);
							Event.Cancelled = true;
							return;
						}					
						
						warpDriveEngine.WarpList(sendingPlayer);
						Event.Cancelled = true;
						return;
					}
					
					// warp commands
					if (commands[0].Equals("/warp")) {	
						// always honor requiresOp for everything
						if (requiresOp && !(sendingPlayer.Op)) {
							sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 0f, 255f, 255f);
							Event.Cancelled = true;
							return;
						}					
						
						if (commands.Length < 2) {
							sendingPlayer.sendMessage("For help, type /warpdrive", 255, 0f, 255f, 255f);
						} else if (commands[1].Equals("+")) {
							if (commands.Length < 3)
								sendingPlayer.sendMessage("Add personal warp error: format must be /warp + <warpname>", 255, 0f, 255f, 255f);
							else {
								warpDriveEngine.WriteWarp(sendingPlayer, commands[2], false);
							}
						} else if (commands[1].Equals("g+")) {
							if (commands.Length < 3)
								sendingPlayer.sendMessage("Add global warp error: format must be /warp g+ <warpname>", 255, 0f, 255f, 255f);
							else {
								warpDriveEngine.WriteWarp(sendingPlayer, commands[2], true);
							}
						} else if (commands[1].Equals("-")) {
							if (commands.Length < 3)
								sendingPlayer.sendMessage("Remove personal warp error: format must be /warp - <warpname>", 255, 0f, 255f, 255f);
							else {
								warpDriveEngine.DelWarp(sendingPlayer, commands[2], false);
							}
						} else if (commands[1].Equals("g-")) {
							if (commands.Length < 3)
								sendingPlayer.sendMessage("Remove global warp error: format must be /warp g- <warpname>", 255, 0f, 255f, 255f);
							else {
								warpDriveEngine.DelWarp(sendingPlayer, commands[2], true);
							}
						} else if (commands.Length < 3) {
							warpDriveEngine.Warp(sendingPlayer, commands[1]);
						}
						
						Event.Cancelled = true;
					}
				}
			}
		}
	}
}