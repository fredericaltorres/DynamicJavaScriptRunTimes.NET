using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Noesis.Javascript;
using System.Dynamic;
using DynamicJavaScriptRunTimes;
using System.Reflection;

namespace DynamicJavaScriptRunTimes.UnitTests {

    [TestClass]
    public class UnitTests {
  
        /// <summary>
        /// Test calling JavaScript function or methods once a script is loaded
        /// </summary>
        [TestMethod]
        public void CallingFunction()
        {
            __CallingFunctions(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));
            __CallingFunctions(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }

        public void __CallingFunctions(dynamic jsContext) {

            DateTime refDate = new DateTime(1964, 12, 11, 01, 02, 03);

            string script = @"
                var F1 = function() { return 'RF1'; };
                var O1 = { F1: function() { return 'ORF1'; } };
                var F2 = function(pInt,pDouble,pString,pBool,pDate) { 
                    return ''+pInt+'-'+pDouble+'-'+pString+'-'+pBool+'-'+formatDateUS(pDate);
                };
                var O2 = { 
                            F2: function(pInt,pDouble,pString,pBool,pDate) { 
                                    return ''+this.Internal+'-'+pInt+'-'+pDouble+'-'+pString+'-'+pBool+'-'+formatDateUS(pDate);
                            },
                            Internal:1
                          }
            ";
            jsContext.Load("format", Assembly.GetExecutingAssembly());
            jsContext.Run(script);

            Assert.AreEqual("RF1" , jsContext.Call("F1"));
            Assert.AreEqual("ORF1", jsContext.Call("O1.F1"));

            var expectedF2 = "1-123.456-hello-true-12/11/1964 1:2:3";
            var f2Result   = jsContext.Call("F2", 1, 123.456, "hello", true, refDate);
            Assert.AreEqual(expectedF2, f2Result);

            expectedF2 = "1-1-123.456-hello-true-12/11/1964 1:2:3";
            f2Result   = jsContext.Call("O2.F2", 1, 123.456, "hello", true, refDate);
            Assert.AreEqual(expectedF2, f2Result);
        }
        //  ____________________________________________________________ 
                                                                         
        // With Noesis run-time there is no way to differenciate undefined and 
        // null once in .NET land. So for now the unit test do nothing
        [TestMethod]
        public void Undefined()
        {
            __Undefined(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));        
            __Undefined(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }                
        public void __Undefined(dynamic jsContext) {
            
            var undefinedValue  = jsContext.Run("undefined");
            var nullValue       = jsContext.Run("null");

            jsContext.Run(@"
                var u = undefined;
                var n = null;
            ");

            object ou = jsContext.u;
            object on = jsContext.n;
            //Assert.AreEqual(null, jsContext.n);
            //Assert.AreEqual(null, jsContext.u);
            //Assert.AreEqual(null, jsContext.n);
        }
        //  ____________________________________________________________ 
                                   
        /// <summary>
        /// Test return values returned by the method Run()
        /// </summary>                              
        [TestMethod]
        public void Run_TestExpressionsReturned()
        {            
            __Run_ExpressionReturned(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));          
            __Run_ExpressionReturned(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }                
        public void __Run_ExpressionReturned(dynamic jsContext) {
                        
            Assert.AreEqual(1  , jsContext.Run("1"));
            Assert.AreEqual("A", jsContext.Run("'A'"));
            Assert.AreEqual(3  , (int)jsContext.Run("[1,2,3]").Length); // Jurassic return an UInt32
            Assert.AreEqual(1  , jsContext.Run("[1,2,3]")[0]);
            Assert.AreEqual(1  , jsContext.Run("({a:1})").a );         
        }
                
        //  ____________________________________________________________ 
                                                                         
        /// <summary>
        /// Test 
        ///     1 passing global value type to the global object from the C# world,
        ///     2 updating the value in JavaScript land
        ///     3 testing the result in C# world
        /// We are using the property syntax to set or get the property values
        /// </summary>
        [TestMethod]
        public void ValueType_InputOutput_PropertySyntax() {

            __ValueType_InputOutput_PropertySyntax(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
            __ValueType_InputOutput_PropertySyntax(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));
        }
        public void __ValueType_InputOutput_PropertySyntax(dynamic jsContext) {

            jsContext.Integer = 1;
            jsContext.Double  = 123.456;
            jsContext.BoolT   = true;
            jsContext.BoolF   = false;
            jsContext.S       = "abcd";            
            jsContext.Today   = "";//jsContext.Date(DateTime.Now);
            
            string script = @"
                Integer         += 2;
                Double          += 2;
                S               += 'abcd';
                BoolT            = !BoolT;
                BoolF            = !BoolF;
                Today            = new Date();
            ";

            var d = jsContext.Run(script);
            
            // Verify that the now generated by JavaScript match at the seconds level
            Assert.AreEqual(0, DateTime.Now.Subtract(jsContext.Today).Seconds);

            Assert.AreEqual(1+2       , jsContext.Integer);
            Assert.AreEqual(123.456+2 , jsContext.Double);
            Assert.AreEqual("abcdabcd", jsContext.S);
            Assert.AreEqual(false     , jsContext.BoolT);
            Assert.AreEqual(true      , jsContext.BoolF);
            
        }
             
        //  ____________________________________________________________ 
                                                                         
        /// <summary>
        /// Test 
        ///     1 passing global value type to the global object from the C# world,
        ///     2 updating the value in JavaScript land
        ///     3 testing the result in C# world
        /// We are using the array syntax to set or get the property values
        /// </summary>
        [TestMethod]
        public void ValueType_InputOutput_ArraySyntax() {
            
            __ValueType_InputOutput_ArraySyntax(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));    
            __ValueType_InputOutput_ArraySyntax(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));
        }
        public void __ValueType_InputOutput_ArraySyntax(dynamic jsContext) {

            jsContext["Integer"]       = 1;
            jsContext["Double" ]       = 123.456;
            jsContext["Bool"   ]       = true;
            jsContext["S"      ]       = "abcd";
            
            string script = @"
                Integer         += 2;
                Double          += 2;
                S               += 'abcd';
                Bool            = !Bool;                
            ";
            
            jsContext.Run(script);

            Assert.AreEqual(1+2       , jsContext["Integer"]);
            Assert.AreEqual(123.456+2 , jsContext["Double" ]);
            Assert.AreEqual("abcdabcd", jsContext["S"      ]);
            Assert.AreEqual(false     , jsContext["Bool"   ]);
        }
        
        //  ____________________________________________________________ 

        /// <summary>
        /// Test set
        ///     1 Setting a int array in the global object
        ///     2 Updating the value in JavaScript land
        ///     3 Testing the result in C# world        
        /// </summary>
        [TestMethod]
        public void IntArray_InputOutput() {
            
            __IntArray_InputOutput(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));           
            __IntArray_InputOutput(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }
        public void __IntArray_InputOutput(dynamic jsContext) {

            jsContext.a         = jsContext.Array( 1, 2, 3 );

            string script = @"
                a.push(4);
            ";
            
            jsContext.Run(script);
            Assert.AreEqual(4, (int)jsContext.a.Length);
            for(var i=0; i<4; i++)
                Assert.AreEqual(i+1, jsContext.a[i]);
        }

        /// <summary>
        /// Test set
        ///     1 Setting a string array in the global object
        ///     2 Updating the value in JavaScript land
        ///     3 Testing the result in C# world        
        /// </summary>
        [TestMethod]
        public void StringArray_InputOutput() {
            
            __StringArray_InputOutput(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));           
            __StringArray_InputOutput(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }
        public void __StringArray_InputOutput(dynamic jsContext) {

            jsContext.a = jsContext.Array( "1", "2", "3" );

            string script = @"
                a.push('4');
            ";
            
            jsContext.Run(script);
            Assert.AreEqual(4, (int)jsContext.a.Length);
            for(var i=0; i<4; i++)
                Assert.AreEqual((i+1).ToString(), jsContext.a[i]);
        }

        //  ____________________________________________________________ 
        
        /// <summary>
        /// Test allocating and return an object by calling the Run() method.
        /// Then we directly use the returned object from the method Run().
        /// </summary>
        [TestMethod]
        public void CreateAndReturnObjectOnTheFly()
        {            
            __CreateAndReturnObjectOnTheFly(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));                   
            __CreateAndReturnObjectOnTheFly(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }        
        public void __CreateAndReturnObjectOnTheFly(dynamic jsContext) {

            Assert.AreEqual(
                "Fred",
                jsContext.Run(@"
                    function Person(firstName){ this.FirstName = firstName; }
                    (new Person('Fred'))"
                ).FirstName
            );
        }
        
        //  ____________________________________________________________ 
                                 
        /// <summary>
        /// Test set
        ///     1 Setting a multi-type array in the global object
        ///     2 Updating the value in JavaScript land
        ///     3 Testing the result in C# world        
        /// </summary>                               
        [TestMethod]
        public void MultiTypeArray_InputOutput()
        {            
            __MultiTypeArray_InputOutput(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));        
            __MultiTypeArray_InputOutput(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }
        public void __MultiTypeArray_InputOutput(dynamic jsContext ) {

            jsContext.a         = jsContext.Array( 1, 2, "A" );

            string script = @"
                a.push('B');
                a.push(true);
            ";
            
            jsContext.Run(script);
            Assert.AreEqual(5   , (int)jsContext.a.Length);
            Assert.AreEqual(1   , jsContext.a[0]);
            Assert.AreEqual(2   , jsContext.a[1]);
            Assert.AreEqual("A" , jsContext.a[2]);
            Assert.AreEqual("B" , jsContext.a[3]);
            Assert.AreEqual(true, jsContext.a[4]);
        }

        //  ____________________________________________________________ 

        /// <summary>
        /// Test
        ///     1 Setting an object in the global object
        ///     2 Updating the object by adding property and nested objects
        ///     3 Testing the result in C# world        
        /// </summary>  
        [TestMethod]
        public void Objects_InputOutput()
        {
            __Objects_InputOutput(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));         
            __Objects_InputOutput(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }
        public void __Objects_InputOutput(dynamic jsContext) {

            jsContext.i = jsContext.Object( new { a=1, b=2 } );

            string script = @"
                i.c                             = i.a + i.b;
                i.Owner                         = 'fred';
                i.NestedObject1                 = {};
                i.NestedObject1.a               = i.a;
                i.NestedObject1.array           = [1,2,3];
                i.NestedObject1.NestedObject2   = {};
                i.NestedObject1.NestedObject2.b = i.b;
            ";
            jsContext.Run(script);

            Assert.AreEqual(3     , jsContext.i.c);
            Assert.AreEqual("fred", jsContext.i.Owner);
            Assert.AreEqual(1     , jsContext.i.NestedObject1.a);
            Assert.AreEqual(3     , jsContext.i.NestedObject1.array.Length);
            Assert.AreEqual(1     , jsContext.i.NestedObject1.array[0]);
            Assert.AreEqual(2     , jsContext.i.NestedObject1.array[1]);
            Assert.AreEqual(3     , jsContext.i.NestedObject1.array[2]);
            Assert.AreEqual(2     , jsContext.i.NestedObject1.NestedObject2.b);
        }
        /// <summary>
        /// Test
        ///     1 Setting an object in the global object, containing a nested object
        ///     2 Updating the object by adding property using the nested objects
        ///     3 Testing the result in C# world        
        /// </summary>  
        [TestMethod]
        public void Objects_InputObjectNested_Output()
        {
            __Objects_Output(new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext()));         
            __Objects_Output(new DynamicJavascriptContext(new Jurassic.ScriptEngine()));
        }
        public void __Objects_Output(dynamic jsContext) {
            
            jsContext.i = jsContext.Object( new {
                a = 1,
                b = "bb",
                c = jsContext.Object( new { LastName="Torres" } )
            } );

            string script = @"
                i.resultLastName = i.c.LastName;
                i.resultab = ''+i.a+i.b;
                var s = 'a'+'b'; // Jurassic return this as a concatenated string
            ";
            jsContext.Run(script);
            Assert.AreEqual("Torres", jsContext.i.resultLastName);            
            Assert.AreEqual("1bb"   , jsContext.i.resultab);
            Assert.AreEqual("ab"   , jsContext.s);
        }
        /// <summary>
        /// Test reading a configuration files, with nested array of objects literal
        /// </summary>
        [TestMethod]
        public void ReadingAConfiguration_NestedArrayOfObjectsLiteral() {

            dynamic jsContext = new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext());                        
            
            string script = @"
                Configuration =  {
                    Server   :    'TOTO', 
                    Database :    'Rene', 
                    Debug    :    true, 
                    MaxUser  :    3, 
                    Users    :    [
                        { UserName:'rdescartes'    ,FirstName:'rene'      ,LastName:'descartes'     }, 
                        { UserName:'bpascal'       ,FirstName:'blaise'    ,LastName:'pascal'        }, 
                        { UserName:'cmontesquieu'  ,FirstName:'charles'   ,LastName:'montesquieu'   } 
                    ]
                }
            ";
            jsContext.Run(script);

            Assert.AreEqual("TOTO"      , jsContext.Configuration.Server);
            Assert.AreEqual("Rene"      , jsContext.Configuration.Database);
            Assert.AreEqual(true        , jsContext.Configuration.Debug);
            Assert.AreEqual(3           , jsContext.Configuration.MaxUser);
            Assert.AreEqual(3           , jsContext.Configuration.Users.Length);
            Assert.AreEqual("rdescartes", jsContext.Configuration.Users[0].UserName);

            Assert.AreEqual("TOTO"      , jsContext["Configuration"]["Server"]);
            Assert.AreEqual("Rene"      , jsContext["Configuration"]["Database"]);
            Assert.AreEqual(true        , jsContext["Configuration"]["Debug"]);
            Assert.AreEqual(3           , jsContext["Configuration"]["MaxUser"]);
            Assert.AreEqual(3           , jsContext["Configuration"]["Users"].Length);
            Assert.AreEqual("rdescartes", jsContext["Configuration"]["Users"][0].UserName);
        }

        private class Person {

            public string LastName { get;set; }
            public int Age { get;set; }

            public Person(string lastName, int age){

                this.LastName = lastName;
                this.Age = age;
            }
        }
        /// <summary>
        /// Test
        ///     1 Setting an object in the global object from anonymous type and poco
        ///     2 Creating new objects and updating their properties with the input objects
        ///     3 Testing the result in C# world        
        /// </summary> 
        [TestMethod]
        public void NestedObjects_NestedInputObject_Output() {

            dynamic jsContext = new DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext());                        

            jsContext.i         = jsContext.Object(
                new {
                    a = jsContext.Object( new { LastName="Torres", Age=46 } ),
                    b = jsContext.Object( new Person("Ferry", 47) ),
                }
            );
            string script = @"
                var p1 = {
                    Name : i.a.LastName,
                    Age  : i.a.Age
                }
                var p2 = {
                    Name : i.b.LastName,
                    Age  : i.b.Age
                }
            ";
            jsContext.Run(script);
            Assert.AreEqual("Torres", jsContext.p1.Name);
            Assert.AreEqual("Torres", jsContext.p1["Name"]);
            Assert.AreEqual("Torres", jsContext["p1"]["Name"]);

            Assert.AreEqual(46      , jsContext.p1.Age);
            Assert.AreEqual(46      , jsContext.p1["Age"]);
            Assert.AreEqual(46      , jsContext["p1"]["Age"]);

            Assert.AreEqual("Ferry" , jsContext.p2.Name);
            Assert.AreEqual("Ferry" , jsContext.p2["Name"]);
            Assert.AreEqual("Ferry" , jsContext["p2"]["Name"]);

            Assert.AreEqual(47      , jsContext.p2.Age);
            Assert.AreEqual(47      , jsContext.p2["Age"]);
            Assert.AreEqual(47      , jsContext["p2"]["Age"]);            
        }
    }
}
