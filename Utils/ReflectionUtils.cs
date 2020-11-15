﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataGate.Utils
{
    public static class ReflectionUtils
    {
        public static VariableInfo ToVariableInfo(this MemberInfo memberInfo)
            => new VariableInfo(memberInfo);

        public static IEnumerable<VariableInfo> GetVariables(this Type type, bool hasPublicSet = true, bool hasPublicGet = true)
        {
            foreach (var f in type.GetFields().Where(f => f.IsPublic || !hasPublicGet || !hasPublicSet))
            {
                yield return new VariableInfo(f);
            }

            foreach (var p in type.GetProperties().Where(info => (!hasPublicSet || info.GetSetMethod() != null) && (!hasPublicGet || info.GetGetMethod() != null)))
            {
                yield return new VariableInfo(p);
            }
        }
    }
}