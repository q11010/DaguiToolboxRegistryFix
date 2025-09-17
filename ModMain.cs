using System;
using System.Reflection;
using UnityEngine;
using MOD_WIFdSk.PlayerPrefsHelper;
using Boo.Lang;
using System.Diagnostics;
using MelonLoader;
using System.Security;

/// <summary>
/// 当你手动修改了此命名空间，需要去模组编辑器修改对应的新命名空间，程序集也需要修改命名空间，否则DLL将加载失败！！！
/// </summary>
namespace MOD_WIFdSk
{
    /// <summary>
    /// 此类是模组的主类
    /// </summary>
    public class ModMain
    {
        private TimerCoroutine corUpdate;
        private static HarmonyLib.Harmony harmony;

        /// <summary>
        /// MOD初始化，进入游戏时会调用此函数
        /// </summary>
        public void Init()
        {
            //使用了Harmony补丁功能的，需要手动启用补丁。
            //启动当前程序集的所有补丁
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                harmony = null;
            }
            if (harmony == null)
            {
                harmony = new HarmonyLib.Harmony("MOD_WIFdSk");
            }
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // run on save; not needed

            //corUpdate = g.timer.Frame(new Action(OnUpdate), 1, true);
            //onSaveDataCall = (System.Action<ETypeData>)OnSaveData;
            //g.events.On(EGameType.SaveData, onSaveDataCall, 0);
            //System.Console.WriteLine("[DaGuiPlayerPrefsFix] Cleaner Injected to run on save game");
        }

        /// <summary>
        /// MOD销毁，回到主界面，会调用此函数并重新初始化MOD
        /// </summary>
        public void Destroy()
        {
            g.timer.Stop(corUpdate);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            ProbablyFastRemovePinyinEntries();
            stopwatch.Stop();
            MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix] Cleaner executed on destroy, time " + stopwatch.Elapsed);
        }

        /// <summary>
        /// 每帧调用的函数
        /// </summary>
        private void OnUpdate()
        {

        }

        //private void OnSaveData(ETypeData e)
        //{
        //    RemovePinyinEntries();
        //}


        /// <summary>
        /// Remove all Pinyin entries from reg
        /// </summary>
        [Obsolete("RemovePinyinEntries is Obsolete due to slowness, use ProbablyFastRemovePinyinEntries instead")]
        private void RemovePinyinEntries()
        {
            /// see https://github.com/sabresaurus/PlayerPrefsEditor/blob/1.4.1/Editor/PlayerPrefsEditor.cs#L273
            /// and https://docs.unity3d.com/ScriptReference/PlayerPrefs.html
            /// only windows
            try
            {
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\guigugame\\guigubahuang");
                if (registryKey != null)
                {
                    string[] valueName = registryKey.GetValueNames();
                    int count = 0;
                    foreach (string name in valueName)
                    {
                        string key = PlayerPrefsHelper.PlayerPrefsHelper.CleanPlayerPrefsKey(name);
                        if (key.EndsWith("py") || key.EndsWith("pinyin"))
                        {
                            count++;
                            if (count % 1000 == 0)
                            {
                                System.Console.WriteLine("[DaGuiPlayerPrefsFix] In Progress: " + count);
                            }
                            PlayerPrefs.DeleteKey(name);
                        }
                    }
                    MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix] " + count + " entries deleted");
                    // check success
                    valueName = registryKey.GetValueNames();
                    foreach (string name in valueName)
                    {
                        string key = PlayerPrefsHelper.PlayerPrefsHelper.CleanPlayerPrefsKey(name);
                        if (key.EndsWith("py") || key.EndsWith("pinyin"))
                        {
                            MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix] Failed Clean for entry " + name);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix] Failed to run OnSaveData: " + ex.ToString());
            }
        }

        public void ProbablyFastRemovePinyinEntries()
        {
            PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs[] rawPlayerPrefs = GetNonPinyinPlayerPrefs();
            if (rawPlayerPrefs == null)
            {
                MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix] No Valid PlayerPrefs, stopping");
                return;
            }
            PlayerPrefs.DeleteAll();
            foreach (PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs raw in rawPlayerPrefs)
            {
                PlayerPrefsHelper.PlayerPrefsHelper.SetPlayerPrefs(raw);
            }
        }

        private static string[] GetNonPinyinPlayerPrefsKeys()
        {
            try
            {
                Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\guigugame\\guigubahuang");
                List<string> tempKeys = new List<string>();
                int pSettingCount = 0;
                int pinyinCount = 0;
                if (registryKey != null)
                {
                    string[] valueName = registryKey.GetValueNames();
                    foreach (string name in valueName)
                    {
                        string key = PlayerPrefsHelper.PlayerPrefsHelper.CleanPlayerPrefsKey(name);
                        if (key.EndsWith("py") || key.EndsWith("pinyin"))
                        {
                           pinyinCount++;
                        } else
                        {
                            pSettingCount++;
                            tempKeys.Add(key);
                        }
                    }
                    MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix]  " + pSettingCount + " settings and " + pinyinCount + " pinyin/py");
                    return tempKeys.ToArray();
                }
                else
                {
                    return null;
                }
            }
            catch (System.Security.SecurityException ex)
            {
                MelonLoader.MelonLogger.Msg("[DaGuiPlayerPrefsFix] Failed to run OnSaveData: " + ex.ToString());
                return null;
            }
        }

        private static PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs[] GetNonPinyinPlayerPrefs()
        {
            string[] keys = GetNonPinyinPlayerPrefsKeys();
            if (keys == null)
            {
                return null;
            }
            List<PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs> tempRawPlayerPrefs = new List<PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs> ();
            foreach (string key in keys)
            {
                tempRawPlayerPrefs.Add(PlayerPrefsHelper.PlayerPrefsHelper.GetRawPlayerPrefsFromReg(key));
            }
            return tempRawPlayerPrefs.ToArray();
        }
    }
}
