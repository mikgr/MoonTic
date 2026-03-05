# MoonTic

## Prio

### Must have

#### Tech stuff
- [x] Off-load onchain jobs to background worker
- [ ] Dynamo DB, switch to
- [ ] Use proper google login https://developers.google.com/identity/sign-in/web/sign-in
- [ ] Deploy to AWS 
- [x] Payment, Remove first dummy, and track fiat payments 

#### User Stories
- [ ] #1 As a **ticket holder**, I want to be able to **put my ticket up for sale**, Such that i can recover my money when im not able to attend the event.

- [ ] As an **organizer**, i want to **export list of tickets/secrets** Such that i can integrate with my own ushering system and have an offline backup of the data.
- [ ] As a **organizer**, I want to be able to **usher tickets**, in the web UI, as a fall back for the app.
- [ ] As an **organizer**, I want to be able to **cap the resale-price** of tickets, Such that i can prevent scalping.


### Should have
- [ ] As a **ticket holder**, I want a **phone-app**, Such that i can hold my ticket in my phone.
- [ ] As **moontic.net**, i want pictures on tickets, such that the tickets look nice and feel more like a real ticket.
- [ ] As **moontic.net**, i want a nice **landing page**, such that the page feels complete and nice .
- [ ] As a **ticket holder/reseller**, I want to **kyc easily**, such that i can receive money legally, 
- [ ] As a **ticket holder**, I want a **notification when venue changes**, such that i can be aware of the change and plan accordingly.
- [ ] As a **ticket holder**, I want to be able to **transfer ticket by email address**, such that i don't have to mess with blockchain addresses. 
- [ ] As a **ticket holder**, I want to always use fresh addresses, such that i will not be doxed!
- [ ] As a **ticket buyer**, I want to be able to buy more than 1 ticket at the time, such that i can bring my friends.

- [ ] As an **organizer**, I want to **kyc easily**, Such that i can receive money legally.
- [ ] As an **organizer**, I want a way to be able to charge back tickets if I have to cancel the event.
- [ ] As an **organizer**, I want to be able to **change the venue+address** of a published event, such that i can change the venue if the venue becomes unavailable.
- [ ] As an **organizer**, I want to be able to **manage orgainzation users**, such that i can multiple event manager / Ushers etc.

#### Tech stuff
- [ ] **user** **aws** **kms** for stroing all the private keys
- [ ] Use proper logging with structured data

### Could have
- [ ] As a **ticket holder**, I want to see an indicator on my wicket when i hav entered the event.
- [ ] As a **ticket holder**, I want to be able to 


### Won't have (this time)
- [ ] As a **ticket buyer**, I want to be able to bid on tickets that are not for sale, such that i can maybe pursuede a holder to sell.
- [ ] As a **ticket buyer**, I want to be able to **connect my wallet**, such that i don't have to log in with email/google/apple and i can pay with stable conin.




// proof checkin event-id ticket-id secret
// [x] todo prevent transfer after checkin
// [x] todo allow check out
// todo prevent check out after cutoff time
// todo re-allow transfer after event end time
// todo withdraw funds (only owner)
// todo enable marketplace after contract sells out
// todo sell ticket (create ask) - Buyer pays in Fiat, must KYC to get money
// todo cancel ask
// todo accept ask
// todo support presale - allocation to owner 