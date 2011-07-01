using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noesis.Javascript;
using System.Dynamic;
using DynamicJavaScriptRunTimes;
using DynamicSugar;
using System.Reflection;

namespace DynamicJavaScriptRunTimes.UnitTests {

    [TestClass]
    public class Samples {

        [TestMethod]
        public void CallingFunctions() {
        
            dynamic jsContext = new DynamicJavascriptContext(new JavascriptContext());

            DateTime refDate = new DateTime(1964, 12, 11, 01, 02, 03);

            string script = @"

                var O2 = { 
                            F2: function(pInt,pDouble,pString,pBool,pDate) { 
                                    return ''+this.Internal+'-'+pInt+'-'+pDouble+'-'+pString+'-'+pBool+'-'+formatDateUS(pDate);
                            },
                            Internal:1
                            }
            ";
            jsContext.Load("format", Assembly.GetExecutingAssembly());
            jsContext.Run(script);
            
            var expectedF2 = "1-1-123.456-hello-true-12/11/1964 1:2:3";
            var f2Result   = jsContext.Call("O2.F2", 1, 123.456, "hello", true, refDate);
            Assert.AreEqual(expectedF2, f2Result);
        }

        [TestMethod]
        public void ReadingAConfiguration() {

            dynamic csContext = new DynamicJavascriptContext(new JavascriptContext());

            //csContext.Run(System.IO.File.ReadAllText("configuration.js"));
            csContext.Run(DS.Resources.GetTextResource("configuration.js", Assembly.GetExecutingAssembly()));

            var server  = csContext.Configuration.Server;
            var databse = csContext.Configuration.Database;
            var debug   = csContext.Configuration.Debug;

            for(var i=0; i<csContext.Configuration.Users.Length; i++) {

                var userName  = csContext.Configuration.Users[i].UserName;
            }
        }
        [TestMethod]
        public void NoesisSampleFromCodeplexWebSite() {

            dynamic jsContext = new DynamicJavascriptContext(
                                      new JavascriptContext()
                                );                                               
            jsContext.message = "Hello World !";
            jsContext.number = 1;

            string script = @"
                var i = 0;
                for (i = 0; i < 5; i++)
                    console.log(message + ' (' + i + ')');
                number += i;
            ";

            jsContext.Run(script);

            Console.WriteLine("number: " + jsContext.number);

        }
        [TestMethod]
        public void ArraySample() {

            dynamic jsContext   = new DynamicJavascriptContext(
                                       new JavascriptContext()
                                  );
            jsContext.a = new object [] { 1, 2, 3 };  // Regular Syntax
            jsContext.a = jsContext.Array( 1, 2, 3 ); // My Syntax

            string script = @"
                a.push(4);
            ";
            
            jsContext.Run(script);

            Assert.AreEqual(4, jsContext.a.Length);
            for(var i=0; i < jsContext.a.Length; i++)
                Assert.AreEqual(i+1, jsContext.a[i]);
        }
    }
}
