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

        //public Dictionary<string, string> RequiredModules = new Dictionary<string,string>();

        /*
        /// <summary>
        /// Return all the javascript source code loaded with call to Require
        /// </summary>
        /// <returns></returns>
        private string GetRequiredModuleCode(){

            var b = new StringBuilder(1024);

            foreach(var c in RequiredModules.Values)
                b.Append(c).AppendLine();

            return b.ToString();
        }*/
        /// <summary>
        /// Load a JavaScript text file or text ressource in the JavaScript context
        /// </summary>
        /// <param name="name"></param>
        public void Load(string name, Assembly a = null) {

            if(!name.ToLower().EndsWith(".js"))
                name += ".js";

            string code = null;

            if(a!=null)
                code = DynamicSugar.DS.Resources.GetTextResource(name, a);

            if(System.IO.File.Exists(name))
                code = System.IO.File.ReadAllText(name);

            if(code==null)
                throw new ApplicationException("Require cannot find module '{0}'".format(name));

            //RequiredModules.Add(name, code);
            this.Run(code);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Call(string functionName, params object[] parameters)
        {
            return _javaScriptContextImplementation.Call(functionName, parameters);
        }
        /// <summary>
        /// Helper function to make date compatible with the used JavaScript
        /// run time. Jurassic date are not .net datetime and need a convertion
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object Date(object o){

            return _javaScriptContextImplementation.Date(o);
        }
        /// <summary>
        /// Helper function to make array compatible with the used JavaScript
        /// run time.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public object Array(params object [] values){

            object a = values;
            return _javaScriptContextImplementation.Array(a);
        }
        /// <summary>
        /// Helper function to make JavaScript object compatible with the used JavaScript
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object Object(object o){

            return _javaScriptContextImplementation.Object(o);
        }        
        /// <summary>
        /// Run the script passed as parameter
        /// </summary>
        /// <param name="script"></param>
        /// <returns>Return the last expression evaluated. The value is converted
        /// into a .NET array or a DynamicJavaScriptInstance</returns>
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
