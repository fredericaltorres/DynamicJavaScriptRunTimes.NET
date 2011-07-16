using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections;
using System.Dynamic;
using System.Reflection;
using DynamicSugar;

namespace CoffeeScriptRunTime {
    
    /// <summary>
    /// 
    /// </summary>
    public static class Resources {

        /// <summary>
        /// Return the fully qualified name of the resource file
        /// </summary>
        /// <param name="resourceFileName">File name of the resource</param>
        /// <returns></returns>
        private static string GetResourceFullName(string resourceFileName, Assembly assembly ) {
        
            foreach(var resource in assembly.GetManifestResourceNames())
                if(resource.EndsWith("."+resourceFileName))
                    return resource;
            throw new System.ApplicationException("Resource '{0}' not find in assembly '{1}'".format(resourceFileName, Assembly.GetExecutingAssembly().FullName));
        }
        /// <summary>
        /// Return the content of a text file embed as a resource.
        /// The function takes care of finding the fully qualify name, in the current
        /// assembly.
        /// </summary>
        /// <param name="resourceFileName">The file name of the resource</param>
        /// <returns></returns>
        public static byte [] GetBinResource(string resourceFileName, Assembly assembly) {

            byte[] buffer;
            var resourceFullName = GetResourceFullName(resourceFileName, assembly);
            var stream           = assembly.GetManifestResourceStream(resourceFullName);
            BinaryReader br      = new BinaryReader(stream);
            buffer               = br.ReadBytes((int)stream.Length);
            return buffer;
        }
        public static void SaveToFile(byte [] buffer, string fileName){

            if(System.IO.File.Exists(fileName))
                System.IO.File.Delete(fileName);

            var objFileStream = new FileStream(fileName, FileMode.Create);
            objFileStream.Write(buffer, 0, (int)buffer.Length);
            objFileStream.Close();
        }
    }    
}