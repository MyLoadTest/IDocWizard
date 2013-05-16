namespace MyLoadTest.IDoc
{
    using System;
    using System.IO;
    
    /// <summary>
    /// DebugLog is an optional trace log for debugging the VuGen Validator DLL.
    /// To enable the logging, copy a file called validator.log to the same directory as validator.dll
    /// The file will be overwritten each time the Validator is run.
    /// Note: code heavily borrows from http://www.codeproject.com/Articles/21338/A-C-Central-Logging-Mechanism-using-the-Observer-a
    /// Note: It is assumed that there will only ever be one Validator running at a time, or else they will fight over the validator.log (i.e. overwrite each other).
    /// <example>
    /// DebugLog log = DebugLog.Instance;
    /// log.Write("hello world");
    /// </example>
    /// </summary>
    public class DebugLog
    {
        private static object mLock = new object();
        private static DebugLog log = null;
        private static StreamWriter logStream = null;
        
        private static bool loggingEnabled = false;
        private const string fileName = "generator.log";
        
        // The public Instance property everyone uses to access the DebugLog
        public static DebugLog Instance {
            get {
                // If this is the first time we're referring to the
                // singleton object, the private variable will be null.
                if (log == null) {
                    // for thread safety, lock an object when
                    // instantiating the new Logger object. This prevents
                    // other threads from performing the same block at the
                    // same time.
                    lock(mLock) {
                        // Two or more threads might have found a null
                        // mLogger and are therefore trying to create a 
                        // new one. One thread will get to lock first, and
                        // the other one will wait until mLock is released.
                        // Once the second thread can get through, mLogger
                        // will have already been instantiated by the first
                        // thread so test the variable again. 
                        if (log == null) {
                            log = new DebugLog();
                        }
                    }
                }
                return log; 
            }
        }        
        
        /// <summary>
        /// Constructor. Don't allow other classes to create instances of this class.
        /// </summary>
        private DebugLog() {
            // If there is a file called "validator.log" in the validator.dll directory, then debug information will be written to it.
            if (File.Exists(fileName) == true) {
                loggingEnabled = true;
            }
            
            // If logging is enabled, open file for writing (overwrite file contents).
            if (loggingEnabled == true) {
                logStream = new StreamWriter(fileName, false);
                // Note: No need to call logStream.Close() in a Destructor, as the object is already 
                // disposed of, giving a "Cannot access a closed file" if this is attempted.
            }
        }
        
        /// <summary>
        /// If logging is enabled, this method writes a message to validator.log.
        /// TODO: do I need a second Write method, to handle the case where the message has {brackets} (causes runtime exception).
        /// </summary>
        /// <param name="message">Message to write to the output log. This accepts varargs string formatting the same as Console.WriteLine().</param>
        public void Write(string message, params Object[] args) { // Note use of "params" keyword for varargs
            if (loggingEnabled == true) {
                // Note: timestamp is current local time.
                if (args.Length == 0) {
                    logStream.WriteLine(DateTime.Now + ":: " + message);
                } else {
                    logStream.WriteLine(DateTime.Now + ":: " + String.Format(message, args));
                }
                logStream.Flush();
            }
            return;
        }
    }
}
