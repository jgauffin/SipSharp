This is a SIP stack which I created a long time ago. 


I do currently not actively maintain it, but moved it here per request from a user.


-------------------------


A sip stack under development. It's far from done and is not usable by any means yet.

Feel free to participate.

Plan:

* Create simplest working sip implementation
 * Create transport layer (done)
 * Create fast parser (done)
 * Create transaction layer (done)
 * Create stack main class (done)
 * Create Registrar (done)
 * Create stateful proxy (in progress)
 * Create B2BUA
 * Create PBX
* Refactor all parts.
* Create SDP implementation
 * Create parser
 * Write tests
 * Add support to the sip stack
* Create RTP/RTCP implementation
 * Create parser
 * Write tests
 * Add support to the sip stack
* Refactor everything
* Write tests
* Refactor more
* Create release.

I'm doing this on my spare time. Development will take time.

The most important goal is to keep all parts modular with as little dependencies as possible. Everything should be very modular.

Steps 3 and 4 doesn't depend on the earlier steps. Feel free to do them.

The goal is to have a complete stack which I can use in our voip system.

