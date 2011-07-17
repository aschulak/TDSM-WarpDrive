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
using Envoy.TDSM_Vault;

/**
 * Based on core functionality developed in the Essentials plugin.
 *      http://www.tdsm.org/index.php?topic=130.0
 *      https://github.com/LukeTDSM/Essentials-TDSM
 *   
 * -Envoy
 */

namespace Envoy.TDSM_WarpDrive
{
    public class WarpDrivePlugin : Plugin
    {
        private Properties properties;
        private WarpDriveEngine warpDriveEngine;
        public bool isEnabled = false;
        public bool requiresOp = true;
        public bool globalOwnershipEnforced = true;
        public bool warpHomeOnDeath = true;

        public override void Load()
        {
            Name = "WarpDrive";
            Description = "Warp commands for TDSM";
            Author = "Envoy"; // see credits above, most of this is borrowed
            Version = "1.4.26";
            TDSMBuild = 26;
         
            Log("version " + base.Version + " Loading...");
         
            string pluginFolder = Statics.PluginPath + Path.DirectorySeparatorChar + Name;
            CreateDirectory(pluginFolder);

            //setup a new properties file
            properties = new Properties(pluginFolder + Path.DirectorySeparatorChar + "warpdrive.properties");
            properties.Load();
            properties.pushData(); //Creates default values if needed.
            properties.Save();

            //read properties data
            requiresOp = properties.requiresOp();
            globalOwnershipEnforced = properties.globalOwnershipEnforced();
            warpHomeOnDeath = properties.warpHomeOnDeath();

            // spit out useful property info
            Log("Requires Op: " + requiresOp);
            Log("Global Ownership Enforced: " + globalOwnershipEnforced);
            Log("Warp Home on Death: " + warpHomeOnDeath);

            //setup new WarpDriveEngine        
            warpDriveEngine = new WarpDriveEngine(this, pluginFolder + Path.DirectorySeparatorChar + "warps.xml");

            isEnabled = true;
        }

        public void Log(string message)
        {
            Program.tConsole.WriteLine("[" + base.Name + "] " + message);
        }
     
        public override void Enable()
        {
            this.registerHook(Hooks.PLAYER_COMMAND);
            this.registerHook(Hooks.PLAYER_DEATH);
            Log("Enabled");
        }

        public override void Disable()
        {
            isEnabled = false;
            Log("Disabled");
        }

        public override void onPlayerDeath(PlayerDeathEvent Event)
        {
            if (warpHomeOnDeath) {
                warpDriveEngine.warp(Event.Player, "home");
            }
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
                            sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }                    

                        sendingPlayer.sendMessage("WarpDrive version " + base.Version + " usage:", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /warplist: Lists all available global and personal warps", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /warp + <warpname>: Adds the personal warp named <warpname>", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /warp - <warpname>: Removes the personal warp named <warpname>", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /warp g+ <warpname>: Adds thr global warp named <warpname>", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /warp g- <warpname>: Removes the global warp named <warpname>", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /warp <warpname>: Warps player to warp named <warpname>", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /sethome: Set 'home' warp.", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /sethome!: Set 'home' warp and overwrite existing 'home' warp if it exists.", 255, 0f, 255f, 255f);
                        sendingPlayer.sendMessage("  /home: Warps player to warp point set by /sethome command.", 255, 0f, 255f, 255f);

                        Event.Cancelled = true;
                        return;
                    }
                 
                    // list of warps
                    if (commands[0].Equals("/warplist")) {
                        // always honor requiresOp for everything
                        if (requiresOp && !(sendingPlayer.Op)) {
                            sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }                    
                     
                        warpDriveEngine.sendWarpList(sendingPlayer);
                        Event.Cancelled = true;
                        return;
                    }

                    if (commands[0].Equals("/sethome")) {
                        // always honor requiresOp for everything
                        if (requiresOp && !(sendingPlayer.Op)) {
                            sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        warpDriveEngine.setHomeWarp(sendingPlayer, false);
                        Event.Cancelled = true;
                        return;
                    }

                    if (commands[0].Equals("/sethome!")) {
                        // always honor requiresOp for everything
                        if (requiresOp && !(sendingPlayer.Op)) {
                            sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        warpDriveEngine.setHomeWarp(sendingPlayer, true);
                        Event.Cancelled = true;
                        return;
                    }

                    if (commands[0].Equals("/home")) {
                        // always honor requiresOp for everything
                        if (requiresOp && !(sendingPlayer.Op)) {
                            sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }

                        warpDriveEngine.warp(sendingPlayer, "home");
                        Event.Cancelled = true;
                        return;
                    }

                    // warp commands
                    if (commands[0].Equals("/warp")) {   
                        // always honor requiresOp for everything
                        if (requiresOp && !(sendingPlayer.Op)) {
                            sendingPlayer.sendMessage("Error: WarpDrive commands require Op status", 255, 255f, 0f, 0f);
                            Event.Cancelled = true;
                            return;
                        }                    
                     
                        if (commands.Length < 2) {
                            sendingPlayer.sendMessage("For help, type /warpdrive", 255, 0f, 255f, 255f);
                        } else if (commands[1].Equals("+")) {
                            if (commands.Length < 3)
                                sendingPlayer.sendMessage("Error: format must be /warp + <warpname>", 255, 255f, 0f, 0f);
                            else {
                                warpDriveEngine.writePersonalWarp(sendingPlayer, commands[2]);
                            }
                        } else if (commands[1].Equals("g+")) {
                            if (commands.Length < 3)
                                sendingPlayer.sendMessage("Error: format must be /warp g+ <warpname>", 255, 255f, 0f, 0f);
                            else {
                                warpDriveEngine.writeGlobalWarp(sendingPlayer, commands[2]);
                            }
                        } else if (commands[1].Equals("-")) {
                            if (commands.Length < 3)
                                sendingPlayer.sendMessage("Error: format must be /warp - <warpname>", 255, 255f, 0f, 0f);
                            else {
                                warpDriveEngine.removePersonalWarp(sendingPlayer, commands[2]);
                            }
                        } else if (commands[1].Equals("g-")) {
                            if (commands.Length < 3)
                                sendingPlayer.sendMessage("Error: format must be /warp g- <warpname>", 255, 255f, 0f, 0f);
                            else {
                                warpDriveEngine.removeGlobalWarp(sendingPlayer, commands[2]);
                            }
                        } else if (commands.Length < 3) {
                            warpDriveEngine.warp(sendingPlayer, commands[1]);
                        }
                     
                        Event.Cancelled = true;
                    }
                }
            }
        }

        //
        // PRIVATE
        //

        private static void CreateDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
        }

    }
}