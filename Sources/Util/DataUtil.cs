using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicJavaScriptRunTimes {

    class DataUtil {

        public static object [] ConcertSystemArrayToArray(object o) {

            var a       = o as System.Array;
            var newL    = new List<object>();
            foreach(var i in a)
                newL.Add(i);                
            return newL.ToArray();
        } 
    }
}
