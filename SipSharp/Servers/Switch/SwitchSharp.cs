using System;
using SipSharp.Messages;
using SipSharp.Servers.Registrar;
using SipSharp.Transactions;
using SipSharp.Transports;
using SipSharp.Calls;

namespace SipSharp.Servers.Switch
{
    public class SwitchSharp
    {
        private SipStack _stack = new SipStack();
        private Registrar.Registrar _registrar;

        public SwitchSharp()
        {
            _stack.RequestReceived += OnRequest;
        }

        private void OnRequest(object sender, RequestEventArgs e)
        {
            if (e.Request.Method == SipMethod.INVITE)
            {
                // Always send trying.
                IResponse response = e.Request.CreateResponse(StatusCode.Trying, "We are trying here.");
                e.Transaction.Send(response);

                // create call info
                Call call = CreateCall(e.Request);
                CallChanged(this, new CallEventArgs(call));

                LookupCaller(e.Request, e.Transaction, call);
                LookupDestination(e.Request, e.Transaction, call);

                DialPlanHunt(call);
            }
            else if (e.Request.Method == SipMethod.CANCEL)
            {
                
            }
        }

        private void LookupCaller(IRequest request, IServerTransaction transaction, Call call)
        {
            Registration reg = _registrar.Get(request.From);
            if (reg == null)
                return;
            call.Caller = new CallParty {Name = reg.RealName, Number = reg.PhoneNumber, IsInternal = true};
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
                return;
            call.Destination = new CallParty { Name = reg.RealName, Number = reg.PhoneNumber, IsInternal = true };
        }


        private Call CreateCall(IRequest request)
        {
            
        }

        public void AddListener(ITransport transport)
        {
            _stack.AddTransport(transport);
        }

        public IRegistrationRepository RegistrationDatabase
        {
            get; 
            set;
        }

        public void Start(string domain)
        {
            _registrar = new Registrar.Registrar(_stack, RegistrationDatabase)
                             {
                                 Domain = new SipUri(null, domain),
                                 Realm = domain
                             };
            
        }

        /// <summary>
        /// A call have changed state.
        /// </summary>
        public event EventHandler<CallEventArgs> CallChanged = delegate { };
    }

    public class CallEventArgs : EventArgs
    {
        private readonly Call _call;

        public CallEventArgs(Call call)
        {
            _call = call;
        }
    }
}
