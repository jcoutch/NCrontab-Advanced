# NCrontab Advanced

**If you have any problems, make sure to file an issue here on Github.**

We were looking to implement [NCrontab](https://github.com/atifaziz/NCrontab) for a project, but found it had a few shortcomings:
* No support for #, L and W
* Only supported two cron formats:  "SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK" and one without SECONDS.

So, I started looking into adding these features.  After some digging through the code, it became apparent that while the code worked well for the cron strings that it supported, it wouldn't scale well for support of #, L and W. :-(

In comes NCrontab-Advanced - a complete re-write of the parsing engine.  Along with the re-write come the following features:

**Support for the following cron formats:**
* `CronStringFormat.Default`: MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK
* `CronStringFormat.WithYears`: MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK YEARS
* `CronStringFormat.WithSeconds`: SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK
* `CronStringFormat.WithSecondsAndYears`: SECONDS MINUTES HOURS DAYS MONTHS DAYS-OF-WEEK YEARS

**How to build project:**

The project can be opened using Visual Studio 2015, or Visual Studio Code.  You can either build using MSBuild against the solution, or using .NET Core's `dotnet` command:

```
# Run this from the NCrontab.Advanced folder (which contains NCrontab.Advanced.csproj)
dotnet restore
dotnet build
```

NOTE - If you're building via `dotnet`, make sure you either comment out the `net35` section in project.json, or install the .NET Framework v3.5 (it's a supported framework by the Nuget package.)

**Support for the following cron expressions:**

```
Field name   | Allowed values  | Allowed special characters
------------------------------------------------------------
Minutes      | 0-59            | * , - /
Hours        | 0-23            | * , - /
Day of month | 1-31            | * , - / ? L W
Month        | 1-12 or JAN-DEC | * , - /
Day of week  | 0-6 or SUN-SAT  | * , - / ? L #
Year         | 0001–9999       | * , - /
```

Instructions for how cron expressions are formatted are on the [Cron Expresssions page on Wikipedia](https://en.wikipedia.org/wiki/Cron#CRON_expression), and documentation for using NCrontab.Advanced is over on the [Getting Started wiki](https://github.com/jcoutch/NCrontab-Advanced/wiki/Getting-started)!
