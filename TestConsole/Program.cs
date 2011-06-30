using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noesis.Javascript;
using System.Dynamic;
using DynamicJavaScriptRunTimes;

namespace Noesisv8Console {

    class Program {

        static void Main(string[] args) {
            
            dynamic jsContext   = new DynamicJavascriptContext(new JavascriptContext());            

            jsContext.message   = "Hello World !";
            jsContext.number    = 1;            
            jsContext.array     = jsContext.Array(1, 2 , 3);
            jsContext.instance  = jsContext.Object( new { b = 2 } );
            jsContext.instance2 = jsContext.Object( new { a = 1, b = 2 } );
                       
            string script = @"
                number           = 123;
                instance['a']    = 1;
                instance['Date'] = new Date();                
                instance2['a']   = 123;
                console.log('Hello log '+instance['Date']);
                console.log('array.length:'+array.length);
                
                array.push(4);
                for(var i=0; i<array.length; i++)
                    console.log(array[i]);
            ";

            jsContext.Run(script);            

            Console.WriteLine("number: "        + jsContext.number       );
            Console.WriteLine("instance: "      + jsContext.instance     );
            Console.WriteLine("instance.a: "    + jsContext.instance.a   );
            Console.WriteLine("instance.b: "    + jsContext.instance.b   );
            Console.WriteLine("instance.Date: " + jsContext.instance.Date);
            Console.WriteLine("instance2.a: "   + jsContext.instance2.a  );
            Console.WriteLine("array:"          + jsContext.array.Length );
            Console.WriteLine("array[3]:"       + jsContext.array[3]     );

            FunctionCall();
            Console.ReadLine();
        }

        private static void FunctionCall()
        {
            dynamic jsContext = new DynamicJavascriptContext(new JavascriptContext());

            string script = @"                
                var F2 = function(pInt,pDouble,pString,pBool,pDate) { 

                    var s = ''+pInt+'-'+pDouble+'-'+pString+'-'+pBool+'-'+pDate;
                    console.log(s);
                    return 'F2'; 
                };
            ";
            jsContext.Run(script);
            
            var result = jsContext.Call("F2", 1, 123.456, "hello", true, DateTime.Now);
        }
    }
}
