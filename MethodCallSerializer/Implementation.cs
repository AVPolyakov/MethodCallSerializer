using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MethodCallSerializer
{
    public static class Implementation
    {
        public static T Create<T>(IImplementation implementation) => Cache<T>.Func(implementation);

        private class Cache<T>
        {
            public static readonly Func<IImplementation, T> Func;

            static Cache()
            {
                var type0 = typeof(T);

                var typeBuilder = moduleBuilder.DefineType(
                    type0.Name + "_" + Guid.NewGuid(), TypeAttributes.NotPublic, null,
                    new[] {type0});

                var fieldBuilder = typeBuilder.DefineField(
                    ImplementationFieldName, typeof(IImplementation), FieldAttributes.Public);

                foreach (var methodInfo in type0.GetMethods())
                    CreateMethod(typeBuilder, fieldBuilder, methodInfo);

                var type = typeBuilder.CreateType();

                var dynamicMethod = new DynamicMethod(Guid.NewGuid().ToString(),
                    type, new[] {typeof (IImplementation)}, true);

                var ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.DeclareLocal(type);
                ilGenerator.Emit(OpCodes.Newobj, type.GetConstructors()[0]);
                ilGenerator.Emit(OpCodes.Stloc_0);
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Stfld, type.GetField(ImplementationFieldName));
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Ret);

                dynamicMethod.DefineParameter(1, ParameterAttributes.In, "arg1");

                Func = (Func<IImplementation, T>)dynamicMethod.CreateDelegate(typeof(Func<IImplementation, T>));
            }
        }

        private const string ImplementationFieldName = "ImplementationField";

        private static void CreateMethod(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, MethodInfo methodInfo)
        {
            const MethodAttributes getSetMethodAttributes =
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName;

            var parameterInfos = methodInfo.GetParameters();
            var getterMethodBuilder = typeBuilder.DefineMethod(methodInfo.Name,
                getSetMethodAttributes, methodInfo.ReturnType,
                parameterInfos.Select(_ => _.ParameterType).ToArray());

            var ilGenerator = getterMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ldstr, methodInfo.Name);
            EmitLength(ilGenerator, parameterInfos);
            ilGenerator.Emit(OpCodes.Newobj, constructorInfo);
            var index = 1;
            foreach (var parameterInfo in parameterInfos)
            {
                Ldarg(ilGenerator, index);
                ilGenerator.Emit(OpCodes.Callvirt, info.MakeGenericMethod(parameterInfo.ParameterType));
                index++;
            }
            ilGenerator.Emit(OpCodes.Callvirt,
                methodInfo.ReturnType != typeof(void)
                    ? info2.MakeGenericMethod(methodInfo.ReturnType)
                    : info3);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void Ldarg(ILGenerator ilGenerator, int index)
        {
            switch (index)
            {
                case 1:
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    ilGenerator.Emit(OpCodes.Ldarg_S, index);
                    break;
            }
        }

        private static void EmitLength(ILGenerator ilGenerator, ParameterInfo[] parameterInfos)
        {
            switch (parameterInfos.Length)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    ilGenerator.Emit(OpCodes.Ldc_I4_S, parameterInfos.Length);
                    break;
            }
        }

        private static readonly ConstructorInfo constructorInfo = typeof(Args).GetConstructors()[0];
        private static readonly MethodInfo info = typeof(Args).GetMethod(nameof(Args.Add));
        private static readonly MethodInfo info2 = typeof(IImplementation).GetMethod(nameof(IImplementation.Call));
        private static readonly MethodInfo info3 = typeof(IImplementation).GetMethod(nameof(IImplementation.CallVoid));

        private static readonly ModuleBuilder moduleBuilder;

        static Implementation()
        {
            var assemblyName = new AssemblyName {Name = "cae932c33f324144958b3201f81eb165"};
            moduleBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).
                DefineDynamicModule(assemblyName.Name);
        }
    }
    
    public interface IImplementation
    {
        T Call<T>(string methodName, Args args);
        void CallVoid(string methodName, Args args);
    }

    public class Args
    {
        public readonly object[] Value;
        private int index;

        public Args(int count)
        {
            Value = new object[count];
        }

        public Args Add<T>(T arg)
        {
            Value[index++] = arg;
            return this;
        }
    }
}
