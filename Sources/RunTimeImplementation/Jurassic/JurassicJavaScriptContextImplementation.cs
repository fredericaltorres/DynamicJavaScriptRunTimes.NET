using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;
using Jurassic.Library;
using Jurassic;
using DynamicSugar;

namespace DynamicJavaScriptRunTimes {

    /// <summary>
    /// Implementation of the IJavaScriptContext interface of the JavaScript
    /// run-time jurassic. See the interface for more comment about the different
    /// methods.
    /// </summary>
    public class JurassicJavaScriptContextImplementation  : BaseJavaScriptContextImplementation, IDynamicJavaScriptContext{

        private Jurassic.ScriptEngine _javascriptContext;
        
        public JurassicJavaScriptContextImplementation(Jurassic.ScriptEngine javascriptContext){

            this._javascriptContext = javascriptContext;
        }
        public SUPPORTED_JAVASCRIPT_RUNTIME Runtime {
            get{
                return SUPPORTED_JAVASCRIPT_RUNTIME.Jurassic;
            }
        }
        public void SetParameter(string name, object value){

            if(value==null){
                
                this._javascriptContext.SetGlobalValue(name, Jurassic.Null.Value);
            }
            else if(value.GetType().IsArray){

                var arrayValue      = DynamicJavaScriptInstance.MakeDynamicObjectArray(value);
                var arrayInstance   = new ArrayInstance(this._javascriptContext.Object.InstancePrototype, 0, 0);

                foreach(var v in arrayValue)
                    arrayInstance.Push(v);
                this._javascriptContext.SetGlobalValue(name, arrayInstance);
            }
            else{
                this._javascriptContext.SetGlobalValue(name, value);
            }
        }
        public object GetParameter(string name){

            return this._javascriptContext.GetGlobalValue(name);
        }
        private object ConvertJavaScriptObjectToDictionary(object o)
        {
            if (o is Jurassic.Library.ArrayInstance)
            {
                var a       = o as Jurassic.Library.ArrayInstance;
                var values  = new List<object>();

                foreach (var e in a.ElementValues)
                    values.Add(e);

                return values.ToArray();
            }
            else if (IsJavaScriptObject(o))
            {
                return JavaScriptObjectToNETDictionary(o);
            }
            else return o;
        }
        public override object Run(string script){

            this._javascriptContext.SetGlobalValue("console", new Jurassic.Library.FirebugConsole(this._javascriptContext));
            return  this._javascriptContext.Evaluate(script);                        
        }
        /// <summary>
        /// A helper class to create object and set JavaScript object into the Jurassic
        /// run-time
        /// </summary>
        public class JurassicArrayInstanceHelper : ArrayInstance {
            
            public JurassicArrayInstanceHelper(ScriptEngine engine, object [] values) : base(engine.Object.Prototype, (uint)values.Length, (uint)values.Length)
            {
                for(var i=0; i<values.Length; i++)
                    this.Push(values[i]);            
            }
        }
        public class JurassicDateInstanceHelper : DateInstance {
            
            public JurassicDateInstanceHelper(ScriptEngine engine, DateTime value) : base(engine.Object.Prototype, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond)
            {                
            }
        }
        /// <summary>
        /// A helper class to create object and set JavaScript object into the Jurassic
        /// run-time
        /// </summary>
        public class JurassicObjectInstanceHelper : ObjectInstance { 

            public JurassicObjectInstanceHelper(ScriptEngine engine, Dictionary<string, object> dic) : base(engine)
            {
                foreach(var k in dic)                
                    this.DefineProperty(k.Key, new PropertyDescriptor(k.Value, Jurassic.Library.PropertyAttributes.FullAccess), true);                
            }
        }
        public object Object(object o){

            return new JurassicObjectInstanceHelper(this._javascriptContext, DynamicSugar.DS.Dictionary(o));            
        }
        public object Array(object o){
            
            return this._javascriptContext.Array.New(DataUtil.ConcertSystemArrayToArray(o));
        }
        public object Date(object o) {

            return new JurassicDateInstanceHelper(this._javascriptContext, (DateTime)o);
        }
        public bool IsJavaScriptArray(object o) {

            return o is ObjectInstance && o is ArrayInstance;
        }
        public bool IsJavaScriptDate(object o) {
            
            return o is DateInstance;
        }
        /// <summary>
        /// Return true if o is a JavaScript object. Date which are a JavaScript
        /// object will not return true
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool IsJavaScriptObject(object o) {
            
            return (
                    (o is ObjectInstance) && 
                    (!(o is ArrayInstance)) &&
                    (!(o is DateInstance))
                    );
        }
        public IDictionary<string, object> JavaScriptObjectToNETDictionary(object o) {

            var oi  = o as ObjectInstance;
            var dic = new Dictionary<string, object>();

            foreach (var p in oi.Properties)
                dic.Add(p.Name, p.Value);

            return dic;
        }
        public object[] JavaScriptArrayToNETArray(object o) {

            var l  = new List<object>();
            var oi = o as ObjectInstance;

            foreach (var p in oi.Properties)
                if(p.Name!="length")
                    l.Add(p.Value);

            return l.ToArray();
        }
        public DateTime JavaScriptDateToNETDateTime(object o){
                        
            return (o as DateInstance).Value;
        }
        /// <summary>
        /// All type are compatibles. Do nothing
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public object JavaScriptValueTypeToNETValueType(object o){

            if(o is Jurassic.ConcatenatedString){

                return (o as Jurassic.ConcatenatedString).ToString();
            }
            else return o;
        }
    }  
}
