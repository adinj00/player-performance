# Context migration record

**Date**: 2026-09-14
**Source revision**: `cfeab02514a2f16b3a245f18c55c34e8552be38a`
**Rollout branch**: `chore/project-context-authority-rollout`

## Inventory

The source inventory was read from the clean source revision before the move. Every source entry was tracked and unchanged. No untracked entry was present. The destination `docs/legacy/context/` did not exist.

| Category | Expected | Found | Result |
|---|---:|---:|---|
| Former core context files | 6 | 6 | match |
| Old feature specifications | 56 | 56 | match |
| Operations files | 5 | 5 | match |
| Reference files | 5 | 5 | match |
| Completed migration prompt | 1 | 1 | match |
| Total | 73 | 73 | match |

The completed migration prompt is `prompt-official-agent-skills-migration-and-review.md`. Its concrete filename differs from the generic category wording in spec 0001, but its content confirms it is the expected completed migration prompt. This is not a new material class.

All 73 entries are regular files. There are no symbolic links or other entry types.

## Preservation manifest

The source manifest was generated before the move. The destination manifest was generated after the move. Paths below are relative to `context/` in the source and `docs/legacy/context/` in the destination.

| Relative path | Type | Source SHA-256 | Destination SHA-256 | Result |
|---|---|---|---|---|
| `ai-workflow-rules.md` | regular file | `6164d56b2fabbff35ef04462afa0229ffb2b6f1c338995f98c8e8e4da0b61b10` | `6164d56b2fabbff35ef04462afa0229ffb2b6f1c338995f98c8e8e4da0b61b10` | match |
| `architecture.md` | regular file | `c6e20cc78769a891f870d4cda152341117c3afd5b95c5938b986b4fc42202982` | `c6e20cc78769a891f870d4cda152341117c3afd5b95c5938b986b4fc42202982` | match |
| `code-standards.md` | regular file | `8eff5a8d64db5b33fbf4c97b5d1c204ec60b7facee272086a0de1b61c38a6bc5` | `8eff5a8d64db5b33fbf4c97b5d1c204ec60b7facee272086a0de1b61c38a6bc5` | match |
| `feature-specs/00-build-plan.md` | regular file | `5cb089f20bca02a4cdc76fb55ca394e1bf32515c3ec4a8bcb10b939997d2bc92` | `5cb089f20bca02a4cdc76fb55ca394e1bf32515c3ec4a8bcb10b939997d2bc92` | match |
| `feature-specs/01-project-boilerplate-cleanup.md` | regular file | `be8093d8eacaff89cca13e009f1c8d8e5464e9933e8931c3238da8e9aece7d25` | `be8093d8eacaff89cca13e009f1c8d8e5464e9933e8931c3238da8e9aece7d25` | match |
| `feature-specs/02-frontend-app-shell-baseline.md` | regular file | `91968e8f15e6f94eb5ad410d08157605b67e5f63bc2adb29f5cacdfe7cee921a` | `91968e8f15e6f94eb5ad410d08157605b67e5f63bc2adb29f5cacdfe7cee921a` | match |
| `feature-specs/03-frontend-routing-foundation.md` | regular file | `253c367360cd52ef88c20e5913924b494f66487deb83671d575c64661ff159ae` | `253c367360cd52ef88c20e5913924b494f66487deb83671d575c64661ff159ae` | match |
| `feature-specs/04-code-formatting-foundation.md` | regular file | `5366e2478caa5baf6d379a7295d0964d43fb2786e6ae4aab32f28f6cb4b9d53c` | `5366e2478caa5baf6d379a7295d0964d43fb2786e6ae4aab32f28f6cb4b9d53c` | match |
| `feature-specs/05-common-ui-primitives-foundation.md` | regular file | `78f6273ea47ef381ea3d357c5b05e8deeaf0d05a880aad96a36e87ce75d3bddd` | `78f6273ea47ef381ea3d357c5b05e8deeaf0d05a880aad96a36e87ce75d3bddd` | match |
| `feature-specs/06-backend-solution-baseline.md` | regular file | `1591fb6702425bf47cfe92d3cd8a03b623991ae359f9f71544518883d86505f4` | `1591fb6702425bf47cfe92d3cd8a03b623991ae359f9f71544518883d86505f4` | match |
| `feature-specs/07-backend-configuration-baseline.md` | regular file | `628e131fc7fc3c2b12ca5129cd23ab18a56b312636b8e05b1be68632a501e71a` | `628e131fc7fc3c2b12ca5129cd23ab18a56b312636b8e05b1be68632a501e71a` | match |
| `feature-specs/08-backend-api-error-handling-foundation.md` | regular file | `9f8385248d034400ed99f9c4514db359d3e212a3c1173888dc18b20026855bc5` | `9f8385248d034400ed99f9c4514db359d3e212a3c1173888dc18b20026855bc5` | match |
| `feature-specs/09-backend-testing-foundation.md` | regular file | `a7b4acc206b3c4dd3f1d953b0b115456fdf52c440cbacf8f56c51bcb7a58433a` | `a7b4acc206b3c4dd3f1d953b0b115456fdf52c440cbacf8f56c51bcb7a58433a` | match |
| `feature-specs/10-frontend-api-client-environment-foundation.md` | regular file | `ca9e89b901521ce1a5392eacb12b0642a46c3dce69868beebb48bd18488a2ca1` | `ca9e89b901521ce1a5392eacb12b0642a46c3dce69868beebb48bd18488a2ca1` | match |
| `feature-specs/11-frontend-form-validation-foundation.md` | regular file | `43a15b960cc0ce47e1debded5b20865bdba214cde32bd23a429247c2f4acb094` | `43a15b960cc0ce47e1debded5b20865bdba214cde32bd23a429247c2f4acb094` | match |
| `feature-specs/12-backend-persistence-foundation.md` | regular file | `ffb96869ede3c810eb3e84cec8503eb50f3382af7480320647dfd2e287088f7b` | `ffb96869ede3c810eb3e84cec8503eb50f3382af7480320647dfd2e287088f7b` | match |
| `feature-specs/13-backend-domain-shared-primitives.md` | regular file | `9ce125dc44d30d5e6dee7716a4b8aebcdf3056e04090180bd808ddf65762522d` | `9ce125dc44d30d5e6dee7716a4b8aebcdf3056e04090180bd808ddf65762522d` | match |
| `feature-specs/14-backend-auth-identity-foundation.md` | regular file | `967a58fb5ac72e3e39b5d6ae7d974219abf9a4cc44f4685b73c1519bbbda695c` | `967a58fb5ac72e3e39b5d6ae7d974219abf9a4cc44f4685b73c1519bbbda695c` | match |
| `feature-specs/15-backend-csrf-session-api.md` | regular file | `0f50cd365b8fb851c60dc5dc3fb46af26233e7807f26468160cba5f93a105588` | `0f50cd365b8fb851c60dc5dc3fb46af26233e7807f26468160cba5f93a105588` | match |
| `feature-specs/16-frontend-auth-shell-session-provider.md` | regular file | `af44327471dddd075bfe1061e8fffa0fc0ebb8ff506bf535c532cc00b0ad4545` | `af44327471dddd075bfe1061e8fffa0fc0ebb8ff506bf535c532cc00b0ad4545` | match |
| `feature-specs/17-first-admin-bootstrap.md` | regular file | `3d24e77305cc276773163fc64871c83b8852174f36e81181b9d51f97aa393357` | `3d24e77305cc276773163fc64871c83b8852174f36e81181b9d51f97aa393357` | match |
| `feature-specs/18-backend-login-required-password-change.md` | regular file | `8eb84198b475655ea9a79ed126d62a5f1e364dd9ba02fc8bc977f7ebecc3212f` | `8eb84198b475655ea9a79ed126d62a5f1e364dd9ba02fc8bc977f7ebecc3212f` | match |
| `feature-specs/19-frontend-sign-in-password-change-wiring.md` | regular file | `3f82fbf9b4eb7c16545ec6dd2f3cea50f0ea0a8a694e128b683788221212515a` | `3f82fbf9b4eb7c16545ec6dd2f3cea50f0ea0a8a694e128b683788221212515a` | match |
| `feature-specs/20-backend-staff-roles-authorization-foundation.md` | regular file | `368df689cba3d0abd140e4a9ae1cdc5f1a072fdd026fc05a1759e1d3bbc0d8c9` | `368df689cba3d0abd140e4a9ae1cdc5f1a072fdd026fc05a1759e1d3bbc0d8c9` | match |
| `feature-specs/21-seasons-competitions-backend.md` | regular file | `25dffccdbfbcd8e10f705520984c9660bf6798b45e1e5b5b861a7123ab56c906` | `25dffccdbfbcd8e10f705520984c9660bf6798b45e1e5b5b861a7123ab56c906` | match |
| `feature-specs/22-teams-selections-backend.md` | regular file | `249a56d4a4e47247f19c64e8f5b1b40e90e736c86e18d74e2f070940afb0d815` | `249a56d4a4e47247f19c64e8f5b1b40e90e736c86e18d74e2f070940afb0d815` | match |
| `feature-specs/23-staff-users-team-scope-account-lifecycle-backend.md` | regular file | `7fb03a103a368a1054b0bfa95c166c53ac17d97f349e528384609ac095a4b6d1` | `7fb03a103a368a1054b0bfa95c166c53ac17d97f349e528384609ac095a4b6d1` | match |
| `feature-specs/24-staff-users-roles-ui.md` | regular file | `5ca57737b6316eaa896f8fcdd42ca6735a5f2f8f35eb2b9658774e776e0d734a` | `5ca57737b6316eaa896f8fcdd42ca6735a5f2f8f35eb2b9658774e776e0d734a` | match |
| `feature-specs/25-venues-opponents-backend.md` | regular file | `d72ca0bb82c14c32efc135b72698d1d5ed4159c2d8fdddee7298e60c376b084d` | `d72ca0bb82c14c32efc135b72698d1d5ed4159c2d8fdddee7298e60c376b084d` | match |
| `feature-specs/26-settings-ui-foundation.md` | regular file | `4aadba39ee0d008960246f57801c3579b99748b42fe531c07c6459308271bfff` | `4aadba39ee0d008960246f57801c3579b99748b42fe531c07c6459308271bfff` | match |
| `feature-specs/27-players-backend-foundation.md` | regular file | `613187606e6aa0e8895ae7f01bc4f7471bd619955debd697b3eac40a152a58e3` | `613187606e6aa0e8895ae7f01bc4f7471bd619955debd697b3eac40a152a58e3` | match |
| `feature-specs/28-player-team-assignment-backend.md` | regular file | `5c4197464751135a588ef9f3fd6e27c44293d50515dba276c12045e569340b29` | `5c4197464751135a588ef9f3fd6e27c44293d50515dba276c12045e569340b29` | match |
| `feature-specs/29-players-ui-foundation.md` | regular file | `1fdc268a414452ecb021baf4886d46e02c710c6c23f87161a5c3673decdc0d4c` | `1fdc268a414452ecb021baf4886d46e02c710c6c23f87161a5c3673decdc0d4c` | match |
| `feature-specs/30-matches-backend-foundation.md` | regular file | `cab6735d12145f1ccddf384682349f25e3e4897041b03b14c100d8c886814688` | `cab6735d12145f1ccddf384682349f25e3e4897041b03b14c100d8c886814688` | match |
| `feature-specs/31-match-lineup-appearance-backend.md` | regular file | `d39ca454b7dc39d9811d5027e2c719da012f7464152c79a159d162cf2a74bb13` | `d39ca454b7dc39d9811d5027e2c719da012f7464152c79a159d162cf2a74bb13` | match |
| `feature-specs/32-match-report-workflow-backend.md` | regular file | `5cbc956c66dfa0d633de42845261c5aa53b47bbc99947a228dc507aaa0bff21a` | `5cbc956c66dfa0d633de42845261c5aa53b47bbc99947a228dc507aaa0bff21a` | match |
| `feature-specs/33-manual-match-statistics-backend.md` | regular file | `eaf109b5d4f4109af1e69d0295d28463ae7e3d1ce8dcf641d8e858b24a71bb8b` | `eaf109b5d4f4109af1e69d0295d28463ae7e3d1ce8dcf641d8e858b24a71bb8b` | match |
| `feature-specs/34-matches-ui-foundation.md` | regular file | `289d61d39930ac6b1744fe70d695a39ce25123e77ec0928589617135c5da1b83` | `289d61d39930ac6b1744fe70d695a39ce25123e77ec0928589617135c5da1b83` | match |
| `feature-specs/35-lineup-appearances-ui.md` | regular file | `b84a2d50a904c7d095c2a344614c207709fcbb082ade6cfa9513225d1636353b` | `b84a2d50a904c7d095c2a344614c207709fcbb082ade6cfa9513225d1636353b` | match |
| `feature-specs/36-manual-match-statistics-ui.md` | regular file | `20f8d092912e7163a6f519a9d41e45032b202aee6fbf59f1473e191a86b77a64` | `20f8d092912e7163a6f519a9d41e45032b202aee6fbf59f1473e191a86b77a64` | match |
| `feature-specs/37-match-report-review-ui.md` | regular file | `153c5f8cf4de16c4699b7154cfe25b66c2b3f76e030d68ede4d71af034ef9f59` | `153c5f8cf4de16c4699b7154cfe25b66c2b3f76e030d68ede4d71af034ef9f59` | match |
| `feature-specs/38-audit-backend-foundation.md` | regular file | `aea335ffd7ce0bb17690e2fed023339213a4759ff669013bab53037fc5bd8a84` | `aea335ffd7ce0bb17690e2fed023339213a4759ff669013bab53037fc5bd8a84` | match |
| `feature-specs/39-audit-ui-foundation.md` | regular file | `7b28092fd5c67b8722b57c7fd1e9afcac005245e21f4a9960afe31307a149369` | `7b28092fd5c67b8722b57c7fd1e9afcac005245e21f4a9960afe31307a149369` | match |
| `feature-specs/40-file-storage-abstraction.md` | regular file | `d30183c4efc3d5508516af7e5d882b247f0149560bf185adb7c3aa750bb79c90` | `d30183c4efc3d5508516af7e5d882b247f0149560bf185adb7c3aa750bb79c90` | match |
| `feature-specs/41-media-backend-foundation.md` | regular file | `8ce388e67090cc4eb23e3689f3d8bab3f896a7c47d1ce48d007bee4e1e7d79a4` | `8ce388e67090cc4eb23e3689f3d8bab3f896a7c47d1ce48d007bee4e1e7d79a4` | match |
| `feature-specs/42-media-ui-foundation.md` | regular file | `0a236878af0e8936c19132dd991445f6dc3f4095a8639f56987212c7e81079bd` | `0a236878af0e8936c19132dd991445f6dc3f4095a8639f56987212c7e81079bd` | match |
| `feature-specs/43-import-workflow-backend-foundation.md` | regular file | `e91bc401ce630e102f70de5df1da02e4a2d54f57aacc3de0ae95138e949350ac` | `e91bc401ce630e102f70de5df1da02e4a2d54f57aacc3de0ae95138e949350ac` | match |
| `feature-specs/44-import-ui-foundation.md` | regular file | `0c793f35231e46988a5d10a7938a6ee18e7ef120c513f5cce73d8cd505a66753` | `0c793f35231e46988a5d10a7938a6ee18e7ef120c513f5cce73d8cd505a66753` | match |
| `feature-specs/45-generic-csv-xlsx-parsing-foundation.md` | regular file | `5271d704455b7ca27d01a696b81a4fe7e6acc7203eb6e086ca707ad1ca159c86` | `5271d704455b7ca27d01a696b81a4fe7e6acc7203eb6e086ca707ad1ca159c86` | match |
| `feature-specs/46-gpexe-sample-review-mapping-gate.md` | regular file | `ceff23b6775b7b95483e6095b5677406f50ecbbc18d7517153dfd43b4beb223d` | `ceff23b6775b7b95483e6095b5677406f50ecbbc18d7517153dfd43b4beb223d` | match |
| `feature-specs/48-training-gps-backend-foundation.md` | regular file | `f9a21a36e6eaf7abeed98a1d005d3263e98a460024768e9a3be13fc35140d240` | `f9a21a36e6eaf7abeed98a1d005d3263e98a460024768e9a3be13fc35140d240` | match |
| `feature-specs/49-training-gps-ui-foundation.md` | regular file | `51f97b53cab3a9847b9627353b8a102195f63ea682fa950d94a3b0172284252d` | `51f97b53cab3a9847b9627353b8a102195f63ea682fa950d94a3b0172284252d` | match |
| `feature-specs/50-medical-availability-backend.md` | regular file | `c6b77d1850fc03f495aed90a0bb7058793baea0c0a788cfe8469a458e9497e93` | `c6b77d1850fc03f495aed90a0bb7058793baea0c0a788cfe8469a458e9497e93` | match |
| `feature-specs/51-medical-availability-ui.md` | regular file | `5fa8c6fb688a7b551a22a1d97a5492079c5ca7b0e9cb0476911a317252466d61` | `5fa8c6fb688a7b551a22a1d97a5492079c5ca7b0e9cb0476911a317252466d61` | match |
| `feature-specs/52-dashboard-backend-read-models.md` | regular file | `8a39edb976f99725a63970b74027c70b80bf7cf433fc71c47e81ce248a8463e0` | `8a39edb976f99725a63970b74027c70b80bf7cf433fc71c47e81ce248a8463e0` | match |
| `feature-specs/53-dashboard-ui.md` | regular file | `d1602be0df9f600811be948eb1cc8ea0b535835985821bbb954bce67519ea654` | `d1602be0df9f600811be948eb1cc8ea0b535835985821bbb954bce67519ea654` | match |
| `feature-specs/54-localization-foundation.md` | regular file | `26f05329c9a5e51746de0dc7f79ef03f31f4214850accc2120ddac0b77fa16f2` | `26f05329c9a5e51746de0dc7f79ef03f31f4214850accc2120ddac0b77fa16f2` | match |
| `feature-specs/56-responsive-accessibility-hardening.md` | regular file | `057e134b09f308b4a88f5f88ee53d31786cfd966d6e8cf7eb604a8bbd3142215` | `057e134b09f308b4a88f5f88ee53d31786cfd966d6e8cf7eb604a8bbd3142215` | match |
| `feature-specs/57-production-configuration-readiness.md` | regular file | `46a247d2ce5cf05ca9596ada79c66750da6c2021ffbca5d272ffafa07e6629f9` | `46a247d2ce5cf05ca9596ada79c66750da6c2021ffbca5d272ffafa07e6629f9` | match |
| `operations/backup-restore-runbook.md` | regular file | `f060099fe66f1646cb743c241768f2bdfc90e06f62bb1102f319b83ae3b20d41` | `f060099fe66f1646cb743c241768f2bdfc90e06f62bb1102f319b83ae3b20d41` | match |
| `operations/deployment-runbook.md` | regular file | `f50c9443f2865b6832dce39428a5c60c4564cdd9f82a33210f08123c186715f4` | `f50c9443f2865b6832dce39428a5c60c4564cdd9f82a33210f08123c186715f4` | match |
| `operations/object-storage-decision.md` | regular file | `879f086c739630d8f7d30d7292203269bd98b5d873d00e8bf94157bfbe5621bd` | `879f086c739630d8f7d30d7292203269bd98b5d873d00e8bf94157bfbe5621bd` | match |
| `operations/production-configuration.md` | regular file | `2606caff3325e8d5e1110668a7595ea3a8b8dcde917084f6dc4aa983e0c30a73` | `2606caff3325e8d5e1110668a7595ea3a8b8dcde917084f6dc4aa983e0c30a73` | match |
| `operations/production-readiness-checklist.md` | regular file | `62cacef23dc2454371ac6f8d92f523ebd2bc5228b45eab87edc3211223cd388e` | `62cacef23dc2454371ac6f8d92f523ebd2bc5228b45eab87edc3211223cd388e` | match |
| `progress-tracker.md` | regular file | `08193ce16a087c470c0326ef7985c90dd29c9811a32cad68aea5a9edf063f1eb` | `08193ce16a087c470c0326ef7985c90dd29c9811a32cad68aea5a9edf063f1eb` | match |
| `project-overview.md` | regular file | `2e4986151064c56dfa27477cda318b52dce39081002c20625b2da28f22a8a382` | `2e4986151064c56dfa27477cda318b52dce39081002c20625b2da28f22a8a382` | match |
| `prompt-official-agent-skills-migration-and-review.md` | regular file | `f6d4aa13cd20abc78fa307c7b278f36222eecbc51a1b0ed62a3124b95c558e0e` | `f6d4aa13cd20abc78fa307c7b278f36222eecbc51a1b0ed62a3124b95c558e0e` | match |
| `references/gpexe-export-review-template.md` | regular file | `3bd62e1652a703623b77490e99937d4dbefd5aca4f02ab5d148f25c6eab5e943` | `3bd62e1652a703623b77490e99937d4dbefd5aca4f02ab5d148f25c6eab5e943` | match |
| `references/official-agent-skills-review.md` | regular file | `ab7322cb2a928f25fef194dd8eae07a4e132ad2f940684ee552d3e69bc8541d9` | `ab7322cb2a928f25fef194dd8eae07a4e132ad2f940684ee552d3e69bc8541d9` | match |
| `references/official-agent-skills.md` | regular file | `c2ba191aec2d1f0f7effc3a5303525e8ff37efee4e27e9eb17da479cd7353de4` | `c2ba191aec2d1f0f7effc3a5303525e8ff37efee4e27e9eb17da479cd7353de4` | match |
| `references/responsive-accessibility-audit.md` | regular file | `45867428482a5a47e342deb90bc5b11be499902a7b8e80c1d8df2357fd6c007e` | `45867428482a5a47e342deb90bc5b11be499902a7b8e80c1d8df2357fd6c007e` | match |
| `references/shadcn-components.md` | regular file | `9c04831547dbc8472fa7706be7633e18a4c349ef5ac7df959d937c77b8d70eb5` | `9c04831547dbc8472fa7706be7633e18a4c349ef5ac7df959d937c77b8d70eb5` | match |
| `ui-context.md` | regular file | `5b81db1e4bac11b45eb27a622835397d26fd13bfefad551411218c19c6cfd555` | `5b81db1e4bac11b45eb27a622835397d26fd13bfefad551411218c19c6cfd555` | match |

Source count: 73.

Destination count: 73.

Type or content mismatches: 0.

Root `context/` remaining after the move: no.

## Load bearing rule disposition

| Legacy rule area | Current owner and exact path | Disposition | Unresolved issue |
|---|---|---|---|
| Documentation authority and reading order | `AGENTS.md` and `docs/specs/0001-project-context-authority.md` | replaced | none |
| Repository boundaries and shared workflow | `AGENTS.md` | retained in current form | none |
| Backend structure, commands, persistence, security, and testing conventions | `backend/AGENTS.md` | retained in current form | none |
| Frontend structure, commands, interface conventions, and testing conventions | `frontend/AGENTS.md` | retained in current form | none |
| Product boundary, priority, and feature status | `docs/scope/index.md` and its linked scope area files | replaced | none |
| Project build approach | `AGENTS.md` and `docs/scope/index.md` | replaced with Tracer Bullet | none |
| Visual and interaction direction | `DESIGN.md` | replaced | none |
| Durable architecture and product decisions | Accepted files in `docs/specs/` | replaced | none |
| Old feature specifications and progress evidence | `docs/legacy/context/feature-specs/` and `docs/legacy/context/progress-tracker.md` | retired to historical evidence | none |
| Old operations guidance and open provider choices | `docs/scope/deferred-and-excluded.md`, with old records under `docs/legacy/context/operations/` | retired as active guidance | none |
| GPeXe, GPS, Zone14, training, and video direction | `docs/scope/deferred-and-excluded.md` | replaced with dropped scope | none |
| Skills guidance and provenance history | `AGENTS.md` for current workflow, old records under `docs/legacy/context/references/` | replaced, history retained | none |
| General progress tracker update rule | `docs/scope/` for feature state and `docs/specs/` for decisions | retired | none |

Every load bearing row has a current owner or an explicit retirement. There are no unresolved disposition rows.

## Reviewed retained matches

Tracked text files were identified with Git and scanned with `git grep -I`, excluding `docs/legacy/`. Current untracked documentation was scanned with `rg -I` before staging. Binary files were excluded by those text filters.

The old literal `context/` path remains only in `docs/specs/0001-project-context-authority.md` at lines 8, 12, 16, 22, 32, 37, 46, 69, 77, 81, 131 to 133, 138, 140, 142, 144, 151, 156, 159, and 161. Every occurrence describes history, the migration source, or the required review rule. None is an active instruction to read from the retired location.

The scan also matched `frontend/package-lock.json` line 1716 because the registry URL contains the package name `react-context`. This is not a repository path or documentation instruction.

Generic references to "context files" in vendored workflow skills refer to current `AGENTS.md` style repository context. They do not name the retired root path and do not instruct readers to use archived files.

No active reading instruction, current link, source file, script, configuration file, template, or structured data file points to the retired root `context/` tree.

## Rollout checks

| Check | Result |
|---|---|
| Accepted decision present at source revision | passed, spec 0001 is `Accepted` in `cfeab02514a2f16b3a245f18c55c34e8552be38a` |
| Destination collision | passed, `docs/legacy/context/` was absent before the move |
| Source inventory | passed, 73 tracked and unchanged regular files, with no untracked entries |
| Source and destination preservation | passed, 73 manifest rows and zero type or SHA-256 mismatches |
| Root source directory removal | passed, no root `context/` directory remains |
| Exact Git rename review | passed, all 73 archived entries are `R100` renames |
| Load bearing rule disposition | passed, no unresolved row remains |
| Current old path scan | passed, 22 matches were reviewed, 21 are migration wording in spec 0001 and one is the `react-context` package URL false positive |
| Active retired reading instruction scan | passed, three matches are historical replacement wording in spec 0001 and none is an active instruction |
| Current scope legacy pointers | passed, all retained pointers use visibly labelled `docs/legacy/context/` destinations |
| Current Markdown local links and heading anchors | passed, 246 tracked Markdown files outside `docs/legacy/` checked with zero errors |
| Existing broken current links found by the check | repaired three generated React guidance index links to their existing `rules/` targets |
| Repository documentation quality command | none is defined, so the specification checks and `git diff --check` are the applicable documentation boundary checks |
| Whitespace check | passed, `git diff --cached --check` returned no error |
| Complete staged diff review | passed, 85 entries consist of 73 exact archive renames and 12 current documentation updates |
| Application and dependency changes | passed, no application source, test, migration, package manifest, lockfile, or runtime configuration changed |
| Isolated worktree state before commit | passed, no unstaged or untracked files remain |

The rollout is documentation only. Application build and test commands do not apply to this boundary and were not run.
