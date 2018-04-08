using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

enum GOCType
{
        GOC_None = 0,
        GOC_BasicData,
        GOC_Items
}

class GOComponent
{
        protected int m_ComponentID;
        public int ComponentID
        {
                get { return m_ComponentID; }
                set { m_ComponentID = value; }
        }

        public GOComponent()
        {
        }


        public static void SetPropertyValue(System.Object instance, FieldInfo prop, string value)
        {
                // 解析枚举类型
                if (prop.FieldType.IsEnum)
                {
                        prop.SetValue(instance, Enum.Parse(prop.FieldType, value));
                }
                // 解析自定义类型(如: ID)
                else if (Type.GetTypeCode(prop.FieldType) == TypeCode.Object)
                {
                        object propObj = Activator.CreateInstance(prop.FieldType, new object[] { value });
                        prop.SetValue(instance, propObj);

                }
                // 解析系统类型(如: int, float, string...)
                else
                {
                        prop.SetValue(instance, Convert.ChangeType(value, prop.FieldType));
                }
        }

        public static void SetPropertyValue(System.Object instance, PropertyInfo prop, string value)
        {
                // 解析枚举类型
                if (prop.PropertyType.IsEnum)
                {
                        prop.SetValue(instance, Enum.Parse(prop.PropertyType, value), null);
                }
                // 解析自定义类型(如: ID)
                else if (Type.GetTypeCode(prop.PropertyType) == TypeCode.Object)
                {
                        object propObj = Activator.CreateInstance(prop.PropertyType, new object[] { value });
                        prop.SetValue(instance, propObj, null);

                }
                // 解析系统类型(如: int, float, string...)
                else
                {
                        prop.SetValue(instance, Convert.ChangeType(value, prop.PropertyType), null);
                }
        }
}
