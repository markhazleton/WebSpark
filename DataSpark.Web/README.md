# DataSpark.Web

DataSpark.Web is a modern ASP.NET Core 9.0 web application for interactive data exploration, CSV file management, and automated machine learning (AutoML) modeling. It features a beautiful Bootswatch-powered UI, robust file upload and EDA (exploratory data analysis), and a RESTful API for data science workflows.

## Features

- **CSV File Upload**: Upload CSV files via a user-friendly web interface. Files are securely stored in the `wwwroot/files` directory.
- **Exploratory Data Analysis (EDA)**: Instantly view summary statistics, column data types, unique values, null counts, and more for each uploaded CSV file.
- **AutoML Modeling**: Select a file and target column, then train a regression or classification model using Microsoft ML.NET. Model metrics (e.g., R², accuracy, log loss) are displayed interactively.
- **RESTful API**: Endpoints for listing files, EDA, column selection, and model training. Designed for integration with front-end apps or automation.
- **Bootswatch Theme Switcher**: Instantly switch between Bootswatch themes using a built-in dropdown in the navbar. Theme preference is persisted per user session.
- **Responsive UI**: Built with Bootstrap 5 and Bootswatch for a modern, mobile-friendly experience.
- **Error Handling**: Friendly error messages and robust validation for file uploads, malformed CSVs, and modeling edge cases.

## Quick Start

1. **Clone the repository**

   ```pwsh
   git clone <your-fork-url>
   cd DataSpark.Web
   ```

2. **Restore dependencies**

   ```pwsh
   dotnet restore
   ```

3. **Run the application**

   ```pwsh
   dotnet run
   ```

4. **Open in your browser**
   Visit [https://localhost:7104](https://localhost:7104) (or the port shown in your terminal).

## Project Structure

- `Controllers/`
  - `HomeController.cs`: Handles web UI, file upload, and navigation.
  - `api/FilesController.cs`: REST API for file listing, EDA, and model training.
- `Views/`
  - `Home/Index.cshtml`: File upload page.
  - `Home/Files.cshtml`: EDA and modeling UI.
  - `Shared/_Layout.cshtml`: Main layout with Bootswatch theme switcher.
- `wwwroot/`
  - `files/`: Uploaded CSV files.
  - `lib/`: Front-end libraries (Bootstrap, jQuery, etc.).
- `Models/ErrorViewModel.cs`: Error model for error pages.
- `Program.cs`: ASP.NET Core app setup, service registration, and middleware.

## API Endpoints

- `GET /api/files/list` — List all CSV files with EDA summary.
- `GET /api/files/analyze?fileName=...` — Get column names for a CSV file.
- `POST /api/files/train` — Train a model on a file and target column. Returns metrics.

## Bootswatch Theme Switcher

- Change the UI theme using the dropdown in the navbar. Powered by [WebSpark.Bootswatch](https://www.nuget.org/packages/WebSpark.Bootswatch).

## Dependencies

- [.NET 9.0](https://dotnet.microsoft.com/)
- [CsvHelper](https://joshclose.github.io/CsvHelper/)
- [Microsoft.ML](https://docs.microsoft.com/en-us/dotnet/machine-learning/)
- [WebSpark.Bootswatch](https://www.nuget.org/packages/WebSpark.Bootswatch)
- [WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility)

## Development

- All C# code uses nullable reference types and modern ASP.NET Core best practices.
- Front-end uses Bootstrap 5, Bootswatch, and unobtrusive validation.
- To add new features, create a new controller or view as needed and follow the existing patterns.

## Contributing

Contributions are welcome! Please open issues or pull requests for bug fixes, new features, or documentation improvements.

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## Visualization & Pivot Table Features

### 1. File Upload & EDA (Exploratory Data Analysis)

- Upload CSV files on the **Home** page (`/`), which are stored in `wwwroot/files`.
- View uploaded files and their EDA (head, tail, summary stats, missing values, correlation matrix) on the **Files** page (`/Home/Files`).

**Example:**

```pwsh
# Upload a file via the web UI
# Or use the API to list files with EDA
Invoke-RestMethod https://localhost:7104/api/files/list
```

### 2. Data Visualization

- Go to **Visualize** (`/Visualization/Index`) to select a CSV file and visualize any column as a histogram (numeric) or bar chart (categorical).
- Choose the column and chart type (auto, histogram, bar chart) and render interactive charts using Chart.js.

**Example:**

```js
// Fetch column data for visualization
fetch('/Visualization/Data?fileName=Sample.csv')
  .then(r => r.json())
  .then(data => console.log(data.headers, data.columns));
```

### 3. Bivariate Analysis

- Go to **Bivariate Analysis** (`/Visualization/Bivariate`) to select two columns from a CSV file and run bivariate analysis.
- Supports numeric-numeric (scatter, correlation, regression), categorical-categorical (contingency table), and numeric-categorical (group stats/boxplot) analysis.

**Example:**

```js
// Run bivariate analysis via API
fetch('/api/files/bivariate', {
  method: 'POST',
  body: new URLSearchParams({
    fileName: 'Sample.csv',
    column1: 'Age',
    column2: 'Salary'
  })
}).then(r => r.json()).then(console.log);
```

### 4. Pivot Table

- Go to **Pivot Table** (`/Visualization/Pivot`) to interactively explore CSV data using [PivotTable.js](https://pivottable.js.org/).
- Select a file, load the data, and use drag-and-drop to build pivot tables and charts (table, heatmap, bar chart, etc.).
- Export results and visualizations directly from the UI.

**Example:**

```js
// The UI loads CSV and renders with PivotTable.js
// Example: fetch CSV file contents
fetch('/files/Sample.csv').then(r => r.text()).then(csv => {
  // Use PapaParse to parse, then pass to PivotTable.js
});
```

---

## API Usage Examples

### List Files with EDA

```pwsh
Invoke-RestMethod https://localhost:7104/api/files/list
```

### Get Column Names for a File

```pwsh
Invoke-RestMethod "https://localhost:7104/api/files/analyze?fileName=Sample.csv"
```

### Train a Model (Regression or Classification)

```pwsh
Invoke-RestMethod https://localhost:7104/api/files/train -Method Post -Body @{fileName='Sample.csv'; targetColumn='Target'}
```

### Get EDA for a File

```pwsh
Invoke-RestMethod "https://localhost:7104/api/files/eda?fileName=Sample.csv"
```

### Run Bivariate Analysis

```pwsh
Invoke-RestMethod https://localhost:7104/api/files/bivariate -Method Post -Body @{fileName='Sample.csv'; column1='Age'; column2='Salary'}
```

---

## UI Navigation

- **Home**: Upload CSV files and see sample records.
- **Files**: Browse uploaded files and view EDA.
- **Visualize**: Render charts for any column in a file.
- **Bivariate Analysis**: Analyze relationships between two columns.
- **Pivot Table**: Drag-and-drop pivot table and chart builder.
- **Theme Switcher**: Instantly change the UI theme from the navbar.

---

*Documentation updated May 21, 2025. For more, see the code or open an issue!*
