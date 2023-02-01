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
        private double _successfulCheckingIntervalMilliseconds;
        public DateTime LastAvailabilityTime { get; protected set; }
        public DateTime LastUnavailabilityTime { get; protected set; }
        public bool CheckIsInProgress { get; protected set; }
        public TimeSpan MaxUnavailabilityIntervalToIgnore;
        protected readonly Logger Logger;
        protected string _lastMessage;

        public AvailabilityCheckIntervalOption WhenCheckIsOngoing =
            AvailabilityCheckIntervalOption.SkipNewCheckIfOngoingCheckIsInProgress;

        protected AvailabilityCheckerBase(Logger logger)
        {
            ClassName = $"{GetType().Name}: ";
            Logger = logger;
            _tmrChecker.Elapsed += _tmrChecker_Elapsed;
        }

        public TimeSpan LastUnavailabilityInterval => LastUnavailabilityTime - LastAvailabilityTime;

        protected readonly string ClassName;

        public bool IsLastUnavailabilityFilteredOut
        {
            get
            {
                if (MaxUnavailabilityIntervalToIgnore == TimeSpan.Zero)
                {
                    Logger.Debug(ClassName + "MaxUnavailabilityIntervalToIgnore is zero. Don't filter out unavailability");
                    return false;
                }

                var filterOut = LastUnavailabilityInterval < MaxUnavailabilityIntervalToIgnore;
                var filterMsg = ClassName + (filterOut ? "Filter out" : "Don't filter out") + " unavailability: ";
                Logger.Debug(filterMsg + $"LastUnavailabilityInterval: {LastUnavailabilityInterval}; MaxUnavailabilityIntervalToIgnore: {MaxUnavailabilityIntervalToIgnore}; LastAvailabilityTime: {LastAvailabilityTime}; LastUnavailabilityTime: {LastUnavailabilityTime}");

                return filterOut;
            }
        }

        private void SetAvailability(bool state, string message)
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
                _tmrChecker.Interval = _successfulCheckingIntervalMilliseconds;
                Logger.Debug(ClassName + $"Set successful check interval: {_tmrChecker.Interval}");
                OnAvailabilityChanged(message);

                return;
            }

            // if _tmrChecker.Interval ~ _successfulCheckingInterval then it's the first false state. Set minimal check interval
            if (_tmrChecker.Interval >= _successfulCheckingIntervalMilliseconds - 1000)
            {
                _tmrChecker.Interval = 1000;
                Logger.Debug(ClassName + $"Set check interval to {_tmrChecker.Interval}");
            }
            else if (_tmrChecker.Interval < _successfulCheckingIntervalMilliseconds / 2)
            {
                _tmrChecker.Interval += 5000;
                Logger.Debug(ClassName + $"Set check interval to {_tmrChecker.Interval}");
            }
            else
                Logger.Debug(ClassName + $" Leave check interval: {_tmrChecker.Interval}");

            if (!_previousState)
            {
                if (message != null)
                    OnAvailabilityChanged(message);
                return;
            }

            if (IsLastUnavailabilityFilteredOut)
            {
                Logger.Debug(ClassName + $"Not available but filtered out for {this}: availability time: {LastAvailabilityTime}, unavailability time: {LastUnavailabilityTime}");
                return;
            }

            _previousState = false;
            OnAvailabilityChanged(message);
        }

        protected void OnAvailabilityChanged(string message)
        {
            var eArgs = new AvailabilityEventArgs(_previousState, message, LastAvailabilityTime, LastUnavailabilityTime);
            Logger.Debug(ClassName + $"Availability changed: {eArgs}");
            AvailabilityChanged?.Invoke(this, eArgs);
        }

        private void _tmrChecker_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CheckIsInProgress)
            {
                switch (WhenCheckIsOngoing)
                {
                    case AvailabilityCheckIntervalOption.SkipNewCheckIfOngoingCheckIsInProgress:
                        return;
                }
            }

            var res = CheckForAvailability(out var message);
            SetAvailability(res, message);
        }

        public double CheckingIntervalMilliseconds
        {
            get => _successfulCheckingIntervalMilliseconds;
            set
            {
                _successfulCheckingIntervalMilliseconds = value;
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

        public bool CheckForAvailability(out string message)
        {
            CheckIsInProgress = true;
            try
            {
                var success = CheckForAvailabilityInternal(out message);
                if (!IsErrorTypeTheSame(message))
                    _lastMessage = message;
                else
                    message = null;

                return success;
            }
            finally
            {
                CheckIsInProgress = false;
            }
        }

        protected abstract bool CheckForAvailabilityInternal(out string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Cached result about availability</returns>
        public bool CheckForAvailabilityAsync(Action<bool> callback = null)
        {
            ThreadPool.QueueUserWorkItem((param) =>
            {
                var res = CheckForAvailability(out var message);
                SetAvailability(res, message);
                callback?.Invoke(res);
            });
            return _previousState;
        }

        /// <summary>
        /// Returns True if the previous error message type is the same as the passed one. Use this method to avoid spamming with the same errors on every check
        /// </summary>
        /// <param name="error">An error message which will be compared with the previous message</param>
        /// <returns>True if the previous error message type is the same as the passed one</returns>
        protected virtual bool IsErrorTypeTheSame(string error) => error == _lastMessage;
    }
}
