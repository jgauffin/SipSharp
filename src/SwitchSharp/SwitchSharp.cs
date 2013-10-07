using System;
using SipSharp;
using SipSharp.Calls;
using SipSharp.Dialogs;
using SipSharp.Messages;
using SipSharp.Servers.Registrar;
using SipSharp.Transactions;
using SipSharp.Transports;
using SwitchSharp.DialPlans;
using SwitchSharp.Modules;

namespace SwitchSharp
{
    public class SwitchSharp : IRequestHandler
    {
        private readonly SipStack _stack = new SipStack();
        private DialogManager _dialogManager;
        private Registrar _registrar;
        private RegistrationRepository _regRepos;
        private CallRouter _callRouter;

        public SwitchSharp()
        {
            _stack.Register(this);
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


        public void Start(string domain)
        {
            if (RegistrationDatabase == null)
                throw new InvalidOperationException("Property RegistrationDatabase must have been assigned.");

            _registrar = new Registrar(_stack, RegistrationDatabase)
                             {
                                 Domain = new SipUri(null, domain),
                                 Realm = domain
                             };
            _callRouter = new CallRouter(_registrar);
            _stack.Start();
        }



        public ProcessingResult ProcessRequest(RequestContext context)
        {
            if (context.Request.Method == SipMethod.INVITE)
                return _callRouter.ProcessRequest(context);
            
            return ProcessingResult.Continue;
        }
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