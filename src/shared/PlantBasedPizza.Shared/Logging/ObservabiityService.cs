using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;
using Amazon.XRay.Recorder.Core;
using Serilog;
using Serilog.Context;

namespace PlantBasedPizza.Shared.Logging
{
    public class ObservabiityService : IObservabilityService
    {
        private readonly AmazonCloudWatchClient _cloudWatchClient;

        public ObservabiityService(AmazonCloudWatchClient cloudWatchClient)
        {
            this._cloudWatchClient = cloudWatchClient;
        }

        public void StartTraceSubsegment(string subsegmentName)
        {
            AWSXRayRecorder.Instance.BeginSubsegment(subsegmentName, DateTime.Now);
        }

        public void EndTraceSubsegment()
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }

        public async Task PutMetric(string metricGroup, string metricName, double metricValue)
        {
            try
            {
                await this._cloudWatchClient.PutMetricDataAsync(new PutMetricDataRequest()
                {
                    Namespace = "PlantBasedPizza",
                    MetricData = new List<MetricDatum>(1)
                    {
                        new MetricDatum()
                        {
                            Dimensions = new List<Dimension>(1)
                            {
                                new Dimension()
                                {
                                    Name = metricGroup,
                                    Value = metricGroup
                                }
                            },
                            MetricName = metricName,
                            Unit = StandardUnit.Count,
                            Value = metricValue,
                            TimestampUtc = DateTime.Now
                        }
                    }
                });
            }
            catch (Exception)
            {
            }
        }
        
        public TResult TraceMethod<TResult>(string methodName, Func<TResult> method)
        {
            return AWSXRayRecorder.Instance.TraceMethod<TResult>(methodName, method);
        }

        public void TraceMethod(string methodName, Action method)
        {
            AWSXRayRecorder.Instance.TraceMethod(methodName, method);
            
            AWSXRayRecorder.Instance.EndSegment();
        }
        
        public async Task TraceMethodAsync(string methodName, Func<Task> method)
        {
            AWSXRayRecorder.Instance.BeginSegment(methodName);
            
            await AWSXRayRecorder.Instance.TraceMethodAsync(methodName, method);
            
            AWSXRayRecorder.Instance.EndSegment();
        }

        public async Task<TResult> TraceMethodAsync<TResult>(string methodName, Func<Task<TResult>> method)
        {
            return await AWSXRayRecorder.Instance.TraceMethodAsync<TResult>(methodName, method);
        }

        public void Info(string message)
        {
            ApplicationLogger.Info(message);
        }
        
        public void Warn(Exception ex, string message)
        {
            ApplicationLogger.Warn(ex, message);
        }
        
        public void Error(Exception ex, string message)
        {
            ApplicationLogger.Error(ex, message);
        }
    }
}