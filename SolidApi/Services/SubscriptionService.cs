using SolidApi.Models;
using SolidApi.Repositories;
using System.Globalization;

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
        // 1. Validar o tipo de recurso
        if (resource.ResType != "subscription")
            throw new ArgumentException("Invalid resource type.");

        // 2. Validar o evento
        if (resource.Evt != 1 && resource.Evt != 2)
            throw new ArgumentException("Invalid event type. Use 1 or 2.");

        // 3. Verificar se a application e o channel existem
        if (!_repo.ChannelExists(app, channel))
            throw new KeyNotFoundException("Channel not found.");

        // 4. Validar o endpoint
        if (string.IsNullOrWhiteSpace(resource.Endpoint) ||
            !Uri.TryCreate(resource.Endpoint, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" &&
             uri.Scheme != "https" &&
             uri.Scheme != "mqtt" &&
             uri.Scheme != "mqtts"))
        {
            throw new ArgumentException("Invalid endpoint.");
        }

        // 5. Validar a data de expiração
        if (!string.IsNullOrWhiteSpace(resource.DtExpiration))
        {
            if (!DateTimeOffset.TryParse(
                resource.DtExpiration,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var expiration))
            {
                throw new ArgumentException("Invalid expiration date.");
            }

            resource.DtExpiration = expiration.ToString(
                "yyyy-MM-ddTHH:mm:ss",
                CultureInfo.InvariantCulture);
        }
        else
        {
            resource.DtExpiration = "";
        }

        // 6. Gerar nome automaticamente se necessário
        if (string.IsNullOrWhiteSpace(resource.ResourceName) ||
            _repo.ExistsByName(app, channel, resource.ResourceName))
        {
            string name;

            do
            {
                name = "sub-" + Guid.NewGuid().ToString("N")[..8];
            }
            while (_repo.ExistsByName(app, channel, name));

            resource.ResourceName = name;
        }

        // 7. Gerar data de criação
        resource.DtCreation = DateTime.UtcNow.ToString(
            "yyyy-MM-ddTHH:mm:ss",
            CultureInfo.InvariantCulture);

        // 8. Guardar na base de dados
        _repo.Create(app, channel, resource);

        // 9. Construir o caminho do recurso
        string path =
            $"/api/solid/{app}/{channel}/subs/{resource.ResourceName}";

        return (path, resource);
    }

    public SubscriptionResource Get(
        string app,
        string channel,
        string resourceName)
    {
        if (!_repo.ChannelExists(app, channel))
            throw new KeyNotFoundException("Channel not found.");

        var subscription = _repo.GetByName(
            app,
            channel,
            resourceName
        );

        if (subscription == null)
            throw new KeyNotFoundException("Subscription not found.");

        return subscription;
    }

    public bool Delete(string app, string channel, string resourceName)
    {
        if (!_repo.ChannelExists(app, channel))
            throw new KeyNotFoundException("Channel not found.");

        var subscription = _repo.GetByName(app, channel, resourceName);

        if (subscription == null)
            throw new KeyNotFoundException("Subscription not found.");

        return _repo.Delete(app, channel, resourceName);
    }

    public List<string> DiscoverByApplication(string app)
    {
        var subscriptions = _repo.GetAllByApplication(app);

        var paths = new List<string>();

        foreach (var subscription in subscriptions)
        {
            string path =
                $"/api/solid/{app}/{subscription.ChannelName}/subs/{subscription.ResourceName}";

            paths.Add(path);
        }

        return paths;
    }

    public List<string> DiscoverAll()
    {
        var subscriptions = _repo.GetAll();

        var paths = new List<string>();

        foreach (var subscription in subscriptions)
        {
            string path =
                $"/api/solid/{subscription.AppName}/{subscription.ChannelName}/subs/{subscription.ResourceName}";

            paths.Add(path);
        }

        return paths;
    }

    public List<string> DiscoverByChannel(string app, string channel)
    {
        var subscriptions = _repo.GetAllByChannel(app, channel);

        var paths = new List<string>();

        foreach (var subscription in subscriptions)
        {
            string path =
                $"/api/solid/{app}/{channel}/subs/{subscription}";

            paths.Add(path);
        }

        return paths;
    }

    public bool ChannelExists(string app, string channel)
    {
        return _repo.ChannelExists(app, channel);
    }

}
