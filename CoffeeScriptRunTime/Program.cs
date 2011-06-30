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
    class CoffeeScriptRunTime {

        const string CoffeeScriptVersion  = "1.1.1";
        const string DisplayJavaScriptTag = "#!DisplayJavaScript";
        
        dynamic _jsContext;
        string  _coffeeScriptFileName;
        bool    _displayJavaScriptFromCoffeeScript;
        
        public CoffeeScriptRunTime(){

            this._jsContext = new DynamicJavaScriptRunTimes.DynamicJavascriptContext(new Noesis.Javascript.JavascriptContext());
        }
        public string JavaScriptOutputfile{
            get{
                var d = System.IO.Path.GetDirectoryName(this._coffeeScriptFileName);
                var n = System.IO.Path.GetFileNameWithoutExtension(this._coffeeScriptFileName);
                return @"{0}\{1}.js".format(d, n);
            }
        }
        public bool Compile(string fileName, Action action, bool displayJavaScriptCode){

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
            return true;
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
        private string LoadAndPreprocessCoffeeScriptSource(string fileName) {

            string coffeeScriptCode = System.IO.File.ReadAllText(fileName);
            if (coffeeScriptCode.Contains(DisplayJavaScriptTag)) 
                _displayJavaScriptFromCoffeeScript = true;
            coffeeScriptCode = coffeeScriptCode.Replace(Environment.NewLine, "\\n");
            return coffeeScriptCode;
        }
        public void DisplayLogo(bool displayLogo) {

            if (displayLogo) {

                Console.Write("CoffeeScript Run-time v 0.1, ");
                Console.Write("CoffeeScript Compiler v {0}, ".format(CoffeeScriptVersion));
                string v = _jsContext.GetVersionInfo();
                Console.WriteLine("{0}.".format(v));
            }
        }
        class CompilationStatus {

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

    class Program {

        static void Main(string[] args) {

            CoffeeScriptRunTime cs = new CoffeeScriptRunTime();

            if(args.Length==0){
                cs.DisplayLogo(true);

                Console.WriteLine(@"
How to execute a coffee script file
    CoffeeScriptRunTime.exe myfile.coffee
How to create the corresponding javascript file from a coffee script file
    CoffeeScriptRunTime.exe myfile.coffee -compile

Others options
-pause - Wait for an enter key at the end
-nologo - Do do not display the logo
-displayJavacript - Display the JavaScript source created.
");
            }

            var displayLogo           = !Environment.CommandLine.Contains("-nologo");
            var pause                 = Environment.CommandLine.Contains("-pause");
            var displayJavaScriptCode = Environment.CommandLine.Contains("-displayJavacript");
            var action                = Environment.CommandLine.Contains("-compile") ? Action.Compile : Action.Run;

            cs.DisplayLogo(displayLogo);
            
            foreach(var p in args){

                if((!p.StartsWith("-"))&&(System.IO.File.Exists(p))){

                    cs.Compile(p, action, displayJavaScriptCode);
                }
            }
            if(pause){
                Console.WriteLine("Hit enter to continue");
                Console.ReadLine();
            }
        }
    }
}
