using System;
using System.Threading.Tasks;

namespace PlantBasedPizza.Shared.Logging
{
    public interface IObservabilityService
    {
        void StartTraceSegment(string segmentName);

        void EndTraceSegment();

        void StartTraceSubsegment(string subSegmentName);

        void EndTraceSubsegment();
        
        Task PutMetric(string metricGroup, string metricName, double metricValue);

        TResult TraceMethod<TResult>(string methodName, Func<TResult> method);

        void TraceMethod(string methodName, Action method);
        
        Task TraceMethodAsync(string methodName, Func<Task> method);
        
        Task<TResult> TraceMethodAsync<TResult>(string methodName, Func<Task<TResult>> method);

        void Info(string message);

        void Warn(Exception ex, string message);

        void Error(Exception ex, string message);
    }
}