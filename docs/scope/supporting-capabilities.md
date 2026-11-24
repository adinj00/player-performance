# Supporting capabilities

These features support the player and match loop. They are not the product center and should not pull priority away from manual data entry or player analysis.

## Supporting

### 16. Availability and restricted medical details · existing · GA

Team scoped availability gives coaches safe operational status, while injury details remain restricted to staff with explicit medical permission.

**Done when:** authorized staff can maintain current availability and injury history, ordinary roles see only coach safe information, and permission loss removes restricted data from active interface state.

Direct database access in the medical endpoints remains a boundary issue recorded in spec 0001. Route any change in that boundary through a focused decision.

- [ ] Verify it: `/check verify availability and restricted medical details`
- [ ] Test it: `/test availability and restricted medical details`
- [ ] Review it: `/check review availability and restricted medical details`
- [ ] Document it: `/document availability and restricted medical details`

Legacy specs [Unit 50](../legacy/context/feature-specs/50-medical-availability-backend.md) and [Unit 51](../legacy/context/feature-specs/51-medical-availability-ui.md) · code in `backend/src/Api/Endpoints/Players/MedicalEndpoints.cs` and `frontend/src/features/medical/`

### 17. Media references · existing · GA

Staff can upload supported files or add external references, then link them to players, matches, or reports through team scoped and workflow aware actions.

**Done when:** authorized staff can retain and find a media reference, connect it to supported records, stream uploaded content safely, and preserve its audit history.

- [ ] Verify it: `/check verify media references`
- [ ] Test it: `/test media references`
- [ ] Review it: `/check review media references`
- [ ] Document it: `/document media references`

Legacy specs Units 40 to 42 · code in `backend/src/Api/Endpoints/Media/` and `frontend/src/features/media/`

### 18. Generic import preview · in-progress · GA

The repository can retain CSV or XLSX source files and show a bounded generic preview with import history and capability driven states. The implementation exists, but the progress record does not establish complete authenticated browser verification. It has no active exact processor and does not confirm data into official player statistics.

**Done when:** authorized staff can upload a supported file, inspect its safe raw preview and history, and clearly see that validation or confirmation is unavailable without an approved exact processor.

- [x] Build the generic backend and interface from legacy specs Units 43 to 45
- [ ] Verify it: `/check verify generic import preview`
- [ ] Test it: `/test generic import preview`
- [ ] Review it: `/check review generic import preview`
- [ ] Document it: `/document generic import preview`

Legacy specs Units 43 to 45 · code in `backend/src/Api/Endpoints/Imports/` and `frontend/src/features/imports/`
