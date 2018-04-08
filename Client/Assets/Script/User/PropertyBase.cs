using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void PropertyUpdateHandle(string name, string oldValue, string newValue);

public class PropertyBase
{
        protected PropertyBase()
        {
        }
        protected Dictionary<string, string> m_PropertyDict = new Dictionary<string, string>();
        public Dictionary<string, PropertyUpdateHandle> m_PropertyHandlers = new Dictionary<string, PropertyUpdateHandle>();

        protected long m_ID;
        public long ID
        {
                get
                {
                        return m_ID;
                }
        }

        protected int m_nLevel;
        public int Level
        {
                get
                {
                        return m_nLevel;
                }
        }
        
        public virtual int GetPropertyInt(string name)
        {
                string property = GetPropertyString(name);
                int value = 0;
                int.TryParse(property, out value);
                return value;
        }

        public virtual long GetPropertyLong(string name)
        {
                string property = GetPropertyString(name);
                long value = 0;
                long.TryParse(property, out value);
                return value;
        }

        public virtual string GetPropertyValue(string propName)
        {
                return GetPropertyString(propName);
        }

        public virtual string GetPropertyString(string name)
        {
                if (m_PropertyDict.ContainsKey(name))
                {
                        return m_PropertyDict[name];
                }
                return "";
        }

        public virtual void SetProperty(string name, string value)
        {
                if (!m_PropertyDict.ContainsKey(name))
                {
                        m_PropertyDict.Add(name, value);
                }
                else
                {
                        m_PropertyDict[name] = value;
                }
        }


        public virtual void SetProperty(string name, int value)
        {
                SetProperty(name, value.ToString());
        }

        public virtual void SetProperty(string name, long value)
        {
                SetProperty(name, value.ToString());
        }

        public virtual void OnUpdateProperty(string name, string value)
        {
                string oldValue = CommonString.zeroStr;
                if (!m_PropertyDict.ContainsKey(name))
                {
                        m_PropertyDict.Add(name, value);
                }
                else
                {
                        oldValue = m_PropertyDict[name];
                        m_PropertyDict[name] = value;
                }

                try
                {
                        if (m_PropertyHandlers.ContainsKey(name))
                        {
                                m_PropertyHandlers[name](name, oldValue, value);
                        }
                }
                catch (System.Exception ex)
                {
                        Debuger.LogError(ex.ToString());
                }
        }


        public virtual void OnUpdateProperty(string name, int value)
        {
                OnUpdateProperty(name, value.ToString());
        }

        public virtual void OnUpdateProperty(string name, long value)
        {
                OnUpdateProperty(name, value.ToString());
        }
}
