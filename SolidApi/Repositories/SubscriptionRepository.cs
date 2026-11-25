using SolidApi.Models;
using System.Linq;
using System.Collections.Generic;

namespace SolidApi.Repositories;

public class SubscriptionRepository
{
    private readonly List<SubscriptionResource> _subscriptions = new();

    public void Create(SubscriptionResource resource)
    {
        _subscriptions.Add(resource);
    }

    public SubscriptionResource? GetByName(string resourceName)
    {
        return _subscriptions.FirstOrDefault(
            s => s.ResourceName == resourceName
        );
    }

    public bool Delete(string resourceName)
    {
        var subscription = GetByName(resourceName);

        if (subscription == null)
            return false;

        _subscriptions.Remove(subscription);
        return true;
    }

    public List<SubscriptionResource> GetAll()
    {
        return _subscriptions;
    }
}