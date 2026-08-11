namespace MyNotes.Application.Results;

internal enum AppUpdateStatus
{
  Succeeded,
  Unchanged,
  TargetNotFound,
  Suspended,
  Canceled,
  Failed
}