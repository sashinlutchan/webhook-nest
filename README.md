## CURL Example ( For testing )

curl -X POST https://rpgsque84c.execute-api.af-south-1.amazonaws.com/stage/api/v1/webhook/updatewebhook/14057232-2530-48f2-9ad7-bbf42d3182ad \
 -H "Content-Type: application/json" \
 -H "X-Tenant-ID: prod-tenant-456" \
 -H "X-Request-ID: req-98765-abcdef" \
 -H "Accept: application/vnd.api+json" \
 -d '{
"eventType": "order.completed",
"merchant": {
"businessName": "Ocean View Electronics",
"vatNumber": "ZA4012345678",
"contactEmail": "orders@oceanview.co.za",
"businessAddress": {
"unitNumber": "12A",
"streetName": "Marine Parade",
"suburb": "South Beach",
"city": "Durban",
"postalCode": "4001"
}
},
"transaction": {
"orderId": "ORD-2025-789123",
"totalAmount": 2847.50,
"currency": "ZAR",
"paymentMethod": "card",
"cardDetails": {
"lastFour": "4567",
"brand": "visa",
"expiryMonth": "08",
"expiryYear": "2027"
},
"processedAt": "2025-07-25T16:42:30Z"
},
"customer": {
"customerId": "CUST-SA-991827",
"fullName": "Priya Naidoo",
"phoneNumber": "+27 31 234 5678",
"deliveryAddress": {
"recipientName": "Priya Naidoo",
"addressLine1": "87 Silverton Road",
"addressLine2": "Morningside",
"city": "Durban",
"province": "KwaZulu-Natal",
"postalCode": "4001"
}
},
"items": [
{
"sku": "LAPTOP-MSI-001",
"productName": "MSI Gaming Laptop 15.6\"",
"quantity": 1,
"unitPrice": 18999.00,
"totalPrice": 18999.00
},
{
"sku": "MOUSE-LOG-002",
"productName": "Logitech MX Master 3",
"quantity": 2,
"unitPrice": 1249.00,
"totalPrice": 2498.00
}
],
"shipping": {
"method": "express",
"trackingNumber": "TRK-ZA-2025-445566",
"estimatedDelivery": "2025-07-27T10:00:00Z",
"shippingCost": 150.00
},
"webhook": {
"attemptNumber": 1,
"maxRetries": 3,
"retryInterval": "5m",
"signature": "sha256=a1b2c3d4e5f6789"
}
}'

curl -X DELETE https://rpgsque84c.execute-api.af-south-1.amazonaws.com/stage/api/v1/webhook/updatewebhook/14057232-2530-48f2-9ad7-bbf42d3182ad \
 -H "Content-Type: application/json" \
 -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9" \
 -H "X-Client-Version: 2.1.4" \
 -H "User-Agent: WebhookClient/2.1.4" \
 -d '{
"userId": "user-789-xyz",
"isEnabled": false,
"currentBalance": 42150.25,
"profile": {
"registeredOn": "2024-09-15T08:20:00Z",
"source": "mobile-app",
"categories": ["premium", "early-adopter"]
},
"location": {
"street": "456 Ocean Drive",
"city": "Durban",
"province": "KwaZulu-Natal",
"country": "ZA",
"postalCode": "4001"
},
"sessions": [
{ "loginTime": "2025-07-25T14:15:00Z", "clientIP": "10.0.1.100", "device": "mobile" },
{ "loginTime": "2025-07-24T16:45:00Z", "clientIP": "10.0.1.101", "device": "desktop" },
{ "loginTime": "2025-07-23T11:30:00Z", "clientIP": "10.0.1.102", "device": "tablet" }
],
"quotas": {
"perHour": 250,
"perDay": 5000,
"perMonth": 150000
},
"preferences": {
"notifications": true,
"theme": "dark",
"language": "en-ZA"
}
}'
