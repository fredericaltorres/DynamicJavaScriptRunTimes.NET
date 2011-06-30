using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace DynamicJavaScriptRunTimes {
   
    /// <summary>
    /// JavaScript object returned by the different JavaScript run-time are converted
    /// into Dictionary string,object, then the dictionary is wrapped into an instance
    /// of this class. In C# land this allow to get or set the name/value of a dictionary using the
    /// property syntax:
    ///     var s = person.LastName;
    /// array syntax    
    ///     var s = person["LastName"];
    /// </summary>
    public class DynamicJavaScriptInstance : DynamicObject {

        /// <summary>
        /// The dictionary when the property name/value are stored internally
        /// </summary>
        private Dictionary<string, object> _dic = new Dictionary<string,object>();
        
        /// <summary>
        /// Instances need to get access to the JavaScript run-time implementation
        /// to convert JavaScript property value array and object in the right .NET object.
        /// For now I used a global static variable, which initialized by the 
        /// DynamicJavascriptContext constructor
        /// </summary>
        public static IDynamicJavaScriptContext ___globalJavaScriptContext;

        /// <summary>
        /// Constuctor. 
        /// Generally speaking a JavaScript object is a IDictionary<string, object>;
        /// </summary>
        /// <param name="d"></param>
        public DynamicJavaScriptInstance(IDictionary<string, object> d){

            foreach(var k in d)
                this._dic.Add(k.Key, k.Value);
        }
        /// <summary>
        /// Get the value of a property using the [] syntax
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {

            if(this._dic.ContainsKey(indexes[0].ToString())){

                result = this._dic[indexes[0].ToString()];
                
                if(result.GetType().IsArray)
                    result = MakeDynamicObjectArray(result);
                
                return true;
            }
            else{
                result = null;
                return false;
            }
        }
        /// <summary>
        /// Set the value of a property using the property syntax
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool TrySetMember(SetMemberBinder binder, object value) {

            if(_dic.ContainsKey(binder.Name))
                _dic.Remove(binder.Name);

            _dic.Add(binder.Name, value);
            return true;
        }     
        /// <summary>
        /// Convert the input which must be an array into a new array, but if the
        /// array contains javascript objects, we wrap each object into a JavaScriptInstance().
        /// Therefore when accessing the array, we can address the property of the object in the
        /// array cell, using the C# dynamic synatx
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static object [] MakeDynamicObjectArray(object input){

            var a       = input as System.Array;
            var newL    = new List<object>();

            foreach(var i in a)
                if(i is IDictionary<string, object>)
                    newL.Add(new DynamicJavaScriptInstance(i as IDictionary<string, object>));                    
                else
                    newL.Add(i);  
                
            return newL.ToArray();
        }
        /// <summary>
        /// Get the value of a property using the property syntax
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result) {

            if(!_dic.ContainsKey(binder.Name))
                throw new ApplicationException(String.Format("property '{0}' not defined in object",binder.Name));

            result = _dic[binder.Name];

            if(___globalJavaScriptContext.IsJavaScriptDate(result)){
                
                result = ___globalJavaScriptContext.JavaScriptDateToNETDateTime(result);
            }
            // If we have to return an array we replace all the element of the array that 
            // are an object by our wrapper JavaScriptInstance around the object.
            // So we can continue to get the C# dynamic syntax
            if(___globalJavaScriptContext.IsJavaScriptArray(result)){

                result = MakeDynamicObjectArray(
                            ___globalJavaScriptContext.JavaScriptArrayToNETArray(result)
                );
            }
            // If the value returned  a JavaScript object we return it as a 
            // JavaScriptInstance so we can handle the dynamic aspect of it in C#
            else if(___globalJavaScriptContext.IsJavaScriptObject(result)){

                var dic = ___globalJavaScriptContext.JavaScriptObjectToNETDictionary(result);
                dynamic d = new DynamicJavaScriptInstance(dic);
                result = d;
            }
            else{
                result = ___globalJavaScriptContext.JavaScriptValueTypeToNETValueType(result);
            }
            return true;
        }
    }   
}
