﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RAspect.Aspects
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Assembly)]
    public abstract class ContractAspect : AspectBase
    {
        /// <summary>
        /// Validate Contract Method
        /// </summary>
        private readonly static MethodInfo ValidateContractMethod = typeof(ContractAspect).GetMethod("Validate", ILWeaver.NonPublicBinding);

        /// <summary>
        /// Get Parameter Contract Aspect Method
        /// </summary>
        private readonly static MethodInfo GetParameterContractAspectMethod = typeof(ContractAspect).GetMethod("GetParameterContractAspect", ILWeaver.NonPublicBinding);

        /// <summary>
        /// Collection for tracking cached aspect for parameters
        /// </summary>
        private readonly static ConcurrentDictionary<string, ContractAspect> ContractAspects = new ConcurrentDictionary<string, ContractAspect>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractAspect"/> class.
        /// </summary>
        public ContractAspect() : base(WeaveTargetType.Parameters | WeaveTargetType.Properties)
        {
            OnBeginBlock = BeginBlock;
        }

        /// <summary>
        /// Invoke before setting property value
        /// </summary>
        /// <param name="context"></param>
        internal override void OnEntry(MethodContext context)
        {
            var argument = context.Arguments.FirstOrDefault();
            if (argument == null)
                return;
            var aspectType = GetType();
            var value = argument.Value;
            var ex = ValidateContract(value, "value", false, context.Attributes.FirstOrDefault(x => x.GetType() == aspectType) as ContractAspect);
            if (ex != null)
                throw ex;
        }

        /// <summary>
        /// Validate value against contract implementation
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="name">Name</param>
        /// <param name="isParameter">Flag indicating if value is from a parameter</param>
        /// <param name="attr">Attribute</param>
        /// <returns>Exception</returns>
        protected abstract Exception ValidateContract(object value, string name, bool isParameter, ContractAspect attr);

        /// <summary>
        /// Validate value against contract implementation
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="name">Name</param>
        /// <param name="isParameter">Flag indicating if value is from a parameter</param>
        /// <param name="contract">Contract</param>
        /// <returns>Exception</returns>
        public Exception Validate(object value, string name, bool isParameter, ContractAspect contract)
        {
            return ValidateContract(value, name, isParameter, contract);
        }

        /// <summary>
        /// Gets weave block type
        /// </summary>
        internal override WeaveBlockType BlockType
        {
            get
            {
                return WeaveBlockType.Inline;
            }
        }

        /// <summary>
        /// Get parameter contract aspect
        /// </summary>
        /// <param name="method">Method</param>
        /// <param name="parameterOffset">Parameter Offset</param>
        /// <returns>ContractAspect</returns>
        public static ContractAspect GetParameterContractAspect(MethodInfo method, int parameterOffset)
        {
            var key = string.Concat(method.DeclaringType.FullName, method.Name, parameterOffset);
            ContractAspect contractAspect = null;

            if(!ContractAspects.TryGetValue(key, out contractAspect))
            {
                contractAspect = ContractAspects[key] = method.GetParameters()[parameterOffset].GetCustomAttribute<ContractAspect>();
            }

            return contractAspect;
        }

        /// <summary>
        /// Aspect code to inject at the beginning of weaved method
        /// </summary>
        /// <param name="typeBuilder">Type Builder</param>
        /// <param name="method">Method</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="il">ILGenerator</param>
        internal void BeginBlock(Mono.Cecil.TypeDefinition typeBuilder, Mono.Cecil.MethodDefinition method, Mono.Cecil.ParameterDefinition parameter, Mono.Cecil.Cil.ILProcessor il)
        {
            var aspectType = this.GetType();
            var hasExLabel = il.DefineLabel();
            var exceptionLocal = il.DeclareLocal(typeof(Exception));
            var offset = method.IsStatic ? 0 : 1;

            var paraMethod = method;
            var parameterDeclaringType = paraMethod.DeclaringType;
            var aspect = parameter.GetCustomAttribute(aspectType);

            if (aspect == null)
                return;

            var aspectField = ILWeaver.TypeFieldAspects[parameterDeclaringType.FullName][aspectType.FullName];

            il.Emit(Mono.Cecil.Cil.OpCodes.Ldsfld, aspectField);
            il.Emit(Mono.Cecil.Cil.OpCodes.Ldarg, parameter.Index + offset);
            il.Emit(Mono.Cecil.Cil.OpCodes.Ldstr, parameter.Name);
            il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4_1);

            il.Emit(Mono.Cecil.Cil.OpCodes.Ldtoken, paraMethod);
            il.Emit(Mono.Cecil.Cil.OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new Type[] { typeof(RuntimeMethodHandle) }));
            il.Emit(Mono.Cecil.Cil.OpCodes.Isinst, typeof(MethodInfo));
            il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, parameter.Index);
            il.Emit(Mono.Cecil.Cil.OpCodes.Call, GetParameterContractAspectMethod);

            il.Emit(Mono.Cecil.Cil.OpCodes.Callvirt, ValidateContractMethod);
            il.Emit(Mono.Cecil.Cil.OpCodes.Stloc, exceptionLocal);

            il.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, exceptionLocal);
            il.Emit(Mono.Cecil.Cil.OpCodes.Brfalse, hasExLabel);

            il.Emit(Mono.Cecil.Cil.OpCodes.Ldloc, exceptionLocal);
            il.Emit(Mono.Cecil.Cil.OpCodes.Throw);

            il.MarkLabel(hasExLabel);
        }
    }
}
