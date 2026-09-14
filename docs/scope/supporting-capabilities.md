# Supporting capabilities

These features support the player and match loop. They are not the product center and should not pull priority away from manual data entry or player analysis.

## Supporting

### 16. Availability and restricted medical details · existing

Team scoped availability gives coaches safe operational status, while injury details remain restricted to staff with explicit medical permission.

**Done when:** authorized staff can maintain current availability and injury history, ordinary roles see only coach safe information, and permission loss removes restricted data from active interface state.

Legacy specs [Unit 50](../../context/feature-specs/50-medical-availability-backend.md) and [Unit 51](../../context/feature-specs/51-medical-availability-ui.md) · code in `backend/src/Api/Endpoints/Players/MedicalEndpoints.cs` and `frontend/src/features/medical/`

### 17. Media references · existing

Staff can upload supported files or add external references, then link them to players, matches, or reports through team scoped and workflow aware actions.

**Done when:** authorized staff can retain and find a media reference, connect it to supported records, stream uploaded content safely, and preserve its audit history.

Legacy specs Units 40 to 42 · code in `backend/src/Api/Endpoints/Media/` and `frontend/src/features/media/`

### 18. Generic import preview · in-progress

The repository can retain CSV or XLSX source files and show a bounded generic preview with import history and capability driven states. The implementation exists, but the progress record does not establish complete authenticated browser verification. It has no active exact processor and does not confirm data into official player statistics.

**Done when:** authorized staff can upload a supported file, inspect its safe raw preview and history, and clearly see that validation or confirmation is unavailable without an approved exact processor.

- [x] Build the generic backend and interface from legacy specs Units 43 to 45
- [ ] Verify it: `/check verify generic import preview`
- [ ] Test it: `/test generic import preview`
- [ ] Review it: `/check review generic import preview`
- [ ] Document it: `/document generic import preview`

Legacy specs Units 43 to 45 · code in `backend/src/Api/Endpoints/Imports/` and `frontend/src/features/imports/`
