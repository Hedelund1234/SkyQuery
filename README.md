SkyQuery is a distributed microservice system designed for secure retrieval, storage, and delivery of satellite imagery based on MGRS coordinates. The project serves as a proof-of-concept for a defense-oriented application where field operators or analysts can quickly request up-to-date-imagery from national geodata sources.

The solution is built using .NET Aspire and Dapr, enabling loosely coupled communication between services through pub/sub messaging, service invocation, and state management. Each microservice follows a clean modular architecture with dedicated responsibilities. For example, AuthService handles authentication and JWT issuance, ImageService manages image retrieval and caching, and AppGateway orchestrates API routing and service access.

Deployed with Docker Compose, the system emphasizes modern DevOps practices and scalability through containerization and sidecar patterns. The project also explores Security-by-design and Privacy-by-Design principles, integrating authentication, authorization and zero trust principles throughout the stack.

Overall, SkyQuery demonstrates a real-world application of miicroservice design, Dapr integration, and cloud-ready architecture in a scenario inspired by tactical intelligence and satellite data interoperability.
