
using Orleans.EventSourcing.KV;

namespace ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            KvConnectionListHelper _connection = new KvConnectionListHelper();
            _connection.AppendToStreamAsync("testlist", 0, null);
            _connection.ReadStreamEventsForwardAsync("testlist", 0, 1);
        }
    }
}



