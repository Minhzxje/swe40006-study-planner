
using System.Globalization;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Display the Study Planner form
app.MapGet("/", () =>
    Results.Content(RenderPage(""), "text/html; charset=utf-8"));

// Process form submission and calculate daily study hours
app.MapPost("/calculate", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();

    string subject = form["subject"].ToString().Trim();
    string hoursInput = form["hours"].ToString();
    string daysInput = form["days"].ToString();

    // Validate user inputs
    if (string.IsNullOrWhiteSpace(subject) ||
        !decimal.TryParse(hoursInput, NumberStyles.Number,
            CultureInfo.InvariantCulture, out decimal totalHours) ||
        !int.TryParse(daysInput, out int days) ||
        totalHours <= 0 || totalHours > 10000 ||
        days <= 0 || days > 365)
    {
        string error = """
            <div class="error">
                Please enter valid study hours and days (1-365).
            </div>
            """;

        return Results.Content(
            RenderPage(error), "text/html; charset=utf-8");
    }

    // Calculate study hours needed per day
    decimal dailyHours =
        Math.Ceiling(totalHours / days * 10m) / 10m;

    string safeSubject = WebUtility.HtmlEncode(subject);

    string result = $"""
        <div class="result">
            <h2>Your Study Plan</h2>
            <p>Subject: <strong>{safeSubject}</strong></p>
            <p>Total study hours: {totalHours}</p>
            <p>Days remaining: {days}</p>
            <h3>
                {dailyHours.ToString("0.0",
                    CultureInfo.InvariantCulture)} hours/day
            </h3>
            <p>Recommended daily study time</p>
        </div>
        """;

    return Results.Content(
        RenderPage(result), "text/html; charset=utf-8");
});

app.Run();

// HTML interface for the application
static string RenderPage(string resultHtml)
{
    const string html = """
    <!DOCTYPE html>
    <html lang="en">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport"
              content="width=device-width, initial-scale=1">
        <title>Study Planner | SWE40006</title>
        <style>
            * { box-sizing: border-box; }
            body {
                font-family: Arial, sans-serif;
                background: #f1f5f9;
                margin: 0;
                color: #1e293b;
            }
            .container {
                max-width: 520px;
                margin: 70px auto;
                padding: 20px;
            }
            .card {
                background: white;
                padding: 35px;
                border-radius: 16px;
                box-shadow: 0 8px 30px #00000012;
            }
            h1 {
                text-align: center;
                color: #2563eb;
                margin-bottom: 8px;
            }
            .subtitle {
                text-align: center;
                color: #64748b;
                margin-bottom: 30px;
            }
            label {
                display: block;
                font-weight: bold;
                margin: 15px 0 8px;
            }
            input {
                width: 100%;
                padding: 12px;
                border: 1px solid #cbd5e1;
                border-radius: 8px;
                font-size: 15px;
            }
            button {
                width: 100%;
                padding: 14px;
                margin-top: 25px;
                background: #2563eb;
                color: white;
                border: none;
                border-radius: 8px;
                font-size: 16px;
                cursor: pointer;
            }
            button:hover { background: #1d4ed8; }
            .result {
                background: #eff6ff;
                padding: 20px;
                margin-top: 25px;
                border-radius: 10px;
                text-align: center;
            }
            .result h3 {
                font-size: 30px;
                color: #2563eb;
                margin: 16px 0 5px;
            }
            .error {
                background: #fef2f2;
                color: #b91c1c;
                padding: 15px;
                margin-top: 20px;
                border-radius: 8px;
            }
            footer {
                text-align: center;
                color: #94a3b8;
                margin-top: 20px;
                font-size: 12px;
            }
        </style>
    </head>
    <body>
        <div class="container">
            <div class="card">
                <h1>Study Planner</h1>
                <p class="subtitle">
                    Plan your study time effectively
                </p>

                <form method="post" action="/calculate">
                    <label for="subject">Subject Name</label>
                    <input id="subject" name="subject"
                           type="text"
                           placeholder="e.g. Software Deployment"
                           maxlength="100" required>

                    <label for="hours">Total Study Hours</label>
                    <input id="hours" name="hours"
                           type="number" min="0.1"
                           max="10000" step="0.1"
                           placeholder="e.g. 40" required>

                    <label for="days">Days Remaining</label>
                    <input id="days" name="days"
                           type="number" min="1"
                           max="365" step="1"
                           placeholder="e.g. 10" required>

                    <button type="submit">
                        Calculate Study Plan
                    </button>
                </form>

                <!--RESULT-->
            </div>
            <footer>
                SWE40006 | Software Deployment
            </footer>
        </div>
    </body>
    </html>
    """;

    return html.Replace("<!--RESULT-->", resultHtml);
}
