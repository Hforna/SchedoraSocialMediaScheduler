using System;

namespace Schedora.Application.Requests;

public class SchedulePostRequest
{
    public DateTime ScheduledAtLocal { get; set; }
}

public class ReschedulePostRequest
{
    public DateTime ScheduledAtLocal { get; set; }
}

