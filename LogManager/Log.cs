﻿using System;
using System.IO;
using System.Collections.Generic;
namespace UnityEngine
{
    /// <summary>
    /// 系统日志模块
    /// </summary>
    public class Log
    {
        public static bool EnableLog = true;           // 是否启用日志，仅可控制普通级别的日志的启用与关闭，LogError和LogWarn都是始终启用的。
        public static bool EnableSave = false;  // 是否允许保存日志，即把日志写入到文件中

        public static bool IsOpenNetPackageLog = false;// 是否开放网络日志
        public static string LogFileDir = null;
        public static string LogFileName = "";
        public static string Prefix = "> ";     // 用于与Unity默认的系统日志做区分。本日志系统输出的日志头部都会带上这个标记。
        public static StreamWriter LogFileWriter = null;

        private static readonly int LineCount = 100;
        //日志列表
        public static List<KeyValuePair<int, string>> ListBugs = new List<KeyValuePair<int, string>>();

        //第一次执行打印log
        private static bool FirstLogTag = true;

        private static string GetLogTime()
        {
            string str = "";

            str = DateTime.Now.ToString("HH:mm:ss.fff") + " ";

            return str;
        }

        public static void Info(string message, params object[] args)
        {
            if (!EnableLog)
                return;

            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }
            string str = GetLogTime() + message;
            if (ListBugs.Count > LineCount)
            {
                ListBugs.RemoveAt(0);
            }
            ListBugs.Add(new KeyValuePair<int, string>(1,str));
            Debug.Log(Prefix + str, null);
            LogToFile("[I]" + str, false);
        }

        public static void Error(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }
            string str = GetLogTime() + message;
            if (ListBugs.Count > LineCount)
            {
                ListBugs.RemoveAt(0);
            }
            ListBugs.Add(new KeyValuePair<int, string>(3, str));
            Debug.LogError(Prefix + str, null);
            LogToFile("[E]" + str, true);
        }

        public static void InfoRed(string message, params object[] args) 
        {
            if (!EnableLog)
                return;

            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }
            string str = GetLogTime() + message;
            if (ListBugs.Count > LineCount)
            {
                ListBugs.RemoveAt(0);
            }
            ListBugs.Add(new KeyValuePair<int, string>(1,str));
            Debug.Log(Prefix + string.Concat("<color=#AB2B2B>", str, "</color>"), null);
            LogToFile("[I]" + str, false);
        }

        /// <summary>
        /// 将日志写入到文件中
        /// </summary>
        /// <param name="message"></param>
        /// <param name="EnableStack"></param>
        private static void LogToFile(string message, bool EnableStack = false)
        {
            if (!EnableSave)
                return;

            if (LogFileWriter == null)
            {
                LogFileName = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
                LogFileName = LogFileName.Replace("-", "_");
                LogFileName = LogFileName.Replace(":", "_");
                LogFileName = LogFileName.Replace(" ", "");
                LogFileName = LogFileName + ".log";
                if (string.IsNullOrEmpty(LogFileDir))
                {
                    try
                    {
#if UNITY_EDITOR
                        if (!Directory.Exists("d:/UnityLog"))
                        {
                            Directory.CreateDirectory("d:/UnityLog");
                        }
                        LogFileDir = "d:/UnityLog/";
#else
                        if ((Application.platform == RuntimePlatform.Android) || (Application.platform == RuntimePlatform.IPhonePlayer))
                        {
                            LogFileDir = Application.persistentDataPath + "/DebuggerLog/";
                        }
#endif
                    }
                    catch (Exception exception)
                    {
                        Debug.Log(Prefix + "获取 Application.persistentDataPath 报错！" + exception.Message, null);
                        return;
                    }
                }
                string path = LogFileDir + LogFileName;
                try
                {
                    if (!Directory.Exists(LogFileDir))
                    {
                        Directory.CreateDirectory(LogFileDir);
                    }
                    LogFileWriter = File.AppendText(path);
                    LogFileWriter.AutoFlush = true;
                }
                catch (Exception exception2)
                {
                    LogFileWriter = null;
                    Debug.Log("LogToCache() " + exception2.Message + exception2.StackTrace, null);
                    return;
                }
            }
            if (LogFileWriter != null)
            {
                try
                {
                    if(FirstLogTag)
                    {
                        FirstLogTag = false;
                        PhoneSystemInfo(LogFileWriter);
                    }
                    LogFileWriter.WriteLine(message);
                    if (EnableStack)
                    {
                        LogFileWriter.WriteLine(StackTraceUtility.ExtractStackTrace());
                    }
                }
                catch (Exception)
                {
                }
            }
        }


        private static void PhoneSystemInfo(StreamWriter sw)
        {
            sw.WriteLine("*********************************************************************************************************start");
            sw.WriteLine("By " + SystemInfo.deviceName);
            DateTime now = DateTime.Now;
            sw.WriteLine(string.Concat(new object[] { now.Year.ToString(), "年", now.Month.ToString(), "月", now.Day, "日  ", now.Hour.ToString(), ":", now.Minute.ToString(), ":", now.Second.ToString() }));
            sw.WriteLine();
            sw.WriteLine("操作系统:  " + SystemInfo.operatingSystem);
            sw.WriteLine("系统内存大小:  " + SystemInfo.systemMemorySize);
            sw.WriteLine("设备模型:  " + SystemInfo.deviceModel);
            sw.WriteLine("设备唯一标识符:  " + SystemInfo.deviceUniqueIdentifier);
            sw.WriteLine("处理器数量:  " + SystemInfo.processorCount);
            sw.WriteLine("处理器类型:  " + SystemInfo.processorType);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceID);
            sw.WriteLine("显卡名称:  " + SystemInfo.graphicsDeviceName);
            sw.WriteLine("显卡标识符:  " + SystemInfo.graphicsDeviceVendorID);
            sw.WriteLine("显卡厂商:  " + SystemInfo.graphicsDeviceVendor);
            sw.WriteLine("显卡版本:  " + SystemInfo.graphicsDeviceVersion);
            sw.WriteLine("显存大小:  " + SystemInfo.graphicsMemorySize);
            sw.WriteLine("显卡着色器级别:  " + SystemInfo.graphicsShaderLevel);
            sw.WriteLine("是否图像效果:  " + SystemInfo.supportsImageEffects);
            sw.WriteLine("是否支持内置阴影:  " + SystemInfo.supportsShadows);
            sw.WriteLine("*********************************************************************************************************end");
            sw.WriteLine("LogInfo:");
            sw.WriteLine();
        }

        public static void CloseLog()
        {
            if (LogFileWriter != null)
            {
                try
                {
                    LogFileWriter.Flush();
                    LogFileWriter.Close();
                    LogFileWriter.Dispose();
                    LogFileWriter = null;
                }
                catch (Exception)
                {
                }
            }
        }


        public enum LogPackageType
        {
            ClientToServer = 0,
            ServerToClient = 1
        }

        public static void ShowPackageInfo(LogPackageType type ,string info ,int cmd)
        {
            if (IsOpenNetPackageLog)
            {
                if(type == LogPackageType.ClientToServer)
                {
                    Log.Warning(string.Format("{0} <color=#ffff00>{1}</color>  - {2}", info, cmd.ToString(), DateTime.Now.ToString()));
                }
                else
                {
                    Log.Warning(string.Format("{0} <color=#01c00f>{1}</color>  - {2}", info, cmd.ToString(), DateTime.Now.ToString()));
                }
            }
        }

        private static void Warning(object message)
        {
            string str = Prefix +  message;
            if (ListBugs.Count > LineCount)
            {
                ListBugs.RemoveAt(0);
            }
            ListBugs.Add(new KeyValuePair<int, string>(2, str));
            Debug.LogWarning(str, null);
            LogToFile("[W]" + str, false);
        }
    }
}