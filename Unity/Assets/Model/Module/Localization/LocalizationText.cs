using System;
using UnityEngine;
using UnityEngine.UI;

namespace ETModel
{
    [RequireComponent(typeof(Text))]
    public class LocalizationText : MonoBehaviour
    {
        [Header("Localization Key")]
        [SerializeField]
        private int m_key;

        private int m_lastKey;

        private Text m_text;

        public int Key
        {
            get { return m_key; }
            set
            {
                if (m_key != value)
                {
                    m_key = value;
                    UpdateDisplay();
                }
            }
        }

        void Awake()
        {
            EventCenter.Register(EventID.LanguageChanged, OnLanguageChanged);
        }

        void Start()
        {
            m_lastKey = m_key;

            m_text = this.GetComponent<Text>();
            m_text.text = LocalizationManager.Instance.GetLanguage(m_key);
        }

        void OnDestroy()
        {
            EventCenter.UnRegister(EventID.LanguageChanged, OnLanguageChanged);
            Log.Debug("LocalizationText Destory. key = {0}", m_key);
        }

        private void OnLanguageChanged()
        {
            m_text.text = LocalizationManager.Instance.GetLanguage(m_key);
        }

        private void UpdateDisplay()
        {
            m_text.text = LocalizationManager.Instance.GetLanguage(m_key);
        }
    }
}