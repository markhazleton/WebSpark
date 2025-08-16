# WebSpark Solution Documentation Index

This directory contains all technical documentation for the WebSpark solution. All new documentation should be created here following the naming conventions outlined in the copilot instructions.

## Documentation Organization

### Current Documentation Files

- **DATASPARK_README.md** - Main DataSpark.Web project overview and setup instructions
- **DATASPARK_IMPLEMENTATION_SUMMARY.md** - Implementation details and architecture summary for DataSpark.Web
- **DATASPARK_CHART_DEBUGGING_GUIDE.md** - Debugging guide for chart functionality issues
- **DATASPARK_CHART_ENHANCEMENT_SUMMARY.md** - Summary of chart feature enhancements
- **DATASPARK_CSV_AI_ENHANCED_FEATURES.md** - AI-enhanced CSV processing features documentation
- **DATASPARK_CSV_AI_UPDATED_TO_EXISTING_FILES.md** - Updates made to existing CSV processing files
- **DATASPARK_CSV_FILE_LOADING_FIX.md** - Fix documentation for CSV file loading issues
- **WEBSPARK_SHAREDKERNEL_README.md** - WebSpark.SharedKernel project documentation

### Documentation Naming Convention

Use descriptive names that indicate the project/area and purpose:

- `{PROJECT}_{FEATURE}_{TYPE}.md` (e.g., `DATASPARK_CHART_DEBUGGING_GUIDE.md`)
- `{AREA}_{PURPOSE}.md` (e.g., `API_INTEGRATION_NOTES.md`)
- `{FEATURE}_IMPLEMENTATION_GUIDE.md` (e.g., `AUTHENTICATION_IMPLEMENTATION_GUIDE.md`)

### Special Files

- **Root `/README.md`** - GitHub repository description (kept in root)
- **`.github/copilot-instructions.md`** - GitHub Copilot instructions (kept in .github)

### Content vs Documentation

Note: Markdown files in `wwwroot/markdown/` directories are content files for the web applications, not documentation. These should remain in their respective project locations as they are served as part of the web applications.

## Best Practices

1. **Always create new documentation in `/copilot`** - Do not create documentation files in individual project directories
2. **Use descriptive filenames** - Include project name and purpose in the filename
3. **Cross-reference properly** - Use relative paths like `../copilot/GUIDE_NAME.md` when referencing from code
4. **Keep documentation current** - Update documentation when making significant changes to code
5. **Follow markdown best practices** - Use proper headers, code blocks, and formatting

## Quick Reference

When working with the WebSpark solution:

- For general solution questions: See `/README.md`
- For GitHub Copilot guidance: See `/.github/copilot-instructions.md`
- For project-specific documentation: See files in `/copilot/` with project prefix
- For debugging issues: Look for `*_DEBUGGING_GUIDE.md` files
- For implementation details: Look for `*_IMPLEMENTATION_*.md` files
