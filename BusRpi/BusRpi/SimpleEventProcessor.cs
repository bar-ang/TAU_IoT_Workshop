using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;

using Newtonsoft.Json;
namespace BusRpi
{
    public class SimpleEventProcessor : IEventProcessor
    {

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            System.Diagnostics.Debug.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            System.Diagnostics.Debug.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            System.Diagnostics.Debug.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                string data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                // Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");
                DRP msg = DRP.deserializeDRP(data);
                //Toast.MakeText(MainActivity.context, msg.UserName, ToastLength.Long);
                System.Diagnostics.Debug.WriteLine("^^^^received: " + msg.UserName);
            }

            return context.CheckpointAsync();
        }
    }
}
/*
 public class SimpleEventProcessor : IEventProcessor
{
    public delegate void DataReceived(string data);
    public event DataReceived OnDataReceived;

    public delegate void ProcessorShutDown(CloseReason reason);
    public event ProcessorShutDown OnProcessorShutDown;

    public delegate void Error(Exception error);
    public event Error OnError;

    public delegate void Initialized();
    public event Initialized OnInit;

    public SimpleEventProcessor(Initialized OnInit, DataReceived OnDataReceived, Error OnError, ProcessorShutDown OnProcessorShutDown)
    {
        this.OnInit += OnInit;
        this.OnDataReceived += OnDataReceived;
        this.OnError += OnError;
        this.OnProcessorShutDown += OnProcessorShutDown;
    }

    public Task CloseAsync(PartitionContext context, CloseReason reason)
    {
        //Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
        OnProcessorShutDown(reason);
        return Task.CompletedTask;
    }

    public Task OpenAsync(PartitionContext context)
    {
        //Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
        OnInit();
        return Task.CompletedTask;
    }

    public Task ProcessErrorAsync(PartitionContext context, Exception error)
    {
        // Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
        OnError(error);
        return Task.CompletedTask;
    }

    public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
    {
        foreach (var eventData in messages)
        {
            string data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
            // Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");
            OnDataReceived(data);
        }

        return context.CheckpointAsync();
    }
}
*/