using Assets.Scripts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace cynofield.mods.utils
{
    /// <summary>
    /// Initialize Logger instance by adding into your class:
    /// 
    ///   private class Logger_ : CLogger { }
    ///   private static readonly CLogger Log = new Logger_();
    /// 
    /// Call your loggin like:
    /// 
    ///   Log.Info(() => $"your message and data {ToString()}");
    ///   Log.Error(e);
    /// 
    /// Logger writes into console (if enabled in LoggerConfig) and into Unity log file.
    /// The Unit log file for Stationeers game under Windows is located here:
    /// `%USERPROFILE%\AppData\LocalLow\Rocketwerkz\rocketstation\Player.log`.
    /// 
    /// Log format like:
    ///   #W    18:01:39.953  cynofield.mods.AugmentedRealityEntry	message
    /// 
    /// Regex parser for log format (Level, Timestamp, Emitter, Message):
    ///   #(\S)\t(\S+)\t(\S+)\t(.*)
    /// 
    /// LogMx regex parser (Level, Timestamp, Emitter, Message):
    ///   (#[DIWE])?(?:\s*)?(\d\d:\S+)?(?:\s*)?([\[\w][\w\._\-\+\[\]]*+)?(?:\s*)?(.*)
    /// </summary>
    public class CLogger
    {
        private static readonly LoggerConfig config = new LoggerConfig();

        public static void SetConfig(Dictionary<string, (LoggerConfig.LogLevel, bool)> cfg)
        {
            config.SetConfig(cfg);
        }

        private readonly string typeName;
        private readonly LoggerConfig.LogLevel logLevel;
        private readonly bool useConsole;
        private static readonly char[] separators = new char[] { '`', '+', '/' }; // delimeters between nested and parent class names
        public CLogger()
        {
            typeName = $"{GetType()}";
            var index = typeName.IndexOfAny(separators);
            if (index > 0) typeName = typeName.Substring(0, index);
            (logLevel, useConsole) = config.GetConfig(typeName);
        }

        public delegate string MsgProvider();

        /// <summary>
        /// Call like:
        /// 
        /// Log(() => $"your message and data {ToString()}");
        /// Log(delegate { return $"your message and data {ToString()}"; });
        ///
        /// </summary>
        /// <param name="msgProvider"></param>
        public void Info(MsgProvider msgProvider) => Info(null, msgProvider);
        public void Info(Exception e) => Info(e, null);
        public void Info(Exception e, MsgProvider msgProvider)
        {
            if (LoggerConfig.LogLevel.INFO < logLevel)
                return;

            var msg = FormatMessage("I", msgProvider, e);
            UnityEngine.Debug.Log(msg);
            if (useConsole)
                ConsoleWindow.Print(msg);
        }

        public void Debug(MsgProvider msgProvider) => Debug(null, msgProvider);
        public void Debug(Exception e) => Debug(e, null);
        public void Debug(Exception e, MsgProvider msgProvider)
        {
            if (LoggerConfig.LogLevel.DEBUG < logLevel)
                return;

            var msg = FormatMessage("D", msgProvider, e);
            UnityEngine.Debug.Log(msg);
        }

        public void Warn(MsgProvider msgProvider) => Warn(null, msgProvider);
        public void Warn(Exception e) => Warn(e, null);
        public void Warn(Exception e, MsgProvider msgProvider)
        {
            if (LoggerConfig.LogLevel.WARN < logLevel)
                return;

            var msg = FormatMessage("W", msgProvider, e);
            UnityEngine.Debug.LogWarning(msg);
            if (useConsole)
                ConsoleWindow.Print(msg);
        }

        public void Error(MsgProvider msgProvider) => Error(null, msgProvider);
        public void Error(Exception e) => Error(e, null);
        public void Error(Exception e, MsgProvider msgProvider)
        {
            if (LoggerConfig.LogLevel.ERROR < logLevel)
                return;

            var msg = FormatMessage("E", msgProvider, e);
            UnityEngine.Debug.LogError(msg); // Debug.LogError includes ConsoleWindow.Print
        }

        private string FormatMessage(string levelCode, MsgProvider msgProvider, Exception e)
        {
            var time = DateTime.Now.ToString("HH:mm:ss.fff");
            var msg = "#" + levelCode + "\t" + time + "\t" + typeName;
            if (msgProvider != null)
            {
                try
                {
                    var m = msgProvider();
                    if (m != null && m.Length > 0)
                        msg += "\t" + m;
                }
                catch (Exception e2)
                {
                    msg = $"{time}\t#E\tError generating log message for {typeName}: {e2}";
                }
            }

            if (e != null)
                msg += "\t" + e;
            return msg;
        }

        private void LogInternals(string msg)
        {
            UnityEngine.Debug.Log($"@@@cynofield.mods.utils.Logger\t{msg}");
        }
    }

    public class LoggerConfig
    {
        private static (LogLevel, bool) DEFAULT = (LogLevel.DEBUG, true);
        public LoggerConfig()
        {
            SetConfig(null);
        }

        public void SetConfig(Dictionary<string, (LogLevel, bool)> cfg)
        {
            staticConfig[""] = DEFAULT; // default configuration
            if (cfg == null)
                return;

            foreach (var entry in cfg)
            {
                staticConfig[entry.Key] = entry.Value;
                configCache[entry.Key] = entry.Value;
            }
        }

        private readonly ConcurrentDictionary<string, (LogLevel, bool)> staticConfig = new ConcurrentDictionary<string, (LogLevel, bool)>();
        private readonly ConcurrentDictionary<string, (LogLevel, bool)> configCache = new ConcurrentDictionary<string, (LogLevel, bool)>();

        public enum LogLevel
        {
            DEBUG,
            INFO,
            WARN,
            ERROR,
            OFF
        }

        public (LogLevel, bool) GetConfig(string typeName)
        {
            if (configCache.TryGetValue(typeName, out (LogLevel, bool) value)) return value;
            (LogLevel, bool) result = DEFAULT;
            string resultName = "";
            foreach (var entry in configCache)
            {
                var n = entry.Key;
                if (typeName.StartsWith(n))
                {
                    if (n.Length > resultName.Length)
                    {
                        resultName = n;
                        result = entry.Value;
                    }
                }
            }
            configCache.TryAdd(resultName, result);
            return result;
        }
    }
}
