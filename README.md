
# 📬 Webhook Nest

**Webhook Nest** gives you a unique URL where external systems can send webhooks/callbacks, and you can view the incoming payloads on a live frontend. It’s built for **near real-time inspection** with **automatic deletion** via DynamoDB TTL.

---

## 🎯 Purpose

- Capture incoming webhook requests from any system.
- Display payloads on the frontend within seconds.
- Keep storage **ephemeral** with auto-expiry (TTL).



---

## 🧩 Service Components

**Backend**
- AWS API Gateway
- AWS Lambda
- AWS DynamoDB (TTL enabled)

**Frontend**
- React app **AWS S3**
- Served globally via **AWS CloudFront**

---

## 🏗️ Architecture Overview

1. External service posts a webhook to the **API Gateway** endpoint.
2. **Lambda** validates and stores the request in **DynamoDB** with a TTL.
3. The **frontend** (S3 + CloudFront) fetches recent items and renders them **near real-time** (polling).

**Retention:** Items auto-delete after the configured TTL period ( set inside code ).

---

## 🔌 API Contract (High-Level)

- **GET /api/v1/webhook/health**  
  Lightweight health check for uptime verification. Returns status and timestamp.

- **POST /api/v1/webhook/createwebhook**  
  Creates a new webhook URL. Returns webhook ID and URL for receiving requests.

- **GET /api/v1/webhook/getwebhook/{id}**  
  Retrieves webhook details by ID. Returns webhook configuration and metadata.

- **GET /api/v1/webhook/getwebhook/events/{id}**  
  Returns the most recent webhook events for a specific webhook ID.

- **ANY /api/v1/webhook/updatewebhook/{id}**  
  Accepts all HTTP methods (GET, POST, PUT, DELETE, PATCH, OPTIONS, HEAD) to capture incoming webhook requests. Stores method, headers, and payload for inspection.

> Notes:  
> • Webhook requests are stored short-term only with TTL auto-expiry.  
> • Request bodies are treated as opaque JSON payloads (no schema enforcement).  
> • All headers and request metadata are captured for debugging.  


---

## 🗃️ Data Model (DynamoDB)

### **Primary Table Structure:**
- **Partition key:** `pk` - Either `WEBHOOK#{id}` for webhook definitions or `EVENT#{guid}` for webhook events
- **Sort key:** `sk` - Either `WEBHOOK` for webhook definitions or `WEBHOOK#{id}` for events  
- **TTL attribute:** `expiresAt` (epoch seconds, 1 hour from creation)

### **Global Secondary Index (GSI1) - "LookUp":**
- **GSI1PK:** `EVENT#{webhook_id}` - Groups events by webhook
- **GSI1SK:** `yyyyMMddHH` - Time-based partitioning for efficient queries

### **Webhook Definition Attributes:**
- `url` - The generated webhook URL endpoint
- `createdAt` - Creation timestamp (yyyy-MM-dd HH:mm:ss format)

### **Webhook Event Attributes:**
- `method` - HTTP method (GET, POST, PUT, DELETE, etc.)
- `headers` - Map of request headers (key-value pairs)
- `data` - Request payload/body as nested JSON object
- `createdAt` - Event timestamp (yyyy-MM-dd HH:mm:ss format)

**Behavior:**  
- Items auto-expire after 1 hour based on `expiresAt` TTL ( Configurable In Code ).  
- Webhook events are queried via GSI1 using webhook ID and current date.  
- Events are grouped by webhook ID for efficient retrieval.

---

## 🖥️ Frontend Behavior

- Hosted as static assets in **S3**, served via **CloudFront**.  
- Periodically polls the **GET /events** endpoint (e.g., every few seconds).  
- Displays for each event: method, path, headers, body, received timestamp.  
- Presents a limited recent window only (aligned with ephemeral retention).

---

## ⚙️ Configuration (High-Level)

**Backend (Lambda)**
- Table name for events storage
- TTL duration (in seconds)
- Lambdas for backend code
- ApiGateway fir serving Api endpoints


**Frontend**
- API base URL (the API Gateway URL) used for polling

---

## 🔐 Security & Limits

- **IAM least privilege** for Lambda access to DynamoDB.  
- **Rate limiting / payload size** governed by API Gateway configuration.  
- **No secrets persisted** in event payloads (treat incoming data as untrusted).  
- **Logging** limited to operational needs, avoid logging sensitive payloads.

---

## 🚀 Deployment (Overview)

1. Provision **DynamoDB** table and enable **TTL** on the `expiresAt` attribute.  
2. Deploy **Lambda** and connect it to **API Gateway** routes (`/api/v1/webhook/createwebhook`, `/api/v1/webhook/getwebhook/{id}`, `/api/v1/webhook/getwebhook/events/{id}`, `/api/v1/webhook/updatewebhook/{id}`, `/api/v1/webhook/health`).  
3. Configure **CORS** on API Gateway to allow the CloudFront origin.  
4. Build the **React** frontend, upload to **S3**, and front it with **CloudFront**.  
5. Point the frontend at the API base URL.  
6. Test: send a webhook to `/api/v1/webhook/updatewebhook/{id}` and verify it appears on the UI within seconds.

---

## ✅ Operational Notes

- **Near real-time** is achieved via short-interval polling from the frontend.  
- **Data lifecycle** is controlled entirely by DynamoDB TTL.  
- **Scaling** is serverless: API Gateway + Lambda + DynamoDB + CloudFront adjust to load.  
- **Observability**: use CloudWatch metrics/logs and DynamoDB metrics for health checks.

---

## 🧪 Testing with cURL

Use these example requests to test your webhook endpoints:

### **GET Request**
```bash
curl -X GET \
  "your-apigateway-url/api/v1/webhook/updatewebhook/your-webhook-id" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-API-Key: your-api-key" \
  -H "X-Request-ID: req_get_12345" \
  -H "Accept: application/json" \
  -H "User-Agent: WebhookClient/1.0"
```

### **POST Request**
```bash
curl -X POST \
  "your-apigateway-url/api/v1/webhook/updatewebhook/your-webhook-id" \
  -H "Content-Type: application/json" \
  -d '{
    "webhook_config": {
      "name": "Payment Webhook",
      "url": "https://myapp.com/webhooks/payments",
      "secret": "wh_secret_12345",
      "active": true
    },
    "events": ["payment.completed", "payment.failed"],
    "metadata": {
      "environment": "staging",
      "created_by": "john.doe@company.com"
    }
  }'
```

### **PUT Request**
```bash
curl -X PUT \
  "your-apigateway-url/api/v1/webhook/updatewebhook/your-webhook-id" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-API-Key: your-api-key" \
  -H "X-Request-ID: req_12345" \
  -d '{
    "employee_id": "EMP-2024-1057",
    "personal_info": {
      "first_name": "Sarah",
      "last_name": "Johnson",
      "email": "sarah.johnson@company.com",
      "phone": "+27-11-555-0123",
      "date_of_birth": "1985-03-15",
      "nationality": "South African"
    },
    "employment_details": {
      "position": "Senior Software Engineer",
      "department": "Engineering",
      "manager": "Mike Thompson",
      "start_date": "2021-07-12",
      "salary": 850000,
      "currency": "ZAR",
      "employment_type": "Full-time"
    },
    "address": {
      "street": "45 Sandton Drive",
      "city": "Johannesburg",
      "province": "Gauteng",
      "postal_code": "2196",
      "country": "South Africa"
    },
    "skills": [
      "JavaScript",
      "Python", 
      "React",
      "Node.js",
      "AWS",
      "Docker"
    ],
    "benefits": {
      "medical_aid": true,
      "pension_fund": true,
      "life_insurance": true,
      "annual_leave_days": 21
    }
  }'
```

### **DELETE Request**
```bash
curl -X DELETE \
  "your-apigateway-url/api/v1/webhook/updatewebhook/your-webhook-id" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "X-API-Key: your-api-key" \
  -H "X-Request-ID: req_delete_67890" \
  -H "X-Confirm-Delete: true" \
  -H "Accept: application/json" \
  -H "User-Agent: WebhookClient/1.0"
```

> **Note:** Replace `your-apigateway-url` with your actual API Gateway URL and `your-webhook-id` with a valid webhook ID from your system.

---

## ⚙️ Pulumi Config Setup

### 📚 Configuration Parameters

**Purpose:** This configuration controls the deployment environment and AWS region for the webhook-nest infrastructure.

### **Required Parameters:**

- **`app:stage`** - Deployment environment (e.g., `staging`, `production`, `dev`)
  - Controls resource naming and environment separation
  - Used in S3 bucket names and resource tags

- **`aws:region`** - AWS region for deployment (e.g., `af-south-1`, `us-east-1`)
  - Determines where all AWS resources will be created
  - Affects latency and compliance requirements

### **Configuration Examples:**

**Backend Configuration (`backend/webhook_nest.main/Pulumi.staging.yaml`):**
```yaml
config:
  aws:region: af-south-1
  app:stage: staging
```

**Frontend Configuration (`frontend/webhook_nest/Pulumi.staging.yaml`):**
```yaml
config:
  aws:region: af-south-1
  app:stage: staging
```

### 🛠️ Deployment Commands

### **Initial Setup (First Time Only):**

**Backend:**
```bash
cd backend/webhook_nest.main
pulumi stack init staging
pulumi config set app:stage staging
pulumi config set aws:region af-south-1
pulumi up --yes   # Deploy infrastructure and Apis
```

**Frontend:**
```bash
cd frontend/webhook_nest
pulumi stack init staging
pulumi config set app:stage staging
pulumi config set aws:region af-south-1
yarn deploy    # Deploy infrastructure and React app
```

### **Subsequent Deployments:**

**Backend:**
```bash
cd backend/webhook_nest.main
pulumi up --yes
```

**Frontend:**
```bash
cd frontend/webhook_nest
yarn deploy    # Deploy infrastructure and React app
yarn destroy   # Remove all deployed infrastructure
yarn dev       # Run React app locally for development
yarn build     # Build React files for production
yarn pretty    # Format code with Prettier
```

