# Design and responsibility views

Solid nodes below identify the implemented read slice. PostgreSQL/EF Core and State are planned integrations described separately in the PED.

```mermaid
flowchart TD
    HTTP[HTTP request and validated cookie] --> C[RequestsController]
    C --> S[RequestQueryService]
    S --> R[IRequestReader]
    R --> D[DevelopmentRequestReader]
    S --> P[IRequestAccessPolicy]
    P --> A[RequestAccessPolicy]
    S --> DTO[Authorised RequestDetailsDto]
    DTO --> C
    C --> JSON[JSON with permitted notes only]
```

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant Service as RequestQueryService
    participant Reader as IRequestReader
    participant Policy as IRequestAccessPolicy
    Client->>Controller: GET /requests/id with authenticated ticket
    Controller->>Controller: Validate UUID
    Controller->>Service: ReadAsync(principal, id, cancellation)
    Service->>Reader: FindAsync(id)
    Reader-->>Service: Snapshot or null
    Service->>Policy: EvaluateRead(principal, snapshot) if found
    Policy-->>Service: Allowed and internal-note permission
    Service-->>Controller: Filtered DTO or null
    Controller-->>Client: 200 JSON or 404 ProblemDetails
```

Authentication is not the resource decision. Permission is checked before a DTO leaves the service. Exceptions reach a generic 500 boundary. Future writes use a distinct application service: authorise action -> Bianca's transition service -> Michael's transactional store. They are not implemented by this diagram.
