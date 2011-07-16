using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using DynamicSugar;

namespace CoffeeScriptRunTime {

    class ClosureCompiler {

        private string JavaExe {
            get{
                var exe = "java.exe";
                var p = @"C:\Program Files\Java\jre6\bin\{0}".format(exe);
                if(System.IO.File.Exists(p))
                    return p;
                return exe;                
            }
        }
        private string GetJavaCommandLine(string javaScriptFile) {

            var jsfile_path = System.IO.Path.GetDirectoryName(javaScriptFile);
            if(!jsfile_path.EndsWith("\\"))
                jsfile_path+="\\";
            
            var javaCommandLine = @"-jar ""{JAR}"" --js ""{JSFILE_PATH}{JSFILE_NOEXT}.js"" --js_output_file ""{LOCALFOLDER}\output.js""".Format(
                new {
                    LOCALFOLDER   = this.LocalFolder,
                    JAVAEXE       = this.JavaExe,
                    JAR           = this.JavaJarFile,
                    JSFILE_NOEXT  = System.IO.Path.GetFileNameWithoutExtension(javaScriptFile),
                    JSFILE_PATH   = jsfile_path
                }
            );
            return javaCommandLine;
        }
        private string LocalFolder {
            get{
                var f = @"{0}\CoffeeScriptRunTime.ClosureCompiler".format(Environment.GetEnvironmentVariable("TEMP"));
                if(!System.IO.Directory.Exists(f))
                    System.IO.Directory.CreateDirectory(f);
                return f;
            }
        }
        private string JavaJarFile {
            get{
                return @"{0}\compiler.jar".format(LocalFolder);
            }
        }
        public ClosureCompiler(){

            UnpackJarFile();
        }
        private void UnpackJarFile(){

            if(!System.IO.File.Exists(this.JavaJarFile))
                Resources.SaveToFile(
                    Resources.GetBinResource("compiler.jar", Assembly.GetExecutingAssembly()),
                    this.JavaJarFile
                    );
        } 
        public class CompileInfo {

            public string Output;
            public string ErrorOutput;
            public int Time;
            public int ErrorLevel;

            public bool Succeeded{
                get{
                    return this.ErrorLevel==0;
                }
            }
            public override string ToString() {

                return this.ErrorOutput;
            }
        }
        public CompileInfo Compile(string javaScriptFileName){

            var compileInfo         = new CompileInfo();
            var info                = Runner.Execute(this.JavaExe, this.GetJavaCommandLine(javaScriptFileName));
            compileInfo.ErrorLevel  = info.ErrorLevel;
            compileInfo.Output      = info.Output;
            compileInfo.ErrorOutput = info.ErrorOutput;
            return compileInfo;
        }
    }
}

