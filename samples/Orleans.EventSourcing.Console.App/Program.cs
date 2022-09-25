// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Orleans.EventSourcing.KV;

class Program
{
    private static readonly KvConnectionListHelper _helper=new KvConnectionListHelper();
    private static List<string> eventData = new List<string>(){"w"};
    private static int taskCount = 0;
    private static int allRequestCount = 0;
    /*private static long success = 0;
    private static long error = 0;*/
    static void Main(string[] args)
    {
        Console.Write("Please enter TaskCount: ");
        taskCount = int.Parse(Console.ReadLine());
        Console.Write("Please enter AllRequestCount: ");
        allRequestCount = int.Parse(Console.ReadLine());
        long allResultCount = 0;
        
        long allTime = 0;
      
        List<Task> listTask = new List<Task>();
        for (int i = 0; i < taskCount; i++)
        {
            Task task = new Task(() => {
                Console.WriteLine("线程ID:{0},开始执行", Thread.CurrentThread.ManagedThreadId);
                Stopwatch stw = new Stopwatch();
                stw.Start();
                long result=Todo(allRequestCount / taskCount);
                //long result = SumNumbers(10);
                stw.Stop();
                allResultCount = allResultCount + result;
                allTime = allTime + stw.ElapsedMilliseconds;
                Console.WriteLine("线程ID:{0},执行完成,执行用时{1},", Thread.CurrentThread.ManagedThreadId, stw.ElapsedMilliseconds);
            });
            
            listTask.Add(task);
            task.Start();
        }
        Task.WaitAll(listTask.ToArray());

        var rps =((decimal)allResultCount / (decimal)allTime)*1000 ;
        Console.WriteLine("allResultCount= " + allResultCount);
        Console.WriteLine("rps= " + rps);
    }
   /*private static long SumNumbers(int count)
    {
        long sum = 0;
        for (int i = 0; i < count; i++)  
        {
            sum += 1;
        }
        Thread.Sleep(1);
        return sum;
    }*/
    
    private static long Todo(long count)
    {
      
        long sum = 0;
        for (int i = 0; i < count; i++)
        {
            var re= _helper.AppendToStreamAsync("listTest2",0,eventData).Result;
            sum =sum+1;
        }
       
        return sum;
    }
}
