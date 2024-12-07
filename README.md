# Assumptions made
I assume we would return a bad request on validation errors. We can alternatively return a valid response with a "rejected" Status. 

Another assumption was that we would return a "rejected" status when the bank post request fails. What this should return depends on what clients/product want. 

# Design choices
Validation of arguments in PostPaymentAsync was done in the controller. This can be done in a separate function or class if the logic is reusable or gets too complicated.

I would consider putting the SendPostRequest in the BankClient in a separate custom generic HttpClient that only handles sending a post request and returning the specified type. The generic HttpClient can also implement retries and authorization. I left it as a private function in the BankClient as we are only sending one http request in the whole solution. 

There are snapshot tests in the integration tests project. If the project was larger and had more tests I would consider splitting snapshot tests out to a separate project. 

I wasn't sure whether "Ensure your submission validates against no more than 3 currency codes" means to only validate against 3 currency codes or currency codes with 3 letter so I chose the former. 

The currency codes are stored in a json, in an actual product I would consider getting this from an external list the company's services shares. 

# Additional comments
PaymentsRepository Get() is async even though it doesn't need to be. This was done assuming that the payments repository would use an actual database instead of just a local collection. 