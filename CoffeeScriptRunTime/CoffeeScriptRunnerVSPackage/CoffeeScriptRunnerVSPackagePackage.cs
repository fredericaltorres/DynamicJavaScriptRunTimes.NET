using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using System.IO;
using System.Reflection;
using System.Text;


namespace FredericTorres.CoffeeScriptRunnerVSPackage
{
    // http://blogs.msdn.com/b/vsxue/archive/2007/11/15/tutorial-2-creating-your-first-vspackage.aspx
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidCoffeeScriptRunnerVSPackagePkgString)]
    public sealed class CoffeeScriptRunnerVSPackagePackage : VSXToolkit.VSPackage
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CoffeeScriptRunnerVSPackagePackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {            
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandRunCoffeeScript = new CommandID(
                    GuidList.guidCoffeeScriptRunnerVSPackageCmdSet,
                    (int)PkgCmdIDList.cmdIdRunCoffeeScript
                );
                MenuCommand menuItem = new MenuCommand(
                    MenuItemCallback,
                    menuCommandRunCoffeeScript 
                );
                mcs.AddCommand( menuItem );

                CommandID menuCommandRunJavaScript = new CommandID(
                    GuidList.guidCoffeeScriptRunnerVSPackageCmdSet,
                    (int)PkgCmdIDList.cmdIdRunJavaScript
                );
                MenuCommand menuItem2 = new MenuCommand(
                    MenuItemCallback, 
                    menuCommandRunJavaScript 
                );
                mcs.AddCommand( menuItem2 );
            }
            base.dte = (EnvDTE80.DTE2)GetService(typeof(EnvDTE.DTE));
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {            
            var uiShell      = (IVsUIShell)GetService(typeof(SVsUIShell));
            var f            = this.GetCurrentFileName();
            var ext          = System.IO.Path.GetExtension(f);
            var isJavaScript = ext==".js";
            
            if((ext==".coffee")||(ext==".js")){
                
                var source = this.GetCurrentTextFileState();
                var debug = source.Contains("//!debug.extension") || source.Contains("#!debug.extension");

                f = this.GetCurrentTextFileStateInFileName(); // The user do not have to save the file, to execute it.
                this.ClearOutputWindow();

                base.SetStatusBar(String.Format("Running {0}", System.IO.Path.GetFileName(f)));
                
                var coffeeScriptRunTimeExe = String.Format(@"{0}\CoffeeScriptRunTime.exe",PackagePath);
                var parameters             = String.Format(@"-nologo ""{0}""",f);
                var info                   = Execute(coffeeScriptRunTimeExe, parameters);

                this.WriteToOutputWindow(info.ToString());

                if(debug){
                    var b = new StringBuilder(1000);
                    b.AppendFormat("Debuging Extension Info").AppendLine();
                    b.Append("".PadLeft(70,'-')).AppendLine();
                    b.AppendFormat("Time:{0}", DateTime.Now).AppendLine();
                    b.AppendFormat("Extension DLL:{0}", Assembly.GetExecutingAssembly().Location).AppendLine();
                    b.AppendFormat("Domain:{0}, User:{1}", Environment.UserDomainName, Environment.UserName).AppendLine();
                    b.AppendFormat("Machine:{0}", Environment.MachineName).AppendLine();
                    this.WriteToOutputWindow(b.ToString());
                }
                
                base.SetStatusBar("");
            }
        }
        private string PackagePath{
            get{
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        class CoffeeScriptExecutionInfo {

            public string   Output;
            public string   Prg;
            public string   ErrorOutput;
            public int      Time;
            public string   CommandLine;
            public int      ErrorLevel;
            
            public bool Succeeded {
                get{
                    return this.ErrorLevel == 0;
                }
            }
            public CoffeeScriptExecutionInfo(){

                Output      = "";
                ErrorOutput = "";
                Time        = -1;
                CommandLine = "";
                ErrorLevel  = -1;
            }
            private string GetFullCommandLine() {                

                return String.Format(@"""{0}"" {1}",this.Prg, this.CommandLine);
            }
            public override string ToString() {

                System.Text.StringBuilder b = new System.Text.StringBuilder(1024);
                
                b.AppendFormat("{0}", this.Output).AppendLine();                                    
                b.AppendFormat("ErrorLevel:{0}, Time:{1}", this.ErrorLevel, (this.Time/1000.0).ToString("0.0")).AppendLine();

                return b.ToString();
            }
        }    
        private CoffeeScriptExecutionInfo Execute(string prg, string commandLine) {

            string outPutFileName     = String.Format(@"{0}\CoffeeScriptRunTimeOutput.txt", System.Environment.GetEnvironmentVariable("TEMP"));
            string batchFileName      = String.Format(@"{0}\CoffeeScriptRunTime.bat", System.Environment.GetEnvironmentVariable("TEMP"));
            var e                     = new CoffeeScriptExecutionInfo();
            e.Time                    = Environment.TickCount;
            e.ErrorLevel              = -1;            
            try {                

                if(System.IO.File.Exists(outPutFileName))
                    System.IO.File.Delete(outPutFileName);

                e.CommandLine                           = String.Format(@"""{0}"" {1} >""{2}""",prg, commandLine, outPutFileName);
                System.IO.File.WriteAllText(batchFileName, e.CommandLine);

                ProcessStartInfo processStartInfo       = new ProcessStartInfo("cmd.exe", String.Format(@"/c ""{0}"" ", batchFileName));
                processStartInfo.ErrorDialog            = false;
                processStartInfo.UseShellExecute        = true;
                processStartInfo.WindowStyle            = ProcessWindowStyle.Hidden;
                Process process                         = new Process();
                process.StartInfo                       = processStartInfo;
                bool processStarted                     = process.Start();

                if (processStarted) {
                    process.WaitForExit();                    
                    e.ErrorLevel = process.ExitCode;
                    e.Output = System.IO.File.ReadAllText(outPutFileName);
                }
            }
            catch (Exception ex) {

                e.ErrorOutput += String.Format("Error lanching the {0} = {1}", prg, ex.Message);
            }
            finally {
                
            }
            e.Time = Environment.TickCount - e.Time;
            return e;
        }

        private const string COFFEE_SCRIPT_TAB_NAME = "CoffeeScript";
        private const string JAVA_SCRIPT_TAB_NAME = "JavaScript";

        private string GetTabName(){

            return String.Format("{0}/{1}", JAVA_SCRIPT_TAB_NAME,COFFEE_SCRIPT_TAB_NAME);
        }  
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        private void ClearOutputWindow(){

            base.ClearOutputWindow(GetTabName());
        }    
        private void WriteToOutputWindow(string m){

            base.WriteToOutputWindow(m ,GetTabName());
        }    
        
    }
}
