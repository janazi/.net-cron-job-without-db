# .net-cron-job-without-db
Simple way to create a scheduled task using .Net Hosted Service, Quartz and Cronos.
No database needed.

# CronJobService.cs
Abstract class to be base of your tasks

# Worker 1 
Sample task

# appsettings.json
Section "cronjobs" created to store configuration to each cron task

# Program.cs
Makes use of an extension method to create a hosted service -> builder.Services.AddCronJob<Worker1>(builder.Configuration); 