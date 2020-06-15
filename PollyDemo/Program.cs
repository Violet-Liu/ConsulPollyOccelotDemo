using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;

namespace PollyDemo
{
    /// <summary>
    /// 服务雪崩 下游服务异常引起服务连锁级雪崩
    /// </summary>
    class Program
    {
        /// <summary>
        /// Polly 弹性和瞬态故障处理库，发生某种故障和异常以后，采用处理方案
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //1. 定义故障 2. 指定策略  3. 执行策略(服务降级)
            Policy.Handle<Exception>().Fallback(() => { Console.WriteLine("polly fallback!"); })
                .Execute(() =>
                {
                    Console.WriteLine("dosomething");
                    throw new ArgumentException("hello polly");
                });

            //带有条件
            Policy.Handle<Exception>(ex => ex.Message == "Hello");

            //多个故障定义
            Policy.Handle<HttpRequestException>().Or<ArgumentException>(ex => ex.Message == "Hellow").Or<AggregateException>();

            //弹性策略 大致分为响应性策略、主动性策略

            //响应性策略：重试、断路器
            Policy.Handle<Exception>().Retry();
            Policy.Handle<Exception>().Retry(3);
            Policy.Handle<Exception>().WaitAndRetry(new[] {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            });

            //断路器：连续触发了指定次数的故障后，就开启断路器(OPEN)，进入熔断状态，1分钟
            var breaker = Policy.Handle<Exception>().CircuitBreaker(2, TimeSpan.FromMinutes(1), (exception, span) => { }, () => { });

            //断路器有三种状态CircuitState,OPEN、CLOSED、HALF-OPEN
            //half-open,半开启状态，断路器会尝试着释放（1）次操作，尝试去请求，成功状态变为CLOSE,如果失败，断路器打开OPEN

            //Polly断路器策略还有一种特殊状态Isolated：手动开启状态
            breaker.Isolate();
            breaker.Reset();

            //高级断路器 如果在故障采样持续时间内，发生故障的比例超过故障阈值，则发生熔断，前提是在此期间，通过断路器的操作的数量至少是最小吞吐量
            Policy.Handle<Exception>().AdvancedCircuitBreaker(0.5, TimeSpan.FromSeconds(10), 8, TimeSpan.FromSeconds(30));

            //主动性策略：超时，舱壁隔离，缓存
            //超时
            Policy.Timeout(3, (context, span, arg3, agr4) => { });

            //舱壁隔离，通过控制并发数量来管理负载，超过12的都拒绝掉
            Policy.Bulkhead(12, 20);

            // 策略包装，策略组合

            // 降级策略
            var fallback = Policy.Handle<Exception>()
                .Fallback(() => { Console.WriteLine("Polly Fallback!"); });

            // 重试策略
            var retry = Policy.Handle<Exception>().Retry(3, (exception, i) =>
            {
                Console.WriteLine($"retryCount:{i}");
            });

            // 如果重试3次，仍然发生故障，就降级
            // 从右到左
            var policy = Policy.Wrap(fallback, retry);
            policy.Execute(() =>
            {
                Console.WriteLine("Polly Begin");
                throw new Exception("Error");
            });

        }
    }
}
