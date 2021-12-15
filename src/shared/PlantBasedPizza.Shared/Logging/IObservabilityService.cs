using System;
using System.Threading.Tasks;

namespace PlantBasedPizza.Shared.Logging
{
    public interface IObservabilityService
    {
        void StartTraceSegment(string segmentName, string correlationId = "");

        void EndTraceSegment();

        void StartTraceSubsegment(string subSegmentName);

        void EndTraceSubsegment();
        
        Task PutMetric(string metricGroup, string metricName, double metricValue);

        TResult TraceMethod<TResult>(string methodName, Func<TResult> method);

        void TraceMethod(string methodName, Action method);
        
        Task TraceMethodAsync(string methodName, Func<Task> method);
        
        Task<TResult> TraceMethodAsync<TResult>(string methodName, Func<Task<TResult>> method);

        void Info(string correlationId, string message);

        void Warn(string correlationId, Exception ex, string message);

        void Error(string correlationId, Exception ex, string message);
    }
}