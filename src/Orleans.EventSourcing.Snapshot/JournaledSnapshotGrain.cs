using Orleans.Concurrency;
using Orleans.MultiCluster;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Storage;
using Orleans.EventSourcing;
using Orleans.LogConsistency;

namespace Orleans.EventSourcing.Snapshot;

public abstract class JournaledSnapshotGrain<TGrainState> : JournaledSnapshotGrain<TGrainState, object>
    where TGrainState : class, new()
{
    protected JournaledSnapshotGrain() { }

    /// <summary>
    /// This constructor is particularly useful for unit testing where test code can create a Grain and replace
    /// the IGrainIdentity, IGrainRuntime and State with test doubles (mocks/stubs).
    /// </summary>
    protected JournaledSnapshotGrain(IGrainIdentity identity, IGrainRuntime runtime)
        : base(identity, runtime)
    {
    }
}

public abstract class JournaledSnapshotGrain<TGrainState,TEventBase> :
    LogConsistentGrain<TGrainState>,
    ILogConsistencyProtocolParticipant,
    ILogViewAdaptorHost<TGrainState, TEventBase>
    where TGrainState : class, new()
    where TEventBase: class
{
    protected JournaledSnapshotGrain() { }

        /// <summary>
        /// This constructor is particularly useful for unit testing where test code can create a Grain and replace
        /// the IGrainIdentity, IGrainRuntime and State with test doubles (mocks/stubs).
        /// </summary>
        protected JournaledSnapshotGrain(IGrainIdentity identity, IGrainRuntime runtime)
            : base(identity, runtime)
        {
        }

        /// <summary>
        /// Raise an event.
        /// </summary>
        /// <param name="event">Event to raise</param>
        /// <returns></returns>
        protected virtual void RaiseEvent<TEvent>(TEvent @event, bool needSnapshot = false)
            where TEvent : TEventBase
        {
            if (@event == null) throw new ArgumentNullException("event");

            if (needSnapshot) LogViewAdaptor.SetNeedSnapshotFlag();
            LogViewAdaptor.Submit(@event);
        }

        /// <summary>
        /// Raise multiple events, as an atomic sequence.
        /// </summary>
        /// <param name="events">Events to raise</param>
        /// <returns></returns>
        protected virtual void RaiseEvents<TEvent>(IEnumerable<TEvent> events, bool needSnapshot = false)
            where TEvent : TEventBase
        {
            if (events == null) throw new ArgumentNullException("events");

            if (needSnapshot) LogViewAdaptor.SetNeedSnapshotFlag();
            LogViewAdaptor.SubmitRange((IEnumerable<TEventBase>)events);
        }


        /// <summary>
        /// Raise an event conditionally. 
        /// Succeeds only if there are no conflicts, that is, no other events were raised in the meantime.
        /// </summary>
        /// <param name="event">Event to raise</param>
        /// <returns>true if successful, false if there was a conflict.</returns>
        protected virtual Task<bool> RaiseConditionalEvent<TEvent>(TEvent @event, bool needSnapshot = false)
            where TEvent : TEventBase
        {
            if (@event == null) throw new ArgumentNullException("event");

            if (needSnapshot) LogViewAdaptor.SetNeedSnapshotFlag();
            return LogViewAdaptor.TryAppend(@event);
        }


        /// <summary>
        /// Raise multiple events, as an atomic sequence, conditionally. 
        /// Succeeds only if there are no conflicts, that is, no other events were raised in the meantime.
        /// </summary>
        /// <param name="events">Events to raise</param>
        /// <returns>true if successful, false if there was a conflict.</returns>
        protected virtual Task<bool> RaiseConditionalEvents<TEvent>(IEnumerable<TEvent> events,
            bool needSnapshot = false)
            where TEvent : TEventBase
        {
            if (events == null) throw new ArgumentNullException("events");

            if (needSnapshot) LogViewAdaptor.SetNeedSnapshotFlag();
            return LogViewAdaptor.TryAppendRange((IEnumerable<TEventBase>)events);
        }

        /// <summary>
        /// The current confirmed state. 
        /// Includes only confirmed events.
        /// </summary>
        protected TGrainState State
        {
            get { return this.LogViewAdaptor.ConfirmedView; }
        }

        /// <summary>
        /// The version of the current confirmed state. 
        /// Equals the total number of confirmed events.
        /// </summary>
        protected int Version
        {
            get { return this.LogViewAdaptor.ConfirmedVersion; }
        }

        /// <summary>
        /// Get the current Snapshot meta data of persistent storage
        /// </summary>
        /// <returns>snapshot meta data object</returns>
        protected async Task<SnapshotStateWithMetaData<TGrainState, TEventBase>> GetLastSnapshotMetaData()
        {
            return await this.LogViewAdaptor.GetLastSnapshotMetaDataAsync(); 
        }

        /// <summary>
        /// Called whenever the tentative state may have changed due to local or remote events.
        /// <para>Override this to react to changes of the state.</para>
        /// </summary>
        protected virtual void OnTentativeStateChanged()
        {
        }

        /// <summary>
        /// The current tentative state.
        /// Includes both confirmed and unconfirmed events.
        /// </summary>
        protected TGrainState TentativeState
        {
            get { return this.LogViewAdaptor.TentativeView; }
        }

        /// <summary>
        /// Called after the confirmed state may have changed (i.e. the confirmed version number is larger).
        /// <para>Override this to react to changes of the confirmed state.</para>
        /// </summary>
        protected virtual void OnStateChanged()
        {
            // overridden by journaled grains that want to react to state changes
        }


        /// <summary>
        /// Waits until all previously raised events have been confirmed. 
        /// <para>await this after raising one or more events, to ensure events are persisted before proceeding, or to guarantee strong consistency (linearizability) even if there are multiple instances of this grain</para>
        /// </summary>
        /// <returns>a task that completes once the events have been confirmed.</returns>
        protected Task ConfirmEvents()
        {
            return LogViewAdaptor.ConfirmSubmittedEntries();

        }

        /// <summary>
        /// Retrieves the latest state now, and confirms all previously raised events. 
        /// Effectively, this enforces synchronization with the global state.
        /// <para>Await this before reading the state to ensure strong consistency (linearizability) even if there are multiple instances of this grain</para>
        /// </summary>
        /// <returns>a task that completes once the log has been refreshed and the events have been confirmed.</returns>
        protected Task RefreshNow()
        {
            return LogViewAdaptor.Synchronize();
        }


        /// <summary>
        /// Returns the current queue of unconfirmed events.
        /// </summary>
        public IEnumerable<TEventBase> UnconfirmedEvents
        {
            get { return LogViewAdaptor.UnconfirmedSuffix; }
        }


        /// <summary>
        /// By default, upon activation, the journaled grain waits until it has loaded the latest
        /// view from storage. Subclasses can override this behavior,
        /// and skip the wait if desired.
        /// </summary>
        public override Task OnActivateAsync()
        {
            return LogViewAdaptor.Synchronize();
        }

        /// <summary>
        /// Retrieves a segment of the confirmed event sequence, possibly from storage. 
        /// Throws <see cref="NotSupportedException"/> if the events are not available to read.
        /// Whether events are available, and for how long, depends on the providers used and how they are configured.
        /// </summary>
        /// <param name="fromVersion">the position of the event sequence from which to start</param>
        /// <param name="toVersion">the position of the event sequence on which to end</param>
        /// <returns>a task which returns the sequence of events between the two versions</returns>
        protected Task<IReadOnlyList<TEventBase>> RetrieveConfirmedEvents(int fromVersion, int toVersion)
        {
            if (fromVersion < 0)
                throw new ArgumentException("invalid range", nameof(fromVersion));
            if (toVersion < fromVersion || toVersion > LogViewAdaptor.ConfirmedVersion)
                throw new ArgumentException("invalid range", nameof(toVersion));

            return LogViewAdaptor.RetrieveLogSegment(fromVersion, toVersion);
        }

        /// <summary>
        /// Called when the underlying persistence or replication protocol is running into some sort of connection trouble.
        /// <para>Override this to monitor the health of the log-consistency protocol and/or
        /// to customize retry delays.
        /// Any exceptions thrown are caught and logged by the <see cref="ILogViewAdaptorFactory"/>.</para>
        /// </summary>
        /// <returns>The time to wait before retrying</returns>
        protected virtual void OnConnectionIssue(ConnectionIssue issue)
        {
        }

        /// <summary>
        /// Called when a previously reported connection issue has been resolved.
        /// <para>Override this to monitor the health of the log-consistency protocol. 
        /// Any exceptions thrown are caught and logged by the <see cref="ILogViewAdaptorFactory"/>.</para>
        /// </summary>
        protected virtual void OnConnectionIssueResolved(ConnectionIssue issue)
        {
        }

        /// <inheritdoc />
        protected void EnableStatsCollection()
        {
            LogViewAdaptor.EnableStatsCollection();
        }

        /// <inheritdoc />
        protected void DisableStatsCollection()
        {
            LogViewAdaptor.DisableStatsCollection();
        }

        /// <inheritdoc />
        protected LogConsistencyStatistics GetStats()
        {
            return LogViewAdaptor.GetStats();
        }



        /// <summary>
        /// Defines how to apply events to the state. Unless it is overridden in the subclass, it calls
        /// a dynamic "Apply" function on the state, with the event as a parameter.
        /// All exceptions thrown by this method are caught and logged by the log view provider.
        /// <para>Override this to customize how to transition the state for a given event.</para>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="event"></param>
        protected virtual void TransitionState(TGrainState state, TEventBase @event)
        {
            dynamic s = state;
            dynamic e = @event;
            s.Apply(e);
        }


        /// <summary>
        /// Adaptor for log consistency protocol.
        /// Is installed by the log-consistency provider.
        /// </summary>
        internal ILogViewSnapshotAdaptor<TGrainState, TEventBase> LogViewAdaptor { get; private set; }

        /// <summary>
        /// Called right after grain is constructed, to install the adaptor.
        /// The log-consistency provider contains a factory method that constructs the adaptor with chosen types for this grain
        /// </summary>
        protected override void InstallAdaptor(ILogViewAdaptorFactory factory, object initialState, string graintypename, IGrainStorage grainStorage, ILogConsistencyProtocolServices services)
        {
            // call the log consistency provider to construct the adaptor, passing the type argument
            LogViewAdaptor = factory.MakeLogViewAdaptor<TGrainState, TEventBase>(this, (TGrainState)initialState, graintypename, grainStorage, services);
        }

        /// <summary>
        /// If there is no log-consistency provider specified, store versioned state using default storage provider
        /// </summary>
        protected override ILogViewAdaptorFactory DefaultAdaptorFactory
        {
            get
            {
                return new StateStorage.DefaultAdaptorFactory();
            }
        }

        /// <summary>
        /// called by adaptor to update the view when entries are appended.
        /// </summary>
        /// <param name="view">log view</param>
        /// <param name="entry">log entry</param>
        void ILogViewAdaptorHost<TGrainState, TEventBase>.UpdateView(TGrainState view, TEventBase entry)
        {
            TransitionState(view, entry);
        }

        /// <summary>
        /// Notify log view adaptor of activation (called before user-level OnActivate)
        /// </summary>
        async Task ILogConsistencyProtocolParticipant.PreActivateProtocolParticipant()
        {
            await LogViewAdaptor.PreOnActivate();
        }

        /// <summary>
        /// Notify log view adaptor of activation (called after user-level OnActivate)
        /// </summary>
        async Task ILogConsistencyProtocolParticipant.PostActivateProtocolParticipant()
        {
            await LogViewAdaptor.PostOnActivate();
        }

        /// <summary>
        /// Notify log view adaptor of deactivation
        /// </summary>
        Task ILogConsistencyProtocolParticipant.DeactivateProtocolParticipant()
        {
            return LogViewAdaptor.PostOnDeactivate();
        }

        /// <summary>
        /// called by adaptor on state change. 
        /// </summary>
        void ILogViewAdaptorHost<TGrainState, TEventBase>.OnViewChanged(bool tentative, bool confirmed)
        {
            if (tentative)
                OnTentativeStateChanged();
            if (confirmed)
                OnStateChanged();
        }

        /// <summary>
        /// called by adaptor on connection issues. 
        /// </summary>
        void IConnectionIssueListener.OnConnectionIssue(ConnectionIssue connectionIssue)
        {
            OnConnectionIssue(connectionIssue);
        }

        /// <summary>
        /// called by adaptor when a connection issue is resolved. 
        /// </summary>
        void IConnectionIssueListener.OnConnectionIssueResolved(ConnectionIssue connectionIssue)
        {
            OnConnectionIssueResolved(connectionIssue);
        }
}