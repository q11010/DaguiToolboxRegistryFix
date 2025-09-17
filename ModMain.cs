using System;
using System.Reflection;
using UnityEngine;
using MOD_WIFdSk.PlayerPrefsHelper;
using Boo.Lang;
using System.Diagnostics;
using MelonLoader;

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
        private Il2CppSystem.Action<ETypeData> onSaveDataCall;

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
            RemovePinyinEntries();
            stopwatch.Stop();
            System.Console.WriteLine("[DaGuiPlayerPrefsFix] Cleaner executed on destroy, time " + stopwatch.Elapsed);
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
                            if (count % 500 == 0)
                            {
                                System.Console.WriteLine("[DaGuiPlayerPrefsFix] In Progress: " + count);
                            }
                            PlayerPrefs.DeleteKey(name);
                        }
                    }
                    System.Console.WriteLine("[DaGuiPlayerPrefsFix] " + count + " entries deleted");
                    // check success
                    valueName = registryKey.GetValueNames();
                    foreach (string name in valueName)
                    {
                        string key = PlayerPrefsHelper.PlayerPrefsHelper.CleanPlayerPrefsKey(name);
                        if (key.EndsWith("py") || key.EndsWith("pinyin"))
                        {
                            System.Console.Error.WriteLine("[DaGuiPlayerPrefsFix] Failed Clean for entry " + name);

                        }
                    }
                }


            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[DaGuiPlayerPrefsFix] Failed to run OnSaveData: " + ex.ToString());
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
                    System.Console.WriteLine("[DaGuiPlayerPrefsFix]  " + pSettingCount + " settings and " + pinyinCount + " pinyin/py");
                    return tempKeys.ToArray();
                }
                else
                {
                    return new string[0];  
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[DaGuiPlayerPrefsFix] Failed to run OnSaveData: " + ex.ToString());
                return new string[0];
            }
        }

        private static PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs[] GetNonPinyinPlayerPrefs()
        {
            string[] keys = GetNonPinyinPlayerPrefsKeys();
            List<PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs> tempRawPlayerPrefs = new List<PlayerPrefsHelper.PlayerPrefsHelper.RawPlayerPrefs> ();
            foreach (string key in keys)
            {
                tempRawPlayerPrefs.Add(PlayerPrefsHelper.PlayerPrefsHelper.GetRawPlayerPrefsFromReg(key));
            }
            return tempRawPlayerPrefs.ToArray();
        }
    }
}
