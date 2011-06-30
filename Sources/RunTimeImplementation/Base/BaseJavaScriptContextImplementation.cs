using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using DynamicSugar;

namespace DynamicJavaScriptRunTimes {

    public class BaseJavaScriptContextImplementation {

        public virtual object Run(string script)
        {
            return null;
        }

        /// <summary>
        /// Call a function
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Call(string functionName, params object[] parameters) {

            var functionID = "__parameters_{0}".format(Guid.NewGuid().ToString("N"));

            if (parameters.Length == 0)
            {
                return this.Run("{0}();".format(functionName));
            }
            else
            {
                var a = new StringBuilder(1024);
                a.Append("(");
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].GetType().FullName.ToLower();

                    if (parameterType == "system.string")
                    {
                        a.AppendFormat("'{0}'", parameters[i]);
                    }
                    else if (parameterType == "system.boolean")
                    {
                        a.AppendFormat("{0}", parameters[i].ToString().ToLower());
                    }
                    else if (parameterType == "system.datetime")
                    {
                        DateTime d = (DateTime)parameters[i];
                        a.AppendFormat("new Date({0}, {1}, {2}, {3}, {4}, {5})",
                            d.Year, d.Month - 1, d.Day, d.Hour, d.Minute, d.Second
                            );
                    }
                    else
                    {
                        a.AppendFormat("{0}", parameters[i]);
                    }
                    if (i < parameters.Length - 1)
                        a.Append(", ");
                }
                a.Append(")");

                var exp = "{0}{1};".format(functionName, a);
                return this.Run(exp);
            }
        }

         public object CallBU(string functionName, params object[] parameters) {

            var functionID = "__parameters_{0}".format(Guid.NewGuid().ToString("N"));

            if (parameters.Length == 0)
            {
                return this.Run("{0}();".format(functionName));
            }
            else
            {
                var a = new StringBuilder(1024);
                a.AppendFormat("var {0} = [", functionID);
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameterType = parameters[i].GetType().FullName.ToLower();

                    if (parameterType == "system.string")
                    {
                        a.AppendFormat("'{0}'", parameters[i]);
                    }
                    else if (parameterType == "system.boolean")
                    {
                        a.AppendFormat("{0}", parameters[i].ToString().ToLower());
                    }
                    else if (parameterType == "system.datetime")
                    {
                        DateTime d = (DateTime)parameters[i];
                        a.AppendFormat("new Date({0}, {1}, {2}, {3}, {4}, {5})",
                            d.Year, d.Month - 1, d.Day, d.Hour, d.Minute, d.Second
                            );
                    }
                    else
                    {
                        a.AppendFormat("{0}", parameters[i]);
                    }
                    if (i < parameters.Length - 1)
                        a.Append(", ");
                }
                a.Append("];");

                var exp = "{0}\n{1}.apply(this,{2});".format(a, functionName, functionID);
                return this.Run(exp);
            }
        }
    }   
    
}
