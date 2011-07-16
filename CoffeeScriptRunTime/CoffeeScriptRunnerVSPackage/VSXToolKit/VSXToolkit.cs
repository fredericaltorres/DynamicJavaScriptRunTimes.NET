using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell.Interop;
//using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

//using Microsoft.VisualStudio.Shell.Interop;
//using Microsoft.VisualStudio.Shell;
using Shell = Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
//using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio;
using EnvDTE80;

using EnvDTE;
using EnvDTE90;

namespace VSXToolkit {

    public class VSPackage : Package {

        protected DTE2 dte;

        protected void SetStatusBar(string m){

            dte.StatusBar.Text = m;

            if(String.IsNullOrEmpty(m)){

                dte.StatusBar.Animate(false, vsStatusAnimation.vsStatusAnimationDeploy);
            }
            else{
                dte.StatusBar.Animate(true, vsStatusAnimation.vsStatusAnimationDeploy);
            }
        }
        public string GetCurrentTextFileState(){

            //TextDocument doc = (TextDocument)(dte.ActiveDocument.Object("TextDocument"));
            //if (doc.Selection.IsEmpty){
            //    doc.Selection.SelectAll();
            //}
            //string s = doc.Selection.Text;
            //return s;

            TextDocument doc = (TextDocument)(dte.ActiveDocument.Object("TextDocument"));
            var p = doc.StartPoint.CreateEditPoint();
            string s = p.GetText(doc.EndPoint);
            return s;            
        }

        public string GetCurrentTextFileStateInFileName(){

            var f = this.GetCurrentFileName();
            var tmpFileName = String.Format(@"{0}\{1}", System.Environment.GetEnvironmentVariable("TEMP"), System.IO.Path.GetFileName(f));
            System.IO.File.WriteAllText(tmpFileName, this.GetCurrentTextFileState());
            return tmpFileName;
        }

        protected string GetCurrentFileName(){
            try{
                string f = this.Application.ActiveDocument.FullName;
                return f;
            }
            catch{
                return null;
            }
        }
        

        public void MsgBox(string m){

            // Show a Message Box to prove we were here            
            Guid clsid = Guid.Empty;
            int result;
            uiShell.ShowMessageBox( 0, ref clsid, "InCisif.net.VSXToolkit", m, string.Empty, 0, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST, OLEMSGICON.OLEMSGICON_INFO, 0, out result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
       public void OpenExistingDocument(string fileName){

           
            if(System.IO.File.Exists(fileName)){
                
                this.Application.Documents.Open(fileName, "text", false);
            }
            else{
                this.MsgBox(string.Format("Cannot find file:'{0}'", fileName));
            }
       }
       public void ReloadCurrentDocument() {

           var f = CloseCurrentDocument();
           System.Threading.Thread.Sleep(2000);
           OpenExistingDocument(f);
       }
       public string CloseCurrentDocument() {

           string f = this.Application.ActiveDocument.FullName;
           this.Application.ActiveDocument.Close();
           return f;
       }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outWindow"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public IVsOutputWindowPane OutputWindow(){
            
            IVsOutputWindow outWindow = base.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid generalWindowGuid = VSConstants.GUID_OutWindowGeneralPane;
            IVsOutputWindowPane windowPane;
            outWindow.GetPane(ref generalWindowGuid, out windowPane);
            return windowPane;            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="outWindow"></param>
        /// <param name="m"></param>
        public void WriteToOutputWindow(string m){
            
            IVsOutputWindowPane w = OutputWindow();
            if(w!=null){
                w.OutputString(string.Format("{0}\n",m));
                w.Activate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static System.Collections.Generic.Dictionary<string , Guid> CustomOutputTab = new System.Collections.Generic.Dictionary<string,Guid>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public IVsOutputWindowPane GetCustomOutputWindow(string tabName){
            
            IVsOutputWindow output = base.GetService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            Guid guidFindRefOutput;

            if(!CustomOutputTab.ContainsKey(tabName)){

                guidFindRefOutput = new Guid();
                output.CreatePane(ref guidFindRefOutput, tabName, 1, 1);
                CustomOutputTab.Add(tabName, guidFindRefOutput);
            }

            guidFindRefOutput = CustomOutputTab[tabName];
            IVsOutputWindowPane pane;
            output.GetPane(ref guidFindRefOutput, out pane);            
            return pane;
        }

        public void ClearOutputWindow(string outputWindow){

            var w = this.GetCustomOutputWindow(outputWindow);
            w.Clear();
        }
        public void WriteToOutputWindow(string m, string outputWindow){

            var w = this.GetCustomOutputWindow(outputWindow);
            if(m!=null){
                w.OutputString(m+"\n");
            }
            w.Activate();
        }
        /// <summary>
        /// 
        /// </summary>
        private IVsUIShell uiShell {
            get{
                IVsUIShell _uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
                return _uiShell;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected EnvDTE80.DTE2 Application {
            get{
                EnvDTE80.DTE2 _applicationObject    = (EnvDTE80.DTE2)this.GetService(typeof(SDTE));
                return _applicationObject;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void InsertTextIntoActiveDocument( string text){

            InsertTextIntoActiveDocument(this.Application.ActiveDocument, text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentMethodName(){

            try{
                EnvDTE.TextSelection SelectedText       = this.Application.ActiveDocument.Selection as EnvDTE.TextSelection;
                EnvDTE.TextPoint tp                     = SelectedText.TopPoint as EnvDTE.TextPoint;
                EnvDTE.CodeElement c                    = this.Application.ActiveDocument.ProjectItem.FileCodeModel.CodeElementFromPoint(tp, EnvDTE.vsCMElement.vsCMElementFunction);
                EnvDTE80.CodeFunction2 CurrentFunction  = c as EnvDTE80.CodeFunction2;
                return c.Name;
            }
            catch{

                return "";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public System.Collections.Generic.List<string> GetMethodNames(){

            System.Collections.Generic.List<string> M = new List<string>();

            EnvDTE80.FileCodeModel2 FileCodeModel = this.Application.ActiveDocument.ProjectItem.FileCodeModel as EnvDTE80.FileCodeModel2;
            EnvDTE.CodeElements elements = FileCodeModel.CodeElements;

            foreach(EnvDTE.CodeElement element in elements){

                if(element.Kind == EnvDTE.vsCMElement.vsCMElementNamespace){

                    foreach(EnvDTE.CodeElement element2 in element.Children){

                        if(element2.Kind == EnvDTE.vsCMElement.vsCMElementClass){

                            EnvDTE80.CodeClass2 CurrentClass = element2 as EnvDTE80.CodeClass2;

                            foreach(EnvDTE.CodeElement element3 in element2.Children){

                                if(element3.Kind == EnvDTE.vsCMElement.vsCMElementFunction){

                                    EnvDTE80.CodeFunction2 CurrentFunction = element3 as EnvDTE80.CodeFunction2;
                                    M.Add(CurrentFunction.Name);
                                }
                            }
                        }
                    }
                }
            }
            return M;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentClassName(){

            EnvDTE80.FileCodeModel2 FileCodeModel = this.Application.ActiveDocument.ProjectItem.FileCodeModel as EnvDTE80.FileCodeModel2;
            EnvDTE.CodeElements elements = FileCodeModel.CodeElements;

            foreach(EnvDTE.CodeElement element in elements){

                if(element.Kind == EnvDTE.vsCMElement.vsCMElementNamespace){

                    foreach(EnvDTE.CodeElement element2 in element.Children){

                        if(element2.Kind == EnvDTE.vsCMElement.vsCMElementClass){

                            EnvDTE80.CodeClass2 CurrentClass = element2 as EnvDTE80.CodeClass2;
                            return CurrentClass.Name;
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool BuildSolution(){
            try{                
                EnvDTE80.SolutionBuild2 b = this.Application.Solution.SolutionBuild as EnvDTE80.SolutionBuild2 ;
                b.Build(true);
                return (b.LastBuildInfo==0);
            }
            catch(System.Exception ex){
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }
        public int GetErrorCountBuilding(){
            try{                
                EnvDTE80.SolutionBuild2 b = this.Application.Solution.SolutionBuild as EnvDTE80.SolutionBuild2 ;
                return b.LastBuildInfo;
            }
            catch(System.Exception ex){
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetProjectProperty(string name, string value){
            
            EnvDTE.Project P = this.Application.ActiveDocument.ProjectItem.ContainingProject;
            
            foreach(EnvDTE.Property p in P.ConfigurationManager.ActiveConfiguration.Properties){

                if(name.ToLower()==p.Name.ToLower()){

                    p.let_Value(value);
                    P.Save(P.FileName);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        public static void Wait(int seconds){

            for(int ii=0; ii<seconds; ii++){

                for (int i = 0; i < 10; i++) {

                    System.Threading.Thread.Sleep(100);
                    System.Windows.Forms.Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool RunSolution(bool debug){
            try{

                //EnvDTE80.Events2 a;
                //a.DebuggerEvents.OnEnterRunMode

                EnvDTE80.Solution2      s = this.Application.Solution as EnvDTE80.Solution2;
                EnvDTE80.SolutionBuild2 b = this.Application.Solution.SolutionBuild as EnvDTE80.SolutionBuild2;
                if(debug){

                    b.Debug();
                    Wait(5);
                    while(this.Application.Debugger.CurrentMode==EnvDTE.dbgDebugMode.dbgRunMode){

                        Wait(1);
                    }
                }
                else{

                    b.Run();
                }
                return true;
            }
            catch(System.Exception ex){
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="text"></param>
        public static void InsertTextIntoActiveDocument(EnvDTE.Document d, string text){

            if((!String.IsNullOrEmpty(text))&&(!String.IsNullOrEmpty(text.Trim()))){

                EnvDTE.TextSelection SelectedText   = d.Selection as EnvDTE.TextSelection;
                EnvDTE.EditPoint TopPoint           = SelectedText.TopPoint.CreateEditPoint();
                TopPoint.LineUp(1);
                TopPoint.EndOfLine();
                TopPoint.Insert(text);
            }
        }

    }
}
