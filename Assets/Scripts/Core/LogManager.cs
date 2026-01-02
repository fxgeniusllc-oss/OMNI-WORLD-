using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OmniWorld.Core
{
    /// <summary>
    /// Advanced logging system with severity levels, structured logging, and multiple output destinations
    /// Provides 100% visibility into system behavior for debugging and analytics
    /// </summary>
    public class LogManager : MonoBehaviour
    {
        private static LogManager _instance;
        private static readonly object _lock = new object();
        
        public static LogManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            GameObject go = new GameObject("LogManager");
                            _instance = go.AddComponent<LogManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                }
                return _instance;
            }
        }
        
        [Header("Logging Configuration")]
        [Tooltip("Minimum severity level to log")]
        public LogLevel minimumLevel = LogLevel.DEBUG;
        
        [Tooltip("Enable file logging")]
        public bool enableFileLogging = true;
        
        [Tooltip("Enable console logging")]
        public bool enableConsoleLogging = true;
        
        [Tooltip("Maximum log file size in MB")]
        public int maxLogFileSizeMB = 100;
        
        [Tooltip("Number of log files to retain")]
        public int maxLogFiles = 5;
        
        [Header("Performance")]
        [Tooltip("Use async logging to avoid blocking")]
        public bool asyncLogging = true;
        
        [Tooltip("Maximum queue size for async logging")]
        public int maxQueueSize = 1000;
        
        private Queue<LogEntry> logQueue = new Queue<LogEntry>();
        private StreamWriter logWriter;
        private string logFilePath;
        private long currentLogSize = 0;
        private int logFileIndex = 0;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeLogging();
        }
        
        private void InitializeLogging()
        {
            if (enableFileLogging)
            {
                string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                Directory.CreateDirectory(logDirectory);
                
                logFilePath = Path.Combine(logDirectory, $"omniworld_{DateTime.Now:yyyyMMdd_HHmmss}_{logFileIndex}.log");
                
                try
                {
                    logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
                    logWriter.AutoFlush = true;
                    
                    Info("=== OmniWorld Logging System Initialized ===", new { logFilePath, minimumLevel });
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize file logging: {ex.Message}");
                    enableFileLogging = false;
                }
            }
            
            // Clean up old log files
            CleanupOldLogs();
        }
        
        private void Update()
        {
            // Process queued log entries
            if (asyncLogging && logQueue.Count > 0)
            {
                lock (_lock)
                {
                    int processCount = Mathf.Min(10, logQueue.Count); // Process up to 10 per frame
                    for (int i = 0; i < processCount; i++)
                    {
                        if (logQueue.Count > 0)
                        {
                            LogEntry entry = logQueue.Dequeue();
                            WriteLog(entry);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Log a trace message (most verbose, development only)
        /// </summary>
        public static void Trace(string message, object data = null)
        {
            Instance.Log(LogLevel.TRACE, message, data);
        }
        
        /// <summary>
        /// Log a debug message (development information)
        /// </summary>
        public static void Debug(string message, object data = null)
        {
            Instance.Log(LogLevel.DEBUG, message, data);
        }
        
        /// <summary>
        /// Log an info message (general information)
        /// </summary>
        public static void Info(string message, object data = null)
        {
            Instance.Log(LogLevel.INFO, message, data);
        }
        
        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void Warn(string message, object data = null)
        {
            Instance.Log(LogLevel.WARN, message, data);
        }
        
        /// <summary>
        /// Log an error message
        /// </summary>
        public static void Error(string message, object data = null)
        {
            Instance.Log(LogLevel.ERROR, message, data);
        }
        
        /// <summary>
        /// Log a fatal error message
        /// </summary>
        public static void Fatal(string message, object data = null)
        {
            Instance.Log(LogLevel.FATAL, message, data);
        }
        
        /// <summary>
        /// Log an exception with full stack trace
        /// </summary>
        public static void Exception(Exception exception, string context = null)
        {
            string message = context != null ? $"{context}: {exception.Message}" : exception.Message;
            Instance.Log(LogLevel.ERROR, message, new { 
                exceptionType = exception.GetType().Name,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            });
        }
        
        private void Log(LogLevel level, string message, object data = null)
        {
            // Filter by minimum level
            if (level < minimumLevel)
                return;
            
            LogEntry entry = new LogEntry
            {
                Level = level,
                Message = message,
                Timestamp = DateTime.Now,
                Data = data != null ? SerializeData(data) : null,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                FrameCount = Time.frameCount
            };
            
            if (asyncLogging)
            {
                lock (_lock)
                {
                    if (logQueue.Count < maxQueueSize)
                    {
                        logQueue.Enqueue(entry);
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("Log queue full! Dropping log message.");
                    }
                }
            }
            else
            {
                WriteLog(entry);
            }
        }
        
        private void WriteLog(LogEntry entry)
        {
            string formattedMessage = FormatLogEntry(entry);
            
            // Console output
            if (enableConsoleLogging)
            {
                switch (entry.Level)
                {
                    case LogLevel.TRACE:
                    case LogLevel.DEBUG:
                    case LogLevel.INFO:
                        UnityEngine.Debug.Log(formattedMessage);
                        break;
                    case LogLevel.WARN:
                        UnityEngine.Debug.LogWarning(formattedMessage);
                        break;
                    case LogLevel.ERROR:
                    case LogLevel.FATAL:
                        UnityEngine.Debug.LogError(formattedMessage);
                        break;
                }
            }
            
            // File output
            if (enableFileLogging && logWriter != null)
            {
                try
                {
                    logWriter.WriteLine(formattedMessage);
                    currentLogSize += formattedMessage.Length;
                    
                    // Check if need to rotate log file
                    if (currentLogSize > maxLogFileSizeMB * 1024 * 1024)
                    {
                        RotateLogFile();
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to write log: {ex.Message}");
                }
            }
        }
        
        private string FormatLogEntry(LogEntry entry)
        {
            StringBuilder sb = new StringBuilder();
            
            // Timestamp
            sb.Append($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] ");
            
            // Level
            sb.Append($"[{entry.Level.ToString()}] ");
            
            // Thread and Frame
            sb.Append($"[T:{entry.ThreadId}] [F:{entry.FrameCount}] ");
            
            // Message
            sb.Append(entry.Message);
            
            // Data
            if (!string.IsNullOrEmpty(entry.Data))
            {
                sb.Append($" | Data: {entry.Data}");
            }
            
            return sb.ToString();
        }
        
        private string SerializeData(object data)
        {
            try
            {
                return JsonUtility.ToJson(data, false);
            }
            catch
            {
                return data.ToString();
            }
        }
        
        private void RotateLogFile()
        {
            try
            {
                logWriter?.Close();
                logWriter?.Dispose();
                
                logFileIndex++;
                string logDirectory = Path.GetDirectoryName(logFilePath);
                logFilePath = Path.Combine(logDirectory, $"omniworld_{DateTime.Now:yyyyMMdd_HHmmss}_{logFileIndex}.log");
                
                logWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
                logWriter.AutoFlush = true;
                currentLogSize = 0;
                
                Info("Log file rotated", new { newLogFile = logFilePath });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to rotate log file: {ex.Message}");
            }
        }
        
        private void CleanupOldLogs()
        {
            try
            {
                string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(logDirectory))
                    return;
                
                var logFiles = Directory.GetFiles(logDirectory, "omniworld_*.log");
                if (logFiles.Length > maxLogFiles)
                {
                    // Sort by creation time
                    Array.Sort(logFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));
                    
                    // Delete oldest files
                    int filesToDelete = logFiles.Length - maxLogFiles;
                    for (int i = 0; i < filesToDelete; i++)
                    {
                        File.Delete(logFiles[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to cleanup old logs: {ex.Message}");
            }
        }
        
        private void OnApplicationQuit()
        {
            Info("=== OmniWorld Shutting Down ===");
            
            // Process remaining queued logs
            while (logQueue.Count > 0)
            {
                LogEntry entry = logQueue.Dequeue();
                WriteLog(entry);
            }
            
            logWriter?.Close();
            logWriter?.Dispose();
        }
        
        private void OnDestroy()
        {
            logWriter?.Close();
            logWriter?.Dispose();
        }
    }
    
    /// <summary>
    /// Log severity levels
    /// </summary>
    public enum LogLevel
    {
        TRACE = 0,   // Most verbose, development only
        DEBUG = 1,   // Development information
        INFO = 2,    // General information
        WARN = 3,    // Warning conditions
        ERROR = 4,   // Error conditions
        FATAL = 5    // Critical failures
    }
    
    /// <summary>
    /// Internal log entry structure
    /// </summary>
    internal struct LogEntry
    {
        public LogLevel Level;
        public string Message;
        public DateTime Timestamp;
        public string Data;
        public int ThreadId;
        public int FrameCount;
    }
}
