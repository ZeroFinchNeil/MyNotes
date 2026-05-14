using MyNotes.Application.Services;

namespace MyNotes.Services.Commands;

internal class CommandServiceFactory() : IServiceFactory
{
  public required Dictionary<CommandServiceType, ICommandService?> ResolveMap { get; init; }

  public ICommandService Resolve(CommandServiceType serviceType)
  {
    return ResolveMap.TryGetValue(serviceType, out var service) && service is ICommandService commandService
      ? commandService
      : throw new ArgumentException("Invalid service type");
  }
}
