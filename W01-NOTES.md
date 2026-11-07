# W01 Notes – Build .NET Applications with C#

**Student:** Heewoong Kim (GitHub: gimdora)
**SDK:** .NET 10 (current LTS)

| Project | Location |
|---|---|
| Work with files and directories (sales summary report) | `week01/mslearn-dotnet-files` |
| Create a web API with ASP.NET Core controllers (ContosoPizza) | `week01/ContosoPizza` |

---

## 1. Existing pizzas + my additional pizza

I added a third pizza, **Bulgogi** (Id 3), to the seed data in `week01/ContosoPizza/Services/PizzaService.cs` and changed `nextId` from 3 to 4 so new pizzas don't reuse that Id.

```csharp
Pizzas = new List<Pizza>
{
    new Pizza { Id = 1, Name = "Classic Italian", IsGlutenFree = false },
    new Pizza { Id = 2, Name = "Veggie", IsGlutenFree = true },
    new Pizza { Id = 3, Name = "Bulgogi", IsGlutenFree = false } // additional pizza
};
```

**Request**

```http
GET http://localhost:5051/pizza
Accept: application/json
```

**Response – `200 OK`**

```json
[
  { "id": 1, "name": "Classic Italian", "isGlutenFree": false },
  { "id": 2, "name": "Veggie", "isGlutenFree": true },
  { "id": 3, "name": "Bulgogi", "isGlutenFree": false }
]
```

---

## 2. CRUD tests (GET, POST, PUT, DELETE)

All requests were sent with the VS Code REST Client extension using `week01/ContosoPizza/ContosoPizza.http`.

### GET – read one pizza by Id

**Request**

```http
GET http://localhost:5051/pizza/4
Accept: application/json
```

**Response – `200 OK`**

```json
{ "id": 4, "name": "Hawaiian", "isGlutenFree": false }
```

### POST – create a pizza

**Request**

```http
POST http://localhost:5051/pizza
Content-Type: application/json

{
    "name": "Hawaiian",
    "isGlutenFree": false
}
```

**Response – `201 Created`**

```http
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Location: http://localhost:5051/Pizza/4
```

```json
{ "id": 4, "name": "Hawaiian", "isGlutenFree": false }
```

### PUT – update a pizza

**Request**

```http
PUT http://localhost:5051/pizza/4
Content-Type: application/json

{
    "id": 4,
    "name": "Hawaiian Deluxe",
    "isGlutenFree": false
}
```

**Response – `204 No Content`** (no body)

Confirmed with `GET http://localhost:5051/pizza/4` → `200 OK`:

```json
{ "id": 4, "name": "Hawaiian Deluxe", "isGlutenFree": false }
```

### DELETE – delete a pizza

**Request**

```http
DELETE http://localhost:5051/pizza/4
```

**Response – `204 No Content`** (no body)

Confirmed with `GET http://localhost:5051/pizza` → `200 OK` (pizza 4 is gone):

```json
[
  { "id": 1, "name": "Classic Italian", "isGlutenFree": false },
  { "id": 2, "name": "Veggie", "isGlutenFree": true },
  { "id": 3, "name": "Bulgogi", "isGlutenFree": false }
]
```

### Summary

| Operation | Request | Status code |
|---|---|---|
| GET | `GET /pizza` and `GET /pizza/4` | 200 OK |
| POST | `POST /pizza` | 201 Created |
| PUT | `PUT /pizza/4` | 204 No Content |
| DELETE | `DELETE /pizza/4` | 204 No Content |

---

## 3. Sales summary function

Added to `week01/mslearn-dotnet-files/Program.cs`. It is called after `totals.txt` is written:

```csharp
GenerateSalesSummaryReport(salesFiles, storesDirectory, Path.Combine(salesTotalDir, "salesSummary.txt"));
```

Function:

```csharp
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
```

The program also sets `CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");` at the top so `:C` always formats as US dollars (my computer uses a Korean locale, which would otherwise show ₩).

**Output – `salesTotalDir/salesSummary.txt`**

```plaintext
Sales Summary
----------------------------
 Total Sales: $2,012.20

 Details:
  sales.json: $88.88
  201\sales.json: $501.22
  202\sales.json: $1,234.22
  203\sales.json: $99.00
  204\sales.json: $88.88
```

The total matches the module's `totals.txt` result (2012.2).
