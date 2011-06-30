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

    }
}
