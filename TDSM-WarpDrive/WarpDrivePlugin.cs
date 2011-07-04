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
using WarpDrive;

/**
 * Giving credit where credit is due, code based on Essentials plugin found here:
 *  
 * http://www.tdsm.org/index.php?topic=130.0
 * https://github.com/LukeTDSM/Essentials-TDSM
 * 
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

        // tConsole is used for when logging Output to the console & a log file.

        public Properties properties;
		public WarpDriveEngine warpDriveEngine;
        public bool isEnabled = false;
		
        public override void Load()
        {
            Name = "WarpDrive";
            Description = "Warp commands for TDSM";
            Author = "Envoy"; // see credits above, most of this is borrowed
            Version = "1.0";
            TDSMBuild = 16;
			
			Log("version " + base.Version + " Loading...");
            string pluginFolder = Statics.getPluginPath + Statics.systemSeperator + Name;
            //Create folder if it doesn't exist
            if (!Program.createDirectory(pluginFolder, true))
            {
                Log("Failed to create plugin folder.");
                return;
            }

            //setup a new properties file
            properties = new Properties(pluginFolder + Statics.systemSeperator + "warpdrive.properties");
            properties.Load();
            properties.pushData(); //Creates default values if needed.
            properties.Save();
            
            //setup new Warp			
            warpDriveEngine = new WarpDriveEngine(this, pluginFolder + Statics.systemSeperator + "warps.xml");

            //read properties data
            warpDriveEngine.enabled = properties.isWarpEnabled();
            warpDriveEngine.requiresOp = properties.warpRequiresOp();

            isEnabled = true;
        }
		
		public void Log(string message)
		{
			Program.tConsole.WriteLine("[" + base.Name + "] " + message);	
		}
		
        public override void Enable()
        {
            Log("Enabled");
            //Register Hooks
            this.registerHook(Hooks.PLAYER_COMMAND);            
        }

        public override void Disable()
        {
            Log("Disabled");
            isEnabled = false;
        }

        public override void onPlayerCommand(PlayerCommandEvent Event)
        {
            if (isEnabled == false) { return; }
            string[] commands = Event.getMessage().ToLower().Split(' '); //Split into sections (to lower case to work with it better)
            if (commands.Length > 0)
            {
                if (commands[0] != null && commands[0].Trim().Length > 0) //If it is not nothing, and the string is actually something
                {
			if (commands[0].Equals("/warpdrive"))
			{
				Player sendingPlayer = Event.getPlayer();
				sendingPlayer.sendMessage("WarpDrive version " + base.Version + " usage:", 255, 0f, 255f, 255f);
				sendingPlayer.sendMessage("  /warplist: Lists all available global and personal warps", 255, 0f, 255f, 255f);
				sendingPlayer.sendMessage("  /warp + <name>: Adds a personal warp called <name>", 255, 0f, 255f, 255f);
				sendingPlayer.sendMessage("  /warp - <name>: Removes the personal warp called <name>", 255, 0f, 255f, 255f);
				sendingPlayer.sendMessage("  /warp g+ <name>: Adds a global warp called <name>", 255, 0f, 255f, 255f);
				sendingPlayer.sendMessage("  /warp g- <name>: Removes the global warp called <name>", 255, 0f, 255f, 255f);
				sendingPlayer.sendMessage("  /warpdrive: Displays this usage text", 255, 0f, 255f, 255f);
			}
			if (commands[0].Equals("/warplist"))
			{
			    Player sendingPlayer = Event.getPlayer();
				if (warpDriveEngine.requiresOp && !(sendingPlayer.isOp()))
                {
                    sendingPlayer.sendMessage("Error: /warplist requires Op status", 255, 0f, 255f, 255f);
                    Event.setCancelled(true);
                } else {
					warpDriveEngine.WarpList(sendingPlayer);
					return;
				}
		    }
                    if (commands[0].Equals("/warp"))
                    {
                        Player sendingPlayer = Event.getPlayer();
                        if (warpDriveEngine.requiresOp && !(sendingPlayer.isOp()))
                        {
                            sendingPlayer.sendMessage("Error: /warp requires Op status", 255, 0f, 255f, 255f);
                            Event.setCancelled(true);
                            return;
                        }
                        if(warpDriveEngine.enabled)
                        {
                            if (commands.Length < 2)
                            {
                                sendingPlayer.sendMessage("For help, type /warpdrive", 255, 0f, 255f, 255f);
                            }
                            else if (commands[1].Equals("+"))
                            {
                                if (commands.Length < 3)
                                    sendingPlayer.sendMessage("Add warp error: format must be /warp + <warpname>", 255, 0f, 255f, 255f);
                                else
                                {
                                    warpDriveEngine.WriteWarp(sendingPlayer, commands[2], false);
                                }
                            }
							else if (commands[1].Equals("g+"))
                            {
                                if (commands.Length < 3)
                                    sendingPlayer.sendMessage("Add warp error: format must be /warp + <warpname>", 255, 0f, 255f, 255f);
                                else
                                {
                                    warpDriveEngine.WriteWarp(sendingPlayer, commands[2], true);
                                }
                            }
                            else if (commands[1].Equals("-"))
                            {
                                if (commands.Length < 3)
                                    sendingPlayer.sendMessage("Remove warp error: format must be /warp - <warpname>", 255, 0f, 255f, 255f);
                                else
                                {
                                    warpDriveEngine.DelWarp(sendingPlayer, commands[2], false);
                                }
                            }
							else if (commands[1].Equals("g-"))
                            {
                                if (commands.Length < 3)
                                    sendingPlayer.sendMessage("Remove warp error: format must be /warp - <warpname>", 255, 0f, 255f, 255f);
                                else
                                {
                                    warpDriveEngine.DelWarp(sendingPlayer, commands[2], true);
                                }
                            }
                            else if (commands.Length < 3)
                            {
                                warpDriveEngine.Warp(sendingPlayer, commands[1]);
                            }
                        }
                        else
                        {
                            sendingPlayer.sendMessage("Error: Warp not enabled", 255, 0f, 255f, 255f);
                        }
                        Event.setCancelled(true);
                    }                    
                }
            }
        }            
    }
}
