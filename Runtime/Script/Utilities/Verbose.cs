
using System;
using System.Diagnostics;

namespace ProjectCore.Utilities
{
    public static class Verbose
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Exception
        }
        
        private static LogLevel _currentLogLevel = LogLevel.Exception;
        
        public static void SetLogLevel(LogLevel level)
        {
            _currentLogLevel = level;
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void D(string message)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#3498db>[INFO]</color> {message}");
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void D(string message, string hexColor)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#3498db>[INFO]</color> <color={hexColor}>{message}</color>");
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void DFormat(string format, params object[] args)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.LogFormat($"<color=#3498db>[INFO]</color> {format}", args);
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void W(string message)
        {
            if (_currentLogLevel >= LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarning($"<color=#f39c12>[WARNING]</color> {message}");
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void WFormat(string format, params object[] args)
        {
            if (_currentLogLevel >= LogLevel.Warning)
            {
                UnityEngine.Debug.LogWarningFormat($"<color=#f39c12>[WARNING]</color> {format}", args);
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void E(string message)
        {
            if (_currentLogLevel >= LogLevel.Error)
            {
                UnityEngine.Debug.LogError($"<color=#e74c3c>[ERROR]</color> {message}");
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void E(string format, params object[] args)
        {
            if (_currentLogLevel >= LogLevel.Error)
            {
                UnityEngine.Debug.LogErrorFormat($"<color=#e74c3c>[ERROR]</color> {format}", args);
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void Ex(Exception exception)
        {
            if (_currentLogLevel >= LogLevel.Exception)
            {
                UnityEngine.Debug.LogException(exception);
            }
        }

        [Conditional("ACTIONFIT_DEBUG")]
        public static void Ex(string message, Exception exception)
        {
            if (_currentLogLevel >= LogLevel.Exception)
            {
                UnityEngine.Debug.LogError($"<color=#9b59b6>[EXCEPTION]</color> {message}\n{exception}");
            }
        }
        
        [Conditional("ACTIONFIT_DEBUG")]
        public static void AssetLoaded(string assetName, float loadTime)
        {
            if (_currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#2ecc71>[ASSET]</color> 로드 완료: {assetName} (소요 시간: {loadTime:F2}초)");
            }
        }

        [Conditional("ACTIONFIT_DEBUG")]
        public static void DIf(bool condition, string message)
        {
            if (condition && _currentLogLevel >= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"<color=#3498db>[CONDITIONAL]</color> {message}");
            }
        }
    }
}