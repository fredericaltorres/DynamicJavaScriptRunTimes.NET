using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace DynamicJavaScriptRunTimes {
   
    /// <summary>
    /// Each JavaScript run-time supported must implement this 
    /// interface
    /// </summary>
    public interface IDynamicJavaScriptContext {

        /// <summary>
        /// The run time implementation name
        /// </summary>
        SUPPORTED_JAVASCRIPT_RUNTIME Runtime {get;}
        /// <summary>
        /// Set a global variable in the JavaScript context
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetParameter(string name, object value);
        /// <summary>
        /// Get a global variable in the JavaScript context
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        object GetParameter(string name);
        /// <summary>
        /// Execute the script and return the last evaluated expression
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        object Run(string script);
        /// <summary>
        /// Helper to make an object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        object Object(object o);
        /// <summary>
        /// Helper to make an array
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        object Array(object o);
        /// <summary>
        /// Helper to make a date
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        object Date(object o);
        /// <summary>
        /// Return true if o is a JavaScript object in the JavaScript sens
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool IsJavaScriptObject(object o);
        /// <summary>
        /// Return true if o is a JavaScript array
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool IsJavaScriptArray(object o);
        /// <summary>
        /// Return true if o is a JavaScript date
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool IsJavaScriptDate(object o);
        /// <summary>
        /// Convert o which is a JavaScript object of the current JavaScript
        /// run-time into a .NET IDictionary[string, object] and return it
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        IDictionary<string, object> JavaScriptObjectToNETDictionary(object o);        
        /// <summary>
        /// Convert o which is a JavaScript array of the current JavaScript
        /// run-time into a .NET array of object and return  it
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        object[] JavaScriptArrayToNETArray(object o);
        /// <summary>
        /// Convert o which is a JavaScript date of the current JavaScript
        /// run-time into a .NET DateTime and return  it        
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        DateTime JavaScriptDateToNETDateTime(object o);
        /// <summary>
        /// Return o which is a Javascript string, boolean or number into a .NET
        /// equivalant.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        object JavaScriptValueTypeToNETValueType(object o);
        /// <summary>
        /// Call a JavaScript function. The function code must be previously loaded with the function
        /// Run()
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object Call(string functionName, params object[] parameters);
    }   
}
