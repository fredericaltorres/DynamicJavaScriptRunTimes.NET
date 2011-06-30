
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using DynamicSugar;

namespace DynamicJavaScriptRunTimes {

    /// <summary>
    /// Implementation of the IJavaScriptContext interface of the JavaScript
    /// run-time Noesis JavaScript.NET. See the interface for more comment about the different
    /// methods.
    /// </summary>
    public class NoesisJavaScriptContextImplementation  : BaseJavaScriptContextImplementation, IDynamicJavaScriptContext {

        private Noesis.Javascript.JavascriptContext _javascriptContext;

        public NoesisJavaScriptContextImplementation(Noesis.Javascript.JavascriptContext javascriptContext){

            this._javascriptContext = javascriptContext;
        }
        public SUPPORTED_JAVASCRIPT_RUNTIME Runtime {
            get{
                return SUPPORTED_JAVASCRIPT_RUNTIME.Noesis;
            }
        }
        public void SetParameter(string name, object value) {

            if(value==null)
                throw new ApplicationException("Noesis JavaScript.NET does support .NET null value");
            
            this._javascriptContext.SetParameter(name, value);
        }
        public object GetParameter(string name) {
            // Noesis object are already a Dictionary<string, object>
            // so if we return a JavaScript object we have no convertion
            // to do.
            return this._javascriptContext.GetParameter(name);
        }
        public override object Run(string script) {

            this._javascriptContext.SetParameter("console", new SystemConsole());
            return this._javascriptContext.Run(script);
        }
        public object Object(object o) {

            return DynamicSugar.DS.Dictionary(o);
        }        
        public object Array(object o) {

            return DataUtil.ConcertSystemArrayToArray(o);
        }        
        /// <summary>
        /// Noesis, date and .NET date are the same
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object Date(object o) {

            return (DateTime)o;
        }
        public bool IsJavaScriptDate(object o) {
            
            return o is DateTime;
        }
        public bool IsJavaScriptArray(object o) {

            return o.GetType().IsArray;
        }
        public bool IsJavaScriptObject(object o) {

            return o is IDictionary<string, object>;
        }
        public IDictionary<string, object> JavaScriptObjectToNETDictionary(object o) {
                        
            return o as IDictionary<string, object>;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object[] JavaScriptArrayToNETArray(object o)
        {
            var a       = o as System.Array;
            var newL    = new List<object>();

            foreach(var i in a)
                newL.Add(i);                

            return newL.ToArray();
        }
        /// <summary>
        /// Noesis JavaScript Date are the same as DateTime.
        /// So we do nothing
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public DateTime JavaScriptDateToNETDateTime(object o){

            DateTime d = (DateTime)o;

            // There is bug in Noesis returning date. Probably the date is converted
            // from an internal format to .NET format and in the process the day is increased
            // by one. This only happens when the date is carry over from the JavaScript
            // world to the net world. In javaScript land every thing is right

            DateTime newD = new DateTime(d.Year, d.Month, 
                                         d.Day-1,  // Fix the bug
                                         d.Hour, d.Minute, d.Second, d.Millisecond);
            return newD;
        }
        /// <summary>
        /// All type are compatibles. Do nothing
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object JavaScriptValueTypeToNETValueType(object o){

            return o;
        }
    
    }
}
