﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RAspect
{
    /// <summary>
    /// Base Aspect for all aspect implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, Inherited = true)]
    public abstract class AspectBase : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AspectBase"/> class.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="modifier">Modifier</param>
        /// <param name="searchTypePattern">Search Type Pattern</param>
        /// <param name="searchMemberPattern">Search Member Pattern</param>
        internal AspectBase(WeaveTargetType target = WeaveTargetType.All, WeaveAccessModifier modifier = WeaveAccessModifier.All, string searchTypePattern = ".*", string searchMemberPattern = ".*")
        {
            Target = target;
            SearchTypePattern = searchTypePattern;
            SearchMemberPattern = searchMemberPattern;
        }

        /// <summary>
        /// Gets or sets Target for determining context that weaving is applied
        /// </summary>
        public WeaveTargetType Target { get; set; } = WeaveTargetType.All;

        /// <summary>
        /// Gets weave block type
        /// </summary>
        internal abstract WeaveBlockType BlockType { get; }

        /// <summary>
        /// Gets or sets Modifier for determining context that weaving is applied
        /// </summary>
        public WeaveAccessModifier Modifier { get; set; } = WeaveAccessModifier.All;

        /// <summary>
        /// Gets or sets Search Type Pattern for determine which assembly namespace/type will be weaved
        /// </summary>
        public string SearchTypePattern { get; set; } = ".*";

        /// <summary>
        /// Gets or sets Search Member Pattern for determine which type members will be weaved
        /// </summary>
        public string SearchMemberPattern { get; set; } = ".*";

        /// <summary>
        /// Determine if aspect should be excluded
        /// </summary>
        public bool Exclude { get; set; } = false;

        /// <summary>
        /// Execute validation rules of aspect on given type
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="methods">Type Methods</param>
        internal virtual void ValidateRules(Mono.Cecil.TypeDefinition type, IEnumerable<Mono.Cecil.MethodDefinition> methods)
        {
        }

        /// <summary>
        /// Gets or sets aspect code to inject at beginning of weaved method
        /// </summary>
        internal Action<Mono.Cecil.TypeDefinition, Mono.Cecil.MethodDefinition, Mono.Cecil.ParameterDefinition, Mono.Cecil.Cil.ILProcessor> OnBeginBlock { get; set; }

        /// <summary>
        /// Gets or sets aspect code to inject at end of weaved method
        /// </summary>
        internal Action<Mono.Cecil.TypeDefinition, Mono.Cecil.MethodDefinition, Mono.Cecil.ParameterDefinition, Mono.Cecil.Cil.ILProcessor> OnEndBlock { get; set; }
        
        /// <summary>
        /// Gets or sets aspect code to inject in weaved method for setting fields
        /// </summary>
        internal Action<Mono.Cecil.Cil.ILProcessor, Mono.Cecil.FieldDefinition> OnBlockSetField { get; set; }
        
        /// <summary>
        /// Gets or sets aspect code to inject in weaved method for getting fields
        /// </summary>
        internal Action<Mono.Cecil.Cil.ILProcessor, Mono.Cecil.FieldDefinition> OnBlockGetField { get; set; }
        
        /// <summary>
        /// Gets or sets aspect code to inject in weaved method for invoking events/delegates
        /// </summary>
        internal Action<Mono.Cecil.Cil.ILProcessor, Mono.Cecil.MethodDefinition, Mono.Cecil.FieldDefinition> OnBlockInvokeEvent { get; set; }
        
        /// <summary>
        /// Gets or sets aspect code to use for substitution of method calls in a given aspect code
        /// </summary>
        internal Func<Mono.Cecil.TypeDefinition, Mono.Cecil.Cil.ILProcessor, Mono.Cecil.MethodDefinition, Mono.Cecil.MethodDefinition, bool> OnMethodCall { get; set; }

        /// <summary>
        /// Method that will be called prior to execute of weaved methods
        /// </summary>
        /// <param name="context">MethodContext</param>
        [EntryPointAttribute(EntryPointType.Enter)]
        internal virtual void OnEntry(MethodContext context) { }

        /// <summary>
        /// Method that will be called prior to exiting of weaved methods
        /// </summary>
        /// <param name="context">MethodContext</param>
        [EntryPointAttribute(EntryPointType.Exit)]
        internal virtual void OnExit(MethodContext context)
        {
        }

        /// <summary>
        /// Method that will be called after success of weaved methods
        /// </summary>
        /// <param name="context">MethodContext</param>
        [EntryPointAttribute(EntryPointType.Success)]
        internal virtual void OnSuccess(MethodContext context)
        {
        }

        /// <summary>
        /// Method that will be called upon exception
        /// </summary>
        /// <param name="context">MethodContext</param>
        /// <param name="ex">Exception that occurred while executing weaved method</param>
        [EntryPointAttribute(EntryPointType.Error)]
        internal virtual void OnException(MethodContext context, Exception ex)
        {
        }

        /// <summary>
        /// Get Hash code
        /// </summary>
        /// <returns>Int</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        /// <summary>
        /// Equality check
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Bool</returns>
        public override bool Equals(object obj)
        {
            return GetType().FullName == obj.GetType().FullName;
        }
    }
}
