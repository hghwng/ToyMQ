using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ToyMQ.Proxy {
    public class Proxifier {
        private Type targetType_;
        private TypeBuilder proxyBuilder_;
        private AssemblyBuilder assBuilder_;

        public static object OnProxifiedCall(object[] args) {
            Console.WriteLine("Call: " + args.Length);
            return null;
        }

        private void CreateTypeBuilder() {
            var assName = new AssemblyName("ToyMQ.Proxyfied");
            assBuilder_ = AppDomain.CurrentDomain.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assBuilder_.DefineDynamicModule(assName.Name, assName.Name + ".dll");
            var typeBuilder = moduleBuilder.DefineType("ToyMQ.Proxyfied." + targetType_.FullName,
                                                       TypeAttributes.Class | TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(targetType_);
            proxyBuilder_ = typeBuilder;
        }

        private void BuildProxyForMethod(MethodInfo targetMethod) {
            var parameters = targetMethod.GetParameters();
            var types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i) types[i] = parameters[i].ParameterType;

            var proxyMethod = proxyBuilder_.DefineMethod(
                targetMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                targetMethod.ReturnType, types);
            var proxyIL = proxyMethod.GetILGenerator();
            var localArgs = proxyIL.DeclareLocal(typeof(Object[]));
            var localResult = proxyIL.DeclareLocal(typeof(Object));

            // Create new Object[] {arg0, arg1, ...}
            // Non static method: arg0 = this
            proxyIL.Emit(OpCodes.Ldc_I4, parameters.Length + 1);
            proxyIL.Emit(OpCodes.Newarr, typeof(Object));
            proxyIL.Emit(OpCodes.Stloc, localArgs);

            // Set args[0] = this
            proxyIL.Emit(OpCodes.Ldloc, localArgs);
            proxyIL.Emit(OpCodes.Ldc_I4_0);
            proxyIL.Emit(OpCodes.Ldarg_0);
            proxyIL.Emit(OpCodes.Stelem_Ref);

            // Other arguments
            for (int i = 1; i <= parameters.Length; ++i) {
                var param = parameters[i - 1];
                if (param.ParameterType.IsValueType || param.ParameterType.IsGenericParameter) {
                    proxyIL.Emit(OpCodes.Box, param.ParameterType);
                }

                proxyIL.Emit(OpCodes.Ldloc, localArgs);
                proxyIL.Emit(OpCodes.Ldc_I4, i);
                proxyIL.Emit(OpCodes.Ldarg, i);
                proxyIL.Emit(OpCodes.Stelem_Ref);
            }

            // Call handler(args)
            proxyIL.Emit(OpCodes.Ldloc, localArgs);
            var proxyProcesserMethod = typeof(Proxifier).GetMethod("OnProxifiedCall");
            proxyIL.Emit(OpCodes.Call, proxyProcesserMethod);
            proxyIL.Emit(OpCodes.Stloc, localResult);
            proxyIL.Emit(OpCodes.Ldloc, localResult);
            proxyIL.Emit(OpCodes.Ret);
        }

        private void BuildProxyForType() {
            foreach (var targetMethod in targetType_.GetMethods()) {
                if (!targetMethod.IsPublic) continue;
                BuildProxyForMethod(targetMethod);
            }
        }

        private void BuildProxyForConstructor() {
            var handler = proxyBuilder_.DefineField("__handler", typeof(ProxyHandler), FieldAttributes.Private);
            var constructor = proxyBuilder_.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { typeof(ProxyHandler) });
            var generator = constructor.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, handler);
            generator.Emit(OpCodes.Ret);
        }

        public Proxifier(Type type) {
            targetType_ = type;
            CreateTypeBuilder();
            BuildProxyForConstructor();
            BuildProxyForType();
        }

        public object GetObject(ProxyHandler handler) {
            Type proxy = this.proxyBuilder_.CreateType();
            return Activator.CreateInstance(proxy, new object[] {handler});
        }
    }
}
