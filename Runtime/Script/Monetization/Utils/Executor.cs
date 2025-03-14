
using System;
using System.Collections;
using System.Collections.Generic;
using ProjectCore.Utilities;
using UnityEngine;

namespace ProjectCore.Monetize
{
    public static class Executor
    {
        #region Fields

        private static MonoBehaviour _executeRunner;
        private static List<Coroutine> _activeCoroutines = new();

        private static bool _isInitialized;

        #endregion

        public static void Initialize()
        {
            if (_isInitialized)
            {
                Verbose.W("[Executor] already initialized");
                return;
            }

            var gameObject = new GameObject("[MONETIZATION EXECUTOR]");
            _executeRunner = gameObject.AddComponent<ExecuteRunner>();
            _isInitialized = true;
        }

        public static Coroutine CallDelayExecute(float delaySeconds, Action process)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (process == null)
            {
                return null;
            }

            var coroutine = _executeRunner.StartCoroutine(DelayCoroutine(delaySeconds, process));
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }

        public static void CancelDelayExecute(Coroutine coroutine)
        {
            if (!_isInitialized || coroutine == null)
            {
                return;
            }

            if (!_activeCoroutines.Contains(coroutine))
            {
                return;
            }
            
            _executeRunner.StopCoroutine(coroutine);
            _activeCoroutines.Remove(coroutine);
        }
        
        public static void CancelAllDelayExecute()
        {
            if (!_isInitialized)
            {
                return;
            }

            _executeRunner.StopAllCoroutines();
            _activeCoroutines.Clear();
        }

        #region Utils

        private static IEnumerator DelayCoroutine(float delaySeconds, Action process)
        {
            yield return new WaitForSeconds(delaySeconds);

            try
            {
                process?.Invoke();
            }
            catch (Exception exception)
            {
                Verbose.E("[Executor] DelayExecute Exception: " + exception);
            }
        }

        #endregion

        #region Private Class

        private class ExecuteRunner : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }

            private void OnDestroy()
            {
                _isInitialized = false;
                _activeCoroutines.Clear();
                _executeRunner = null;
            }
        }

        #endregion
    }
}