﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RAspect.Patterns
{
    /// <summary>
    /// Attribute when applied to method or properties, swallows thrown exception
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly)]
    public class SwallowExceptionAttribute : AspectBase
    {
        /// <summary>
        /// Local Builder for return value
        /// </summary>
        [ThreadStatic]
        private static LocalBuilder exLocal;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwallowExceptionAttribute"/> class.
        /// </summary>
        public SwallowExceptionAttribute() : base(WeaveTargetType.Methods | WeaveTargetType.Constructors)
        {
            OnBeginAspectBlock = BeginAspectBlock;
            OnEndAspectBlock = EndAspectBlock;
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
        /// Aspect code to inject at the beginning of weaved method
        /// </summary>
        /// <param name="typeBuilder">Type Builder</param>
        /// <param name="method">Method</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="il">ILGenerator</param>
        internal void BeginAspectBlock(TypeBuilder typeBuilder, MethodBase method, ParameterInfo parameter, ILGenerator il)
        {
            var returnType = method.IsConstructor ? typeof(void) : (method as MethodInfo).ReturnType;
            exLocal = returnType != typeof(void) ? il.DeclareLocal(returnType) : null;

            il.BeginExceptionBlock();
        }

        /// <summary>
        /// Aspect code to inject at the end of weaved method
        /// </summary>
        /// <param name="typeBuilder">Type Builder</param>
        /// <param name="method">Method</param>
        /// <param name="parameter">Parameter</param>
        /// <param name="il">ILGenerator</param>
        internal void EndAspectBlock(TypeBuilder typeBuilder, MethodBase method, ParameterInfo parameter, ILGenerator il)
        {
            if(exLocal != null)
                il.Emit(OpCodes.Stloc, exLocal);
            
            il.BeginCatchBlock(typeof(System.Exception));
            il.EndExceptionBlock();

            if (exLocal != null)
                il.Emit(OpCodes.Ldloc, exLocal);
        }
    }
}
