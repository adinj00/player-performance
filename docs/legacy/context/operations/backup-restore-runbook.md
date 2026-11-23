# Backup and restore runbook

Status: BLOCKED pending PostgreSQL and object-storage providers. Owner: Database/backup owner. RPO/RTO, retention, encryption, and test frequency are approval decisions and are not guessed here.

Before every production migration, obtain and record a successful PostgreSQL backup or provider snapshot identifier. Backups must be encrypted, access-controlled, retained according to the approved policy, and tested through a documented restore rehearsal. Use distinct runtime, migration, and backup identities where supported.

The restore rehearsal must verify: database integrity, application migration compatibility, Data Protection key availability/recovery controls, and object-storage media/import recovery/versioning/retention. Import temporary working data is not authoritative and is not backed up as application data.

Record backup identifier, encryption/retention decision, operator, restore-test date, result, and evidence location. Never record credentials, connection strings, bucket names, or keys.

