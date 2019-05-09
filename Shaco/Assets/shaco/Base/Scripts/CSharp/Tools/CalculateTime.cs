using System.Collections;
using System;

namespace shaco.Base
{
    public class CalculateTime
    {
        /// <summary>
        /// 当超时的时候自动关闭计时器
        /// </summary>
        public bool AutoCloseTimerWhenTimeout = true;

        private long _lCurTimeTicks = 0;
        private long _lStartTimeTicks = 0;
        private long _lPrevTimeTicks = 0;
        private long _lLastUpdateTime = 0;
        private TimeSpan _timeout = new TimeSpan(0, 0, 10);
        private bool _hasStarted = false;
        private System.Threading.Thread _currentThread = null;
        private bool _IsTimeout = false;

        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public double TimeoutSeconds
        {
            get
            {
                return ((double)_timeout.Ticks / 10000000);
            }
            set
            {
                if (value <= 0)
                    return;

                _timeout = new TimeSpan(0, 0, 0, 0, (int)value * 1000);
            }
        }

        /// <summary>
        /// 是否已经开始计时
        /// </summary>
        public bool HasStarted
        {
            get
            {
                return _hasStarted;
            }
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        public void Start()
        {
            ResetTimeout();

            _lStartTimeTicks = DateTime.Now.Ticks;
            _lPrevTimeTicks = DateTime.Now.Ticks;
            _hasStarted = true;
            _IsTimeout = false;

            if (_currentThread != null)
            {
                _currentThread = null;
            }

            _currentThread = new System.Threading.Thread(WaitCall);
            _currentThread.IsBackground = true;
            _currentThread.Start();
        }

        /// <summary>
        /// 重置超时计算
        /// </summary>
        public void ResetTimeout()
        {
            //restart
            if (_IsTimeout && _hasStarted)
            {
                Log.Info("Reset Restart thread !!!");
                _currentThread = new System.Threading.Thread(WaitCall);
                _currentThread.IsBackground = true;
                _currentThread.Start();
                _IsTimeout = false;
            }

            _lCurTimeTicks = DateTime.Now.Ticks;
        }

        /// <summary>
        /// 重置已经经过的时间
        /// </summary>
        public void ResetEplaseTime()
        {
            _lLastUpdateTime = _lStartTimeTicks = System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public void Stop()
        {
            _hasStarted = false;
            _currentThread = null;
        }

        //是否超时
        public bool IsTimeout()
        {
            return _IsTimeout;
        }

        /// <summary>
        /// 已经经过的时间(单位:秒)
        /// </summary>
        /// <returns>The time seconds.</returns>
        public double GetElapseTimeSeconds()
        {
            long ticksOffset = _lLastUpdateTime - _lStartTimeTicks;
            double ret = ((double)ticksOffset / 10000000.0);
            return ret;
        }

        /// <summary>
        /// 当前运行的间隔时间
        /// </summary>
        /// <returns>The interval time seconds.</returns>
        public double GetIntervalTimeSeconds()
        {
            long ticks = DateTime.Now.Ticks - _lPrevTimeTicks;
            double ret = (double)ticks / 10000000;
            _lPrevTimeTicks = DateTime.Now.Ticks;
            return ret;
        }

        /// <summary>
        /// 检查是否过期
        /// </summary>
        /// <returns></returns>
        private bool CheckTimeout()
        {
            long tickTmp = DateTime.Now.Ticks - _lCurTimeTicks;
            return (double)(tickTmp) / 10000000 >= TimeoutSeconds;
        }

        private void WaitCall()
        {
            try
            {
                //循环检测是否过期
                while (_hasStarted)
                {
                    if (CheckTimeout())
                    {
                        _IsTimeout = true;

                        if (AutoCloseTimerWhenTimeout)
                            break;
                    }
                    _lLastUpdateTime = DateTime.Now.Ticks;

                    System.Threading.Thread.Sleep(1);
                }

                Stop();
            }
            catch (Exception e)
            {
                Log.Error("CalculateTimeout catch a exception =" + e);
                Stop();
            }
        }
    }
}