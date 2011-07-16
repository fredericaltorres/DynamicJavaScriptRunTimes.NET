//#define JURASSIC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynamicSugar;

namespace CoffeeScriptRunTime {

    public enum Action {

        Compile,
        Run
    }
    public class CoffeeScriptRunTime {

        const string CoffeeScriptVersion  = "1.1.1";
        const string DisplayJavaScriptTag = "#!DisplayJavaScript";
        
        dynamic _jsContext;
        string  _coffeeScriptFileName;
        string  _javaScriptFileName;
        bool    _displayJavaScriptFromCoffeeScript;

        public CoffeeScriptRunTime(){

            #if JURASSIC
                this._jsContext = new DynamicJavaScriptRunTimes.DynamicJavascriptContext(new Jurassic.ScriptEngine());
            #else
                this._jsContext = new DynamicJavaScriptRunTimes.DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext());
            #endif
        }
        public string JavaScriptOutputfile{
            get{
                var d = System.IO.Path.GetDirectoryName(this._coffeeScriptFileName);
                var n = System.IO.Path.GetFileNameWithoutExtension(this._coffeeScriptFileName);
                return @"{0}\{1}.js".format(d, n);
            }
        }

        public bool RunJavaScript(string fileName){
            
            
            this._javaScriptFileName   = fileName;
            string javaScriptReady     = LoadAndPreprocessJavaScriptSource(fileName);
            var finalJavaScript        = LoadJAVAScriptLibrariesForCompiling(javaScriptReady);
            
            var es = this.ExecuteJavaScript(finalJavaScript);
            if(!es.Succeeded)
                Console.WriteLine(es.ToString());

            return true;
        }
        public CompilationStatus CompileAndOrRunCoffeeScript(string fileName, Action action, bool displayJavaScriptCode){

            this._coffeeScriptFileName = fileName;
            string coffeeScriptReady   = LoadAndPreprocessCoffeeScriptSource(fileName);
            var finalJavaScript        = LoadJavaScriptLibrariesForCompiling(coffeeScriptReady);
            var compilationStatus      = this.ExecuteCompilation(finalJavaScript);

            if(compilationStatus.Succeeded){

                if(displayJavaScriptCode || this._displayJavaScriptFromCoffeeScript)
                    Console.WriteLine(compilationStatus.javaScriptCode);
            
                if(action==Action.Compile)
                    System.IO.File.WriteAllText(this.JavaScriptOutputfile, compilationStatus.javaScriptCode);
                else {
                    var es = this.ExecuteJavaScript(compilationStatus.javaScriptCode);
                    if(!es.Succeeded)
                        Console.WriteLine(es.ToString());
                }
            }
            else{
                Console.WriteLine(compilationStatus.ToString());
            }
            return compilationStatus;
        }
        private string LoadJAVAScriptLibrariesForCompiling(string script) {

            var jsCode = new StringBuilder();            
            jsCode.Append(DynamicSugar.DS.Resources.GetTextResource("RunTimeHelper.js", System.Reflection.Assembly.GetExecutingAssembly())).AppendLine();
            jsCode.Append(script).AppendLine();
            return jsCode.ToString();
        }
        private string LoadJavaScriptLibrariesForCompiling(string coffeeScriptCode) {

            var jsCode = new StringBuilder();
            jsCode.Append(DynamicSugar.DS.Resources.GetTextResource("coffee-script.v1.1.1.js", System.Reflection.Assembly.GetExecutingAssembly())).AppendLine();
            jsCode.Append(DynamicSugar.DS.Resources.GetTextResource("RunTimeHelper.js", System.Reflection.Assembly.GetExecutingAssembly())).AppendLine();
            jsCode.Append(DynamicSugar.DS.Resources.GetTextResource("Runner.js", System.Reflection.Assembly.GetExecutingAssembly())).AppendLine().AppendLine();
            jsCode.AppendFormat("CompileCoffeeScript('{0}');", coffeeScriptCode).AppendLine();
            return jsCode.ToString();
        }
        private string LoadJavaScriptLibrariesForExecution(string javaScriptCode) {

            var jsCode = new StringBuilder();            
            jsCode.Append(DynamicSugar.DS.Resources.GetTextResource("RunTimeHelper.js", System.Reflection.Assembly.GetExecutingAssembly())).AppendLine();
            jsCode.Append(javaScriptCode).AppendLine();
            return jsCode.ToString();
        }
        private string LoadAndPreprocessJavaScriptSource(string fileName) {

            var javaScriptCode = System.IO.File.ReadAllText(fileName);
            return javaScriptCode;
        }
        private string LoadAndPreprocessCoffeeScriptSource(string fileName) {

            string coffeeScriptCode = System.IO.File.ReadAllText(fileName);
            if (coffeeScriptCode.Contains(DisplayJavaScriptTag)) 
                _displayJavaScriptFromCoffeeScript = true;
            coffeeScriptCode = coffeeScriptCode.Replace(Environment.NewLine, "\\n");
            // For the f*@#... mac and linux users
            coffeeScriptCode = coffeeScriptCode.Replace(Environment.NewLine[0].ToString(), "\\n");
            coffeeScriptCode = coffeeScriptCode.Replace(Environment.NewLine[1].ToString(), "\\n");

            coffeeScriptCode = coffeeScriptCode.Replace("'",@"\'");
            return coffeeScriptCode;
        }
        public void DisplayLogo(bool displayLogo) {

            if (displayLogo) {

                Console.Write("CoffeeScript Run-time v 0.1, ");
                Console.Write("CoffeeScript Compiler v {0}, ".format(CoffeeScriptVersion));
                Console.Write("The Closure Compiler - Copyright 2009 Google - http://code.google.com/closure/compiler");

                string v = _jsContext.GetVersionInfo();
                Console.WriteLine("{0}.".format(v));
            }
        }
        public class CompilationStatus {

            public System.Exception ex;
            public string javaScriptCode;
            public bool Succeeded {
                get{
                    return this.ex == null;
                }
            }
            public override string ToString() {
                return "{0} - {1}".format(ex.GetType().Name, ex.Message);
            }            
        }
        private CompilationStatus ExecuteCompilation(string code){            

            CompilationStatus c = new CompilationStatus();
            try{                
                c.javaScriptCode = _jsContext.Run(code);                
            }
            catch(System.Exception ex){                
                c.ex = ex;                
            }            
            return c;
        }
        class ExecutionStatus: CompilationStatus {

            public object result;            
        }
        private ExecutionStatus ExecuteJavaScript(string code){
            
            ExecutionStatus c = new ExecutionStatus();

            try{
                c.javaScriptCode =  LoadJavaScriptLibrariesForExecution(code);
                c.result = _jsContext.Run(c.javaScriptCode);
            }
            catch(System.Exception ex){                
                c.ex = ex;                
            }
            return c;
        }
    }

    public class Program {

        static void Main(string[] args) {

            __Main(args);
        }
        public static void __Main(string[] args, bool throwException = false) {

            CoffeeScriptRunTime cs = new CoffeeScriptRunTime();

            if(args.Length==0){
                cs.DisplayLogo(true);

                Console.WriteLine(@"
How to execute a coffee script file
    CoffeeScriptRunTime.exe myfile.coffee
How to create the corresponding javascript file from a coffee script file
    CoffeeScriptRunTime.exe myfile.coffee -compile
How to execute a javascript script file
    CoffeeScriptRunTime.exe myfile.js
How to compile a javascript script file with Google Closure Compiler
    CoffeeScriptRunTime.exe myfile.js -compile

Others options:
-pause: Wait for an enter key at the end
-nologo: Do do not display the logo
-displayJavacript: -Display the JavaScript source created (CoffeeScript only)
");
            }

            var displayLogo           = !args.Contains("-nologo");
            var pause                 =  args.Contains("-pause");
            var displayJavaScriptCode =  args.Contains("-displayJavacript");
            var action                =  args.Contains("-compile") ? Action.Compile : Action.Run;

            cs.DisplayLogo(displayLogo);
            
            foreach(var p in args){

                if((!p.StartsWith("-"))&&(System.IO.File.Exists(p))){

                    if(System.IO.Path.GetExtension(p)==".coffee"){

                        var status = cs.CompileAndOrRunCoffeeScript(p, action, displayJavaScriptCode);
                        if (throwException && !status.Succeeded)
                            throw new ApplicationException(status.ToString());
                    }
                    else if(System.IO.Path.GetExtension(p)==".js"){

                        if(action==Action.Run){

                            cs.RunJavaScript(p);
                        }
                        else if(action== Action.Compile) {

                            ClosureCompiler closure = new ClosureCompiler();
                            var status = closure.Compile(p);
                            if (throwException && !status.Succeeded)
                                throw new ApplicationException(status.ToString());
                        }
                    }
                }
            }
            if(pause){
                Console.WriteLine("Hit enter to continue");
                Console.ReadLine();
            }
        }
    }
}
