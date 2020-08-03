namespace ETModel
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    public class LocalizationManager : MonoSingleton<LocalizationManager>
    {
        [Header("Localization Language")]
        [SerializeField]
        private SystemLanguage m_selectedLanguage;

        private SystemLanguage m_lastSelectedLanguage;

        private SystemLanguage m_defaultLanguage;

        private List<int> m_supportLanguages = new List<int>();

        private Dictionary<int, Dictionary<int, string>> m_languageCache = new Dictionary<int, Dictionary<int, string>>();

        private int m_currentLanguage = -1;  //当前选中的语言

        void Awake()
        {
            m_defaultLanguage = SystemLanguage.English;
            m_lastSelectedLanguage = m_selectedLanguage = m_defaultLanguage;
            //如果当前选中

            // EventCenter.Register<int>(EventID.LanguageChanged, OnLanguageChanged);
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            Debug.Log("OnValidate in LocalizationManager");

            if (m_selectedLanguage != m_lastSelectedLanguage)
            {
                m_lastSelectedLanguage = m_selectedLanguage;
                SetLanguage(m_selectedLanguage);
            }
        }

#endif

        public void AddLanguageConfig(int language, int key, string value)
        {
            AddLanguageToSupport(language);
            Dictionary<int, string> dict = GetLanguageDict(language);
            if (dict.ContainsKey(key))
            {
                Log.Error("language config error: key = {0} repeat.", key);
            }
            else
            {
                dict.Add(key, value);
            }
        }

        private void AddLanguageToSupport(int language)
        {
            if (!m_supportLanguages.Contains(language))
            {
                m_supportLanguages.Add(language);
            }
        }

        public void Initialize()
        {
            int defaultId = (int)m_defaultLanguage;
            if (m_supportLanguages.Contains(defaultId))
            {
                m_currentLanguage = defaultId;
            }
            else
            {
                m_currentLanguage = (int)SystemLanguage.English;
            }
        }

        public void SetLanguage(int language)
        {
            Debug.Log("ChangeLanguage language = " + language);
            if (!m_supportLanguages.Contains(language))
            {
                Log.Error(" language = {0} not support!", Enum.GetName(typeof(SystemLanguage), language));
                return;
            }
            m_currentLanguage = language;
            EventCenter.Dispatch(EventID.LanguageChanged);
        }

        public void SetLanguage(SystemLanguage language)
        {
            Debug.Log("ChangeLanguage language = " + language);
            if (!m_supportLanguages.Contains((int)language))
            {
                Log.Error(" language = {0} not support!", Enum.GetName(typeof(SystemLanguage), language));
                return;
            }
            m_defaultLanguage = language;
            m_currentLanguage = (int)m_defaultLanguage;
            EventCenter.Dispatch(EventID.LanguageChanged);
        }

        private Dictionary<int, string> GetLanguageDict(int language)
        {
            Dictionary<int, string> dict = null;
            m_languageCache.TryGetValue(language, out dict);
            if (dict == null)
            {
                dict = new Dictionary<int, string>();
                m_languageCache.Add(language, dict);
            }
            return dict;
        }

        public string GetLanguage(int key)
        {
            Dictionary<int, string> dict = GetLanguageDict(m_currentLanguage);
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            Log.Error("key = {0} has not config in Localization. please check",key);
            return string.Empty;
        }

    }
}
