using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace DynamicJavaScriptRunTimes {
   
    /// <summary>
    /// 
    /// </summary>
    public class SystemConsole
    {
        public SystemConsole() { }

        public void error(string s) {
            this.log(String.Format("[ERROR]{0}",s));
        }
        public void warn(string s) {
            this.log(String.Format("[WARNING]{0}",s));
        }
        public void info(string s) {
            this.log(String.Format("[INFO]{0}",s));
        }
        public void log(string s) {
            Console.WriteLine(s);
        }
    }
    
}
