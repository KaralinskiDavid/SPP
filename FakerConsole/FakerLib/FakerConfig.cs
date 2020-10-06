using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace FakerLib
{
    public class FakerConfig
    {
        internal Dictionary<(Type ClassT, MemberInfo member), Type> configOptions = new Dictionary<(Type ClassT, MemberInfo Member), Type>();

        public void Add<TClass, TMember, TDtoGenerator>(Expression<Func<TClass,TMember>> expr)
        {
            MemberExpression rule = (MemberExpression)expr.Body;
            MemberInfo member = rule.Member;

            var key = (typeof(TClass), member);
            if (!configOptions.ContainsKey(key)) configOptions[key] = typeof(TDtoGenerator);
        }
    }
}
