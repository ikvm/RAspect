﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RAspect
{
    /// <summary>
    /// Encapsulate method information.
    /// </summary>
    public sealed class MethodContext
    {
        /// <summary>
        /// Argument values
        /// </summary>
        private object[] argumentValues;

        /// <summary>
        /// Arguments
        /// </summary>
        private MethodParameterContext[] arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodContext"/> class.
        /// </summary>
        /// <param name="arguments">Arguments</param>
        /// <param name="argumentValues">Argument Values</param>
        public MethodContext(MethodParameterContext[] arguments, object[] argumentValues)
        {
            this.arguments = arguments;
            this.argumentValues = argumentValues;
        }

        /// <summary>
        /// Gets aspect method arguments
        /// </summary>
        public IEnumerable<MethodParameterContext> Arguments
        {
            get
            {
                if (arguments == null)
                {
                    yield break;
                }

                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = arguments[i];
                    yield return new MethodParameterContext(argument.Name, argument.IsRef) { Value = argumentValues[i] };
                }
            }
        }

        /// <summary>
        /// Gets or sets Method
        /// </summary>
        public MethodBase Method { get; set; }

        /// <summary>
        /// Gets or sets Method Attributes
        /// </summary>
        public List<Attribute> Attributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether flag to indicate continuation of code execution
        /// </summary>
        public bool Proceed { get; set; } = true;

        /// <summary>
        /// Gets or sets return value of weaved method to be used after execution is completed
        /// </summary>
        public object Returns { get; set; }

        /// <summary>
        /// Gets or sets instance of class weaved method was invoked on
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// Gets or sets token to be used by weaved method for passing context between various entry points
        /// </summary>
        public object Token { get; set; }

        /// <summary>
        /// Gets or sets exception for the current method context
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Set values for arguments
        /// </summary>
        /// <param name="values">Values</param>
        public void SetValues(object[] values)
        {
            this.argumentValues = values;
        }

        /// <summary>
        /// Set arguments
        /// </summary>
        /// <param name="arguments">Arguments</param>
        public void SetArguments(MethodParameterContext[] arguments)
        {
            this.arguments = arguments;
        }
    }
}
