using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MsgMgr.Utilities
{
    /// <summary>
    /// Priority level for logging
    /// </summary>
    public enum LogPriority
    {
        LOW = 0,
        MEDIUM = 1,
        HIGH = 2,
        WARNING = 3,
        CRITICAL = 4
    }

    /// <summary>
    /// Categories for logging.
    /// </summary>
    public enum LogCategory
    {
        INFO,
        ERROR,
        NETWORK,
        DEBUG,
        EXCEPTION,
        VERBOSE,
        ALL
    }

    /// <summary>
    /// Modes are:
    /// 
    /// PRIORITY_ONLY
    ///     -- disregard categories and only print messages called with Priority >= threshold
    /// PRIORITY_OR_CATEGORY
    ///     -- print messages called with Priority >= threshold, OR (which were called with one of the configured categories (or with the ALL category))
    /// PRIORITY_AND_CATEGORY
    ///     -- print messages called with Priority >= threshold, AND (which were called with one of the configured categories (or with the ALL category))
    /// CATEGORY_ONLY
    ///     -- disregard the priority and only print messages called with one of the configured categories (or with the ALL category)
    /// 
    /// </summary>
    public enum LogMode
    {
        PRIORITY_ONLY,
        PRIORITY_OR_CATEGORY,
        PRIORITY_AND_CATEGORY,
        CATEGORY_ONLY
    }

    /// <summary>
    /// Prints log messages to the console and/or to a specified log file
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class Logger : IDisposable
    {
        #region Static Singleton

        /// <summary>
        /// Lock used when initializing the singleton (to prevent multiple inits)
        /// </summary>
        private static object _initObjLock = new object();

        /// <summary>
        /// The singleton instance
        /// </summary>
        private static Logger _instance;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static Logger Instance
        {
            get
            {
                _instance.ThrowIfNull("Logger cannot be null when called!");

                return _instance;
            }
        }

        #region Public Static methods

        /// <summary>
        /// Initializes this instance, default is no log file, only log to console
        /// </summary>
        public static void Init(bool logAsync, LogPriority threshold, LogMode mode, params LogCategory[] categories)
        {
            PrivateInit(logAsync, true, null, threshold, mode, categories);
        }

        /// <summary>
        /// Initializes this instance using specified parameters.
        /// </summary>
        /// <param name="useConsole">if set to <c>true</c> print to the console</param>
        /// <param name="logFile">The log file. If null, don't print to a log file</param>
        public static void Init(bool logAsync, bool useConsole, string logFile, LogPriority threshold, LogMode mode, params LogCategory[] categories)
        {
            PrivateInit(logAsync, useConsole, logFile, threshold, mode, categories);
        }

        /// <summary>
        /// Stops the logger and disposes the singleton.
        /// </summary>
        public static void StopLogger()
        {
            lock(_initObjLock)
            {
                _instance.ThrowIfNull("Cannot stop or dispose logger which is null");
                _instance.Dispose();
                _instance = null;
            }
        }

        #endregion

        /// <summary>
        /// Calls the singleton constructor.
        /// </summary>
        /// <param name="useConsole">if set to <c>true</c> [use console].</param>
        /// <param name="logFile">The log file.</param>
        /// <param name="threshold">The threshold for priority level -- will print any message with priority >= threshold.</param>
        /// <param name="mode">The mode for this logger.</param>
        /// <param name="categories">The categories to print.</param>
        /// <exception cref="InvalidOperationException">Cannot reinitialize Logger!</exception>
        private static void PrivateInit(bool logAsync, bool useConsole, string logFile, LogPriority threshold, LogMode mode, params LogCategory[] categories)
        {
            bool shouldThrow;

            // lock around calling the constructor prevents multiple threads from constructing it
            lock (_initObjLock)
            {
                // _instance is not null after the first thread leaves this critical section
                shouldThrow = _instance.IsNotNull();
                _instance = new Logger(logAsync, useConsole, logFile, threshold, mode, categories);
            }

            // throw if this was called more than once
            if (shouldThrow)
            {
                throw new InvalidOperationException("Cannot reinitialize Logger!");
            }
        }

        #endregion
        
        # region Private members

        /// <summary>
        /// Set of categories for which to print messages
        /// </summary>
        private ICollection<LogCategory> _categories = new HashSet<LogCategory>();

        /// <summary>
        /// The threshold for priority levels
        /// </summary>
        private LogPriority _threshold;

        /// <summary>
        /// The mode of this logger
        /// </summary>
        private LogMode _mode;
        
        /// <summary>
        /// The cancellation token for the logging task
        /// </summary>
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        
        /// <summary>
        /// The queue of logs that the logging task pulls from
        /// </summary>
        private ConcurrentQueue<LogMessageDTO> _logQueue = new ConcurrentQueue<LogMessageDTO>();
        
        /// <summary>
        /// The log file name.  If null, no log file
        /// </summary>
        private string _logFile;
        
        /// <summary>
        /// Whether or not to print to the console
        /// </summary>
        private bool _useConsole;

        /// <summary>
        /// Whether or not to use the asyncronous logging capabilities
        /// </summary>
        private bool _logAsync;

        /// <summary>
        /// The logging task
        /// </summary>
        private Task _loggingTask;
        
        /// <summary>
        /// The file stream for the log file
        /// </summary>
        private StreamWriter _fileStream;

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="useConsole">if set to <c>true</c> [use console].</param>
        /// <param name="logFile">The log file.</param>
        /// <param name="threshold">The threshold.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="categories">The categories.</param>
        private Logger(bool logAsync, bool useConsole, string logFile, LogPriority threshold, LogMode mode, params LogCategory[] categories)
        {
            // set parameters
            _threshold = threshold;
            _mode = mode;
            _categories.AddRange(categories);
            _useConsole = useConsole;
            _logFile = logFile;
            _logAsync = logAsync;

            if (_logFile.IsNotNull())
            {
                _fileStream = new StreamWriter(_logFile, true);
            }

            if(_logAsync)
            {
                // start logging
                _loggingTask = LogAsync();
            }
        }

        #endregion

        #region Public Logging methods

        /// <summary>
        /// Adds a logging message to the FIFO logging queue.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="priority">The priority level of this message.</param>
        /// <param name="primaryCategory">The primary category of this message.</param>
        /// <param name="otherCategories">Other categories that this message could be included in.</param>
        public void LogMessage(string message, LogPriority priority, LogCategory primaryCategory, params LogCategory[] otherCategories)
        {
            // only queue the message if it's priority or categories indicate that it should be logged, based on the configuration of this logger
            if (ShouldLog(priority, primaryCategory, otherCategories))
            {
                LogMessageDTO toLog = new LogMessageDTO(message, priority, primaryCategory);

                if (_logAsync)
                {
                    QueueLog(toLog);
                }
                else
                {
                    WriteToLog(toLog);
                }
            }
        }
        
        /// <summary>
        /// Logs an exception.  Assumes HIGH priority but can be configured higher or lower with parameter
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="priority">The priority.</param>
        public void LogException(Exception ex, LogPriority priority = LogPriority.HIGH)
        {
            string message = "{0}: {1}: {2}".FormatStr(ex.GetType(), ex.Message, ex.StackTrace);

            LogMessageDTO toLog = new LogMessageDTO(message + " " + _logQueue.Count, LogPriority.HIGH, LogCategory.EXCEPTION);

            if (_logAsync)
            {
                QueueLog(toLog);
            }
            else
            {
                WriteToLog(toLog);
            }
        }

        #endregion
        
        #region Private helper methods

        /// <summary>
        /// Queues the message to be logged.
        /// </summary>
        /// <param name="message">The message.</param>
        private void QueueLog(LogMessageDTO message)
        {
            _logQueue.Enqueue(message);
        }

        private void WriteToLog(LogMessageDTO toLog)
        {
            // log apprpriately based on configuration
            if (_useConsole)
            {
                WriteToConsole(toLog);
            }

            if (_logFile.IsNotNull())
            {
                WriteToLogFile(toLog);
            }
        }

        /// <summary>
        /// Asynchronously pulls messages off the queue and logs them.
        /// </summary>
        /// <returns></returns>
        private Task LogAsync()
        {
            // action to run asynchronously
            Action logAction = new Action(() =>
            {
                LogMessageDTO toLog;

                // log until cancelled
                while (!_cancellationToken.IsCancellationRequested)
                {
                    // try dequeue returns false if nothing is in the queue
                    if (_logQueue.TryDequeue(out toLog))
                    {
                        WriteToLog(toLog);
                    }
                }

                // log a message saying that the logger is finishing, ignore ShouldLog for this
                toLog = new LogMessageDTO("Logger cancelled, terminating.", LogPriority.CRITICAL, LogCategory.INFO);
                
                WriteToLog(toLog);
            });

            //start task
            Task sendTask = new Task(logAction, _cancellationToken.Token, TaskCreationOptions.LongRunning);

            sendTask.Start();

            return sendTask;
        }

        /// <summary>
        /// Writes the log message to the console.
        /// </summary>
        /// <param name="toLog">DTO containing message to log.</param>
        private void WriteToConsole(LogMessageDTO toLog)
        {
            // could eventually also log the priority and category from DTO
            Console.WriteLine(toLog.Message);
        }

        /// <summary>
        /// Writes the log message to the log file.
        /// </summary>
        /// <param name="toLog">DTO containing message to log.</param>
        private void WriteToLogFile(LogMessageDTO toLog)
        {
            try
            {
                // could eventually also log the priority and category from DTO
                _fileStream.WriteLine(toLog.Message);
                _fileStream.Flush();
            }
            catch (IOException ex)
            {
                // log the exception
                // TODO: I expect that this would cause a loop of logging exceptions
                LogException(ex);
            }
        }

        /// <summary>
        /// Determines if the given priority and categories indicate that this message should be logged, based on the configuration of the logger.
        /// </summary>
        /// <param name="priority">The priority of the message.</param>
        /// <param name="primaryCategory">The primary category of the message.</param>
        /// <param name="otherCategories">The other categories of this message.</param>
        /// <returns></returns>
        private bool ShouldLog(LogPriority priority, LogCategory primaryCategory, params LogCategory[] otherCategories)
        {
           /*
            *  Print verbose messages only if they belong to a category or priority that is configured to be printed.  See truth tables:
            *                 
            *  loggerIsVerbose  messageTaggedVerbose   verboseFilter
            *        0                 0                    1      --> "Can log because message is not verbose"
            *        0                 1                    0      --> "Cannot log because message is verbose but logger is not"
            *        1                 0                    1      --> "Can log because message is not verbose"
            *        1                 1                    1      --> "Can log because message is verbose and logger is verbose"
            *
            *  result  verboseFilter   shouldPrint
            *     0          0              0     --> "message should not be logged based on category/priority, and it should not be logged because it was tagged verbose but the logger is not verbose"
            *     0          1              0     --> "message should not be logged based on category/priority"
            *     1          0              0     --> "message should not be logged because it was tagged verbose but the logger is not verbose"
            *     1          1              1     --> "message should be logged based on category/priority, and it is not the case that (the message was tagged verbose and the logger is not verbose)"
            *
            */

            bool result;

            bool verboseFilter = _categories.Contains(LogCategory.VERBOSE) || !(primaryCategory == LogCategory.VERBOSE || otherCategories.Contains(LogCategory.VERBOSE));

            switch (_mode)
            {
                case LogMode.CATEGORY_ONLY:
                    result = _categories.Contains(LogCategory.ALL) || _categories.Contains(primaryCategory) || _categories.ContainsAny(otherCategories);
                    break;
                case LogMode.PRIORITY_OR_CATEGORY:
                    result = (priority >= _threshold) || (_categories.Contains(LogCategory.ALL) || _categories.Contains(primaryCategory) || _categories.ContainsAny(otherCategories));
                    break;
                case LogMode.PRIORITY_AND_CATEGORY:
                    result = (priority >= _threshold) && (_categories.Contains(LogCategory.ALL) || _categories.Contains(primaryCategory) || _categories.ContainsAny(otherCategories));
                    break;
                case LogMode.PRIORITY_ONLY:
                    result = (priority >= _threshold);
                    break;
                default:
                    // completeness -- this is never hit
                    result = true;
                    break;
            }

            return result && verboseFilter;
        }

        #endregion 

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancellationToken.Cancel();

                    _loggingTask.Wait();

                    _fileStream.Close();
                    _fileStream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Logger() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

        #region Helper Class

        /// <summary>
        /// Helper class used as a data transfer object for the logging queue
        /// </summary>
        private class LogMessageDTO
        {
            public string Message { get; }

            public LogPriority Priority { get; }

            public LogCategory PrimaryCategory { get; }

            public LogMessageDTO(string message, LogPriority priority, LogCategory category)
            {
                // append timestamp automatically
                Message = "[{0}]: {1}".FormatStr(DateTime.Now, message);
                Priority = priority;
                PrimaryCategory = category;
            }
        }

        #endregion

    }
}
