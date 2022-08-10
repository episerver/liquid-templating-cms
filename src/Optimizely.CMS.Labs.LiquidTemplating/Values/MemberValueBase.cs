using Fluid;
using Fluid.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Optimizely.CMS.Labs.LiquidTemplating.Values
{
    public class MemberValueBase : ObjectValueBase
    {
        public Dictionary<string, VirtualMemberDelegate> MethodMap = new Dictionary<string, VirtualMemberDelegate>();
        public delegate FluidValue VirtualMemberDelegate(FunctionArguments arguments, TemplateContext context);

        public MemberValueBase() : base(new object())
        {
            // Add static methods
            foreach (var method in GetType().GetMethods()
                .Where(m => m.ReturnType == typeof(FluidValue) && m.IsStatic))
            {
                var dg = (VirtualMemberDelegate)Delegate.CreateDelegate(typeof(VirtualMemberDelegate), method);
                MethodMap[dg.Method.Name.ToLower()] = dg;
            }

            // And instance methods
            foreach (var method in GetType().GetMethods()
                .Where(m => m.ReturnType == typeof(FluidValue) && !m.IsStatic))
            {
                var dg = (VirtualMemberDelegate)Delegate.CreateDelegate(typeof(VirtualMemberDelegate), this, method.Name, false);
                MethodMap[dg.Method.Name.ToLower()] = dg;
            }
        }

        public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
        {
            var key = name.ToLower();
            if (MethodMap.ContainsKey(key))
            {
                return new FunctionValue(new Func<FunctionArguments, TemplateContext, FluidValue>(MethodMap[key]));
            }

            return NilValue.Instance;
        }
    }
}
