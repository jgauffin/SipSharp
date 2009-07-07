using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Dialogs
{
    class DialogManager
    {
        public IDialog Create(IResponse response)
        {
            return null;
        }

        public IResponse CreateResponse(IRequest request)
        {
            /*
             * When a UAS responds to a request with a response that establishes a dialog 
             * (such as a 2xx to INVITE), the UAS MUST copy all Record-Route header field 
             * values from the request into the response (including the URIs, URI parameters, 
             * and any Record-Route header field parameters, whether they are 
             * known or unknown to the UAS) and MUST maintain the order of those values. 
             */

            /* The UAS MUST add a Contact header field to the response. 
             * The Contact header field contains an address where the UAS would like to be 
             * contacted for subsequent requests in the dialog (which includes the ACK for 
             * a 2xx response in the case of an INVITE). Generally, the host portion of this 
             * URI is the IP address or FQDN of the host. The URI provided in the Contact header 
             * field MUST be a SIP or SIPS URI. If the request that initiated the dialog contained a
             * SIPS URI in the Request-URI or in the top Record-Route header field value, 
             * if there was any, or the Contact header field if there was no Record-Route header field, 
             * the Contact header field in the response MUST be a SIPS URI. 
             * The URI SHOULD have global scope (that is, the same URI can be used in messages 
             * outside this dialog). The same way, the scope of the URI in the Contact header 
             * field of the INVITE is not limited to this dialog either. It can therefore be used 
             * in messages to the UAC even outside this dialog.
            */
            return null;
        }
    }
}
