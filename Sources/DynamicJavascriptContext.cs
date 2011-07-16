using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using DynamicSugar;
using System.Reflection;

namespace DynamicJavaScriptRunTimes {
   
    /// <summary>
    /// 
    /// </summary>
    public class DynamicJavascriptContext : DynamicObject {

        IDynamicJavaScriptContext _javaScriptContextImplementation;

        public string GetVersionInfo(){

            return "JavaScript runtime:{0}".format(_javaScriptContextImplementation.Runtime);
        }
        /// <summary>
        /// Load a JavaScript text file or text ressource in the JavaScript context
        /// </summary>
        /// <param name="name">The name without the .js extension</param>
        /// <param name="assembly">The assembly is loading a ressource</param>
        public void Load(string name, Assembly assembly = null) {

            if(!name.ToLower().EndsWith(".js"))
                name += ".js";

            string code = null;

            if(assembly!=null)
                code = DynamicSugar.DS.Resources.GetTextResource(name, assembly);

            if(System.IO.File.Exists(name))
                code = System.IO.File.ReadAllText(name);

            if(code==null)
                throw new ApplicationException("Require cannot find module '{0}'".format(name));

            //RequiredModules.Add(name, code);
            this.Run(code);
        }
        /// <summary>
        /// Execute a javascript global function or method
        /// </summary>
        /// <param name="functionName">the function name</param>
        /// <param name="parameters">The parameters</param>
        /// <returns></returns>
        public object Call(string functionName, params object[] parameters)
        {
            return _javaScriptContextImplementation.Call(functionName, parameters);
        }
        /// <summary>
        /// Helper function to make date compatible with the JavaScript
        /// run time. Jurassic date are not .net datetime and need a convertion
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object Date(DateTime d){

            return _javaScriptContextImplementation.Date(d);
        }
        /// <summary>
        /// Helper function to make array compatible with the JavaScript run time.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public object Array(params object [] array){

            return _javaScriptContextImplementation.Array(array);
        }
        /// <summary>
        /// Helper function to make a JavaScript object compatible with the JavaScript run-time
        /// </summary>
        /// <param name="netObject"></param>
        /// <returns></returns>
        public object Object(object netObject){

            return _javaScriptContextImplementation.Object(netObject);
        }        
        /// <summary>
        /// Run the script and return the last value evaluated. Executing a declaration function 
        /// or a global object literal, will load the function or object in the JavaScript context.
        /// </summary>
        /// <param name="script"></param>
        /// <returns>
        /// </returns>
        public object Run(string script){
            
            var o = this._javaScriptContextImplementation.Run(script);

            if(o!=null){

                if(_javaScriptContextImplementation.IsJavaScriptArray(o)){

                    o = _javaScriptContextImplementation.JavaScriptArrayToNETArray(o);
                }
                else if (_javaScriptContextImplementation.IsJavaScriptObject(o)) {

                    var dic = _javaScriptContextImplementation.JavaScriptObjectToNETDictionary(o);
                    dynamic d = new DynamicJavaScriptInstance(dic);
                    o = d;
                }
            }
            return o;
        }
        /// <summary>
        /// Initialize the context for Noesis JavaScript run-time
        /// </summary>
        /// <param name="javascriptContext"></param>
        public DynamicJavascriptContext(Noesis.Javascript.JavascriptContext javascriptContext){

            _javaScriptContextImplementation = new NoesisJavaScriptContextImplementation(javascriptContext);
            DynamicJavaScriptInstance.___globalJavaScriptContext = _javaScriptContextImplementation;
        }
        /// <summary>
        /// Initialize the context for Jurassic JavaScript run-time
        /// </summary>
        /// <param name="javascriptContext"></param>
        public DynamicJavascriptContext(Jurassic.ScriptEngine javascriptContext){

            _javaScriptContextImplementation = new JurassicJavaScriptContextImplementation(javascriptContext);
            DynamicJavaScriptInstance.___globalJavaScriptContext = _javaScriptContextImplementation;
        }
        /// <summary>
        /// Set a global variable into the JavaScript context
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value) {

            _javaScriptContextImplementation.SetParameter(binder.Name, value);
            return true;
        }
        /// <summary>
        /// Get the value of a global variable from the JavaScript context using the [] syntax
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {

            return GetMember(indexes[0].ToString(), out result);
        }      
        /// <summary>
        /// Get a global variable value from the JavaScript context
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result) {

            return GetMember(binder.Name, out result);
        }
        /// <summary>
        /// Get a global variable value from the JavaScript context
        /// </summary>
        /// <param name="name">The variable name</param>
        /// <param name="result">The output value</param>
        /// <returns></returns>
        private bool GetMember(string name, out object result) {

            if(name.EndsWith("()")){
                var script = "{0}".format(name);
                result = this.Run(script);
            }       
            else{
                result = _javaScriptContextImplementation.GetParameter(name);
            }

            if (_javaScriptContextImplementation.IsJavaScriptDate(result)) {

                result = _javaScriptContextImplementation.JavaScriptDateToNETDateTime(result);
            }
            // If the value returned is a IDictionary<string, object> AKA a JavaScript
            // object we return it as a JavaScriptInstance so we can handle the dynamic
            // aspect of it in C#
            else if (_javaScriptContextImplementation.IsJavaScriptObject(result)) {

                var dic = _javaScriptContextImplementation.JavaScriptObjectToNETDictionary(result);
                dynamic d = new DynamicJavaScriptInstance(dic);
                result = d;
            }
            else { // Convert some value type into .net value type. Jurassic support string and concatenated string
                   // which we need to convert into string on the fly. this is why we have this call
                result = _javaScriptContextImplementation.JavaScriptValueTypeToNETValueType(result);
            }
            return true;
        }
        /// <summary>
        /// Set a global variable into the JavaScript context using the [] syntax
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {

            _javaScriptContextImplementation.SetParameter(indexes[0].ToString(), value);
            return  true;
        }
    }
}
