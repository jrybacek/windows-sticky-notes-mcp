# Releasing

Maintainer runbook for cutting a new StickyNotesMcp release. This publishes self-contained `win-x64` and `win-arm64` builds and attaches them to a GitHub Release.

## Steps

1. **Bump the version** in [src/StickyNotesMcp.csproj](../src/StickyNotesMcp.csproj) (`<Version>`).
2. Commit the bump:

   ```powershell
   git add src\StickyNotesMcp.csproj
   git commit -m "🔖 chore: bump version to 0.2.0"
   git push
   ```

3. **Tag and push the tag** — this is what triggers the release workflow:

   ```powershell
   git tag v0.2.0
   git push origin v0.2.0
   ```

   The tag must match `v*.*.*` (e.g. `v0.2.0`) — [.github/workflows/release.yml](../.github/workflows/release.yml) only fires on tags matching that pattern (plus manual `workflow_dispatch` runs, which build and smoke-test but don't publish a release since they have no version tag).

4. **Watch the workflow** in the Actions tab. It:
   - builds and tests once,
   - publishes self-contained single-file exes for `win-x64` and `win-arm64`,
   - runs a smoke test against the `win-x64` build (starts it, sends an MCP `initialize` request over stdio, confirms a response with `serverInfo` comes back) — this is what actually proves the published single-file exe works, not just that it compiles,
   - hashes each asset (SHA-256),
   - creates a GitHub Release with all four files attached.

5. **Confirm the release** has:

   ```
   StickyNotesMcp-win-x64.exe
   StickyNotesMcp-win-x64.exe.sha256
   StickyNotesMcp-win-arm64.exe
   StickyNotesMcp-win-arm64.exe.sha256
   ```

6. Sanity-check the README's download instructions still match — filenames in particular, since the workflow file names the assets, not the README.

## Notes

- `gh` (the GitHub CLI) isn't required for any of this — plain `git tag`/`git push` is enough. If you do have `gh` installed, `gh release view v0.2.0` is a quick way to double check the assets after the workflow finishes.
- Release assets are **not code-signed**. Windows SmartScreen will warn on first run for anyone who downloads them; there's no way around that without a paid code-signing certificate, which is out of scope for now. The README documents the `Unblock-File` + SHA-256 verification workaround.
- If the smoke test fails, the release job is skipped entirely (it depends on `build` succeeding) — no partial/broken release gets published.
