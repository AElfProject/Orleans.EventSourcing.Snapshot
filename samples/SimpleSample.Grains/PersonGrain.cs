using Orleans.EventSourcing;
using SimpleSample.GrainInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.EventSourcing.Snapshot;

namespace SimpleSample.Grains
{
    public class PersonGrain : JournaledSnapshotGrain<PersonState>, IPersonGrain
    {
        public Task Say(string content)
        {
            bool isNeedStorageSnapshot = false;
            if (content.Contains("5"))
            {
                isNeedStorageSnapshot = true;
            }
            
            RaiseEvent(new PersonSaidEvent { Said = content },isNeedStorageSnapshot);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetHistorySaids()
        {
            return Task.FromResult(TentativeState.HistorySaids);
        }

        public Task UpdateNickName(string newNickName)
        {
            RaiseEvent(new PersonNickNameUpdatedEvent { NewNickName = newNickName },true);

            return Task.CompletedTask;
        }

        public Task<string> GetNickName()
        {
            return Task.FromResult(TentativeState.NickName);
        }

        public async Task<List<string>> GetLastSnapshotSaids()
        {
            SnapshotStateWithMetaData<PersonState,object> metaData = await GetLastSnapshotMetaData();

            return metaData.Snapshot.HistorySaids;
        }

        public async Task<int> GetLastSnapshotGlobalVersion()
        {
            SnapshotStateWithMetaData<PersonState, object> metaData = await GetLastSnapshotMetaData();

            return metaData.GlobalVersion;
        }
    }

    public class PersonState 
    {
        public string NickName { get; private set; } = "anonymous";

        public List<string> HistorySaids { get; private set; } = new List<string>();

        public void Apply(PersonSaidEvent @event) 
        {
            HistorySaids.Add(@event.Said);
        }
        
        public void Apply(PersonNickNameUpdatedEvent @event) 
        {
            NickName = @event.NewNickName;
        }
    }

    public class PersonSaidEvent 
    {
        public string Said { get; set; }
    }

    public class PersonNickNameUpdatedEvent
    {
        public string NewNickName { get; set; }
    }
}
