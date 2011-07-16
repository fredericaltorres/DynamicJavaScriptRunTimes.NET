using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  DynamicSugar;
using System.IO;
using System.Diagnostics;

namespace CoffeeScriptRunTime {


    /// <summary>
    /// 
    /// </summary>
    class RunnerExecutionInfo {

        public string Output;
        public string ErrorOutput;
        public int Time;
        public int ErrorLevel;

        public RunnerExecutionInfo(){

            Output      = "";
            ErrorOutput = "";
            Time        = -1;
            ErrorLevel  = -1;
        }
    }

    class Runner {

        public static RunnerExecutionInfo Execute(string exe, string commandLine) {

            var e                     = new RunnerExecutionInfo();
            e.Time                    = Environment.TickCount;
            e.ErrorLevel              = -1;
            StreamReader outputReader = null;
            StreamReader errorReader  = null;
            try {
                ProcessStartInfo processStartInfo       = new ProcessStartInfo(exe, commandLine);
                processStartInfo.ErrorDialog            = false;
                processStartInfo.UseShellExecute        = false;
                processStartInfo.RedirectStandardError  = true;
                processStartInfo.RedirectStandardInput  = false;
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.CreateNoWindow         = true;
                processStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;
                //processStartInfo.WorkingDirectory       = this.NodePath;

                Process process                         = new Process();

                process.StartInfo                       = processStartInfo;
                bool processStarted                     = process.Start();

                if (processStarted) {
                    outputReader = process.StandardOutput;
                    errorReader  = process.StandardError;
                    process.WaitForExit();
                    e.ErrorLevel = process.ExitCode;
                    e.Output     = outputReader.ReadToEnd();
                    e.ErrorOutput= errorReader.ReadToEnd();
                }
            }
            catch (Exception ex) {
                e.ErrorOutput += "Error lanching the nodejs.exe = {0}".format(ex.ToString());
            }
            finally {
                if (outputReader != null) 
                    outputReader.Close();
                if (errorReader != null)
                    errorReader.Close();

                e.Output      = e.Output.Replace("\n", "\r\n");
                e.ErrorOutput = e.ErrorOutput.Replace("\n", "\r\n");
            }
            e.Time = Environment.TickCount - e.Time;
            return e;
        }


    }
}
