Simple example to demo the usage of Azure Durable Functions 

Features:

- Request a Quote and set a due time.
  If due time is reached before anyone can get to review and confirm the Quote, then the Quote will get automatically cancelled.
  
- Confirm a Quote
  If someone confirms the Quote within the time limit then its status will be set to Pending.

- Once Confirmed, we start a sequential workflow of Activities until we set the Quote as "Delivered".