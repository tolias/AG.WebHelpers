using System;
using System.Threading;
using System.Timers;
using Serilog.Core;

namespace AG.WebHelpers
{
    public abstract class AvailabilityCheckerBase
    {
        private System.Timers.Timer _tmrChecker = new System.Timers.Timer(60000);
        private bool _previousState;
        private bool _previousStateWithoutFiltering;
        public event EventHandler<AvailabilityEventArgs> AvailabilityChanged;
        private double _successfulCheckingInterval;
        public DateTime LastAvailabilityTime { get; protected set; }
        public DateTime LastUnavailabilityTime { get; protected set; }
        public TimeSpan MaxUnavailabilityIntervalToIgnore;
        private readonly Logger _logger;

        protected AvailabilityCheckerBase(Logger logger)
        {
            _logger = logger;
            _tmrChecker.Elapsed += _tmrChecker_Elapsed;
        }

        public TimeSpan LastUnavailabilityInterval => LastUnavailabilityTime - LastAvailabilityTime;

        public bool IsLastUnavailabilityFilteredOut
        {
            get
            {
                if (MaxUnavailabilityIntervalToIgnore == TimeSpan.Zero)
                    return false;

                return LastUnavailabilityInterval < MaxUnavailabilityIntervalToIgnore;
            }
        }

        private void SetAvailability(bool state, Func<string> messageFunc)
        {
            _previousStateWithoutFiltering = state;

            if (state)
                LastAvailabilityTime = DateTime.Now;
            else
                LastUnavailabilityTime = DateTime.Now;

            if (state)
            {
                if (_previousState)
                    return;

                _previousState = true;
                _tmrChecker.Interval = _successfulCheckingInterval;
                _logger.Debug($"Set successful check interval: {_tmrChecker.Interval}");
                OnAvailabilityChanged(messageFunc);

                return;
            }

            if (_tmrChecker.Interval >= _successfulCheckingInterval - 1000)
            {
                _tmrChecker.Interval = 1000;
                _logger.Debug($"Set check interval to {_tmrChecker.Interval}");
            }
            else if (_tmrChecker.Interval < _successfulCheckingInterval / 2)
            {
                _tmrChecker.Interval += 5000;
                _logger.Debug($"Set check interval to {_tmrChecker.Interval}");
            }
            else
                _logger.Debug($"Leave check interval: {_tmrChecker.Interval}");

            if (!_previousState)
                return;

            if (IsLastUnavailabilityFilteredOut)
            {
                _logger.Debug($"Not available but filtered out for {this}: availability time: {LastAvailabilityTime}, unavailability time: {LastUnavailabilityTime}");
                return;
            }

            _previousState = false;
            OnAvailabilityChanged(messageFunc);
        }

        protected void OnAvailabilityChanged(Func<string> messageFunc)
        {
            var eArgs = new AvailabilityEventArgs(_previousState, messageFunc?.Invoke(), LastAvailabilityTime, LastUnavailabilityTime);
            _logger.Debug($"Availability changed for {this}: {eArgs}");
            AvailabilityChanged?.Invoke(this, eArgs);
        }

        private void _tmrChecker_Elapsed(object sender, ElapsedEventArgs e)
        {
            var res = CheckForAvailability(out var messageFunc);
            SetAvailability(res, messageFunc);
        }

        public double CheckingInterval
        {
            get => _successfulCheckingInterval;
            set
            {
                _successfulCheckingInterval = value;
                _tmrChecker.Interval = value;
            }
        }

        public bool CheckingOnIntervalEnabled
        {
            get => _tmrChecker.Enabled;
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
            return _previousState;
        }
    }
}
