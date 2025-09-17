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

        private static string GetPlayerPrefsString(string raw_key)
        {
            int index = raw_key.LastIndexOf('_');
            return raw_key.Remove(index);
        }

        public RawPlayerPrefs GetRawPlayerPrefsFromReg(string reg_key)
        {
            string processed_key = GetPlayerPrefsString(reg_key);
            if (IsPlayerPrefsInt(processed_key))
            {
                return new RawPlayerPrefs { Key = processed_key, Value = PlayerPrefs.GetInt(processed_key) };
            }
            else if (IsPlayerPrefsFloat(processed_key))
            {
                return new RawPlayerPrefs { Key = processed_key, Value = PlayerPrefs.GetFloat(processed_key) };
            }
            else
            {
                return new RawPlayerPrefs { Key = processed_key, Value = PlayerPrefs.GetString(processed_key) };
            }
        }
    }
}
