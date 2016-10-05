using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace ToyMQ.Proxy {
    public class Proxifier {
        public static object OnProxifiedCall(object[] args) {
            var self = args[0];
            var proxyHandlerField = self.GetType().GetField("__handler");
            var proxyHandler = (ProxyHandler)proxyHandlerField.GetValue(self);
            var calleeFrame = new StackFrame(1, false);
            var calleeMethod = calleeFrame.GetMethod();
            return proxyHandler.HandleCall(calleeMethod, args);
        }

        private static TypeBuilder CreateTypeBuilder(Type targetType) {
            var assName = new AssemblyName("ToyMQ.Proxyfied");
            var assBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assBuilder.DefineDynamicModule(assName.Name, assName.Name + ".dll");
            var typeBuilder = moduleBuilder.DefineType("ToyMQ.Proxyfied." + targetType.FullName,
                                                       TypeAttributes.Class | TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(targetType);
            return typeBuilder;
        }

        private static void BuildMethod(TypeBuilder proxyBuilder, MethodInfo targetMethod) {
            var parameters = targetMethod.GetParameters();
            var types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i) types[i] = parameters[i].ParameterType;

            var proxyMethod = proxyBuilder.DefineMethod(
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
            if (targetMethod.ReturnType.IsValueType || targetMethod.ReturnType.IsGenericParameter) {
                proxyIL.Emit(OpCodes.Unbox_Any, targetMethod.ReturnType);
            }
            proxyIL.Emit(OpCodes.Ret);
        }

        private static void BuildMethods(TypeBuilder builder, Type targetType) {
            foreach (var targetMethod in targetType.GetMethods()) {
                if (!targetMethod.IsPublic) continue;
                BuildMethod(builder, targetMethod);
            }
        }

        private static void BuildConstructor(TypeBuilder proxyBuilder) {
            var handler = proxyBuilder.DefineField("__handler", typeof(ProxyHandler), FieldAttributes.Public);
            var constructor = proxyBuilder.DefineConstructor(
                MethodAttributes.Public, CallingConventions.Standard,
                new Type[] { typeof(ProxyHandler) });
            var generator = constructor.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, handler);
            generator.Emit(OpCodes.Ret);
        }

        public Proxifier(Type type) {
            var builder = CreateTypeBuilder(type);
            BuildConstructor(builder);
            BuildMethods(builder, type);
            proxy_ = builder.CreateType();
        }

        private Type proxy_;

        public object ProxyTo(ProxyHandler handler) {
            return Activator.CreateInstance(proxy_, new object[] {handler});
        }
    }
}
