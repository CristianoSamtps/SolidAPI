using SolidApi.Models;
using SolidApi.Repositories;

namespace SolidApi.Services;

public class SubscriptionService
{
    private readonly SubscriptionRepository _repo;

    public SubscriptionService(SubscriptionRepository repo)
    {
        _repo = repo;
    }

    public (string Path, SubscriptionResource Resource) Create(
        string app,
        string channel,
        SubscriptionResource resource)
    {
        if (resource.ResType != "subscription")
            throw new ArgumentException("Invalid resource type.");

        if (resource.Evt != 1 && resource.Evt != 2)
            throw new ArgumentException("Invalid event type.");

        if (string.IsNullOrWhiteSpace(resource.ResourceName))
            throw new ArgumentException("Resource name is required.");

        if (_repo.GetByName(resource.ResourceName) != null)
            throw new ArgumentException("Subscription already exists.");

        resource.DtCreation = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        _repo.Create(resource);

        string path = $"/api/solid/{app}/{channel}/subs/{resource.ResourceName}";

        return (path, resource);
    }
}
