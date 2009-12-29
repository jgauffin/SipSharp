using System;
using SipSharp;
using SipSharp.Calls;
using SipSharp.Dialogs;
using SipSharp.Messages;
using SipSharp.Servers.Registrar;
using SipSharp.Transactions;
using SipSharp.Transports;
using SwitchSharp.DialPlans;

namespace SwitchSharp
{
    public class SwitchSharp
    {
        private readonly DialPlanManager _dialPlanManager;
        private readonly SipStack _stack = new SipStack();
        private DialogManager _dialogManager;
        private Registrar _registrar;
        private RegistrationReporsitory _regRepos;

        public SwitchSharp()
        {
            _stack.RequestReceived += OnRequest;
        }

        public IRegistrationRepository RegistrationDatabase { get; set; }

        /// <summary>
        /// Factory used to parse messages.
        /// </summary>
        public MessageFactory MessageFactory { get { return _stack.MessageFactory; } }


        public void AddListener(ITransport transport)
        {
            _stack.AddTransport(transport);
        }

        private Call CreateCall(IRequest request)
        {
            var call = new Call {State = CallState.Proceeding, Reason = CallReasons.Direct};
            return call;
        }

        private void DialPlanHunt(Call call, IServerTransaction transaction)
        {
            var dialPlan = new DialPlan(call);
            _dialPlanManager.PreProcess(dialPlan);

            try
            {
                DialPlanRequested(this, new DialPlanEventArgs(dialPlan));
            }
            catch (Exception err)
            {
                // oops, someone threw an exception.
            }

            _dialPlanManager.Process(dialPlan, transaction);
        }

        private void LookupCaller(IRequest request, IServerTransaction transaction, Call call)
        {
            Registration reg = _registrar.Get(request.From);
            if (reg == null)
            {
                call.Origin = CallOrigins.External;
                call.Caller = new CallParty {Contact = request.From, IsInternal = false};
                return;
            }

            call.Id = request.CallId;
            call.Origin = CallOrigins.Internal;
            call.Caller = new CallParty {Contact = reg.Contacts[0], IsInternal = true};
        }


        /// <summary>
        /// Hunt for destination.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="transaction"></param>
        /// <param name="call"></param>
        private void LookupDestination(IRequest request, IServerTransaction transaction, Call call)
        {
            Registration reg = _registrar.Get(request.To);
            if (reg == null)
            {
                call.Destination = new CallParty {Contact = request.To, IsInternal = false};
                call.Origin = call.Origin | CallOrigins.Outbound;
                return;
            }
            call.Destination = new CallParty {Contact = reg.Contacts[0], IsInternal = true};
            call.Origin = call.Origin | CallOrigins.Inbound;
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            if (e.Request.Method == SipMethod.INVITE)
            {
                // Always send trying.
                ProcessInvite(e);
            }
            else if (e.Request.Method == SipMethod.CANCEL)
            {
            }
            if (e.Request.Method == SipMethod.REGISTER)
            {
            }
        }

        private void ProcessInvite(RequestEventArgs e)
        {
            IResponse trying = e.Request.CreateResponse(StatusCode.Trying, "We are trying here.");
            e.Transaction.Send(trying);

            // create call info
            Call call = CreateCall(e.Request);
            CallChanged(this, new CallEventArgs(call));
            LookupCaller(e.Request, e.Transaction, call);
            LookupDestination(e.Request, e.Transaction, call);
            call.CalledParty = call.Destination;

            // filter out unwanted calls.
            TriggerCallFiltering(call, e);

            // Caller and Destination are both external (not found)
            if (!call.Caller.IsInternal && !call.Caller.IsInternal)
            {
                IResponse response = e.Request.CreateResponse(StatusCode.NotFound,
                                                              "Destination is not found in this server.");
                e.Transaction.Send(response);
                return;
            }

            // hunt dial plan.
            DialPlanHunt(call, e.Transaction);
        }


        public void Start(string domain)
        {
            _registrar = new Registrar(_stack, RegistrationDatabase)
                             {
                                 Domain = new SipUri(null, domain),
                                 Realm = domain
                             };
            _stack.Start();
        }

        private void TriggerCallFiltering(Call call, RequestEventArgs e)
        {
            try
            {
                CallFiltering(this, new CallEventArgs(call));
            }
            catch (Exception err)
            {
                if (err is SipException)
                {
                    var exception = (SipException) err;
                    IResponse response = e.Request.CreateResponse(exception.StatusCode, exception.Message);
                    e.Transaction.Send(response);
                }

                //Unhandled exception logger.
            }
        }


        /// <summary>
        /// A call have changed state.
        /// </summary>
        public event EventHandler<CallEventArgs> CallChanged = delegate { };

        /// <summary>
        /// Used to filter out incoming calls.
        /// </summary>
        /// <remarks>
        /// Can be used either to revoke incoming calls or
        /// to modify data in the call.
        /// </remarks>
        public event EventHandler<CallEventArgs> CallFiltering = delegate { };

        /// <summary>
        /// A dial plan is wanted.
        /// </summary>
        public event EventHandler<DialPlanEventArgs> DialPlanRequested = delegate { };
    }

    public class CallEventArgs : EventArgs
    {
        private readonly Call _call;

        public CallEventArgs(Call call)
        {
            _call = call;
        }
    }

    public class DialPlanEventArgs : EventArgs
    {
        public DialPlanEventArgs(DialPlan dialPlan)
        {
            DialPlan = dialPlan;
        }

        public DialPlan DialPlan { get; private set; }
    }
}