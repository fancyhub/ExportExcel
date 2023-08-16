// See https://aka.ms/new-console-template for more information
using GGame.UI;
using Test;



//TableMgr.Inst.LoadAllTable();
//TableMgr.Get(1, 2, out TTestComposeKey v);
//v = TableMgr.GetTTestComposeKey(1, 2);
//Console.WriteLine("Hello, World! " + v.Name);



ClockManual clock = new ClockManual(0);
Clock clock1 = new Clock(clock);
Clock clock2 = new Clock(clock1);

clock.Time = 1000;
Console.WriteLine($"{clock.Time} : {clock1.Time} : {clock2.Time}");
clock2.ScaleFloat = 2;
Console.WriteLine($"{clock.Time} : {clock1.Time} : {clock2.Time}");
clock.Time = 2000;
Console.WriteLine($"{clock.Time} : {clock1.Time} : {clock2.Time}");

Console.WriteLine("Done");

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 11:44:42
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace GGame.UI
{
    public interface IClock
    {
        //默认，不缩放，千分位
        public const uint ScaleOne = 1000;
        public const float ScaleUint2Float = 1.0f / ScaleOne;

        /// <summary>
        /// 基于千分位, 如果返回1000 ,就是不缩放
        /// </summary>
        public uint Scale { get; set; }
        public float ScaleFloat { get; set; }
        public bool Pause { get; set; }

        public long Time { get; }
    }


    #region 带有缩放的 Clock 实现

    /// <summary>
    /// 基于的clock系统 <para/>
    /// 最终时间 = base_clock.GetTime() * mul_factor / div_factor <para/>
    /// 可以暂停,可以加速     <para/>
    /// div_factor: 是为了解决 从 毫秒 -> 秒 的转换 <para/>
    /// mul_factor: 是为了解决 从 秒 -> 毫秒 的转换 <para/>
    /// </summary>
    public class Clock : IClock
    {
        public IClock _base_clock;
        public ClockPauseScale _pause_scale;
        public ClockData _data;
        public ClockTransformer _transformer;

        public Clock(IClock base_clock,
                    int mul_factor = 1,
                    int div_factor = 1)
        {
            _transformer = new ClockTransformer(mul_factor, div_factor);
            _base_clock = base_clock;
            _data = new ClockData();
            _pause_scale = new ClockPauseScale();
            _data.Init();
            _pause_scale.Init();
        }

        public float ScaleFloat
        {
            get { return Scale * IClock.ScaleUint2Float; }
            set
            {
                _pause_scale.Scale(value);
                _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
            }
        }

        public uint Scale
        {
            get
            {
                return _pause_scale._scale;
            }
            set
            {
                _pause_scale.Scale(value);
                _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
            }
        }

        public bool Pause
        {
            get { return _pause_scale._pause; }
            set
            {
                if (_pause_scale._pause == value)
                    return;
                if (value)
                {
                    _pause_scale.Pause();
                    _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
                }
                else
                {
                    _pause_scale.Resume();
                    _data.SetScale(_pause_scale.GetFinalScale(), _base_clock.Time);
                }
            }
        }

        public long Time
        {
            get
            {
                long src_time = _base_clock.Time;
                long scaled_time = _data.GetTime(src_time);
                long ret = _transformer.Transform(scaled_time);
                return ret;
            }
        }

        public struct ClockData
        {
            //开始记录时间的 时间戳
            public long _src_ts;

            //每次状态变化之后，就要改变
            public long _virtual_ts;

            //缩放值, 千分位
            public uint _scale;

            public void Init()
            {
                _scale = IClock.ScaleOne;
                _src_ts = 0;
                _virtual_ts = 0;
            }

            public void SetScale(uint scale, long now_time)
            {
                if (_scale == scale)
                    return;

                _virtual_ts = GetTime(now_time);
                _src_ts = now_time;
                _scale = scale;
            }

            public long GetTime(long now_time)
            {
                //如果 归到一起，可以不用区分是否是暂停状态
                //不过这种 暂停，或者缩放值为0 的情况下，可以少调用一次 获取时间戳
                if (_scale == 0)
                    return _virtual_ts;

                long dt = now_time - _src_ts;
                if (_scale != IClock.ScaleOne)
                    dt = (dt * _scale) / IClock.ScaleOne;
                return _virtual_ts + dt;
            }
        }

        public struct ClockPauseScale
        {
            public uint _scale;
            public bool _pause;

            public void Init()
            {
                _scale = IClock.ScaleOne;
                _pause = false;
            }

            public void Scale(uint scale)
            {
                _scale = scale;
            }

            public void Pause()
            {
                _pause = true;
            }

            public void Resume()
            {
                _pause = false;
            }

            public void Scale(float scale)
            {
                float scale_f = IClock.ScaleOne * scale;
                if (scale_f < 0)
                    _scale = 0;
                else
                    _scale = (uint)(int)scale_f;
            }

            public uint GetFinalScale()
            {
                if (_pause)
                    return 0;
                return _scale;
            }
        }

        public struct ClockTransformer
        {
            public int _mul_factor;
            public int _div_factor;

            public ClockTransformer(int mul_factor = 1, int div_factor = 1)
            {
                _mul_factor = Math.Max(1, mul_factor);
                _div_factor = Math.Max(1, div_factor);
            }

            public long Transform(long time)
            {
                if (_mul_factor == _div_factor)
                    return time;
                return (time * _mul_factor) / _div_factor;
            }
        }

    }
    #endregion

    // <summary>
    /// 不支持 暂停,加速
    /// 如果需要,在上面套接一个
    /// </summary>
    public class ClockManual : IClock
    {
        public long _time;
        public ClockManual(long init_time) { _time = init_time; }

        public float ScaleFloat
        {
            get { return IClock.ScaleUint2Float; }
            set { }
        }
        public uint Scale
        {
            get => IClock.ScaleOne;
            set { }
        }

        public bool Pause { get { return false; } set { } }

        public long Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }
    }
}
