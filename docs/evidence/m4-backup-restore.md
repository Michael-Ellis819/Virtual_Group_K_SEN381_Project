
# M4 — PostgreSQL Backup and Restore Verification

**Project:** CivicConnect  
**Requirement:** NFR-007 — Database Backup and Recovery  
**Database:** PostgreSQL 17.11  
**Test Date:** 16 October 2026  
**Test Result:** PASS

## 1. Objective

The objective of this test was to verify that the CivicConnect PostgreSQL database could be backed up and successfully restored without affecting the original database.

This test provides implementation evidence for NFR-007, demonstrating basic database backup and recovery capability.

## 2. Test Environment

| Component | Details |
|---|---|
| Operating System | macOS |
| Database System | PostgreSQL 17.11 |
| Original Database | `civicconnect_dev` |
| Restored Database | `civicconnect_restore_test` |
| Backup Tool | `pg_dump` |
| Restore Tool | `pg_restore` |
| Backup Format | PostgreSQL custom format |
| Backup File | `civicconnect_backup.dump` |
| Backup Size | 7.9 KB |

The backup file was stored outside the GitHub repository to prevent accidental exposure of database information.

## 3. Backup Procedure

A custom-format PostgreSQL backup was created using the following command:

```bash
/Library/PostgreSQL/17/bin/pg_dump \
  -h localhost \
  -p 5432 \
  -U postgres \
  -d civicconnect_dev \
  -F c \
  -f ~/CivicConnect_Backups/civicconnect_backup.dump
```

**Result:** PASS

The backup completed without errors.

## 4. Backup Validation

The backup contents were inspected using:

```bash
/Library/PostgreSQL/17/bin/pg_restore \
  --list ~/CivicConnect_Backups/civicconnect_backup.dump
```

The backup contained the following tables:

- `ServiceRequests`
- `RequestNotes`
- `RequestStatusHistory`
- `__EFMigrationsHistory`

The backup also included table data entries, primary-key constraints, indexes and foreign-key constraints.

**Result:** PASS

## 5. Database Restoration

A separate database was created to ensure the original development database remained unchanged.

```bash
/Library/PostgreSQL/17/bin/createdb \
  -h localhost \
  -p 5432 \
  -U postgres \
  civicconnect_restore_test
```

The backup was restored using:

```bash
/Library/PostgreSQL/17/bin/pg_restore \
  -h localhost \
  -p 5432 \
  -U postgres \
  -d civicconnect_restore_test \
  --exit-on-error \
  ~/CivicConnect_Backups/civicconnect_backup.dump
```

**Result:** PASS

The restoration completed without errors.

## 6. Restored Database Verification

The restored database tables were inspected using:

```bash
/Library/PostgreSQL/17/bin/psql \
  -h localhost \
  -p 5432 \
  -U postgres \
  -d civicconnect_restore_test \
  -c "\dt"
```

All four expected tables were present.

**Result:** PASS

## 7. Data Integrity Verification

Record counts were compared between the original database and the restored database.

| Table | Original Records | Restored Records | Result |
|---|---:|---:|---|
| ServiceRequests | 1 | 1 | PASS |
| RequestNotes | 2 | 2 | PASS |
| RequestStatusHistory | 0 | 0 | PASS |
| __EFMigrationsHistory | 3 | 3 | PASS |

All record counts matched.

**Result:** PASS

This confirms that the restored database contained the expected number of records in each table at the time of verification.

## 8. Security Considerations

The following precautions were taken:

- The database backup was stored outside the GitHub repository.
- Database passwords were not included in the backup commands.
- Restoration was performed into a separate test database.
- The original development database was not modified by the restoration procedure.
- Only test procedures and verification results are intended to be committed to GitHub.

## 9. Limitations

The following areas were not verified:

- Field-by-field comparison of restored records.
- Automated or scheduled database backups.
- Recovery time and recovery point objectives.
- Restoration under production conditions.
- Recovery from a corrupted or incomplete backup.
- Application functionality against the restored database.

Therefore, this test demonstrates basic backup and restore capability rather than complete production disaster-recovery readiness.

## 10. Conclusion

The CivicConnect PostgreSQL backup and restore test was successful.

A custom-format backup was created from `civicconnect_dev` and restored into `civicconnect_restore_test`.

The restored database contained all four expected tables, and the record counts matched those in the original database.

**Overall Test Result: PASS**

This provides supporting implementation evidence for **NFR-007 — Database Backup and Recovery** in Milestone 4.

Further verification would be required before claiming production recovery readiness.

## Final Windows recovery exercise — 18 October 2026

A fresh custom-format PostgreSQL backup of the controlled fixture database was restored into a separate disposable database. A before/after snapshot compared the same request, active eligible staff owner, five status-history rows, six notes and one scheduling-audit row. The snapshots matched. Conservative backup age was 0.917 seconds; restore plus consistency verification was 0.676 seconds, meeting the recorded 24-hour RPO and four-hour RTO for this small dataset. The original database was retained. See m4-final-20261018/recovery.json.

Repeat with a disposable database: capture the selected request and associated owner/history/notes/scheduling rows; take `pg_dump --format=custom`; create a separate empty target; restore using `pg_restore --exit-on-error`; query the identical row projection and compare; record actual timestamps and outcomes. Credentials and dump contents remain outside Git. This exercise demonstrates a local recovery procedure, not automatic schedules, off-site backups, production-size recovery or hosted rollback.
