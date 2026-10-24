# Unity Editor automation

- Target this project's Editor explicitly with `--project-path E:\Unity\Monster`.
- The installed Pipeline package is `0.8.0-exp.1`. Its MCP `eval` and `eval_file` argument `timeout` is in **milliseconds**: use `15000` for 15 seconds, never `15`. For short queries, omitting it uses the package's 5000 ms default.
- The CLI-level `unity command --timeout` option is in seconds; do not copy that value directly into an MCP tool's `timeout` argument. Check the live tool metadata after changing package versions.
- Use the command names exposed by the connected Editor. Pipeline 0.8 uses names such as `editor_status`, rather than the grouped `editor status` naming introduced in 0.9.
- Fix timeout values at the caller. Do not change generated files under `Library/PackageCache` to compensate for incorrect units.
