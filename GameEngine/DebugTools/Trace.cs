using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GameEngine.DebugTools
{
    /// <summary>
    /// Provides a set of methods and properties that help you trace the execution of your code.
    /// </summary>
    public static class TraceLog
    {
        #region Inventory

        /// <summary>
        /// Log inventory 
        /// </summary>
        public static void TraceInventory()
        {
#if WINDOWS
            WriteWarning(DateTime.Now.ToString());
            WriteWarning("Hardware informations :");
            Trace.Indent();
            Write("OS : {0}", Environment.OSVersion.ToString());
            Write("Platform : {0}", IntPtr.Size == 4 ? "x86" : "x64");
            Write("SP : {0}", Environment.OSVersion.ServicePack);
            Write("Processor count : {0}", Environment.ProcessorCount);
            Trace.Unindent();

            WriteWarning("Software informations :");
            Trace.Indent();
            Write("CLR : {0}", Environment.Version.ToString());
            Trace.Unindent();

            Trace.Unindent();

            WriteWarning("GameEngine assembly :");
            Trace.Indent();
            Assembly asb = Assembly.GetCallingAssembly();
            Write("{0}", asb.ToString());

            var info = new FileInfo(asb.Location);
            Write("Creation time : {0}", info.CreationTime.ToString());


            Trace.Unindent();

            // Look through each loaded dll
            var list = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Select(name => name.FullName).ToList();
            list.Sort();

            WriteWarning("Referenced assemblies :");
            Trace.Indent();
            foreach (string name in list)
                Write(" - {0}", name);
            Trace.Unindent();
#endif
        }

        #endregion

        #region Write

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="message">Message to write</param>
        public static void WriteWarning(string message)
        {
            WriteMessage("[WAR] " + message);
        }

        private static void WriteMessage(string message)
        {
#if WINDOWS
            Trace.Write(message);
            Console.WriteLine(message);
#endif   
        }

        /// <summary>
        /// Writes a line to the log
        /// </summary>
        /// <param name="format">String format</param>
        /// <param name="args">Arguments</param>
        private static void Write(string format, params object[] args)
        {
            WriteMessage(string.Format(format, args));
        }

        #endregion

        #region Debug

        /// <summary>
        /// Writes a line to the log
        /// </summary>
        /// <param name="message">Message</param>
        [Conditional("DEBUG")]
        public static void Write(string message)
        {
            WriteMessage("[DEB] " + message);
        }

        #endregion
    }
}