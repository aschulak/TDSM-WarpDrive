using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria_Server;
using Envoy.TDSM_Passport;
using Envoy.TDSM_Vault;

namespace Envoy.TDSM_WarpDrive
{
    public class WarpDriveEngine
    {
        public bool enabled;
        private WarpDrivePlugin warpDrivePlugin;
        private WarpList globalWarpList;
        private bool globalOwnershipEnforced;
        private PassportManager passportManager;
        private Vault vault;

        public WarpDriveEngine(WarpDrivePlugin warpDrivePlugin, string xmlFile)
        {
            this.warpDrivePlugin = warpDrivePlugin;
            this.globalOwnershipEnforced = this.warpDrivePlugin.globalOwnershipEnforced;
            globalWarpList = new WarpList();
            passportManager = PassportManagerFactory.getPassportManager();
            vault = VaultFactory.getVault();
            loadGlobalWarps();
        }

        public void warp(Player player, string warpName)
        {
            warpDrivePlugin.Log("Attempting to warp [" + player.Name + "] to <" + warpName + ">");

            Warp warp = null;

            // always try personal warp first
            // must be logged in to warp to personal warp
            Passport passport = passportManager.getPassport(player);
            if (passport != null) {
                User user = passport.getUser();
                WarpList personalWarpList = getPersonalWarpList(player);
                if (personalWarpList != null && personalWarpList.ContainsKey(warpName)) {
                    personalWarpList.TryGetValue(warpName, out warp);
                }
            }

            // if we fail to find a personal warp, look in the global warps
            if (warp == null) {
                globalWarpList.TryGetValue(warpName, out warp);
            }

            if (warp != null) {
                warpPlayerTo(player, warp);
            } else {
                player.sendMessage("Error: warp <" + warpName + "> does not exist.", 255, 255f, 0f, 0f);
            }

        }

        public void setHomeWarp(Player player, bool forceOverwrite)
        {
            if (forceOverwrite) {
                removePersonalWarp(player, "home");
            }
            writePersonalWarp(player, "home");
        }

        public void writePersonalWarp(Player player, string warpName)
        {            
            Passport passport = passportManager.getPassport(player);

            // must be logged in to save a personal warp
            if (passport == null) {
                player.sendMessage("Error: Must be logged in to Passport to create a personal warp.", 255, 255f, 0f, 0f);
                return;
            }

            if (personalWarpExists(player, warpName)) {
                player.sendMessage("Error: Personal warp <" + warpName + "> exists.", 255, 255f, 0f, 0f);
                return;
            }

            User user = passport.getUser();
            Warp warp = new Warp();
            warp.type = WarpType.PERSONAL;
            warp.name = warpName;
            warp.owner = user.username; // use Account username here
            warp.loc.X = player.Location.X;
            warp.loc.Y = player.Location.Y;

            WarpList personalWarpList = getPersonalWarpList(player);
            try {
                personalWarpList.Add(warp.name, warp);
                vault.store(personalWarpList);
            } catch (Exception e) {

            }

            player.sendMessage("Personal warp <" + warpName + "> created.", 255, 0f, 255f, 255f);
            warpDrivePlugin.Log(player.Name + " created personal warp " + warpName + " at " + warp.loc.X + "," + warp.loc.Y);
        }

        public void writeGlobalWarp(Player player, string warpName)
        {
            Passport passport = passportManager.getPassport(player);

            // must be logged in to save a personal warp
            if (passport == null) {
                player.sendMessage("Error: Must be logged in to Passport to create a global warp.", 255, 255f, 0f, 0f);
                return;
            }

            if (globalWarpExists(warpName)) {
                player.sendMessage("Error: Global warp <" + warpName + "> exists.", 255, 255f, 0f, 0f);
                return;
            }

            User user = passport.getUser();
            Warp warp = new Warp();
            warp.type = WarpType.GLOBAL;
            warp.name = warpName;
            warp.owner = user.username;
            warp.loc.X = player.Location.X;
            warp.loc.Y = player.Location.Y;

            globalWarpList.Add(warpName, warp);
            vault.store(globalWarpList);
            player.sendMessage("Global warp <" + warpName + "> created.", 255, 0f, 255f, 255f);
            warpDrivePlugin.Log(player.Name + " created global warp " + warpName + " at " + warp.loc.X + "," + warp.loc.Y);
        }

        public void removePersonalWarp(Player player, string warpName)
        {
            Passport passport = passportManager.getPassport(player);
            // must be logged in to remove a personal warp
            if (passport == null) {
                player.sendMessage("Error: Must be logged in to Passport to remove a personal warp.", 255, 255f, 0f, 0f);
                return;
            }

            if (!personalWarpExists(player, warpName)) {
                player.sendMessage("Error: Personal warp <" + warpName + "> does not exist.", 255, 255f, 0f, 0f);
                return;
            }

            User user = passport.getUser();
            WarpList personalWarpList = getPersonalWarpList(player);

            Warp warp = null;
            personalWarpList.TryGetValue(warpName, out warp);

            // only allow if they own the warp
            if (globalOwnershipEnforced && warp.owner != user.username) {
                player.sendMessage("Error: Cannot delete global warp you do not own.", 255, 255f, 0f, 0f);
                warpDrivePlugin.Log(player.Name + " attempted to remove warp <" + warpName + "> unsuccessfully.");
                return;
            }

            personalWarpList.Remove(warpName);
            vault.store(personalWarpList);
            player.sendMessage("Personal warp <" + warpName + "> removed.", 255, 0f, 255f, 255f);
            warpDrivePlugin.Log("<" + user.username + ">[" + player.Name + "] removed warp " + warpName);
        }

        public void removeGlobalWarp(Player player, string warpName)
        {
            Passport passport = passportManager.getPassport(player);
            // must be logged in to remove a global warp
            if (passport == null) {
                player.sendMessage("Error: Must be logged in to Passport to remove a global warp.", 255, 255f, 0f, 0f);
                return;
            }

            if (!globalWarpExists(warpName)) {
                player.sendMessage("Error: Global warp <" + warpName + "> does not exist.", 255, 255f, 0f, 0f);
                return;
            }

            User user = passport.getUser();
            Warp warp = null;
            globalWarpList.TryGetValue(warpName, out warp);

            // only allow if they own the warp
            if (globalOwnershipEnforced && !warp.owner.Equals(user.username)) {
                player.sendMessage("Error: Cannot delete global warp you do not own.", 255, 255f, 0f, 0f);
                warpDrivePlugin.Log(player.Name + " attempted to remove warp <" + warpName + "> unsuccessfully.");
                return;
            }

            globalWarpList.Remove(warpName);
            vault.store(globalWarpList);
            player.sendMessage("Global warp <" + warpName + "> removed.", 255, 0f, 255f, 255f);
            warpDrivePlugin.Log("<" + user.username + ">[" + player.Name + "] removed warp " + warpName);
        }

        public bool globalWarpExists(string warpName)
        {
            return globalWarpList.ContainsKey(warpName);
        }

        public bool warpAlreadyExists(Player player, string warpName, bool isGlobal)
        {
            bool warpAlreadyExists = false;
         
            if (isGlobal) {
                warpAlreadyExists = globalWarpList.ContainsKey(warpName);
            } else {
                warpAlreadyExists = personalWarpExists(player, warpName);
            }
         
            return warpAlreadyExists;
        }

        public bool personalWarpExists(Player player, string warpName)
        {
            bool warpAlreadyExists = false;
            Passport passport = passportManager.getPassport(player);
            if (passport != null) {
                WarpList personalWarpList = getPersonalWarpList(player);
                if (personalWarpList != null) {
                    warpAlreadyExists = personalWarpList.ContainsKey(warpName);
                }
            }
            return warpAlreadyExists;
        }
     
        // Sends a list of all valid warp locations to the player
        public void sendWarpList(Player player)
        {
            sendGlobalWarpList(player);
            player.sendMessage("", 255, 0f, 255f, 255f);
            sendPersonalWarpList(player);
        }


        // Sends a list of all valid global warp locations to the player
        public void sendGlobalWarpList(Player player)
        {
            player.sendMessage("Available global warps:", 255, 0f, 255f, 255f);
            String warpList = "";
            foreach (KeyValuePair<string, Warp> pair in globalWarpList.getWarps()) {
                warpList += pair.Key + ", ";
            }

            // cut off trailing comma and whitespace
            if (warpList.Length >= 2) {
                warpList = warpList.Substring(0, warpList.Length - 2);
            }

            player.sendMessage(warpList, 255, 0f, 255f, 255f);
        }
     

        // Sends a list of all valid personal warp locations to the player
        public void sendPersonalWarpList(Player player)
        {            
            Passport passport = passportManager.getPassport(player);
            if (passport != null) {
                player.sendMessage("Available personal warps:", 255, 0f, 255f, 255f);
                WarpList personalWarpList = getPersonalWarpList(player);
                String warpList = "";
                if (personalWarpList != null) {
                    foreach (KeyValuePair<string, Warp> pair in personalWarpList.getWarps()) {
                        warpList += pair.Key + ", ";
                    }
                    // cut off trailing comma and whitespace
                    if (warpList.Length >= 2) {
                        warpList = warpList.Substring(0, warpList.Length - 2);
                    }

                    player.sendMessage(warpList, 255, 0f, 255f, 255f);
                }            
            } else {
                player.sendMessage("Personal warps only available when logged in to Passport.", 255, 0f, 255f, 255f);
            }
        }

        //
        // PRIVATE and INTERNAL
        //

        private void loadGlobalWarps()
        {
            try {
                vault.getVaultObject(globalWarpList);
            } catch (VaultObjectNotFoundException e) {

            }
        }

        private WarpList getPersonalWarpList(Player player)
        {
            WarpList warpList = new WarpList();
            try {
                Passport passport = passportManager.getPassport(player);
                warpList.setPassport(passport);
                vault.getVaultObject(warpList);
            } catch (Exception e) {
                // noop
            }

            return warpList;
        }
        
        // Warps the player to the warp
        private void warpPlayerTo(Player player, Warp warp)
        {
            player.teleportTo(warp.loc.X, warp.loc.Y);
            player.sendMessage("Warped to " + warp.type + " warp <" + warp.name + ">.", 255, 0f, 255f, 255f);
            warpDrivePlugin.Log(player.Name + " used /warp " + warp.name);
        }

    }
    
}