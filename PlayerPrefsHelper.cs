using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MOD_WIFdSk.PlayerPrefsHelper
{
    public class PlayerPrefsHelper
    {

        public struct RawPlayerPrefs
        {
            public string Key { get; set; }
            public object Value { get; set; }
        }

        private static bool IsPlayerPrefsInt(string key)
        {
            return !(PlayerPrefs.GetInt(key, 0) == 0 && PlayerPrefs.GetInt(key, 1) == 1); 
        }

        private static bool IsPlayerPrefsFloat(string key)
        {
            return !(PlayerPrefs.GetFloat(key, 0.0f) == 0.0f && PlayerPrefs.GetFloat(key, 0.1f) == 0.1f);
        }

        private static bool IsPlayerPrefsString(string key) { 
            return !(PlayerPrefs.GetString(key, "") == "" && PlayerPrefs.GetString(key,"0") == "0");
        }

        public static string CleanPlayerPrefsKey(string raw_key)
        {
            int index = raw_key.LastIndexOf('_');
            return raw_key.Remove(index);
        }

        public static RawPlayerPrefs GetRawPlayerPrefsFromReg(string processed_key)
        {
            if (IsPlayerPrefsInt(processed_key))
            {
                return new RawPlayerPrefs { Key = processed_key, Value = PlayerPrefs.GetInt(processed_key) };
            }
            else if (IsPlayerPrefsFloat(processed_key))
            {
                return new RawPlayerPrefs { Key = processed_key, Value = PlayerPrefs.GetFloat(processed_key) };
            }
            else if (IsPlayerPrefsString(processed_key))
            {
                return new RawPlayerPrefs { Key = processed_key, Value = PlayerPrefs.GetString(processed_key) };
            }
            else
            {
                System.Console.Error.WriteLine("[DaGuiPlayerPrefsFix] No valid type for: " + processed_key);
                return new RawPlayerPrefs { Key = processed_key, Value = null };
            }

        }

        public static void SetPlayerPrefs(RawPlayerPrefs raw_player_prefs)
        {
            if (raw_player_prefs.Value.GetType() == typeof(int))
            {
                PlayerPrefs.SetInt(raw_player_prefs.Key, (int)raw_player_prefs.Value);
            } else if (raw_player_prefs.Value.GetType() == typeof (float))
            {
                PlayerPrefs.SetFloat(raw_player_prefs.Key, (float)raw_player_prefs.Value);
            } else if (raw_player_prefs.Value.GetType() == typeof(string))
            {
                PlayerPrefs.SetString(raw_player_prefs.Key, (string)raw_player_prefs.Value);
            } else
            {
                System.Console.Error.WriteLine("[DaGuiPlayerPrefsFix] PlayerPrefs " + raw_player_prefs.Key + "has no valid key");
            }
        }
    }
}
