﻿using System;
using System.Reflection;

namespace DataGate.Utils
{
    public class VariableInfo
    {
        public MemberInfo MemberInfo;

        public Type VariableType
        {
            get
            {
                switch (MemberInfo.MemberType)
                {
                    case MemberTypes.Field:
                        return ((FieldInfo) MemberInfo).FieldType;
                    case MemberTypes.Property:
                        return ((PropertyInfo) MemberInfo).PropertyType;
                }

                return null;
            }
        }

        public object this[object obj]
        {
            get => Get(obj);
            set => Set(obj, value);
        }

        public VariableInfo(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }

        public object Get(object obj)
        {
            switch (MemberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo) MemberInfo).GetValue(obj);
                case MemberTypes.Property:
                    return ((PropertyInfo) MemberInfo).GetValue(obj);
            }

            return null;
        }
        
        public void Set(object obj, object value)
        {
            switch (MemberInfo.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo) MemberInfo).SetValue(obj, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo) MemberInfo).SetValue(obj, value);
                    break;
            }
        }

        public static explicit operator MemberInfo(VariableInfo variableInfo)
            => variableInfo.MemberInfo;
    }
}