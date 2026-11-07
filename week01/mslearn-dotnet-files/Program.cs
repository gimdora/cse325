using System.Globalization;
using System.Text;
using Newtonsoft.Json;

// Always format currency as US dollars (on Korean Windows, :C would show ₩)
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

var currentDirectory = Directory.GetCurrentDirectory();
var storesDirectory = Path.Combine(currentDirectory, "stores");

var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
Directory.CreateDirectory(salesTotalDir);

var salesFiles = FindFiles(storesDirectory);

var salesTotal = CalculateSalesTotal(salesFiles);

File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesTotal}{Environment.NewLine}");

// W01 assignment: generate the sales summary report
GenerateSalesSummaryReport(salesFiles, storesDirectory, Path.Combine(salesTotalDir, "salesSummary.txt"));

IEnumerable<string> FindFiles(string folderName)
{
    List<string> salesFiles = new List<string>();

    var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);

    foreach (var file in foundFiles)
    {
        var extension = Path.GetExtension(file);
        if (extension == ".json")
        {
            salesFiles.Add(file);
        }
    }

    return salesFiles;
}

double CalculateSalesTotal(IEnumerable<string> salesFiles)
{
    double salesTotal = 0;

    // Loop over each file path in salesFiles
    foreach (var file in salesFiles)
    {
        // Read the contents of the file
        string salesJson = File.ReadAllText(file);

        // Parse the contents as JSON
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

        // Add the amount found in the Total field to the salesTotal variable
        salesTotal += data?.Total ?? 0;
    }

    return salesTotal;
}

// Builds a text report with the overall sales total and each sales file's total
void GenerateSalesSummaryReport(IEnumerable<string> salesFiles, string baseDirectory, string reportPath)
{
    var details = new StringBuilder();
    double total = 0;

    foreach (var file in salesFiles)
    {
        // Only sales.json files hold a store's daily "Total"
        // (salestotals.json uses "OverallTotal", so skip it)
        if (Path.GetFileName(file) != "sales.json")
        {
            continue;
        }

        string salesJson = File.ReadAllText(file);
        SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);
        double fileTotal = data?.Total ?? 0;
        total += fileTotal;

        // Every file is named sales.json, so show the path relative to stores (e.g. 201\sales.json)
        details.AppendLine($"  {Path.GetRelativePath(baseDirectory, file)}: {fileTotal:C}");
    }

    var report = new StringBuilder();
    report.AppendLine("Sales Summary");
    report.AppendLine("----------------------------");
    report.AppendLine($" Total Sales: {total:C}");
    report.AppendLine();
    report.AppendLine(" Details:");
    report.Append(details);

    File.WriteAllText(reportPath, report.ToString());
}

record SalesData(double Total);
