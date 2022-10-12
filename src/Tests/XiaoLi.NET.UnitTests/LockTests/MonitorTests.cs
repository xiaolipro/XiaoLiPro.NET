﻿using Xunit.Abstractions;

namespace XiaoLi.NET.UnitTests;

public class LockTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private static readonly object _locker = new object();
    private static int _val1 = 1, _val2 = 1, num = 0;

    public LockTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(20)]
    private void 测试Go(int threadNum)
    {
        num = 0;
        Parallel.For(0, threadNum, s => 线程不安全的Go());

        _testOutputHelper.WriteLine(num.ToString());

        num = 0;
        Parallel.For(0, threadNum, s => Monitor实现线程安全的Go2());

        _testOutputHelper.WriteLine(num.ToString());

        num = 0;
        Parallel.For(0, threadNum, s => Lock语法糖实现线程安全的Go());

        _testOutputHelper.WriteLine(num.ToString());
    }


    void 线程不安全的Go()
    {
        for (int i = 0; i < 10000; i++)
        {
            num++;
        }
    }

    void Monitor实现线程安全的Go()
    {
        for (int i = 0; i < 10000; i++)
        {
            // C# 1.0、2.0 和 3.0
            Monitor.Enter(_locker); // 20ns
            try
            {
                num++;
            }
            finally
            {
                Monitor.Exit(_locker);
            }
        }
    }

    void Lock语法糖实现线程安全的Go()
    {
        for (int i = 0; i < 10000; i++)
        {
            lock (_locker) num++;
        }
    }

    #region lockTaken

    void Monitor实现线程安全的Go2()
    {
        for (int i = 0; i < 10000; i++)
        {
            bool taken = false;
            try
            {
                // JIT应该内联此方法，以便在典型情况下优化lockTaken参数的检查。请注意，要使VM允许内联，方法必须是透明的。
                Monitor.Enter(_locker,ref taken);
                num++;
            }
            finally
            {
                // C# 4.0 解决锁泄露问题
                if (taken) Monitor.Exit(_locker);
            }
        }
    }

    #endregion
}