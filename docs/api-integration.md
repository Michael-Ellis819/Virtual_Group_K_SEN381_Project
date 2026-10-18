# CivicConnect application interfaces

Current M4 code uses MVC forms and the retained JSON details/status endpoints in one application. Controllers obtain identity from the protected server session. Stored account activation, role, category grants and security stamp are rechecked on each authenticated request.

| Route | Access and behaviour |
| --- | --- |
| GET/POST /account/sign-in | Enabled stored account and password hash verification. No client-selected role. Forms use antiforgery |
| POST /account/sign-out | Ends the session; antiforgery required |
| GET /portal/requests | Requesters see their own records; staff see permitted categories. Search/filter/sort is applied after database permission scope |
| GET/POST /portal/requests/new | Requester submission; agreed category, 2,000-character description and 250-character location bounds; actor/key uniqueness and atomic history/feedback |
| GET /portal/requests/{id} | Filtered details. Missing and inaccessible records share 404. Requesters receive public notes/status history |
| POST .../assign | Staff assignment/claim; eligible active stored owner, expected version and atomic owner/status/history/feedback |
| POST .../status | Staff lifecycle action with required feedback; first assignment uses the separate ownership command |
| POST .../notes | Appends author/time and public/internal visibility; earlier entries remain |
| POST .../schedule | Controlled priority and nullable UTC due date with version check and scheduling audit |
| GET /portal/management | Management-only saved counts and matching reference/category/status rows |
| GET /requests/{id} | Retained protected JSON details projection |
| GET /requests/antiforgery | Antiforgery token for authenticated JSON callers |
| POST /requests/{id}/status | Retained JSON status update with antiforgery, central permission and expected status/version. Ownerless assignment is rejected |

All form mutations validate antiforgery. A stale form/session token is rejected rather than replayed; after signing out or switching account, a fresh page is required. Conflicting versions return 409. Internal exception messages and payloads are excluded from responses; action-specific 500 titles identify the failed operation. Custom logs retain only exception type and aggregate degradation signals. Responses use Cache-Control: no-store.

IPortalAccounts and IPortalRequestStore have registered EF implementations. IRequestReader and IRequestStatusWriter retain the established read/status boundaries. The writer checks lifecycle legality, while the assignment writer also checks acting and owning stored accounts. Application append-only history does not prevent a privileged database administrator changing tables.

See the continuing RTM and final verification reports for acceptance evidence and limits. A controlled local host and generated test certificate do not establish production deployment security.
