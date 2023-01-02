using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace cynofield.mods.utils
{
    public class AssetsLoader
    {
        private class Logger_ : CLogger { }
        private static readonly CLogger Log = new Logger_();

        public static void SetConfig(List<string> bundleFiles)
        {
            AssetsLoader.bundleFiles.Clear();
            AssetsLoader.bundleFiles.AddRange(bundleFiles);
        }

        private static readonly List<string> bundleFiles = new List<string>();

        public static AssetsLoader Instance;
        public static AssetsLoader Load()
        {
            // to hide spam warnings like:
            // The character with Unicode value \u041F was not found in the [LiberationSans SDF] font asset or any potential fallbacks. It was replaced by Unicode character \u25A1 in text object [New Game Object].
            try { Traverse.Create(TMP_Settings.instance).Field("m_warningsDisabled").SetValue(true); }
            catch (Exception) { }

            if (Instance != null) Destroy();
            Instance = new AssetsLoader();
            return Instance;
        }

        public static void Destroy()
        {
            foreach (var bundle in bundles)
            {
                bundle.Unload(true);
            }
            bundles.Clear();
            bundleSources.Clear();
            Instance = null;
        }

        private static readonly List<AssetBundle> bundles = new List<AssetBundle>();
        private static readonly Dictionary<AssetBundle, string> bundleSources = new Dictionary<AssetBundle, string>();
        private AssetsLoader()
        {
            foreach (var file in bundleFiles)
            {
                AssetBundle bundle = null;
                try
                {
                    if (!IsFileExists(file))
                    {
                        Log.Warn(() => $"asset bundle file not found {file}");
                        continue;
                    }
                    Log.Debug(() => $"loading {file}");
                    bundle = AssetBundle.LoadFromFile(file);
                    if (bundle != null)
                        bundleSources[bundle] = file;
                }
                catch (Exception e) { Log.Error(e); }

                if (bundle != null)
                    bundles.Add(bundle);
            }
        }

        private bool IsFileExists(string path)
        {
            try
            {
                new StreamReader(path).Close();
                return true;
            }
            catch (Exception) { return false; }
        }

        public void DebugInfo()
        {
            if (bundles.Count == 0)
            {
                Log.Info(() => $"no asset bundles loaded");
            }
            else
            {
                foreach (var bundle in bundles)
                {
                    Log.Info(() => $"bundle {bundle} from {bundleSources[bundle]}:\n{string.Join("\n", bundle.GetAllAssetNames())}");
                }
            }
        }

        public T LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            foreach (var bundle in bundles)
            {
                var result = bundle.LoadAsset<T>(name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
