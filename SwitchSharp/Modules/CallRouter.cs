using System;
using SipSharp;
using SipSharp.Calls;
using SipSharp.Servers.Registrar;
using SwitchSharp.DialPlans;

namespace SwitchSharp.Modules
{
    public class CallRouter : ISwitchModule
    {
        private readonly DialPlanManager _dialPlanManager;
        private readonly Registrar _registrar;

        public CallRouter(Registrar registrar)
        {
            _registrar = registrar;
        }

        private Call CreateCall(IRequest request)
        {
            var call = new Call {State = CallState.Proceeding, Reason = CallReasons.Direct};
            return call;
        }

        private void DialPlanHunt(IRequest request)
        {
            var dialPlan = new DialPlan(request.From, request.To);
            _dialPlanManager.PreProcess(dialPlan);

            try
            {
                DialPlanRequested(this, new DialPlanEventArgs(dialPlan));
            }
            catch (Exception err)
            {
                // oops, someone threw an exception.
            }

            _dialPlanManager.Process(dialPlan);
        }

        private void LookupCaller(IRequest request, Call call)
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
        /// <param name="call"></param>
        private void LookupDestination(IRequest request, Call call)
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


        private void TriggerCallFiltering(Call call, RequestContext context)
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
                    IResponse response = context.Request.CreateResponse(exception.StatusCode, exception.Message);
                    context.Transaction.Send(response);
                }

                //Unhandled exception logger.
            }
        }

        #region ISwitchModule Members

        /// <summary>
        /// A new request have come.
        /// </summary>
        /// <param name="context">Request context</param>
        /// <returns>What we should to with the request.</returns>
        public ProcessingResult ProcessRequest(RequestContext context)
        {
            IResponse trying = context.Request.CreateResponse(StatusCode.Trying, "We are trying here.");
            context.Transaction.Send(trying);

            // create call info
            Call call = CreateCall(context.Request);
            CallChanged(this, new CallEventArgs(call));
            LookupCaller(context.Request, call);
            LookupDestination(context.Request, call);
            call.CalledParty = call.Destination;

            // filter out unwanted calls.
            TriggerCallFiltering(call, context);

            // Caller and Destination are both external (not found)
            if (!call.Caller.IsInternal && !call.Caller.IsInternal)
            {
                context.Response.StatusCode = StatusCode.NotFound;
                context.Response.ReasonPhrase = "Destination is not found in this server.";
                return ProcessingResult.SendResponse;
            }

            // hunt dial plan.
            DialPlanHunt(context.Request);
            return ProcessingResult.SendResponse;
        }

        #endregion

        /// <summary>
        /// A dial plan is wanted.
        /// </summary>
        public event EventHandler<DialPlanEventArgs> DialPlanRequested = delegate { };

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
    }
}