namespace Orleans.EventSourcing.API.Contracts;

public class RedisSettingOption
{
    public string KvConnectionSTR { set; get; }
    public string PartitionSize { set; get; }
    
}