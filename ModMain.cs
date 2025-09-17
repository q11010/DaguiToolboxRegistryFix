using System;
using System.Reflection;
using UnityEngine;

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
            RemovePinyinEntries();
            System.Console.WriteLine("[DaGuiPlayerPrefsFix] Cleaner executed on destroy");
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
                    foreach (string name in valueName)
                    {
                        string[] substrings = name.Split('_');
                        foreach (string substring in substrings)
                        {
                            if (substring == "pinyin" || substring == "py")
                            {
                                System.Console.WriteLine("[DaGuiPlayerPrefsFix] " + name + " cleaned");
                                PlayerPrefs.DeleteKey(name);
                                break;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("[DaGuiPlayerPrefsFix] Failed to run OnSaveData: " + ex.ToString());
            }
        }
    }
}
