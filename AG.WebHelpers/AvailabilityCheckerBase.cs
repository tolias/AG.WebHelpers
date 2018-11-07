using AG.Utilities.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;

namespace AG.WebHelpers
{
    public abstract class AvailabilityCheckerBase
    {
        private System.Timers.Timer _tmrChecker = new System.Timers.Timer(60000);
        private bool _isAvailable;
        public event EventHandler<AvailabilityEventArgs> AvailabilityChanged;
        private double _successfulCheckingInterval;

        protected AvailabilityCheckerBase()
        {
            _tmrChecker.Elapsed += _tmrChecker_Elapsed;
        }

        private void SetAvailability(bool state, Func<string> messageFunc)
        {
            if(_isAvailable != state)
            {
                _isAvailable = state;
                AvailabilityChanged?.Invoke(null, new AvailabilityEventArgs(state, messageFunc?.Invoke()));
            }

            if (state)
            {
                _tmrChecker.Interval = _successfulCheckingInterval;
            }
            else
            {
                if (_tmrChecker.Interval >= _successfulCheckingInterval - 1000)
                {
                    _tmrChecker.Interval = 1000;
                }
                else if (_tmrChecker.Interval < _successfulCheckingInterval / 2)
                {
                    _tmrChecker.Interval += 5000;
                }
            }
        }

        private void _tmrChecker_Elapsed(object sender, ElapsedEventArgs e)
        {
            var res = CheckForAvailability(out var messageFunc);
            SetAvailability(res, messageFunc);
        }

        public double CheckingInterval
        {
            get { return _successfulCheckingInterval; }
            set
            {
                _successfulCheckingInterval = value;
                _tmrChecker.Interval = value;
            }
        }

        public bool CheckingOnIntervalEnabled
        {
            get { return _tmrChecker.Enabled; }
            set
            {
                if (_tmrChecker.Enabled != value)
                {
                    CheckForAvailabilityAsync();
                    _tmrChecker.Enabled = value;
                }
            }
        }

        public bool EnableCheckingOnInterval(Action<bool> firstCheckCallback = null)
        {
            _tmrChecker.Enabled = true;
            return CheckForAvailabilityAsync(firstCheckCallback);
        }

        public abstract bool CheckForAvailability(out Func<string> messageFunc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Cached result about availability</returns>
        public bool CheckForAvailabilityAsync(Action<bool> callback = null)
        {
            ThreadPool.QueueUserWorkItem((param) =>
            {
                var res = CheckForAvailability(out var messageFunc);
                SetAvailability(res, messageFunc);
                callback?.Invoke(res);
            });
            return _isAvailable;
        }
    }
}
