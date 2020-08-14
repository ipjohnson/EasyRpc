﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace EasyRpc.AspNetCore.CodeGeneration
{
    /// <summary>
    /// IL Generation utilities 
    /// </summary>
    public static class ILGenerationUtilities
    {
        /// <summary>
        /// Emit il for loading arg
        /// </summary>
        /// <param name="ilGenerator"></param>
        /// <param name="parameterIndex"></param>
        public static void EmitLoadArg(this ILGenerator ilGenerator, int parameterIndex)
        {
            switch (parameterIndex)
            {
                case 0:
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    break;
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
                    ilGenerator.Emit(OpCodes.Ldarg_S, (byte)(parameterIndex));
                    break;
            }
        }

        /// <summary>
        /// Emit method call 
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="method"></param>
        public static void EmitMethodCall(this ILGenerator generator, MethodInfo method)
        {
            generator.Emit(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method);
        }

        /// <summary>
        /// Emit int value
        /// </summary>
        /// <param name="generator">generator</param>
        /// <param name="intValue">value to emit</param>
        public static void EmitInt(this ILGenerator generator, int intValue)
        {
            switch (intValue)
            {
                case 0:
                    generator.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    generator.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    generator.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    generator.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    generator.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    generator.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    generator.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    generator.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    generator.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    generator.Emit(OpCodes.Ldc_I4, intValue);
                    break;
            }
        }

        public static void EmitBoolean(this ILGenerator generator, bool boolValue)
        {
            generator.Emit(boolValue ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }
    }
}
