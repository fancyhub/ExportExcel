using System;
using System.Collections.Generic;

namespace Test
{
    public static class LocLang
    {
        public const string C_DEFAULT = "";
        private static string _Lang = C_DEFAULT;
        private static bool _ShowKey = false;

        public static Action EvtLangChange0; //当Lang 发生变化的时候,优先触发
        public static Action EvtShowKeyChange0; //当 ShowKey 发生变化的时候,优先触发
        public static event Action OnLangChange;

        public static bool ShowKey
        {
            get
            {
                return _ShowKey;
            }
            set
            {
                if (_ShowKey == value)
                    return;
                _ShowKey = value;
                EvtShowKeyChange0?.Invoke();
                OnLangChange?.Invoke();
            }
        }

        public static string Lang
        {
            get
            {
                return _Lang;
            }
            set
            {
                if (_Lang == value)
                    return;
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                    return;

                _Lang = value;
                EvtLangChange0?.Invoke();
                OnLangChange?.Invoke();
            }
        }
    }
}
