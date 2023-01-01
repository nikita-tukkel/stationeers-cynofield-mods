using System;
using System.IO;

namespace cynofield.mods.utils
{
    /// <summary>
    /// Not trying to be complicated:
    /// - mod must have it's name hardcoded;
    /// - mod must be placed into local (not Steam) mod dir (which is "save path"/mods);
    /// 
    /// For more details see:
    ///  Assets.Scripts.Networking.NetworkManager.GetLocalAndWorkshopItems(Assets.Scripts.Networking.Transports.SteamTransport.WorkshopType.Mod);
    /// and
    ///  Assets.Scripts.Networking.Transports.WorkshopUtils.GetLocalDirInfo
    /// </summary>
    public class ModDirLocator
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        private readonly string modDir = null;
        private readonly string modContentDir = null;
        public ModDirLocator(string modName)
        {
            var savePath = Assets.Scripts.Serialization.Settings.CurrentData.SavePath;
            var modsPath = savePath + "/mods";
            var modPath = modsPath + "/" + modName;
            var contentPath = modPath + "/Content";
            var aboutPath = modPath + "/About";
            var aboutXml = aboutPath + "/About.xml";

            if (!FileExists(aboutXml))
            {
                Log.Warn(() => $"Mod directory not found in {modPath}");
                return;
            }

            Log.Info(() => $"found {modPath}");
            this.modDir = modPath;
            this.modContentDir = contentPath;
        }

        public string GetContentDir()
        {
            return modDir == null ? null : modContentDir;
        }

        private bool FileExists(string path)
        {
            bool exists = false;

            // this bullshit is required because Stationeers.Addons blacklists a lot of usefull standard classes.
            try
            {
                new StreamReader(path).Close();
                exists = true;
            }
            catch (Exception e)
            {
                if (e.ToString().StartsWith("System.IO.UnauthorizedAccessException")
                    || e.ToString().StartsWith("System.IO.DirectoryNotFoundException"))
                { }
                else
                {
                    Log.Error(e);
                }
            }

            return exists;
        }

        private bool WarnDirectoryNotFound_StreamWriterBased(string path)
        {
            bool exists = false;

            // this bullshit is required because Stationeers.Addons blacklists a lot of usefull standard classes.
            try
            {
                new StreamWriter(path + "/_", append: true).Close();
                exists = true;
            }
            catch (Exception e)
            {
                if (e.ToString().StartsWith("System.IO.UnauthorizedAccessException")
                    || e.ToString().StartsWith("System.IO.DirectoryNotFoundException"))
                { }
                else
                {
                    Log.Error(e);
                }
            }

            if (!exists)
                Log.Warn(() => $"expected path not found {path}");
            return exists;
        }
    }
}
