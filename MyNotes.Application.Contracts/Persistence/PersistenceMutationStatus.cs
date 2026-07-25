namespace MyNotes.Application.Contracts.Persistence;

internal enum PersistenceMutationStatus
{
  Applied,
  Unchanged,
  NotFound,
  Expired
}